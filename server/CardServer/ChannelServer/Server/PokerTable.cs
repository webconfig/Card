//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChannelServer.Server
//{
//    public class PokerTable
//    {
//        #region Fields and properties

//        /// <summary>
//        /// 旁观的人
//        /// </summary>
//        [NonSerialized]
//        private List<Observer> _observers = new List<Observer>();

//        /// <summary>
//        /// 洗牌的人
//        /// </summary>
//        private readonly Deck _deck = new Deck();

//        [NonSerialized]
//        private int _lastToAct = -1;

//        /// <summary>
//        /// 玩牌的人
//        /// </summary>
//        public List<PokerPlayer> Players { get; } = new List<PokerPlayer>();

//        public int SmallBlind { get; set; } = 25;
//        public int BigBlind { get; set; } = 50;

//        public int DealerPosition { get; set; } = -1;
//        public int ToAct { get; set; } = -1;

//        public int ChipsToCall { get; set; }

//        public List<Pot> Pots { get; set; } = new List<Pot>();

//        public bool BigBlindException { get; set; } = true;

//        public Card[] Flop { get; set; }
//        public Card Turn { get; set; }
//        public Card River { get; set; }
//        /// <summary>
//        /// 旁观者
//        /// </summary>
//        public List<Observer> Observers
//        {
//            get
//            {
//                return _observers;
//            }
//        }

//        public bool HasFreeSeats
//        {
//            get
//            {
//                return Players.Count < 9;
//            }
//        }

//        public bool IsRoundOver
//        {
//            get
//            {
//                if (Flop == null && ToAct == ((DealerPosition + 1) % Players.Count) && BigBlindException && Players[ToAct].Chips != 0)
//                {
//                    BigBlindException = false;
//                    return false;
//                }

//                foreach (PokerPlayer player in Players)
//                {
//                    if (!player.IsFolded && player.ChipsInPot < ChipsToCall && player.Chips != 0)
//                    {
//                        return false;
//                    }
//                }

//                if (ChipsToCall == 0 && ToAct != GetLastToAct(_lastToAct) && !Array.TrueForAll(Players.ToArray(), c => c.Chips == 0))
//                {
//                    return false;
//                }

//                return true;
//            }
//        }

//        #endregion

//        #region Initialization

//        public PokerTable()
//        {

//        }

//        #endregion

//        #region Methods

//        public void SeatPlayer(PokerPlayer player)
//        {
//            if (HasFreeSeats)
//            {
//                player.IsFolded = true;
//                player.Chips = 1000;

//                Players.Add(player);
//            }
//            else
//            {
//                throw new InvalidOperationException("Table has no seats left!");
//            }
//        }

//        public void StartNewRound()
//        {
//            PokerPlayer[] toRemove = Players.Where(c => c.Chips == 0).ToArray();

//            for (int i = 0; i < toRemove.Length; i++)
//            {
//                Players.Remove(toRemove[i]);
//            }
            
//            _deck.Shuffle();

//            for (int i = 0; i < Players.Count; i++)
//            {
//                if (Players[i].Chips == 0)
//                {
//                    Players[i].IsFolded = true;
//                }
//                else
//                {
//                    Players[i].IsFolded = false;
//                }

//                Players[i].IsWinner = false;
//                Players[i].ValueText = "";
//                Players[i].Card1 = _deck.Cards[i * 2 + 5];
//                Players[i].Card2 = _deck.Cards[i * 2 + 6];
//            }

//            Pots.Add(new Pot());

//            Flop = null;
//            Turn = null;
//            River = null;

//            if (DealerPosition == -1)
//            {
//                DealerPosition = Players.Count - 1;
//            }

//            DealerPosition = GetNextActive(DealerPosition);
//            ToAct = GetNextActive(DealerPosition);

//            BetRaise(SmallBlind);
//            BetRaise(BigBlind);
//            BigBlindException = true;
//        }

//        private int GetNextActive(int startingPoint)
//        {
//            int next = ((startingPoint + 1) % Players.Count);

//            for (int i = 0; i < Players.Count; i++)
//            {
//                if (!Players[next].IsFolded && Players[next].Chips > 0)
//                {
//                    break;
//                }

//                next = (next + 1) % Players.Count;
//            }

//            return next;
//        }

//        public void RemovePlayer(int index)
//        {
//            if (ToAct == index)
//            {
//                Fold();
//            }

//            Players.RemoveAt(index);

//            if (ToAct > 0)
//            {
//                ToAct--;
//            }

//            if (DealerPosition == Players.Count)
//            {
//                DealerPosition--;
//            }


//        }

//        public int SetAfterDealer(out bool isEarly)
//        {
//            BigBlindException = false;

//            isEarly = Players.Count(c => c.Chips != 0 && c.IsFolded == false) < 2;

//            ChipsToCall = 0;
//            ToAct = GetNextActive(DealerPosition);

//            _lastToAct = GetLastToAct(DealerPosition % Players.Count);

//            int first = -1;

//            if (Flop == null)
//            {
//                Flop = new Card[3];
//                Flop[0] = _deck.Cards[0];
//                Flop[1] = _deck.Cards[1];
//                Flop[2] = _deck.Cards[2];

//                if (!isEarly) return 0;
//                first = 0;
//            }

//            if (Turn == null)
//            {
//                Turn = _deck.Cards[3];

//                if (!isEarly) return 1;
//                if (first == -1) first = 1;
//            }

//            if (River == null)
//            {
//                River = _deck.Cards[4];

//                if (!isEarly) return 2;
//                if (first == -1) first = 2;
//            }
//            else
//            {
//                if (!isEarly) return 3;
//                if (first == -1) first = 3;
//            }

//            return first;
//        }

//        private int GetLastToAct(int startingPoint)
//        {
//            for (int i = 0; i < Players.Count; i++)
//            {
//                if (!Players[startingPoint].IsFolded && Players[startingPoint].Chips > 0)
//                {
//                    return startingPoint;
//                }

//                startingPoint -= 1;

//                if (startingPoint == -1)
//                {
//                    startingPoint = Players.Count - 1;
//                }
//            }

//            return -1;
//        }

//        public void NextToAct()
//        {
//            if (IsRoundOver)
//            {
//                ManagePots();

//                ToAct = -1;
//            }
//            else
//            {
//                ToAct = GetNextActive(ToAct);
//            }
//        }

//        private void ManagePots()
//        {
//            PokerPlayer[] playersAllIn = Players.Where(c => c.Chips == 0 && c.ChipsInPot > 0).OrderBy(c => c.ChipsInPot).ToArray();

//            for (int i = 0; i < playersAllIn.Length; i++)
//            {
//                int currentChipsInPot = playersAllIn[i].ChipsInPot;

//                if (i > 0 && currentChipsInPot == 0)
//                {
//                    Pots[1].IndexexToWinFor.Add(Players.IndexOf(playersAllIn[i]));
//                    continue;
//                }

//                int toPot = 0;
//                Pot pot = new Pot();

//                if (i == playersAllIn.Length - 1)
//                {
//                    pot.Chips += Pots[0].Chips;
//                    Pots[0].Chips = 0;
//                }

//                foreach (PokerPlayer player in Players)
//                {
//                    if (player.ChipsInPot < currentChipsInPot)
//                    {
//                        toPot = player.ChipsInPot;
//                    }
//                    else
//                    {
//                        toPot = currentChipsInPot;
//                    }

//                    player.ChipsInPot -= toPot;
//                    pot.Chips += toPot;
//                }

//                if (pot.Chips == currentChipsInPot)
//                {
//                    playersAllIn[i].Chips += pot.Chips;
//                }
//                else
//                {
//                    pot.IndexexToWinFor.Add(Players.IndexOf(playersAllIn[i]));
//                    Pots.Insert(1, pot);
//                }
//            }

//            foreach (PokerPlayer player in Players)
//            {
//                Pots[0].Chips += player.ChipsInPot;
//                player.ChipsInPot = 0;
//            }

//            ChipsToCall = 0;
//        }

//        public void Fold()
//        {
//            Players[ToAct].IsFolded = true;

//            PokerPlayer[] result = Players.Where(c => c.IsFolded == false).ToArray();

//            if (result.Length == 1)
//            {
//                foreach (PokerPlayer player in Players)
//                {
//                    Pots[0].Chips += player.ChipsInPot;
//                    player.ChipsInPot = 0;
//                }

//                result[0].IsWinner = true;
//                result[0].Chips += Pots[0].Chips;

//                Pots.Clear();
//                ToAct = -2;

//                return;
//            }

//            NextToAct();
//        }

//        public void CheckCall()
//        {
//            if (Players[ToAct].TotalChips <= ChipsToCall)
//            {
//                Players[ToAct].ChipsInPot += Players[ToAct].Chips;
//                Players[ToAct].Chips = 0;

//                NextToAct();
//                return;
//            }

//            BetRaise(ChipsToCall);
//        }

//        public void BetRaise(int chips)
//        {
//            if (ToAct < 0) return;

//            ChipsToCall = chips;

//            int difference = ChipsToCall - Players[ToAct].ChipsInPot;

//            Players[ToAct].Chips -= difference;
//            Players[ToAct].ChipsInPot += difference;

//            NextToAct();
//        }

//        public void Finish(List<List<int>> ranking, string[] valueTexts)
//        {
//            foreach (PokerPlayer player in Players)
//            {
//                Pots[0].Chips += player.ChipsInPot;
//                player.ChipsInPot = 0;
//            }

//            PokerPlayer[] showDownPlayers = Players.Where(c => c.IsFolded == false).ToArray();

//            for (int i = 0; i < valueTexts.Length; i++)
//            {
//                showDownPlayers[i].ValueText = valueTexts[i];
//            }

//            // MANAGE WHO GETS WHAT

//            for (int potI = (Pots.Count - 1); potI > 0; potI--)
//            {
//                int chipsToGet = Pots[potI].Chips / ranking[0].Count;
//                bool anyWinner = false;

//                for (int sameI = 0; sameI < ranking[0].Count; sameI++)
//                {
//                    if (Pots[potI].IndexexToWinFor.Contains(ranking[0][sameI]))
//                    {
//                        Players[ranking[0][sameI]].Chips += chipsToGet;
//                        Pots[potI].Chips -= chipsToGet;

//                        Players[ranking[0][sameI]].HasCashed = true;

//                        anyWinner = true;
//                    }
//                }

//                if (!anyWinner) Pots[potI - 1].Chips += Pots[potI].Chips;
//            }

//            for (int rankI = 0; rankI < ranking.Count; rankI++)
//            {
//                int mainPotWinnsers = 0;

//                for (int sameI = 0; sameI < ranking[rankI].Count; sameI++)
//                {
//                    if (!Players[ranking[rankI][sameI]].HasCashed)
//                    {
//                        mainPotWinnsers++;
//                    }
//                }

//                if (mainPotWinnsers > 0)
//                {
//                    int chipsToGet = Pots[0].Chips / mainPotWinnsers;

//                    for (int sameI = 0; sameI < ranking[rankI].Count; sameI++)
//                    {
//                        if (!Players[ranking[rankI][sameI]].HasCashed)
//                        {
//                            Players[ranking[rankI][sameI]].Chips += chipsToGet;
//                        }
//                    }
//                    break;
//                }
//            }

//            foreach (int playerIndex in ranking[0])
//            {
//                Players[playerIndex].IsWinner = true;
//            }

//            foreach (PokerPlayer player in Players)
//            {
//                player.HasCashed = false;
//                player.ChipsInPot = 0;
//            }

//            Pots.Clear();
//            ToAct = -1;
//        }

//        public override string ToString()
//        {
//            string started = "";

//            if (ToAct >= 0)
//            {
//                started = " (Already Started)";
//            }

//            return $"Table {PokerSalon.Tables.IndexOf(this) + 1}  {Players.Count}/9" + started;
//        }

//        #endregion
//    }
//}
