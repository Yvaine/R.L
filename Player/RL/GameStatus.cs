using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Player.RL
{
    class GameStatus
    {
        public GameStatus(byte[] status)
        {
            if (status.Length != 26)
                throw new ArgumentException(
                    String.Format("Expecting status bytes to have length of `26`, but got {0}.", status.Length));
        }
    }
}