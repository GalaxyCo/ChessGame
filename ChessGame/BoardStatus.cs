using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ChessGame
{
    class BoardStatus
    {
        /*
         * Die Board Status Objekte Spielfeld:
         * + die Konfiguration der Steine
         * + wer am zug ist
         * + der Zug der Gespielt wurde 
         */

        //The game Configuration
        public BoardPiece[,] gameConfiguration { get; }

        //Speichert den Spieler der Am zug ist aus GameManager
        public byte mover { get; }

        //Die neue Konfiguration, wer am zug ist (GameManager)
        public BoardStatus(BoardPiece[,] pieces, byte mover)
        {
            gameConfiguration = pieces;
            this.mover = mover;
        }

        //dirty fix
        public static BoardStatus getStatusOffOldBoard(BoardStatus oldStatus, BoardPiece toMove, Point moveTo, Boolean realMove)
        {
            /*
             * tempStatus is for the Ais not ruining the Board Piece data
             */
            byte mover = oldStatus.mover;

            int oldX = toMove.x;
            int oldY = toMove.y;

            //changes the mover
            if (realMove)
                mover = toMove.owner == GameManager.white ? GameManager.black : GameManager.white;

            //copy the old configuration
            BoardPiece[,] temp = new BoardPiece[GameManager.boardSize, GameManager.boardSize];
            for (int x = 0; x < GameManager.boardSize; x++)
            {
                for (int y = 0; y < GameManager.boardSize; y++)
                {
                    temp[x, y] = oldStatus.gameConfiguration[x, y];
                }
            }

            //set the old pos empty
            temp[oldX, oldY] = null;

            if (realMove)
            {
                //put the Piece to the new Pos
                toMove.x = moveTo.X;
                toMove.y = moveTo.Y;
               
                //sets the was Moved Parameter
                toMove.wasMoved = true;

                temp[moveTo.X, moveTo.Y] = toMove;
            }
            else
            {
                BoardPiece bp = new BoardPiece(toMove.piece, toMove.owner, moveTo.X, moveTo.Y);
                bp.wasMoved = true;
                temp[moveTo.X, moveTo.Y] = bp;
            }

            //check if a castle was played and move the rook in the corner
            if (toMove.piece == ChessPiece.king)
            {
                if (Math.Abs(moveTo.X - oldX) == 2)
                {
                    if (moveTo.X == 2)
                    {
                        if(realMove)
                        {
                            BoardPiece bp = temp[0, oldY];
                            bp.x = 3;
                            temp[0, oldY] = null;
                            temp[3, oldY] = bp;
                            bp.wasMoved = true;
                        } else
                        {
                            temp[0, oldY] = null;
                            BoardPiece newRook = new BoardPiece(ChessPiece.rook, mover, 3, oldY);
                            temp[3, oldY] = newRook;
                            newRook.wasMoved = true;
                        }
                    }
                    if (moveTo.X == 6)
                    {
                        if (realMove)
                        {
                            BoardPiece bp = temp[7, oldY];
                            bp.x = 5;
                            temp[7, oldY] = null;
                            temp[5, oldY] = bp;
                            bp.wasMoved = true;
                        }
                        else
                        {
                            temp[7, oldY] = null;
                            BoardPiece newRook = new BoardPiece(ChessPiece.rook, mover, 5, oldY);
                            temp[5, oldY] = newRook;
                            newRook.wasMoved = true;
                        }
                    }
                }
            }

            //Promote Pawns
            for (int i = 0; i < GameManager.boardSize * 2; i++)
            {
                int y = 0;
                int x = i;
                if (x >= GameManager.boardSize)
                {
                    y = GameManager.boardSize - 1;
                    x -= GameManager.boardSize;
                }


                if (temp[x, y] != null)
                {
                    if (temp[x, y].piece == ChessPiece.pawn)
                    {
                        //Make Queen, Dont Destroy Data
                        if (realMove)
                        {
                            temp[x, y].piece = ChessPiece.queen;
                        }
                        else
                        {
                            temp[x, y] = new BoardPiece(ChessPiece.queen, temp[x, y].owner, x, y);
                        }
                    }
                }
            }

            return new BoardStatus(temp, mover);
        }

        //returns the balance of the board
        public double getBoardWorth(byte color)
        {
            return getBoardWorthForStatus(color) - getBoardWorthForStatus(GameManager.invertPlayer(color));
        }

        //returns the entire balance of the board for one color
        private double getBoardWorthForStatus(byte color)
        {
            double toOut = 0;
            foreach (BoardPiece bp in gameConfiguration)
            {
                if (bp == null)
                    continue;

                if (bp.owner == color)
                {
                    toOut += bp.piece.value;

                    int yStart = bp.owner == GameManager.white ? 0 : GameManager.boardSize - 1;
                    int far = bp.owner == GameManager.white ? bp.y : GameManager.boardSize - 1 - bp.y;
                    toOut += Math.Abs(far) * 0.15D;
                }
            }
            return toOut;
        }

        //Returns a list of all moves Possible for a player
        public List<Point> getAllPossibleMovesForColor(byte color, out List<BoardPiece> movers)
        {
            List<Point> temp = new List<Point>();
            movers = new List<BoardPiece>();
            //loop trough all pieces
            for (int x = 0; x < GameManager.boardSize; x++)
            {
                for (int y = 0; y < GameManager.boardSize; y++)
                {
                    BoardPiece bp = gameConfiguration[x, y];
                    //skip if not existant
                    if (bp == null)
                        continue;
                    if (bp.owner == color)
                    {
                        List<Point> toAdd = bp.getPossibleMoves(this);
                        for (int i = 0; i < toAdd.Count; i++)
                        {
                            temp.Add(toAdd[i]);
                            movers.Add(bp);
                        }
                    }
                }
            }
            return temp;
        }

        //returns the color of the winner or null
        public int checkForWinner()
        {
            Boolean whitesKingIsThere = false;
            Boolean blacksKingIsThere = false;

            //Look which kings are there
            foreach (BoardPiece bp in gameConfiguration)
            {
                if (bp != null)
                {
                    if (bp.piece == ChessPiece.king)
                    {
                        if (bp.owner == GameManager.white)
                        {
                            whitesKingIsThere = true;
                        }
                        if (bp.owner == GameManager.black)
                        {
                            blacksKingIsThere = true;
                        }
                    }
                }
            }

            //check if one is missing and display a winner
            if (!whitesKingIsThere && blacksKingIsThere)
            {
                return GameManager.black;
            }

            if (!blacksKingIsThere && whitesKingIsThere)
            {
                return GameManager.white;
            }

            return -1;
        }

        //creates the default Chess starting Position
        public static BoardStatus createNewBoard()
        {
            byte boardSize = GameManager.boardSize;
            byte white = GameManager.white;
            byte black = GameManager.black;

            BoardPiece[,] tempPieces = new BoardPiece[boardSize, boardSize];

            //Add Rooks in Corners
            tempPieces[0, 0] = new BoardPiece(ChessPiece.rook, white, 0, 0);
            tempPieces[7, 0] = new BoardPiece(ChessPiece.rook, white, 7, 0);

            tempPieces[0, 7] = new BoardPiece(ChessPiece.rook, black, 0, 7);
            tempPieces[7, 7] = new BoardPiece(ChessPiece.rook, black, 7, 7);

            //Add knights next to Rooks
            tempPieces[1, 0] = new BoardPiece(ChessPiece.knight, white, 1, 0);
            tempPieces[6, 0] = new BoardPiece(ChessPiece.knight, white, 6, 0);

            tempPieces[1, 7] = new BoardPiece(ChessPiece.knight, black, 1, 7);
            tempPieces[6, 7] = new BoardPiece(ChessPiece.knight, black, 6, 7);

            //Add bishops
            tempPieces[2, 0] = new BoardPiece(ChessPiece.bishop, white, 2, 0);
            tempPieces[5, 0] = new BoardPiece(ChessPiece.bishop, white, 5, 0);

            tempPieces[2, 7] = new BoardPiece(ChessPiece.bishop, black, 2, 7);
            tempPieces[5, 7] = new BoardPiece(ChessPiece.bishop, black, 5, 7);

            //Add Queens
            tempPieces[3, 0] = new BoardPiece(ChessPiece.queen, white, 3, 0);

            tempPieces[3, 7] = new BoardPiece(ChessPiece.queen, black, 3, 7);

            //Add Kings
            tempPieces[4, 0] = new BoardPiece(ChessPiece.king, white, 4, 0);

            tempPieces[4, 7] = new BoardPiece(ChessPiece.king, black, 4, 7);

            //Add pawns
            for (int i = 0; i < boardSize; i++)
            {
                tempPieces[i, 1] = new BoardPiece(ChessPiece.pawn, white, i, 1);
                tempPieces[i, 6] = new BoardPiece(ChessPiece.pawn, black, i, 6);
            }

            //Return the finished BoardStatus Object with White as the next Mover
            return new BoardStatus(tempPieces, white);
        }

    }
}
