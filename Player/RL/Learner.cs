#undef __DEBUG__
using System.Collections.Generic;
using System;
using System.Collections;
using System.Diagnostics;
namespace Player.RL
{
    class Learner
    {
        /// <summary>
        /// The gamma value for Q-Learning
        /// </summary>
        protected const float GAMMA = 0.8F;
        /// <summary>
        /// The game states' stack
        /// </summary>
        protected static Stack<GameState> GameStates { get; set; }
        /// <summary>
        /// The `Q` matrix
        /// </summary>
        protected static Dictionary<String, float> _Q { get; set; }
        /// <summary>
        /// Holds mutation factores
        /// </summary>
        protected static KeyValuePair<SAPair, uint> PrevMutationFactore { get; set; }
        /// <summary>
        /// Debug UI handler
        /// </summary>
        protected static iDebug DebugUI { get; set; }
        /// <summary>
        /// Random number generator
        /// </summary>
        protected static Random RandGen { get; set; }
        /// <summary>
        /// Static constructor
        /// </summary>
        static Learner()
        {
            DebugUI = new iDebug(Console.Title);
            DebugUI.Hide();
            RandGen = new Random(Environment.TickCount);
            PrevMutationFactore = new KeyValuePair<SAPair, uint>();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 100;
            t.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                lock (RandGen)
                {
                    RandGen = new Random(Environment.TickCount);
                }
            });
            System.IO.File.WriteAllText("goals.log", "");
            System.IO.File.WriteAllText("event.log", "");
        }
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public static void Init()
        {
            // init game's states
            GameStates = new Stack<GameState>();
            // init hash table
            _Q = new Dictionary<String, float>();
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameState">The game status</param>
        /// <returns>The direction</returns>
        public static Direction ChooseAction(GameState gameState)
        {
            try
            {
                // next-state max Q pair list
                List<KeyValuePair<float, Direction>> nsmqpl = new List<KeyValuePair<float, Direction>>();
                // next-state max Q pair
                KeyValuePair<float, Direction> nsmqp = new KeyValuePair<float, Direction>(float.NegativeInfinity, Direction.HOLD);
                foreach (var direction in Enum.GetValues(typeof(Direction)))
                {
                    float nsqv = getQVal(gameState, (Direction)direction) + getReward(getNextState(gameState, (Direction)direction));
                    if (nsqv > nsmqp.Key)
                    {
                        nsmqp = new KeyValuePair<float, Direction>(nsqv, (Direction)direction);
                        // in next `if` statementment we will fall-back to
                        // adding currently updated `nsmqp` into `nsmqpl`.
                        // but for now we need to clear the list
                        nsmqpl.Clear();
#if __DEBUG__
                        DebugUI.AddLine("=====CLEARED=====");
#endif
                    }
                    if (nsqv == nsmqp.Key)
                    {
                        nsmqpl.Add(new KeyValuePair<float, Direction>(nsqv, (Direction)direction));
#if __DEBUG__
                        DebugUI.AddLine("\t{0} = {1}", ((Direction)direction).ToString(), nsqv);
#endif
                    }
                }
                int candIndex = -1;
                // candidate container
                KeyValuePair<float, Direction> candidate = new KeyValuePair<float, Direction>();
                // while there is a item in list
                while (nsmqpl.Count != 0)
                {
                    lock (RandGen)
                    {
                        candIndex = RandGen.Next(0, nsmqpl.Count);
                        // pick a random candidate
                        candidate = nsmqpl[candIndex];
                        // validate the mutation factore
                        if (getMutationVal(gameState, candidate.Value) < 10) goto __PROCEED;
                        // remove the current
                        nsmqpl.RemoveAt(candIndex);
                    }
                }
                var dirs = (int[])Enum.GetValues(typeof(Direction));
                var randDir = (Direction)dirs[RandGen.Next(0, dirs.Length)];
                candidate = new KeyValuePair<float, Direction>(getQVal(gameState, randDir), randDir);
#if __DEBUG__
                DebugUI.AddLine("CAND_INDEX: {0}", candIndex);
#endif
            __PROCEED:
                // update the mution factore for current state
                updateMutaionFactore(gameState, candidate.Value);
                //candidate = new KeyValuePair<float, Direction>(getRVal(gameState, Direction.EAST), Direction.EAST);
#if __DEBUG__
                DebugUI.AddLine("{0} Candidate Detected.", nsmqpl.Count);
#endif
                var nextState = getNextState(gameState, candidate.Value);
                var R = getReward(gameState);// getRVal(gameState, candidate.Value);
                var nQv = R + GAMMA * getQVal(gameState, candidate.Value);
#if __DEBUG__
                DebugUI.AddText("Q{1} {0:F3} -> ", getQVal(gameState, candidate.Value), "{");
#endif
                updateQ(
                    gameState,                                                      // The state
                    candidate.Value,                                                // The direction
                    nQv                                                             // The update value
                );
#if __DEBUG__
                DebugUI.AddLine("{0:F3} {1}, RVAL {2} {3:F3} {1}:\t\t\t\tMOVE `{4}`", getQVal(gameState, candidate.Value), "}", "{", R, candidate.Value.ToString());
#endif
                if (GameStates.Count > 0)
                    GameStates.Pop();
                // push current game status
                GameStates.Push(gameState);
#if __DEBUG__
                DebugUI.AddLine(gameState.MyLocation.ToString());
#endif
                //return Direction.EAST;
                return candidate.Value;
            }
            catch (Exception e) { Console.WriteLine(); Console.WriteLine(e.ToString()); Debug.WriteLine(e.ToString()); throw e; }
        }
        protected static GameState getNextState(GameState s, Direction a)
        {
            GameState _s = s.Clone() as GameState;
            switch (a)
            {
                case Direction.HOLD: break;
                case Direction.NORTH:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X - 1, s.MyLocation.Y);
                    break;
                case Direction.SOUTH:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X + 1, s.MyLocation.Y);
                    break;
                case Direction.WEST:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X, s.MyLocation.Y - 1);
                    break;
                case Direction.EAST:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X, s.MyLocation.Y + 1);
                    break;
            }
            if (_s.MyLocation == _s.OpponentLocation) _s.IsBallMine = !_s.IsBallMine;
            if (_s.IsBallMine)
            {
                if (CalcDistanceToGate(s, true, true) == 0)
                {
                    _s.MyScore++;
                    _s.Game_State = GameState.State.PLAYER_SCORED;
                }
                else if (CalcDistanceToGate(s, false, true) == 0)
                {
                    _s.OpponentScore++;
                    _s.Game_State = GameState.State.OPPONENT_SCORED;
                }
            }
            else
            {
                if (CalcDistanceToGate(s, false, false) == 0)
                {
                    _s.OpponentScore++;
                    _s.Game_State = GameState.State.OPPONENT_SCORED;
                }
                else if (CalcDistanceToGate(s, true, false) == 0)
                {
                    _s.MyScore++;
                    _s.Game_State = GameState.State.PLAYER_SCORED;
                }
            }
            return _s;
        }
        /// <summary>
        /// Get reward of passed game status
        /// </summary>
        /// <param name="gs">The game state</param>
        /// <returns>The related reward for passed game state</returns>
        protected static float getReward(GameState gs)
        {
            // if no previously game state has been recorded
            if (GameStates.Count == 0) /* NO REWARD AT INTI STATE */ return 0;
            // get reward of current state based on combination of current and previous state's status
            float r = 0;
            // +75 if i score-up
            if (gs.Game_State == GameState.State.PLAYER_SCORED)
            {
                DebugUI.AddLine(String.Format("+ GS[{0}]: ME Scored-up{5}Me: `{1}`{5}|{5}OP: `{2}`{5}|{5}MyLoc: `{3}`{5}|{5}OpLoc: `{4}`", gs.GameStep, gs.MyScore, gs.OpponentScore, GameStates.Peek().MyLocation, GameStates.Peek().OpponentLocation, new string(' ', 4)));
#if __DEBUG__
                DebugUI.AddLine("+75.0 ");
#endif
                r += 100.0F;
            }
            // -100 if i score-down
            if (gs.Game_State == GameState.State.OPPONENT_SCORED)
            {
                DebugUI.AddLine(String.Format("- GS[{0}]: OPPONENT Scored-up{5}Me `{1}`{5}|{5}OP `{2}`{5}|{5}MyLoc: `{3}`{5}|{5}OpLoc: `{4}`", gs.GameStep, gs.MyScore, gs.OpponentScore, GameStates.Peek().MyLocation, GameStates.Peek().OpponentLocation, new string(' ', 4)));
#if __DEBUG__
                DebugUI.AddLine("-100.0 ");
#endif
                r -= 100.0F;
            }
            // +10 if i acquired the bull
            if (!GameStates.Peek().IsBallMine && gs.IsBallMine)
            {
                DebugUI.AddLine("===== CAUGHT THE BALL =====");
#if __DEBUG__
                DebugUI.AddLine("+10.0 ");
#endif
                r += 50.0F / CalcDistanceToGate(gs, false);
            }
            // -20 if i lost the ball
            if (GameStates.Peek().IsBallMine && !gs.IsBallMine)
            {
                DebugUI.AddLine("===== LOST THE BALL =====");
#if __DEBUG__
                DebugUI.AddLine("-20.0 ");
#endif
                r -= 50.0F / CalcDistanceToGate(gs);
            }
            float d = 0;
            if (gs.IsBallMine)
            {
                d = (10 * (CalcDistanceToGate(gs)));
                //DebugUI.AddLine("CalcDistanceToGate(gs) = {0}", d);
            }
            else
            {
                d = (10 * (CalcDistanceToBall(gs)));
                //DebugUI.AddLine("CalcDistanceToBall(gs) = {0}", d);
            }
            r -= d;
            //DebugUI.AddLine("r = {0}", r);
            if (gs.Game_State == GameState.State.OPPONENT_SCORED && GameStates.Peek().IsBallMine)
            { DebugUI.AddLine(String.Format("GS[{0}]: Made an own-goal: Me {1} | OP {2}", gs.GameStep, gs.MyScore, gs.OpponentScore, Console.Title)); }
            return r;
        }
        /// <summary>
        /// Updates the `Q` table
        /// </summary>
        /// <param name="s">The game state</param>
        /// <param name="a">The action</param>
        /// <param name="val">The value of state-action</param>
        protected static void updateQ(GameState s, Direction a, float val)
        {
            var key = new SAPair(s, a).GetHashCode();
            if (_Q.ContainsKey(key))
                _Q[key] = val;
            else
                _Q.Add(key, val);
        }
        /// <summary>
        /// Calculates the `Q` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `Q` value</returns>
        protected static float getQVal(GameState s, Direction a)
        {
            var key = new SAPair(s, a).GetHashCode();
            if (!_Q.ContainsKey(key))
            {
                float q = 0;
                var ns = getNextState(s, a);
                if (a == Direction.EAST)
                {
                    if (ns.IsBallMine) q += 5.0F;
                    else q -= 5.0F;
                }
                if (ns.Game_State == GameState.State.OPPONENT_SCORED) q = float.NegativeInfinity;
                if (ns.Game_State == GameState.State.PLAYER_SCORED) q = float.PositiveInfinity;
                updateQ(s, a, q);
            }
            // if making an own-goal
            if (IsInFrontOfGate(s, false) && a == Direction.WEST) updateQ(s, a, float.NegativeInfinity);
            if (IsInFrontOfGate(s, true))
            {
                switch (a)
                {
                    case Direction.EAST: updateQ(s, a, float.PositiveInfinity); break;
                    case Direction.HOLD: updateQ(s, a, +100); break;
                    default: updateQ(s, a, float.NegativeInfinity); break;
                }
            }
            if (_Q.ContainsKey(key))
                return _Q[key];
            return 0;
        }
        /// <summary>
        /// Updates the `MutationFactore` table
        /// </summary>
        /// <param name="s">The game state</param>
        /// <param name="a">The action</param>
        /// <param name="val">The mutation factore's value</param>
        protected static void updateMutaionFactore(GameState s, Direction a)
        {
            var key = new SAPair(s, a);
            uint val = 0;
            if (PrevMutationFactore.Key != null && PrevMutationFactore.Key.GetHashCode() == key.GetHashCode())
                val = PrevMutationFactore.Value + 1;
            PrevMutationFactore = new KeyValuePair<SAPair, uint>(key, val);
        }
        /// <summary>
        /// Get `MutationFactore` for the state-action
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The mutation factore's value</returns>
        protected static uint getMutationVal(GameState s, Direction a)
        {
            if (PrevMutationFactore.Key == null) return 0;
            var key = new SAPair(s, a);
            if (PrevMutationFactore.Key.GetHashCode() == key.GetHashCode())
                return PrevMutationFactore.Value;
            return 0;
        }
        /// <summary>
        /// Calculates the `R` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `R` value</returns>
        protected static float getRVal(GameState s, Direction a)
        {
            float r = 0;
            GameState _s = s.Clone() as GameState;
            switch (a)
            {
                case Direction.HOLD: break;
                case Direction.NORTH:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X - 1, s.MyLocation.Y);
                    break;
                case Direction.SOUTH:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X + 1, s.MyLocation.Y);
                    break;
                case Direction.WEST:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X, s.MyLocation.Y - 1);
                    break;
                case Direction.EAST:
                    _s.MyLocation = new System.Drawing.Point(_s.MyLocation.X, s.MyLocation.Y + 1);
                    break;
            }
#if __DEBUG__
            DebugUI.AddLine("GATE DIST: {0}", 100/CalcDistanceToGate(_s));
            DebugUI.AddLine("BALL DIST: {0}", -10*CalcDistanceToBall(_s));
#endif
            if (_s.MyLocation == _s.OpponentLocation) _s.IsBallMine = !_s.IsBallMine;
            float e = 0;
            if (_s.IsBallMine)
            {
                e = (float)Math.Exp(-1 * CalcDistanceToGate(s));
                if (CalcDistanceToGate(s, true, true) == 0)
                    _s.MyScore++;
                else if (CalcDistanceToGate(s, false, true) == 0)
                    _s.OpponentScore++;
            }
            else
            {
                e = (float)Math.Exp(-10 * CalcDistanceToBall(s));
                if (CalcDistanceToGate(s, false, false) == 0)
                    _s.OpponentScore++;
                else if (CalcDistanceToGate(s, true, false) == 0)
                    _s.MyScore++;
            }
            r = (1 - e) / (1 + e);
            if (_s.IsBallMine) r *= 10;
            else r *= -10;
            return r + getReward(_s);
        }
        /// <summary>
        /// Calculates the distance to goal 
        /// </summary>
        /// <param name="s">Game's status</param>
        /// <param name="opGate">Check distance to opponent's gate or mine?</param>
        /// <param name="myDist">If `true` calculates my distance to a gate; otherwise calculates the opponent's to a gate</param>
        /// <returns>The distance to gate</returns>
        protected static float CalcDistanceToGate(GameState s, bool opGate = true, bool myDist = true)
        {
            var gColumn = 10;
            var g1 = new System.Drawing.Point(3, gColumn);
            var g2 = new System.Drawing.Point(4, gColumn);
            if (!opGate)
            {
                g1 = new System.Drawing.Point(3, 0);
                g2 = new System.Drawing.Point(4, 0);
            }
            var x = s.MyLocation.X;
            var y = s.MyLocation.Y;
            if (!myDist)
            {
                x = s.OpponentLocation.X;
                y = s.OpponentLocation.Y;
            }
            var g39 = (float)(Math.Abs(g1.X - x) + Math.Abs(g1.Y - y));
            var g49 = (float)(Math.Abs(g2.X - x) + Math.Abs(g2.Y - y));
            return Math.Min(g39, g49);
        }
        /// <summary>
        /// Calculates the distance to the ball
        /// </summary>
        /// <param name="s">Game's status</param>
        /// <returns>The distance to ball</returns>
        protected static float CalcDistanceToBall(GameState s)
        {
            if (s.IsBallMine) return 0;
            var b = s.OpponentLocation;
            var x = s.MyLocation.X;
            var y = s.MyLocation.Y;
            var bd = (float)(Math.Abs(b.X - x) + Math.Abs(b.Y - y));
            return bd;
        }
        public static bool IsInFrontOfGate(GameState s, bool opGate = true)
        {
            var gColumn = 9;
            var g1 = new System.Drawing.Point(3, gColumn);
            var g2 = new System.Drawing.Point(4, gColumn);
            if (!opGate)
            {
                g1 = new System.Drawing.Point(3, 1);
                g2 = new System.Drawing.Point(4, 1);
            }
            return s.MyLocation == g1 || s.MyLocation == g2;
        }
    }
}