using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace ChessGame
{
    class BoardPiece
    {

        /*
         * + Speichert ein ChessPiece (die eigene Figur)
         * + eine Position x, y (Um züge zu Berechnen aka. Der Index im Array in BoardStatus)
         * + einen Besitzer
         */

        //Die eigene Figur
        public ChessPiece piece { get; set; }

        //True if the Piece was moved once (Used for Pawns and Castle (Rochade))
        public Boolean wasMoved { get; set; }

        //Die Position bzw. der index im BoardStatus [x,y]
        public int x { get; set; }
        public int y { get; set; }

        //Der Besitzer
        public byte owner { get; }

        //Figur, Schwarz/ Weiß, position
        public BoardPiece(ChessPiece piece, byte owner, int x, int y)
        {
            this.piece = piece;
            this.owner = owner;

            wasMoved = false;

            this.x = x;
            this.y = y;
        }

        public List<Point> getPossibleMoves(BoardStatus status)
        {
            return piece.getPossibleMoves(status,this);
        }
    }
}
