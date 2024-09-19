using TicTacToe.AI;
using TicTacToe.Enums;

namespace TicTacToe.Game;

public class GameState
{
    public int GameOverDelay { get; private set; } = 800;
    public int StepDelay { get; private set; } = 400;

    public Player[,] GameGrid { get; private set; }
    public Player CurrentPlayer { get; private set; }
    public int TurnPassed { get; private set; }
    public bool GameOver { get; private set; }
    public int AIRow { get; private set; }
    public int AIColumn { get; private set; }

    private BotAI _AIAlgorithm;
    private WinInfo _winInfo;
    private GameResult _gameResult;


    public GameState()
    {
        _AIAlgorithm = new BotAI();
        GameGrid = new Player[3, 3];
        _winInfo = new WinInfo();
        _gameResult = new GameResult();

        CurrentPlayer = Player.X;
        TurnPassed = 0;
        GameOver = false;
    }


    public GameResult GetGameResult() => _gameResult;


    public void Reset()
    {
        _AIAlgorithm = new BotAI();
        GameGrid = new Player[3, 3];
        _winInfo = new WinInfo();
        _gameResult = new GameResult();

        CurrentPlayer = Player.X;
        TurnPassed = 0;
        GameOver = false;
    }


    public void AIMove()
    {
        (int, int) coordinates = _AIAlgorithm.MakeBestMove(GameGrid, TurnPassed);
        int row = coordinates.Item1;
        int col = coordinates.Item2;

        if (!(row == -1 && col == -1))
        {
            AIRow = row;
            AIColumn = col;
            GameGrid[row, col] = CurrentPlayer;
            TurnPassed++;
        }

        if (DidMoveEndGame(row, col, ref _gameResult))
            GameOver = true;
        else
            SwitchPlayer();
    }


    public bool PlayerMove(int row, int column)
    {
        if (!CanMakeMove(row, column))
            return false;

        GameGrid[row, column] = CurrentPlayer;
        TurnPassed++;

        if (DidMoveEndGame(row, column, ref _gameResult))
            GameOver = true;
        else
            SwitchPlayer();

        return true;
    }


    private bool CanMakeMove(int row, int column) => GameGrid[row, column] == Player.None && !GameOver;


    private bool IsGridFull() => TurnPassed == 9;


    private void SwitchPlayer() => CurrentPlayer = (CurrentPlayer == Player.X 
        ? CurrentPlayer = Player.O 
        : CurrentPlayer = Player.X);


    private bool AreSquaresMarked((int, int)[] WinningCombo, Player player)
    {
        foreach ((int r, int c) in WinningCombo)
        {
            if (GameGrid[r, c] != player)
                return false;
        }

        return true;
    }


    private bool DidMoveWin(int row, int column, ref WinInfo winInfo)
    {
        (int, int)[] winRow = [(row, 0), (row, 1), (row, 2)];
        (int, int)[] winColumn = [(0, column), (1, column), (2, column)];
        (int, int)[] winDiagonal = [(0, 0), (1, 1), (2, 2)];
        (int, int)[] winReverseDiagonal = [(0, 2), (1, 1), (2, 0)];

        if (AreSquaresMarked(winRow, CurrentPlayer))
        {
            winInfo = new WinInfo { Winner = CurrentPlayer, WinTypeNum = row, WinTypeKey = WinType.Row };

            return true;
        }

        if (AreSquaresMarked(winColumn, CurrentPlayer))
        {
            winInfo = new WinInfo { Winner = CurrentPlayer, WinTypeNum = column, WinTypeKey = WinType.Column };

            return true;
        }

        if (AreSquaresMarked(winDiagonal, CurrentPlayer))
        {
            winInfo = new WinInfo { Winner = CurrentPlayer, WinTypeKey = WinType.Diagonal };

            return true;
        }

        if (AreSquaresMarked(winReverseDiagonal, CurrentPlayer))
        {
            winInfo = new WinInfo { Winner = CurrentPlayer, WinTypeKey = WinType.ReverseDiagonal };

            return true;
        }

        return false;
    }


    private bool DidMoveEndGame(int row, int column, ref GameResult gameResult)
    {
        if (DidMoveWin(row, column, ref _winInfo))
        {
            gameResult.WinInfo = _winInfo;
            gameResult.Winner = CurrentPlayer;

            return true;
        }

        if (IsGridFull())
        {
            gameResult.Winner = Player.None;

            return true;
        }

        return false;
    }
}