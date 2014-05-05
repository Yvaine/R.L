
using System.Collections.Generic;
using System;
namespace Player.RL
{
    public class Learner
    {
        /// <summary>
        /// The direction defined based on project's specification document
        /// </summary>
        public enum Direction { HOLD = 0, NORTH = 1, SOUTH = 2, WEST = 3, EAST = 4 };
        /// <summary>
        /// The game states' stack
        /// </summary>
        protected static Stack<GameState> GameStates { get; set; }
        /// <summary>
        /// The `R` matrix
        /// </summary>
        protected static int[][] R { get; set; }
        /// <summary>
        /// The `Q` matrix
        /// </summary>
        protected static int[][] Q { get; set; } 
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public static void Init()
        {
            // init game's states 
            GameStates = new Stack<GameState>();
            /**
             * Init the `Q` and `R` arraies
             */ 
            uint horzLength = 6 * 9;
            uint vertLength = (uint)Enum.GetValues(typeof(Direction)).Length;
            R = new int[horzLength][];
            Q = new int[horzLength][];
            for (int i = 0; i < R.GetLength(0); i++)
            {
                R[i] = new int[vertLength];
                Q[i] = new int[vertLength];
            }
            // in here both `Q` and `R` has been init with `0` value
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameState">The game status</param>
        /// <returns>The direction</returns>
        public static Direction ChooseAction(GameState gameState)
        {
            // push current game status
            GameStates.Push(gameState);
            return Direction.EAST;
        }
        /// <summary>
        /// Get reward of passed game status
        /// </summary>
        /// <param name="gs">The game state</param>
        /// <returns>The related reward for passed game state</returns>
        protected static int getReward(GameState gs)
        {
            // if no previously game state has been recorded
            if(GameStates.Count == 0) /* NO REWARD AT INTI STATE */ return 0;
            // get reward of current state based on combination of current and previous state's status
            return ((gs.MyScore - GameStates.Peek().MyScore) + (GameStates.Peek().OpponentScore - gs.OpponentScore)) > 0 ? 1 : 0;
        }
    }
}