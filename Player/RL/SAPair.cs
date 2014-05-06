using System;

namespace Player.RL
{
    /// <summary>
    /// A State-Action pair container
    /// </summary>
    class SAPair
    {
        /// <summary>
        /// Get the linked state entity
        /// </summary>
        public GameState State { get; set; }
        /// <summary>
        /// Get the linked action entity
        /// </summary>
        public Direction Action { get; set; }
        /// <summary>
        /// Construct a State-Action pair instance
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="action">The action</param>
        public SAPair(GameState state, Direction action) { this.State = state; this.Action = action; }
        /// <summary>
        /// Get unique hash-code for state-action pair
        /// </summary>
        /// <returns>The hash-code</returns>
        public new string GetHashCode()
        {
            return (String.Format("{0}{1}", this.State.GetHashCode(), (int)this.Action));
        }
    }
}
