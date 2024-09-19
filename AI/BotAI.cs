using TicTacToe.Enums;


namespace TicTacToe.AI;

public class BotAI
{
    private int _row;
    private int _column;

    private readonly Dictionary<Player, int> scores = new()
    {
        { Player.X, 10 },
        { Player.O, -10 },
        { Player.None, 0 }
    };


    public (int, int) MakeBestMove(Player[,] GameGrid, int turnPassed)
    {
        double evaluation;
        double maxEvaluation = double.NegativeInfinity;

        if (turnPassed == 9)
            return (-1, -1);
        else
        {
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    if (GameGrid[row, column] == Player.None)
                    {
                        GameGrid[row, column] = Player.X;
                        evaluation = MiniMax(GameGrid, turnPassed, false, double.NegativeInfinity, double.PositiveInfinity, 0);
                        GameGrid[row, column] = Player.None;

                        if (evaluation > maxEvaluation)
                        {
                            maxEvaluation = evaluation;
                            _row = row;
                            _column = column;
                        }
                    }
                }
            }

            return (_row, _column);
        }
    }


    private double MiniMax(Player[,] GameGrid, int turnPassed, bool maximizinPlayer, double alpha, double beta, double depth)
    {
        Player winner = WinnerCheck(GameGrid);
        double evaluation;

        if (turnPassed == 9 || winner != Player.None)
            return scores[winner];

        if (maximizinPlayer)
        {
            double maxEvaluation = double.NegativeInfinity;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    if (GameGrid[row, column] == Player.None)
                    {
                        GameGrid[row, column] = Player.X;
                        evaluation = MiniMax(GameGrid, turnPassed + 1, false, alpha, beta, depth + 1);
                        GameGrid[row, column] = Player.None;
                        maxEvaluation = Math.Max(evaluation, maxEvaluation);
                        alpha = Math.Max(evaluation, alpha);

                        if (beta <= alpha)
                            break;
                    }
                }
            }

            return maxEvaluation - depth;
        }
        else
        {
            double minEvaluation = double.PositiveInfinity;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    if (GameGrid[row, column] == Player.None)
                    {
                        GameGrid[row, column] = Player.O;
                        evaluation = MiniMax(GameGrid, turnPassed + 1, true, alpha, beta, depth + 1);
                        GameGrid[row, column] = Player.None;
                        minEvaluation = Math.Min(minEvaluation, evaluation);
                        alpha = Math.Min(evaluation, alpha);

                        if (beta <= alpha)
                            break;
                    }
                }
            }

            return minEvaluation - depth;
        }
    }


    private Player WinnerCheck(Player[,] GameGrid)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                if (GameGrid[row, 0] == GameGrid[row, 1] && GameGrid[row, 1] == GameGrid[row, 2])
                    return GameGrid[row, 0];

                else if (GameGrid[0, column] == GameGrid[1, column] && GameGrid[1, column] == GameGrid[2, column])
                    return GameGrid[0, column];

                else if (GameGrid[0, 0] == GameGrid[1, 1] && GameGrid[1, 1] == GameGrid[2, 2])
                    return GameGrid[0, 0];

                else if (GameGrid[0, 2] == GameGrid[1, 1] && GameGrid[1, 1] == GameGrid[2, 0])
                    return GameGrid[0, 2];
            }
        }

        return Player.None;
    }
}