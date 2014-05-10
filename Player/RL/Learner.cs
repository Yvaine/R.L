#undef __DEBUG__
//#define __USE_PREPROCESSED_Q__
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace Player.RL
{
    /// <summary>
    /// Learns how to play soccer using Reinforcement Learning's technique
    /// </summary>
    class Learner
    {
        /// <summary>
        /// The gamma value for Q-Learning
        /// </summary>
        protected const float GAMMA = 0.5F;
        /// <summary>
        /// The game states' stack
        /// </summary>
        protected Stack<GameState> GameStates { get; set; }
        /// <summary>
        /// The `Q` matrix
        /// </summary>
        protected Dictionary<String, float> _Q { get; set; }
        /// <summary>
        /// Holds mutation factors
        /// </summary>
        protected KeyValuePair<SAPair, uint> PrevMutationfactor { get; set; }
#if __DEBUG__
        /// <summary>
        /// Debug UI handler
        /// </summary>
        protected iDebug DebugUI { get; set; }
#endif
        /// <summary>
        /// Random number generator
        /// </summary>
        protected Random RandGen { get; set; }
        /// <summary>
        /// constructor
        /// </summary>
        public Learner()
        {
#if __DEBUG__
            this.DebugUI = new iDebug(Console.Title);
            this.DebugUI.Hide();
#endif
            // init random # generator
            this.RandGen = new Random(Environment.TickCount);
            // init previous mutation factor container
            this.PrevMutationfactor = new KeyValuePair<SAPair, uint>();
            /**
             * Make timer to update the random # generator's seed value
             */
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Tick += new EventHandler((object sender, EventArgs e) => { lock (this.RandGen)this.RandGen = new Random(Environment.TickCount); });
            // with 100ms interval
            t.Interval = 100;
            t.Start();
        }
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public void Init()
        {
            // init game's states
            this.GameStates = new Stack<GameState>();
            // init hash table
            this._Q = new Dictionary<String, float>();
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameState">The game status</param>
        /// <returns>The direction</returns>
        public Direction ChooseAction(GameState gameState)
        {
            try
            {
                // Next-State Max Q Pair List
                List<KeyValuePair<KeyValuePair<float, float>, Direction>> nsmqpl = new List<KeyValuePair<KeyValuePair<float, float>, Direction>>();
                // Next-State All Possible Q Pair List
                List<KeyValuePair<KeyValuePair<float, float>, Direction>> nsAPqpl = new List<KeyValuePair<KeyValuePair<float, float>, Direction>>();
                // Next-State Max Q Pair
                KeyValuePair<KeyValuePair<float, float>, Direction> nsmqp = new KeyValuePair<KeyValuePair<float, float>, Direction>(new KeyValuePair<float, float>(float.NegativeInfinity, float.NegativeInfinity), Direction.HOLD);
                // foreach valid directions
                foreach (Direction direction in this.getValidDirections(gameState))
                {
                    // fetch the Next-State Q Value.
                    var nsqv = new KeyValuePair<float, float>(this.getQVal(gameState, direction), this.getReward(this.getNextState(gameState, direction), gameState));
                    // current next-state pair
                    var cnsp = new KeyValuePair<KeyValuePair<float, float>, Direction>(nsqv, direction);
                    // add it to all possible next-state list
                    nsAPqpl.Add(cnsp);
                    // fetch Next-State Q Value's Value
                    var nsqvV = nsqv.Key + nsqv.Value;
                    // fetch Next-State Max Q Pair's Value
                    var nsmqpV = nsmqp.Key.Key + nsmqp.Key.Value;
                    // check for MAX value
                    if ( nsqvV > nsmqpV)
                    {
                        nsmqp = cnsp;
                        // in next `if` statement we will fall-back to
                        // adding currently updated `nsmqp` into `nsmqpl`.
                        // but for now we need to clear the list
                        nsmqpl.Clear();
                    }
                    // if current next-state is equal to MAX
                    if (nsqvV == nsmqp.Key.Key + nsmqp.Key.Value)
                        // add it to MAX list too
                        nsmqpl.Add(cnsp);
                }
                // init the candidate index
                int candIndex = -1;
                // candidate container
                KeyValuePair<KeyValuePair<float, float>, Direction> candidate = new KeyValuePair<KeyValuePair<float, float>, Direction>();
                // while there is a item in list
                while (nsmqpl.Count != 0)
                {
                    // pick a randon index
                    lock (this.RandGen) candIndex = this.RandGen.Next(0, nsmqpl.Count);
                    // pick a random candidate
                    candidate = nsmqpl[candIndex];
                    // validate the mutation factor
                    // why the THRESHOLD is {10}? because we should give the agent the chance
                    // if he wants to cross-pass the whole horizon with one direction without 
                    // getting intruptted by mutation factor.
                    if (this.getMutationVal(gameState, candidate.Value) < 10) goto __PROCEED;
                    // remove the current
                    nsmqpl.RemoveAt(candIndex);
                }
                /**
                 * If we reach here it means me have failed to pick a direction
                 * and we have fell into a action-loop and we need to pick a new
                 * random action to break the action-loop.
                 */
                // get an index from all possible state-action list
                lock (this.RandGen) candIndex = this.RandGen.Next(0, nsAPqpl.Count);
                // make the random choosen as candidate
                candidate = nsAPqpl[candIndex];
            __PROCEED:
                // update the mution factor for current state
                this.updateMutaionFactor(gameState, candidate.Value);
                // the New Q Value
                var nQv = this.getReward(gameState) + Learner.GAMMA * (candidate.Key.Key + candidate.Key.Value);
                // update the Q value
                this.updateQ(
                    gameState,          // The state
                    candidate.Value,    // The direction
                    nQv                 // The update value
                );
                // we don't need to create a full-stack property
                if (this.GameStates.Count > 0)
                    // pop any previously pushed state
                    this.GameStates.Pop();
                // push current game status
                this.GameStates.Push(gameState);
                //return the candidate's direction;
                return candidate.Value;
            }
            catch (Exception e) { Console.WriteLine(); Console.WriteLine(e.ToString()); Debug.WriteLine(e.ToString()); throw e; }
        }
        /// <summary>
        /// Get valid directions of a game state
        /// </summary>
        /// <param name="gameState">The game state</param>
        /// <returns>The valid game directions</returns>
        protected IEnumerable<Direction> getValidDirections(GameState gameState)
        {
            List<Direction> directions = new List<Direction>() { /* constant direction for any state */ Direction.HOLD };
            // if not on horizontal edges?
            if (gameState.MyLocation.X > 1 && gameState.MyLocation.X < 6) directions.AddRange(new[] { Direction.NORTH, Direction.SOUTH });
            // if on top edge?
            if (gameState.MyLocation.X == 1) directions.Add(Direction.SOUTH);
            // if on bottom edge?
            if (gameState.MyLocation.X == 6) directions.Add(Direction.NORTH);
            // if not on vertical edges?
            if (gameState.MyLocation.Y > 1 && gameState.MyLocation.Y < 9) directions.AddRange(new[] { Direction.EAST, Direction.WEST });
            // if on left edge?
            if (gameState.MyLocation.Y == 1) directions.Add(Direction.EAST);
            // if on right edge?
            if (gameState.MyLocation.Y == 9) directions.Add(Direction.WEST);
            // if we are in front if any gate? /* YES: Add a direction to the gate */
            // my own gate?
            if (gameState.MyLocation.Y == 1 && (gameState.MyLocation.X == 3 || gameState.MyLocation.X == 4)) directions.Add(Direction.WEST);
            // opponent's gate?
            if (gameState.MyLocation.Y == 9 && (gameState.MyLocation.X == 3 || gameState.MyLocation.X == 4)) directions.Add(Direction.EAST);
            // return directions
            return directions;
        }
        /// <summary>
        /// Get reward of passed game status
        /// </summary>
        /// <param name="gs">The game state</param>
        /// <param name="prevgs">[OPTIONAL] The previous game state; If not provided the top of stack will be used!</param>
        /// <returns>The related reward for passed game state</returns>
        protected float getReward(GameState gs, GameState prevgs = null)
        {
            // if no previously game state has been recorded
            if (this.GameStates.Count == 0) /* NO REWARD AT INIT STATE */ return 0;
            // flag that we have peeked prev-gs from stack
            bool STACK_PEEKED = (prevgs == null);
            // if no previous game state has been provided? select the top of the stack
            if (prevgs == null) prevgs = this.GameStates.Peek();
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
                r += 50.0F / this.CalcDistanceToGate(gs, false);
            // dynamic reward if i lost the ball
            if (prevgs.IsBallMine && !gs.IsBallMine)
                // it is worst if we lost the ball nearby the opponent's gate
                // to make a goal opportunity
                r -= 50.0F / this.CalcDistanceToGate(gs, true);
            // if i am in position of the ball
            if (gs.IsBallMine)
                // make the agent eager to go make a goal
                r += (70.0F / this.CalcDistanceToGate(gs, true));
            else
                // make the agent eager to go and catch the ball
                r -= (30.0F * (this.CalcDistanceToBall(gs)));
            // if i made an own-goal?
            if (STACK_PEEKED && prevgs.IsBallMine && gs.Game_State == GameState.State.OPPONENT_SCORED)
                // if i made an own-goal, the direction would be 100% {WEST}.
                // if i made an own-goal, update the previous game-state's Q's value to {-INF}
                this.updateQ(prevgs, Direction.WEST, float.NegativeInfinity);
            // return the reward value
            return r;
        }
        /// <summary>
        /// Get next state of the game based on an action
        /// </summary>
        /// <param name="s">Current state of the game</param>
        /// <param name="a">The action to apply on current state</param>
        /// <returns>The next state</returns>
        protected GameState getNextState(GameState s, Direction a)
        {
            // make a clone of current status
            GameState _s = s.Clone() as GameState;
            /**
             * Update the location of mine by choosing the action
             */
            switch (a)
            {
                case Direction.HOLD: /* return currently cloned state */ return _s;
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
                if (this.CalcDistanceToGate(s, true, true) == 0) { _s.MyScore++; _s.Game_State = GameState.State.PLAYER_SCORED; }
                else if (this.CalcDistanceToGate(s, false, true) == 0) { _s.OpponentScore++; _s.Game_State = GameState.State.OPPONENT_SCORED; }
            }
            else
            {
                if (this.CalcDistanceToGate(s, false, false) == 0) { _s.OpponentScore++; _s.Game_State = GameState.State.OPPONENT_SCORED; }
                else if (this.CalcDistanceToGate(s, true, false) == 0) { _s.MyScore++; _s.Game_State = GameState.State.PLAYER_SCORED; }
            }
            return _s;
        }
        /// <summary>
        /// Updates the `Q` table
        /// </summary>
        /// <param name="s">The game state</param>
        /// <param name="a">The action</param>
        /// <param name="val">The value of state-action</param>
        protected void updateQ(GameState s, Direction a, float val)
        {
            var key = new SAPair(s, a).GetHashCode();
            if (this._Q.ContainsKey(key))
                this._Q[key] = val;
            else
                this._Q.Add(key, val);
        }
        /// <summary>
        /// Calculates the `Q` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `Q` value</returns>
        protected float getQVal(GameState s, Direction a)
        {
            var key = new SAPair(s, a).GetHashCode();
#if __USE_PREPROCESSED_Q__
            if (!this._Q.ContainsKey(key))
            {
                float q = 0;
                var ns = this.getNextState(s, a);
                if (a == Direction.EAST)
                {
                    if (ns.IsBallMine) q += 5.0F;
                    else q -= 5.0F;
                }
                if (ns.Game_State == GameState.State.OPPONENT_SCORED) q = float.NegativeInfinity;
                if (ns.Game_State == GameState.State.PLAYER_SCORED) q = float.PositiveInfinity;
                this.updateQ(s, a, q);
            }
            // if making an own-goal
            if (this.IsInFrontOfGate(s, false) && s.IsBallMine)
            {
                if(a == Direction.WEST)
                    this.updateQ(s, a, float.NegativeInfinity);
            }
            if (this.IsInFrontOfGate(s, true) && s.IsBallMine)
            {
                switch (a)
                {
                    case Direction.EAST: this.updateQ(s, a, float.PositiveInfinity); break;
                    case Direction.HOLD: this.updateQ(s, a, +100); break;
                    default: this.updateQ(s, a, float.NegativeInfinity); break;
                }
            }
#endif
            if (this._Q.ContainsKey(key))
                return _Q[key];
            return 0;
        }
        /// <summary>
        /// Updates the `Mutationfactor` table
        /// </summary>
        /// <param name="s">The game state</param>
        /// <param name="a">The action</param>
        /// <param name="val">The mutation factor's value</param>
        protected void updateMutaionFactor(GameState s, Direction a)
        {
            var key = new SAPair(s, a);
            uint val = 0;
            if (this.PrevMutationfactor.Key != null && this.PrevMutationfactor.Key.GetHashCode() == key.GetHashCode())
                val = this.PrevMutationfactor.Value + 1;
            this.PrevMutationfactor = new KeyValuePair<SAPair, uint>(key, val);
        }
        /// <summary>
        /// Get `Mutationfactor` for the state-action
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The mutation factor's value</returns>
        protected uint getMutationVal(GameState s, Direction a)
        {
            if (this.PrevMutationfactor.Key == null) return 0;
            var key = new SAPair(s, a);
            if (this.PrevMutationfactor.Key.GetHashCode() == key.GetHashCode())
                return this.PrevMutationfactor.Value;
            return 0;
        }
        /// <summary>
        /// Calculates the distance to goal
        /// </summary>
        /// <param name="s">Game's status</param>
        /// <param name="opGate">Check distance to opponent's gate or mine?</param>
        /// <param name="myDist">If `true` calculates my distance to a gate; otherwise calculates the opponent's to a gate</param>
        /// <returns>The distance to gate</returns>
        protected float CalcDistanceToGate(GameState s, bool opGate = true, bool myDist = true)
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
        protected float CalcDistanceToBall(GameState s)
        {
            if (s.IsBallMine) return 0;
            var b = s.OpponentLocation;
            var x = s.MyLocation.X;
            var y = s.MyLocation.Y;
            var bd = (float)(Math.Abs(b.X - x) + Math.Abs(b.Y - y));
            return bd;
        }
#if __USE_PREPROCESSED_Q__
        /// <summary>
        /// Check if the state is in front of a gate
        /// </summary>
        /// <param name="s">The game's state</param>
        /// <param name="opGate">`true` to check if the state is in front of opponent's gate; otherwise `false` to check if the state is in front of mine gate</param>
        /// <returns>True if state is in front of a gate</returns>
        public bool IsInFrontOfGate(GameState s, bool opGate = true)
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
#endif
    }
}