using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    class AIRandom : AIPlayer 
    {
        //A simple random test ai
        public AIRandom(byte color) : base (color)
        {
        }

        public override Point playNextTurn(BoardStatus status, out BoardPiece moved)
        {
            //Chooses A Random Move from all possible Movers
            Random r = new Random();
            List<Point> moves = status.getAllPossibleMovesForColor(color, out List<BoardPiece> movers);
            int toPlay = r.Next(0, Math.Abs(moves.Count - 1));
            moved = movers[toPlay];
            return moves[toPlay];
        }
    }
}
