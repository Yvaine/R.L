#undef __DEBUG__
//#define __USE_PREPROCESSED_Q__
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
            t.Tick += new EventHandler((object sender, EventArgs e) => { lock (RandGen)RandGen = new Random(Environment.TickCount); });
            t.Interval = 100;
            t.Start();
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
                List<KeyValuePair<KeyValuePair<float, float>, Direction>> nsmqpl = new List<KeyValuePair<KeyValuePair<float, float>, Direction>>();
                // next-state all possible Q pair list
                List<KeyValuePair<KeyValuePair<float, float>, Direction>> nsAPqpl = new List<KeyValuePair<KeyValuePair<float, float>, Direction>>();
                // next-state max Q pair
                KeyValuePair<KeyValuePair<float, float>, Direction> nsmqp = new KeyValuePair<KeyValuePair<float, float>, Direction>(new KeyValuePair<float, float>(float.NegativeInfinity, float.NegativeInfinity), Direction.HOLD);
                foreach (var direction in Enum.GetValues(typeof(Direction)))
                {
                    // fetch the Next-State Q Value.
                    var nsqv = new KeyValuePair<float, float>(getQVal(gameState, (Direction)direction), getReward(getNextState(gameState, (Direction)direction), gameState));
                    // add it to all possible next-state list
                    nsAPqpl.Add(new KeyValuePair<KeyValuePair<float, float>, Direction>(nsqv, (Direction)direction));
                    var nsqvV = nsqv.Key + nsqv.Value;
                    var nsmqpV = nsmqp.Key.Key + nsmqp.Key.Value;
                    // check for MAX value
                    if ( nsqvV > nsmqpV)
                    {
                        nsmqp = new KeyValuePair<KeyValuePair<float, float>, Direction>(nsqv, (Direction)direction);
                        // in next `if` statementment we will fall-back to
                        // adding currently updated `nsmqp` into `nsmqpl`.
                        // but for now we need to clear the list
                        nsmqpl.Clear();
                    }
                    // if current next-state is equal to MAX
                    if (nsqvV == nsmqp.Key.Key + nsmqp.Key.Value)
                        // add it to MAX list too
                        nsmqpl.Add(new KeyValuePair<KeyValuePair<float, float>, Direction>(nsqv, (Direction)direction));
                }
                // init the candidate index
                int candIndex = -1;
                // candidate container
                KeyValuePair<KeyValuePair<float, float>, Direction> candidate = new KeyValuePair<KeyValuePair<float, float>, Direction>();
                // while there is a item in list
                while (nsmqpl.Count != 0)
                {
                    // pick a randon index
                    lock (RandGen)
                        candIndex = RandGen.Next(0, nsmqpl.Count);
                    // pick a random candidate
                    candidate = nsmqpl[candIndex];
                    // validate the mutation factore
                    if (getMutationVal(gameState, candidate.Value) < 5) goto __PROCEED;
                    // remove the current
                    nsmqpl.RemoveAt(candIndex);
                }
                /**
                 * If we reach here it means me have failed to pick a direction
                 * and we have fell into a action-loop and we need to pick a new
                 * random action to break the action-loop.
                 */
                // get an index from all possible state-action list
                lock (RandGen) candIndex = RandGen.Next(0, nsAPqpl.Count);
                // make the random choosen as candidate
                candidate = nsAPqpl[candIndex];
            __PROCEED:
                // update the mution factore for current state
                updateMutaionFactore(gameState, candidate.Value);
                // the New Q Value
                var nQv = getReward(gameState) + GAMMA * (candidate.Key.Key + candidate.Key.Value);
                // update the Q value
                updateQ(
                    gameState,          // The state
                    candidate.Value,    // The direction
                    nQv                 // The update value
                );
                // we don't need to create a full-stack property
                if (GameStates.Count > 0)
                    // pop any previously pushed state
                    GameStates.Pop();
                // push current game status
                GameStates.Push(gameState);
                //return Direction.EAST;
                return candidate.Value;
            }
            catch (Exception e) { Console.WriteLine(); Console.WriteLine(e.ToString()); Debug.WriteLine(e.ToString()); throw e; }
        }
        /// <summary>
        /// Get next state of the game based on an action
        /// </summary>
        /// <param name="s">Current state of the game</param>
        /// <param name="a">The action to apply on current state</param>
        /// <returns>The next state</returns>
        protected static GameState getNextState(GameState s, Direction a)
        {
            // make a clone of current status
            GameState _s = s.Clone() as GameState;
            /**
             * Update the location of mine by choosing the action
             */
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
            // update the status of ball
            if (_s.MyLocation == _s.OpponentLocation) _s.IsBallMine = !_s.IsBallMine;
            /**
             * Update the scores status
             */
            if (_s.IsBallMine)
            {
                if (CalcDistanceToGate(s, true, true) == 0) { _s.MyScore++; _s.Game_State = GameState.State.PLAYER_SCORED; }
                else if (CalcDistanceToGate(s, false, true) == 0) { _s.OpponentScore++; _s.Game_State = GameState.State.OPPONENT_SCORED; }
            }
            else
            {
                if (CalcDistanceToGate(s, false, false) == 0) { _s.OpponentScore++; _s.Game_State = GameState.State.OPPONENT_SCORED; }
                else if (CalcDistanceToGate(s, true, false) == 0) { _s.MyScore++; _s.Game_State = GameState.State.PLAYER_SCORED; }
            }
            return _s;
        }
        /// <summary>
        /// Get reward of passed game status
        /// </summary>
        /// <param name="gs">The game state</param>
        /// <param name="prevgs">[OPTIONAL] The previous game state; If not provided the top of stack will be used!</param>
        /// <returns>The related reward for passed game state</returns>
        protected static float getReward(GameState gs, GameState prevgs = null)
        {
            // if no previously game state has been recorded
            if (GameStates.Count == 0) /* NO REWARD AT INTI STATE */ return 0;
            // if no previous game state has been provided? select the top of the stack
            if (prevgs == null) prevgs = GameStates.Peek();
            // get reward of current state based on combination of current and previous state's status
            float r = 0;
            // +100 if i score-up
            if (gs.Game_State == GameState.State.PLAYER_SCORED) r += 100.0F;
            // -100 if i score-down
            if (gs.Game_State == GameState.State.OPPONENT_SCORED) r -= 100.0F;
            // dynamic reward if i acquired the bull
            if (!prevgs.IsBallMine && gs.IsBallMine)
                // it is good to catch the ball nearby the friendly gate
                // to prevent the opponent to make a goal
                r += 50.0F / CalcDistanceToGate(gs, false);
            // dynamic reward if i lost the ball
            if (prevgs.IsBallMine && !gs.IsBallMine)
                // it is worst if we lost the ball nearby the opponent's gate
                // to make a goal opportunity
                r -= 50.0F / CalcDistanceToGate(gs, true);
            // if i am in position of the ball
            if (gs.IsBallMine)
                // it is good to catch the ball nearby the friendly gate
                // to prevent the opponent to make a goal
                r += (50.0F / CalcDistanceToGate(gs, true));
            else
                // make the agent eager to go and catch the ball
                r -= (10.0F * (CalcDistanceToBall(gs)));
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
#if __USE_PREPROCESSED_Q__
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
            if (IsInFrontOfGate(s, false) && s.IsBallMine)
            {
                if(a == Direction.WEST)
                    updateQ(s, a, float.NegativeInfinity);
            }
            if (IsInFrontOfGate(s, true) && s.IsBallMine)
            {
                switch (a)
                {
                    case Direction.EAST: updateQ(s, a, float.PositiveInfinity); break;
                    case Direction.HOLD: updateQ(s, a, +100); break;
                    default: updateQ(s, a, float.NegativeInfinity); break;
                }
            }
#endif
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
        /// <summary>
        /// Check if the state is in front of a gate
        /// </summary>
        /// <param name="s">The game's state</param>
        /// <param name="opGate">`true` to check if the state is in front of opponent's gate; otherwise `false` to check if the state is in front of mine gate</param>
        /// <returns>True if state is in front of a gate</returns>
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