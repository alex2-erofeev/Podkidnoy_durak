using System;
using System.Linq;
using System.Collections.Generic;

namespace CardFool
{
    public class MPlayer1
    {
        private string Name = "First";
        private List<SCard> hand = new List<SCard>();       // карты на руке
        private SCard Trump;
        private int cards_num = 36; // количество карт, находящихся в игре
        private List<SCard> deck = MGameRules.GetDeck(); //карты, находящиеся в игре

        // Возвращает имя игрока
        public string GetName()
        {
            return Name;
        }
        //Возвращает количество карт на руке
        public int GetCount()
        {
            return hand.Count;
        }
        //Добавление карты в руку, во время добора из колоды, или взятия карт
        public void AddToHand(SCard card)
        {
            hand.Add(card);
            hand = Sort_for_attac();
        }

        //Начальная атака
        public List<SCard> LayCards()
        {
            SCard card_attac = hand[0];
            hand.Remove(card_attac);
            return new List<SCard> { card_attac };
        }

        private List<SCard> Sort_for_attac()
        {
            List<SCard> new_hand = new List<SCard>();
            List<SCard> trumps = new List<SCard>();
            List<SCard> no_trumps = new List<SCard>();
            foreach (SCard card in hand)
            {
                if (card.Suit == Trump.Suit)
                    trumps.Add(card);
                else
                    no_trumps.Add(card);
            }
            no_trumps.Sort((a, b) => a.Rank.CompareTo(b.Rank));
            trumps.Sort((a, b) => a.Rank.CompareTo(b.Rank));

            new_hand.AddRange(no_trumps);
            new_hand.AddRange(trumps);
            return new_hand;
        }

        //Защита от карт
        //На вход подается набор карт на столе, часть из них могут быть уже покрыты
        public bool Defend(List<SCardPair> table)
        {
            bool CardBeaten = true;
            for (int i = 0; i < table.Count; i++)
            {
                List<SCard> defend_cards = new List<SCard>();
                SCardPair Pair = table[i];
                if (table[i].Beaten)
                    continue;
                foreach (SCard card in hand)
                {

                    if (Pair.SetUp(card, Trump.Suit))
                    {
                        defend_cards.Add(card);
                    }
                }
                if (defend_cards.Count != 0)
                {
                    SCard fin_card = Choosing_card(defend_cards);
                    Pair.SetUp(fin_card, Trump.Suit);
                    table[i] = Pair;
                    hand.Remove(fin_card);
                }
                else
                {
                    CardBeaten = false;
                }
            }
            return CardBeaten;
        }

        public SCard Choosing_card(List<SCard> defend_cards)
        {
            SCard nice_card = new SCard();
            int k1 = 100;
            foreach (SCard card in defend_cards)
            {
                if (defend_cards[0].Suit == Trump.Suit)
                {
                    nice_card = defend_cards[0];
                    break;
                }
                int k = 0;
                foreach (SCard card2 in deck)
                {
                    if (card.Rank == card2.Rank)
                    {
                        k++;
                    }
                }
                if (k < k1)
                {
                    nice_card = card;
                    k1 = k;
                    k = 0;
                }
            }
            return nice_card;
        }

        //Добавление карт
        //На вход подается набор карт на столе, а также отбился ли оппонент
        public bool AddCards(List<SCardPair> table, bool OpponentDefenced)
        {
            bool flag = false;
            int not_beaten = 0;
            int table_cards = 0;
            foreach (SCardPair pair in table)
            {
                if (pair.Beaten)
                    table_cards += 2;
                else
                {
                    not_beaten++;
                    table_cards += 1;
                }
            }
            if (table.Count == 6 || (cards_num - hand.Count - table_cards) == 0 || (cards_num - hand.Count - table_cards) == not_beaten)
                return false;
            else
            {
                int i = 0;
                List<SCard> attack_card = new List<SCard>();
                List<Suits> weak_suits = Weak_Suits(table);
                while (i < hand.Count)
                {
                    for (int j = 0; j < table.Count; j++)
                    {
                        if (hand[i].Rank == table[j].Down.Rank || hand[i].Rank == table[j].Up.Rank)
                        {
                            attack_card.Add(hand[i]);
                            flag = true;
                        }
                    }
                    i++;
                }
                if (flag)
                {
                    if ((!OpponentDefenced) && (attack_card[0].Suit == Trump.Suit))
                    {
                        return false;
                    }
                    foreach (SCard card in attack_card)
                    {
                        if (weak_suits.Contains(card.Suit))
                        {
                            table.Add(new SCardPair(card));
                            hand.Remove(card);
                            return true;
                        }
                    }
                    table.Add(new SCardPair(attack_card[0]));
                    hand.Remove(attack_card[0]);
                    return true;
                }
            }
            return false;
        }
        public List<Suits> Weak_Suits(List<SCardPair> tbl)
        {
            List<Suits> ws = new List<Suits>();
            //List<SCard> ws = new List<SCard>();
            foreach (SCardPair pair in tbl)
            {
                if (pair.Up.Suit == Trump.Suit && pair.Down.Suit != Trump.Suit)
                    ws.Add(pair.Down.Suit);
            }
            return ws;
        }

        //Вызывается после основной битвы, когда известно отбился ли защищавшийся
        //На вход подается набор карт на столе, а также была ли успешной защита
        public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
        {
            if (IsDefenceSuccesful)
            {
                cards_num -= table.Count * 2;
                foreach (SCardPair pair in table)
                {
                    if (pair.Beaten)
                        deck.Remove(pair.Up);
                    deck.Remove(pair.Down);
                }
            }
        }
        //Установка козыря, на вход подаётся козырь, вызывается перед первой раздачей карт
        public void SetTrump(SCard NewTrump)
        {
            Trump = NewTrump;
        }
    }
}