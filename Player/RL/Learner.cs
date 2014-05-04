
namespace Player.RL
{
    class Learner
    {
        /// <summary>
        /// The direction defined based on project's specification document
        /// </summary>
        public enum Direction { HOLD = 0, NORTH = 1, SOUTH = 2, WEST = 3, EAST = 4 };
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public static void Init()
        {
            // TODO: Init your learner here
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameStatus">The game status</param>
        /// <returns>The direction</returns>
        public static Direction ChooseAction(GameStatus gameStatus)
        {
            return Direction.EAST;
        }
    }
}