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
        /// Static constructor
        /// </summary>
        static Learner()
        {
            DebugUI = new iDebug(Console.Title);
            DebugUI.Hide();
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
                    }
                }
                // pick a random candidate
                var candidate = nsmqpl[new Random(Environment.TickCount).Next(0, nsmqpl.Count)];
                DebugUI.AddText("Q{1} {0} -> ", getQVal(gameState, candidate.Value), "{");
                updateQ(
                    gameState,                                                      // The state
                    candidate.Value,                                                // The direction
                    getRVal(gameState, candidate.Value) + GAMMA * candidate.Key     // The update value
                );
                DebugUI.AddLine("{0} {1}, R {2} {3} {1}: MOVE {4}", getQVal(gameState, candidate.Value), "}", "{", getReward(gameState), candidate.Value.ToString());
                // push current game status
                GameStates.Push(gameState);
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
            if(gs.MyScore > GameStates.Peek().MyScore)
                r += 75.0F;
            // -100 if i score-down
            if (GameStates.Peek().OpponentScore > gs.OpponentScore)
                r -= 100.0F;
            // +10 if i acquired the bull
            if (!GameStates.Peek().IsBallMine && gs.IsBallMine)
                r += 10.0F;
            // -20 if i lost the ball
            if (GameStates.Peek().IsBallMine && !gs.IsBallMine)
                r -= 20.0F;
            // +30 if i move toward the opponent's gate
            if (gs.MyLocation.Y > GameStates.Peek().MyLocation.Y)
                r += 30;
            // -10 if i move backward the my own gate
            if (gs.MyLocation.Y < GameStates.Peek().MyLocation.Y)
                r -= 10;
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
            switch (a)
            {
                // +35 if i move toward the opponent's gate
                case Direction.EAST: r += 35; break;
                // -10 if i move backward the my own gate
                case Direction.WEST: r -= 10; break;
            }
            return r;
        }
    }
}