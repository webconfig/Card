//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChannelServer.Server
//{
//    public class Card
//    {
//        public CardValue Value { get; set; }
//        public CardSuit Suit { get; set; }

//        public Card()
//        {

//        }

//        public Card(CardValue value, CardSuit suit)
//        {
//            Value = value;
//            Suit = suit;
//        }

//        public override string ToString()
//        {
//            return Value.ToString() + " of " + Suit.ToString() + "s";
//        }
//    }
//    public enum CardValue
//    {
//        Deuce,
//        Tray,
//        Four,
//        Five,
//        Six,
//        Seven,
//        Eight,
//        Nine,
//        Ten,
//        Jack,
//        Queen,
//        King,
//        Ace
//    }
//    public enum CardSuit
//    {
//        Heart,
//        Diamond,
//        Club,
//        Spade
//    }
//}
