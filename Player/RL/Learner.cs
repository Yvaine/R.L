using System.Collections.Generic;
using System;
using System.Collections;
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
        /// The `R` matrix
        /// </summary>
        protected static Hashtable _R { get; set; }
        /// <summary>
        /// The `Q` matrix
        /// </summary>
        protected static Hashtable _Q { get; set; } 
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public static void Init()
        {
            // init game's states 
            GameStates = new Stack<GameState>();
            // init hash tables
            _R = new Hashtable();
            _Q = new Hashtable();
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameState">The game status</param>
        /// <returns>The direction</returns>
        public static Direction ChooseAction(GameState gameState)
        {
            foreach (var direction in Enum.GetValues(typeof(Direction)))
            {
                throw new NotImplementedException("THIS HAS NOT IMPLEMENTED, JUST A PROTO!!");
                updateQ(
                    gameState,                                          // The state
                    (Direction)direction,                               // The direction
                    R(gameState, (Direction)direction) + GAMMA * -1     // The update value
                );   
            }
            // push current game status
            GameStates.Push(gameState);
            return Direction.EAST;
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
            if (_Q.ContainsKey(key))
                _Q[key] = val;
            else
                _Q.Add(key, val);
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
            return (float)((gs.MyScore - GameStates.Peek().MyScore) + (GameStates.Peek().OpponentScore - gs.OpponentScore)) + 0.5F * (gs.IsBallMine ? 1 : 0);
        }
        /// <summary>
        /// Calculates the `Q` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `Q` value</returns>
        protected static float Q(GameState s, Direction a)
        {
            return 0;
        }
        /// <summary>
        /// Calculates the `R` value
        /// </summary>
        /// <param name="s">For the game state</param>
        /// <param name="a">For the action</param>
        /// <returns>The `R` value</returns>
        protected static float R(GameState s, Direction a)
        {
            return getReward(s);
        }
    }
}