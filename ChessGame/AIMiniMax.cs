using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace ChessGame
{
    class AIMiniMax : AIPlayer
    {
        int depth;

        struct ScoredMove
        {
            public ScoredMove(int i, double score) {
                index = i;
                this.score = score;
            }

            public int index { get; }
            public double score { get; }
        }

        public AIMiniMax(byte color, int depth) : base(color)
        {
            this.depth = depth;
        }

        public override Point playNextTurn(BoardStatus status, out BoardPiece moved)
        {
            List<Point> movesInPos = status.getAllPossibleMovesForColor(color, out List<BoardPiece> movers);
            double scoreMax = double.MinValue;

            List<ScoredMove> moveScores = new List<ScoredMove>();

            for(int i = 0; i < movesInPos.Count; i++)
            {
                double score = miniMax(BoardStatus.getStatusOffOldBoard(status, movers[i], movesInPos[i], false), GameManager.invertPlayer(color), depth, double.MinValue, double.MaxValue);
                moveScores.Add(new ScoredMove(i, score));

                if (score > scoreMax)
                {
                    scoreMax = score;
                }
            }
            moveScores = filterList(moveScores, scoreMax, 0.175D);

            int move = moveScores[new Random().Next(0, Math.Abs(moveScores.Count - 1))].index;
            moved = movers[move];
            return movesInPos[move];
        }

        private List<ScoredMove> filterList(List<ScoredMove> scores, double minScore, double offset)
        {
            List<ScoredMove> outp = new List<ScoredMove>();
            for(int i = 0; i < scores.Count; i++)
            {
                if(scores[i].score >= minScore - offset)
                {
                    outp.Add(scores[i]);
                }
            }
            return outp;
        }

        private double miniMax(BoardStatus status, byte curPlayer, int searchDepth, double alpha, double beta)
        {
            if (searchDepth == 0 || status.checkForWinner() != -1)
                return status.getBoardWorth(color);

            //Maximizing
            if(curPlayer == color)
            {
                double maxEval = double.MinValue;
                List<Point> movesInPos = status.getAllPossibleMovesForColor(color, out List<BoardPiece> movers);
                for(int i = 0; i < movesInPos.Count; i++)
                {
                    double eval = miniMax(BoardStatus.getStatusOffOldBoard(status, movers[i], movesInPos[i], false), GameManager.invertPlayer(color), searchDepth - 1, alpha, beta);
                    maxEval = Math.Max(maxEval, eval);

                    alpha = Math.Max(alpha, maxEval);

                    if (beta <= alpha)
                        break; 
                }
                return maxEval;
            } else
            {
                double minEval = double.MaxValue;
                List<Point> movesInPos = status.getAllPossibleMovesForColor(GameManager.invertPlayer(color), out List<BoardPiece> movers);
                for (int i = 0; i < movesInPos.Count; i++)
                {
                    double eval = miniMax(BoardStatus.getStatusOffOldBoard(status, movers[i], movesInPos[i], false), color, searchDepth - 1, alpha, beta);
                    minEval = Math.Min(minEval, eval);

                    beta = Math.Min(beta, minEval);

                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }
    }
}
