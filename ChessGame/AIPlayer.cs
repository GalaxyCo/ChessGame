using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ChessGame
{
    abstract class AIPlayer
    {
        public byte color { get; }

        public AIPlayer(byte color)
        {
            //every AI has an color
            this.color = color;
        }

        //the abstract method used by every AI Player
        //TODO: Make Task
        public abstract Point playNextTurn(BoardStatus status, out BoardPiece moved);
    }
}
