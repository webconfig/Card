//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChannelServer.Server
//{
//    public class Deck
//    {
//        private readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

//        public int CardCount { get; private set; }
//        public Card[] Cards { get; private set; }

//        public Deck()
//        {
//            CardCount = 52;
//            Cards = GetSortedDeck();
//        }

//        public void Shuffle()
//        {
//            Card[] newCardOrder = new Card[CardCount];

//            for (int i = CardCount - 1; i >= 0; i--)
//            {
//                int result = GetRandomNumber(i);
//                int count = -1;

//                for (int cardIndex = 0; cardIndex < CardCount; cardIndex++)
//                {
//                    if (newCardOrder[cardIndex] == null)
//                    {
//                        count++;
//                    }

//                    if (count == result)
//                    {
//                        int index = 0;

//                        for (int i2 = 0; i2 < Cards.Length; i2++)
//                        {
//                            if (Cards[i2] != null)
//                            {
//                                index = i2;
//                            }
//                        }

//                        newCardOrder[cardIndex] = Cards[index];
//                        Cards[index] = null;
//                        break;
//                    }
//                }
//            }

//            Cards = newCardOrder;
//        }

//        private int GetRandomNumber(int maxValue)
//        {
//            byte[] randomNumber = new byte[1];

//            _generator.GetBytes(randomNumber);

//            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);
//            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

//            int range = maxValue + 1;

//            double randomValueInRange = Math.Floor(multiplier * range);

//            return (int)(randomValueInRange);
//        }

//        private Card[] GetSortedDeck()
//        {
//            Card[] sortedDeck = new Card[CardCount];

//            for (int suitIndex = 0; suitIndex < 4; suitIndex++)
//            {
//                for (int valueIndex = 0; valueIndex < 13; valueIndex++)
//                {
//                    sortedDeck[suitIndex * 13 + valueIndex] = new Card((CardValue)valueIndex, (CardSuit)suitIndex);
//                }
//            }

//            return sortedDeck;
//        }
//    }
//}
