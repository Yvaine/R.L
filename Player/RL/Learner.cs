#undef __DEBUG__
using System.Collections.Generic;
using System;
using System.Collections;
using System.Diagnostics;
namespace Player.RL
{
    public class Learner
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
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 4000;
            t.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                lock (RandGen)
                {
                    RandGen = new Random(Environment.TickCount);
                }
            });
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
                    float nsqv = getQVal(gameState, (Direction)direction);
                    if (nsqv > nsmqp.Key)
                    {
                        nsmqp = new KeyValuePair<float, Direction>(nsqv, (Direction)direction);
                        // in next `if` statementment we will fall-back to
                        // adding currently updated `nsmqp` into `nsmqpl`.
                        // but for now we need to clear the list
                        nsmqpl.Clear();
                    }
                    if (nsqv == nsmqp.Key)
                    {
                        nsmqpl.Add(new KeyValuePair<float, Direction>(nsqv, (Direction)direction));
#if __DEBUG__
                        DebugUI.AddLine("\t{0} = {1}", ((Direction)direction).ToString(), nsqv);
#endif
                    }
                }
                var candIndex = -1;
                lock (RandGen)
                {
                    candIndex = RandGen.Next(0, nsmqpl.Count);
                }
#if __DEBUG__
                DebugUI.AddLine("CAND_INDEX: {0}", candIndex);
#endif
                // pick a random candidate
                var candidate = nsmqpl[candIndex];
                //candidate = new KeyValuePair<float, Direction>(getRVal(gameState, Direction.EAST), Direction.EAST);
#if __DEBUG__
                DebugUI.AddLine("{0} Candidate Detected.", nsmqpl.Count);
#endif
                var R = getRVal(gameState, candidate.Value);
                var nQv = R + GAMMA * candidate.Key;
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
        /// <summary>
        /// Updates the `Q` table
        /// </summary>
        /// <param name="s">The game state</param>
        /// <param name="a">The action</param>
        /// <param name="val">The value of state-action</param>
        protected static void updateQ(GameState s, Direction a, float val)
        {
            var key = new SAPair(s, a);
            if (_Q.ContainsKey(key.GetHashCode()))
                _Q[key.GetHashCode()] = val;
            else
                _Q.Add(key.GetHashCode(), val);
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
            if (gs.MyScore > GameStates.Peek().MyScore)
            {
#if __DEBUG__
                DebugUI.AddLine("+75.0 ");
#endif
                r += 75.0F;
            }
            // -100 if i score-down
            if (GameStates.Peek().OpponentScore > gs.OpponentScore)
            {
#if __DEBUG__
                DebugUI.AddLine("-100.0 ");
#endif
                r -= 100.0F;
            }
            // +10 if i acquired the bull
            if (!GameStates.Peek().IsBallMine && gs.IsBallMine)
            {
#if __DEBUG__
                DebugUI.AddLine("+10.0 ");
#endif
                r += 10.0F;
            }
            // -20 if i lost the ball
            if (GameStates.Peek().IsBallMine && !gs.IsBallMine)
            {
#if __DEBUG__
                DebugUI.AddLine("-20.0 ");
#endif
                r -= 20.0F;
            }
            return r;
        }
        /// <summary>
        /// Calculates the `Q` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `Q` value</returns>
        protected static float getQVal(GameState s, Direction a)
        {
            var key = new SAPair(s, a);
            if (_Q.ContainsKey(key.GetHashCode()))
                return _Q[key.GetHashCode()];
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
            float r = getReward(s);
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
            if (_s.IsBallMine)
                r += 100 / CalcDistanceToGate(_s);
            else r -= 10 * CalcDistanceToBall(_s);
            return r;
            switch (a)
            {
                // +35 if i move toward the opponent's gate
                case Direction.EAST: r += 35; break;
                // -10 if i move backward the my own gate
                case Direction.WEST: r -= 10; break;
            }
            return r;
        }
        protected static float CalcDistanceToGate(GameState s)
        {
            var gColumn = 10;
            var g1 = new System.Drawing.Point(3, gColumn);
            var g2 = new System.Drawing.Point(4, gColumn);
            var x = s.MyLocation.X;
            var y = s.MyLocation.Y;
            var g39 = (float)Math.Sqrt(Math.Pow(g1.X - x, 2) + Math.Pow(g1.Y - y, 2));
            var g49 = (float)Math.Sqrt(Math.Pow(g2.X - x, 2) + Math.Pow(g2.Y - y, 2));
            return Math.Min(g39, g49);
        }
        protected static float CalcDistanceToBall(GameState s)
        {
            if (s.IsBallMine) return 0;
            var b = s.OpponentLocation;
            var x = s.MyLocation.X;
            var y = s.MyLocation.Y;
            var bd = (float)Math.Sqrt(Math.Pow(b.X - x, 2) + Math.Pow(b.Y - y, 2));
            return bd;
        }
    }
}