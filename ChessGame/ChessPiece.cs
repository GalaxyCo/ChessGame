using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ChessGame
{
    class ChessPiece
    {
        /*
         * Ein ChessPiece ist ein eine Schach Figur
         * Jede Figur existiert Statisch einmal
         * Jede Figur hat einen Namen für Debug zwecke
         * Jede Figur hat MovementAtributes (die Bestimmen wie sie sich bewegen kann)
        */

        public static ChessPiece pawn;
        public static ChessPiece king;
        public static ChessPiece queen;
        public static ChessPiece bishop;
        public static ChessPiece knight;
        public static ChessPiece rook;

        //Name Der Figur
        public String name { get; }

        //The Value for the MinMax AI
        public int value { get; }

        private List<MoveAttribute> moveAttributes;

        public ChessPiece(String name, int value, List<MoveAttribute> moveAttributes)
        {
            this.value = value;
            this.name = name;
            this.moveAttributes = moveAttributes;
        }

        public List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            foreach(MoveAttribute moveAttribute in moveAttributes)
            {
                temp.AddRange(moveAttribute.getPossibleMoves(status, toSearchFor));
            }
            return temp;
        }

        //Used to init the static pieces, Called only once
        public static void initPieces()
        {
            pawn = new ChessPiece("Pawn", 1, new List<MoveAttribute> { new MovePawnAttribute()});
            king = new ChessPiece("King", 99999, new List<MoveAttribute> { new MoveStraightAttribute(1), new MoveDiagonalAttribute(1), new MoveCastle()});
            queen = new ChessPiece("Queen", 9, new List<MoveAttribute> { new MoveStraightAttribute(GameManager.boardSize), new MoveDiagonalAttribute(GameManager.boardSize) });
            bishop = new ChessPiece("Bishop", 3, new List<MoveAttribute> { new MoveDiagonalAttribute(GameManager.boardSize) });
            knight = new ChessPiece("Knight", 3, new List<MoveAttribute> { new MoveJumpAttribute() });
            rook = new ChessPiece("Rook", 5, new List<MoveAttribute> { new MoveStraightAttribute(GameManager.boardSize) });
        }
    }

    /*
     * The Diffrent MoveAttributes describe the Pieces movement capabilities
     */

    abstract class MoveAttribute 
    {
        public abstract List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor);

        //is a position on the board
        protected Boolean isPosOnBoard(int x, int y)
        {
            return x >= 0 && y >= 0 && x < GameManager.boardSize && y < GameManager.boardSize;
        }

        //is a field empty
        protected Boolean isFieldEmpty(int x, int y, BoardStatus boardStatus)
        {
            return isPosOnBoard(x, y) ? boardStatus.gameConfiguration[x, y] == null : true;
        }

        //is a field empty
        protected Boolean isFieldEmpty(BoardPiece bp, BoardStatus status)
        {
            return bp == null ? true : isFieldEmpty(bp.x, bp.y, status);
        }

        //returns a move {x,y} and collided: collided = 0 => no collision, collided = 1 => returns proper Move, there is an enemy Piece, collieded = 2 => There is a friedly Piece, no move returned
        protected byte getPosType(int x, int y, byte player, BoardStatus status)
        {
            BoardPiece bp = status.gameConfiguration[x, y];
            if(isFieldEmpty(bp, status))
            {
                //Collision with no piece
                return 0;
            }

            if(bp.owner == player)
            {
                //Collison with own piece
                return 2;
            }

            //Collision with enemy piece
            return 1;
        }

        //go in a direction defined by incX and incY
        protected List<Point> loopCordsInDirection(int startX, int startY, int incX, int incY, int times, BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            for (int i = 0; i < times; i++)
            {
                int x = startX + (incX * i);
                int y = startY + (incY * i);

                //skip if toSearchFor == Pos 
                if (x == toSearchFor.x && y == toSearchFor.y)
                    continue;

                //skip if not on the board
                if (!isPosOnBoard(x, y))
                    continue;

                //get the colide value
                byte moveType = getPosType(x, y, toSearchFor.owner, status);

                //add the move because the piece is not owned by the own player
                if (moveType != 2)
                {
                    //add the move
                    temp.Add(new Point(x,y));
                }

                //if collison happend, stop the search
                if (moveType > 0)
                {
                    break;
                }
            }
            return temp;
        }
    }

    class MoveCastle : MoveAttribute
    {
        public override List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            if (!toSearchFor.wasMoved)
            {
                int y = toSearchFor.y;
                Boolean castleShort = canCastleAt(0, y, toSearchFor, status);
                Boolean castleLong = canCastleAt(7, y, toSearchFor, status);
                if(castleShort)
                    temp.Add(new Point(2, y));

                if (castleLong)
                    temp.Add(new Point(6, y));
            }
            return temp;
        }

        Boolean canCastleAt(int x, int y, BoardPiece theKing, BoardStatus status)
        {
            if (!isFieldEmpty(x, y, status))
            {
                BoardPiece rook = status.gameConfiguration[x, y];
                if (!rook.wasMoved)
                {
                    bool foundBad = false;
                    for(int i = Math.Min(x, theKing.x) + 1; i < Math.Max(x, theKing.x); i++)
                    {
                        if(!isFieldEmpty(i, y, status))
                        {
                            foundBad = true;
                            break;
                        }
                    }
                    if(!foundBad)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    class MoveStraightAttribute : MoveAttribute
    {
        //How far the piece can Move
        int range;

        public MoveStraightAttribute(int range)
        {
            this.range = range + 1;
        }
       
        public override List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();

            int x = toSearchFor.x;
            int y = toSearchFor.y;

            //loop trough all straight lines
            temp.AddRange(loopCordsInDirection(x, y, 1, 0, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -1, 0, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 0, 1, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 0, -1, range, status, toSearchFor));

            return temp;
        }
    }

    class MoveDiagonalAttribute : MoveAttribute
    {
        int range;

        public MoveDiagonalAttribute(int range)
        {
            this.range = range + 1;
        }

        public override List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            int x = toSearchFor.x;
            int y = toSearchFor.y;

            //loop trough all diagonals
            temp.AddRange(loopCordsInDirection(x, y, 1, 1, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -1, 1, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 1, -1, range, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -1, -1, range, status, toSearchFor));

            return temp;
        }
    }

    class MoveJumpAttribute : MoveAttribute
    {
        public override List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            int x = toSearchFor.x;
            int y = toSearchFor.y;

            //loop trough all possible jumps
            temp.AddRange(loopCordsInDirection(x, y, 1, 2, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -1, 2, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 2, 1, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 2, -1, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, 1, -2, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -1, -2, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -2, 1, 2, status, toSearchFor));
            temp.AddRange(loopCordsInDirection(x, y, -2, -1, 2, status, toSearchFor));
            return temp;
        }
    }

    class MovePawnAttribute : MoveAttribute
    {
        public override List<Point> getPossibleMoves(BoardStatus status, BoardPiece toSearchFor)
        {
            List<Point> temp = new List<Point>();
            int x = toSearchFor.x;
            int y = toSearchFor.y;

            //Defines the Up Direction
            int yDirection = toSearchFor.owner == GameManager.black ? -1 : 1;

            y += yDirection;

            //Move up if a field is empty
            if (isFieldEmpty(x, y, status))
            {
                if(isPosOnBoard(x,y))
                    temp.Add(new Point( x, y ));
            }

            //Can move two up if it wasnt moved before
            if(isFieldEmpty(x, y + yDirection, status) && !toSearchFor.wasMoved && isFieldEmpty(x, y, status))
            {
                if (isPosOnBoard(x, y + yDirection))
                    temp.Add(new Point ( x, y + yDirection ));
            }
            
            //Move Left / Right if a enemy piece is on a pos
            if(checkForEnemy(x + 1, y, toSearchFor.owner, status))
            {
                if (isPosOnBoard(x + 1, y))
                    temp.Add(new Point ( x + 1, y ));
            }
            if (checkForEnemy(x - 1, y, toSearchFor.owner, status))
            {
                if (isPosOnBoard(x - 1, y))
                temp.Add(new Point ( x - 1, y ));
            }

            return temp;
        }

        //checks if an enemy piece owns set x, y pos
        private Boolean checkForEnemy(int x, int y, byte player, BoardStatus status)
        {
            if (isPosOnBoard(x, y))
            {
                byte type = getPosType(x, y, player, status);
                if (type == 1)
                    return true;
            }
            return false;
        }
    }
}
