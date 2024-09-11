using TicTacToe.Enums;

namespace TicTacToe.Game;

public class GameState
{
    public Player[,] GameGrid { get; set; }
    public Player CurrentPlayer { get; private set; }
    public int TurnsPassed { get; private set; }
    public bool GameOver { get; private set; }

    public event Action<int, int> MoveMade;
    public event Action<GameResult?> GameEnded;
    public event Action GameRestarted;

    public GameState()
    {
        GameGrid = new Player[3, 3];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;
        GameOver = false;
    }

    public void MakeMove(int row, int column)
    {
        if (!CanMakeMove(row, column))
            return;

        GameGrid[row, column] = CurrentPlayer;
        TurnsPassed++;

        if (DidMoveEndGame(row, column, out GameResult? gameResult))
        {
            GameOver = true;

            MoveMade?.Invoke(row, column);
            GameEnded?.Invoke(gameResult);
        }
        else
        {
            SwitchPlayer();

            MoveMade?.Invoke(row, column);
        }
    }

    public void ResetGame()
    {
        GameOver = false;
        GameGrid = new Player[3, 3];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;

        GameRestarted?.Invoke();
    }

    private bool CanMakeMove(int row, int column) => !GameOver && GameGrid[row, column] == Player.None;

    private bool IsGridFull() => TurnsPassed == 9;

    private void SwitchPlayer() => CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;

    private bool AreSquaresMarked((int, int)[] squares, Player player)
    {
        foreach ((int row, int column) in squares)
        {
            if (GameGrid[row, column] != player)
                return false;
        }

        return true;
    }

    private bool DidMoveWin(int row, int column, out WinInfo? winInfo)
    {
        (int, int)[] winRow =
        [
            (row, 0),
            (row, 1),
            (row, 2)
        ];

        (int, int)[] winColumn =
        [
            (0, column),
            (1, column),
            (2, column)
        ];

        (int, int)[] winDiagonal =
        [
            (0, 0),
            (1, 1),
            (2, 2)
        ];

        (int, int)[] winReverseDiagonal =
        [
            (0, 2),
            (1, 1),
            (2, 0)
        ];

        if (AreSquaresMarked(winRow, CurrentPlayer))
        {
            winInfo = new WinInfo
            {
                Type = WinType.Row,
                Number = row
            };

            return true;
        }

        if (AreSquaresMarked(winColumn, CurrentPlayer))
        {
            winInfo = new WinInfo
            {
                Type = WinType.Column,
                Number = column
            };

            return true;
        }

        if (AreSquaresMarked(winDiagonal, CurrentPlayer))
        {
            winInfo = new WinInfo
            {
                Type = WinType.Diagonal
            };

            return true;
        }

        if (AreSquaresMarked(winReverseDiagonal, CurrentPlayer))
        {
            winInfo = new WinInfo
            {
                Type = WinType.ReverseDiagonal
            };

            return true;
        }

        winInfo = null;

        return false;
    }

    private bool DidMoveEndGame(int row, int column, out GameResult? gameResult)
    {
        if (DidMoveWin(row, column, out WinInfo? winInfo))
        {
            gameResult = new GameResult
            {
                Winner = CurrentPlayer,
                WinInfo = winInfo
            };

            return true;
        }

        if (IsGridFull())
        {
            gameResult = new GameResult
            {
                Winner = Player.None,
            };

            return true;
        }

        gameResult = null;

        return false;
    }
}