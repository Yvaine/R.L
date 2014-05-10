#undef __DEBUG__
//#define __USE_PREPROCESSED_Q__
namespace Player.RL
{
    /// <summary>
    /// Learns how to play soccer using Reinforcement Learning's technique
    /// </summary>
    class Learner
    {
        /// <summary>
        /// Initialize the learner
        /// </summary>
        public void Init()
        {
            // TODO: do your init here
        }
        /// <summary>
        /// Executes ops and returns a movement direction based on game's status
        /// </summary>
        /// <param name="gameState">The game status</param>
        /// <returns>The direction</returns>
        public Direction ChooseAction(GameState gameState)
        {
            return Direction.EAST;
        }
    }
}