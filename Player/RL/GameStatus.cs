using System;
using System.Text;
using System.Drawing;

namespace Player.RL
{
    public class GameState : ICloneable
    {
        /// <summary>
        /// The origin status bytes
        /// </summary>
        protected readonly byte[] __status;
        /// <summary>
        /// The origin status string
        /// </summary>
        protected readonly string __string;
        /// <summary>
        /// Get my location point { X: Row # | Y: Column # }
        /// </summary>
        public Point MyLocation { get; set; }
        /// <summary>
        /// Get the opponent's location point { X: Row # | Y: Column # }
        /// </summary>
        public Point OpponentLocation { get; set; }
        /// <summary>
        /// Check if ball is mine or not
        /// </summary>
        public bool IsBallMine { get; set; }
        /// <summary>
        /// Get the state of game
        /// </summary>
        public State Game_State { get; set; }
        /// <summary>
        /// Get game's step
        /// </summary>
        public ulong GameStep { get; set; }
        /// <summary>
        /// Get my scrore
        /// </summary>
        public uint MyScore { get; set; }
        /// <summary>
        /// Get the opponent's score
        /// </summary>
        public uint OpponentScore { get; set; }
        /// <summary>
        /// The state of game
        /// </summary>
        public enum State { RUNNING, PLAYER_SCORED, OPPONENT_SCORED, GAME_FINISHED }
        /// <summary>
        /// Construct a new game status based on status byte
        /// </summary>
        /// <param name="status">The status bytes</param>
        public GameState(byte[] status)
        {
            this.__status = status;
            // fetch the status string
            this.__string = Encoding.ASCII.GetString(status).Trim();
            if (status.Length != 26 || __string.Length != 26)
                throw new ArgumentException(
                    String.Format("Expecting status bytes to have length of `26`, but got {0}.", status.Length));
            // fetching current player's loc.
            this.MyLocation = new Point(
                Convert.ToInt32(__string[0].ToString()),
                Convert.ToInt32(__string[1].ToString()));
            // fetching opponent's loc.
            this.OpponentLocation = new Point(
                Convert.ToInt32(__string[2].ToString()),
                Convert.ToInt32(__string[3].ToString()));
            // fetch ball-owning statu
            switch (__string[4])
            {
                case 'P': this.IsBallMine = true; break;
                case 'O': this.IsBallMine = false; break;
                default:
                    throw new ArgumentException(String.Format("Undefined status `{0}`.", __string[4]));
            }
            // fetch game's statu
            switch (__string[5])
            {
                case 'C': this.Game_State = State.RUNNING; break;
                case 'L': this.Game_State = State.PLAYER_SCORED; break;
                case 'R': this.Game_State = State.OPPONENT_SCORED; break;
                case 'F': this.Game_State = State.GAME_FINISHED; break;
                default:
                    throw new ArgumentException(String.Format("Undefined status `{0}`.", __string[4]));
            }
            // fetch game's step count
            this.GameStep = ulong.Parse(__string.Substring(6, 10));
            // fetch current player's score
            this.MyScore = uint.Parse(__string.Substring(16, 5));
            // fetch opponent's score
            this.OpponentScore = uint.Parse(__string.Substring(21));
        }
        /// <summary>
        /// Clone current game's status
        /// </summary>
        public object Clone()
        {
            return new GameState(this.__status);
        }
    }
}