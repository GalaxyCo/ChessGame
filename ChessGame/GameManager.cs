using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace ChessGame
{
    class GameManager
    {
        /*
       * Die GameManager Klasse ist dafür da den Code schöner zu machen und um nicht alles in das Main Window zu schreiben
       * Speichert eine Liste von BordStatus (Den Spielverlauf) Element 0 ist dabei immer das akutelle Feld
       */

        /*
         * TODO:
         *  - Replace Piece Names with Images
         *  - Proper Chechmate check
         *  - Highlight the last Move
         */

        //Used to determine the Color/ Player
        public const byte white = 0, black = 1;

        //The AIs if used
        private static AIPlayer whiteAI, blackAI;

        //The Board Size (Normal Chess has 8)
        public const int boardSize = 8;

        //Const for the cell backgrounds (Default: Color.LightGray, Color.Gray)
        private static Color whiteBackground = Color.LightGray, blackBackground = Color.Gray;

        //Const for the piece names (Default: Color.Black, Color.White)
        private static Color blackColor = Color.Black, whiteColor = Color.White;

        //Const for da Memez
        private static Color windowBackground = Color.White;

        //Const for the move Highlight
        private static Color highlightColor = Color.OrangeRed;

        //The Size of every chess field (Default: 65), Font Size Default = 10 (alt: 72, 15)
        const int cellSize = 60, fontSize = 15;

        //Holds the entire Game (Maybe add Game Class or something)
        private static List<BoardStatus> boardStatuses;

        //The Index of the Current Board
        private static int boardIndex;

        //The last Clicked Label is the Piece the player Wants to Move
        private static BoardPiece lastClicked;

        //Has the Game Ended?
        private static Boolean gameEnded;

        //Is it an AIS Turn?
        private static Boolean aisTurn;

        //The Labels Representing the Pieces
        private static Label[,] boardLabels = new Label[boardSize, boardSize];

        //The Window
        private static GameWindow gameWindow;

        //inits the Game Manager
        public static void initGameManager(GameWindow window)
        {
            gameWindow = window;
            gameEnded = false;

            window.BackColor = windowBackground;

            //Init the chess Pieces
            ChessPiece.initPieces();

            //create a new BoardStatus List and add the typical chess start configuartion
            boardStatuses = new List<BoardStatus>();
            setCurrentBoardStatus(BoardStatus.createNewBoard());
            boardIndex = 0;

            gameWindow.Size = new Size(cellSize * boardSize + 40, cellSize * (boardSize + 2));

            //Setup the boardPanel
            gameWindow.boardPanel.Size = new Size(cellSize * boardSize, cellSize * boardSize);

            addBoardLabels();
            updateMoveCounter();

            //registerAI(new AIMiniMax(black, 4));
            //registerAI(new AIMiniMax(white, 4));
            //registerAI(new AIRandom(white));
            //registerAI(new AIRandom(black));
            gameWindow.Update();
        }

        //simple way to display the board
        private static void addBoardLabels()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    //The piece to display next
                    BoardPiece bp = getCurrentStatus().gameConfiguration[x, y];

                    //Calc if a square is black or white
                    Color backgroundColor = getBackgroundColorForCell(x, y);
                    //set the text color acording to the player
                    Color textColor = bp != null ? bp.owner == black ? blackColor : whiteColor : Color.Black;

                    //Create The Label Object
                    Label lbl = new Label { Size = new Size(cellSize, cellSize), Location = new Point((boardSize * cellSize) - ((x + 1) * cellSize), (boardSize * cellSize) - ((y + 1) * cellSize)), BackColor = backgroundColor, ForeColor = textColor, Font = new Font(FontFamily.GenericSansSerif, fontSize), TextAlign = ContentAlignment.MiddleCenter, Padding = Padding.Empty };

                    if (bp != null)
                        lbl.Image = getPieceImage(bp);

                    //Add the Click Event
                    lbl.MouseDown += boardLabelClick;

                    //Add Position Data
                    lbl.Tag = new Point(x, y);

                    //add the Label to the Panel
                    gameWindow.boardPanel.Controls.Add(lbl);

                    //Add the Label to the Array
                    boardLabels[x, y] = lbl;
                }
            }
            gameWindow.Update();
        }

        //Updates all Labels to represent the current Game
        private static void updateBoardLabels()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    //The piece to update
                    BoardPiece bp = getCurrentStatus().gameConfiguration[x, y];

                    //Update the Image
                    if (bp != null)
                        boardLabels[x, y].Image = getPieceImage(bp);
                    else
                        boardLabels[x, y].Image = null;
                }
            }
            gameWindow.Update();
        }

        //Gets called by every click event of the Labels
        private static void boardLabelClick(object sender, EventArgs e)
        {
            //you can only move pieces when youre on the latest Board
            //Dont allow the player to move while an ai is thinking
            if (boardIndex != 0 || gameEnded || aisTurn)
                return;

            //The Clicked Label
            Label btn = (Label)sender;
            Point p = (Point)btn.Tag;

            BoardPiece bp = getCurrentStatus().gameConfiguration[p.X, p.Y];


            if (lastClicked != null)
            {
                //Move Logic
                List<Point> moves = lastClicked.getPossibleMoves(getCurrentStatus());

                Boolean contains = false;
                //moves.contains doesnt seem to be working so:
                foreach (Point move in moves)
                {
                    if (move.X == p.X && move.Y == p.Y)
                    {
                        contains = true;
                        //break since contains stays true no matter what
                        break;
                    }
                }

                if (contains)
                {
                    //play a move
                    playMove(lastClicked, p);

                    //the return fixes a weird bug
                    return;
                }
            }
            if (bp == null)
            {
                return;
            }
            //is the field owned by the current player
            Boolean pieceOwned = getCurrentStatus().mover == bp.owner;

            if (lastClicked == bp)
            {
                //Unselect Logic
                removeLastClicked();
            }
            else if (pieceOwned)
            {
                //Select Logic
                if (lastClicked != null)
                    removeLastClicked();

                //Only select a pice if it can move somewhere
                if (bp.getPossibleMoves(getCurrentStatus()).Count > 0)
                {
                    setLastClicked(bp);
                }
            }
        }

        //called if a moved happend
        public static void tryAIMove()
        {
            if (gameEnded)
                return;
            aisTurn = false;
            byte player = getCurrentStatus().mover;
            if (whiteAI != null)
            {
                if (player == whiteAI.color)
                {
                    aisTurn = true;
                    Point nextPos = whiteAI.playNextTurn(getCurrentStatus(), out BoardPiece movedPiece);
                    playMove(movedPiece, nextPos);
                }
            }
            if (blackAI != null)
            {
                if (player == blackAI.color)
                {
                    aisTurn = true;
                    Point nextPos = blackAI.playNextTurn(getCurrentStatus(), out BoardPiece movedPiece);
                    playMove(movedPiece, nextPos);
                }
            }
            aisTurn = false;
        }

        //Plays Moves
        private static void playMove(BoardPiece toMove, Point newPos)
        {
            //create a new BoardStatus
            BoardStatus status = BoardStatus.getStatusOffOldBoard(getCurrentStatus(), toMove, newPos, true);

            setCurrentBoardStatus(status);

            removeLastClicked();

            updateMoveCounter();
            updateBoardLabels();

            gameWindow.Update();

            checkForWinner();

            //Dirty fix for AIs being too fast
            Thread.Sleep(200);
            tryAIMove();
        }

        //scrolls throug the Statuses by n
        public static void scrollStatus(int n)
        {
            //calc new board Index (Sorry for bool operators :D)
            n += boardIndex;
            boardIndex = (n >= boardStatuses.Count ? 0 : (n < 0 ? (gameEnded ? boardStatuses.Count - 1 : boardIndex) : n));

            //update the counter to reflect the new boardIndex
            updateMoveCounter();

            //update the Labels to reflect the new boardIndex
            updateBoardLabels();
        }

        //adds an ai to a certain color
        private static void registerAI(AIPlayer ai)
        {
            if (ai.color == white)
                whiteAI = ai;
            else if (ai.color == black)
                blackAI = ai;
        }

        //updates the move counter
        private static void updateMoveCounter()
        {
            String scrollPos = boardStatuses.Count - boardIndex + " / " + boardStatuses.Count;

            String playerTurn = " | It is " + getPlayerName(getCurrentStatus().mover) + "s Turn";

            if (gameEnded && boardIndex == 0)
                playerTurn = " The Game Ended " + getPlayerName(invertPlayer(getCurrentStatus().mover)) + " Won";

            gameWindow.scrollPosLabel.Text = scrollPos + playerTurn;
        }

        //sets the last clicked Piece
        private static void setLastClicked(BoardPiece bp)
        {
            //the selected Label
            Label btn = boardLabels[bp.x, bp.y];

            //Highlight the piece
            btn.Font = new Font(btn.Font, FontStyle.Bold);
            btn.BackColor = highlightColor;

            //Highlight its moves
            List<Point> moves = bp.getPossibleMoves(getCurrentStatus());
            foreach (Point pos in moves)
            {
                boardLabels[pos.X, pos.Y].BackColor = highlightColor;
            }

            //set lastClicked
            lastClicked = bp;
        }

        //removes the last clicked element
        private static void removeLastClicked()
        {
            if (lastClicked == null)
                return;
            //the selected Label
            Label btn = boardLabels[lastClicked.x, lastClicked.y];

            //remove Highlight
            btn.Font = new Font(btn.Font, FontStyle.Regular);
            resetLabelBackgrounds();

            //remove lastClicked
            lastClicked = null;
        }

        //Returns a Image to Display
        private static Image getPieceImage(BoardPiece bp)
        {
            String pieceName = bp.piece.name.ToLower();
            String owner = bp.owner == white ? "WHITE" : "BLACK";

            String imgName = pieceName + owner;


            return Image.FromFile(@"img\" + imgName + ".png");
        }

        //Displays a win message and ends the game if there is a Winner
        public static void checkForWinner()
        {
            int winner = getCurrentStatus().checkForWinner();

            if(winner != -1)
            {
                displayWinMessage((byte)winner);
                gameEnded = true;
                resetLabelBackgrounds();
                gameWindow.Update();
            }
        }

        //The Message
        private static void displayWinMessage(byte winner)
        {
            MessageBox.Show(getPlayerName(winner) + " Won the Game!", "WINNER! :D");
        }

        //Returns Black or white
        private static String getPlayerName(byte player)
        {
            return player == white ? "White" : "Black";
        }

        //Inverts the player id
        public static byte invertPlayer(byte player)
        {
            return player == white ? black : white;
        }

        //goes trough all Labels and sets the BackColor Normal
        private static void resetLabelBackgrounds()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    Label btn = boardLabels[x, y];
                    btn.BackColor = getBackgroundColorForCell(x, y);
                }
            }
        }

        //gets the Color for a Label
        private static Color getBackgroundColorForCell(int x, int y)
        {
            return (x + y) % 2 == 0 ? whiteBackground : blackBackground;
        }

        //set the current Board
        private static void setCurrentBoardStatus(BoardStatus status)
        {
            boardStatuses.Insert(0, status);
        }

        //Returns the 0th element from boardStatuses
        private static BoardStatus getCurrentStatus()
        {
            return boardStatuses[boardIndex];
        }

    }
}
