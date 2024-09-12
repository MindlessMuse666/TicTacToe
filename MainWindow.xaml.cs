using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TicTacToe.Enums;
using TicTacToe.Game;

namespace TicTacToe;

public partial class MainWindow : Window
{
    private readonly Dictionary<Player, ImageSource> _imageSources = new()
    {
        { Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
        { Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
    };

    private readonly Image[,] _imageControls = new Image[3, 3];
    private readonly GameState? _gameState = new();

    public MainWindow()
    {
        InitializeComponent();
        ConnectEventHandlers();
        SetupGameGrid();
    }

    protected override void OnClosed(EventArgs e)
    {
        DisconnectEventHandlers();

        base.OnClosed(e);
    }

    private void OnMoveMade(int row, int column)
    {
        if (_gameState == null)
            return;

        Player player = _gameState.GameGrid[row, column];

        _imageControls[row, column].Source = _imageSources[player];
        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
    }

    private async void OnGameEnded(GameResult? gameResult)
    {
        await Task.Delay(1000);

        if (gameResult?.Winner == Player.None)
        {
            TransitionToEndScreen("Tie!", null);
        }
        else
        {
            DrawLine(gameResult?.WinInfo);

            await Task.Delay(2000);

            TransitionToEndScreen("Winner:", _imageSources[gameResult.Winner]);
        }
    }

    private void OnGameRestart()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                _imageControls[row, column].Source = null;
            }
        }

        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];

        TransitionToGameScreen();
    }

    private void SetupGameGrid()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                Image imageControl = new();

                GameGrid.Children.Add(imageControl);
                _imageControls[row, column] = imageControl;
            }
        }
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        double squareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squareSize);
        int column = (int)(clickPosition.X / squareSize);

        _gameState?.MakeMove(row, column);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _gameState?.ResetGame();
    }

    private void DrawLine(WinInfo winInfo)
    {
        (Point start, Point end) = FindLinePoints(winInfo);

        WinLine.X1 = start.X;
        WinLine.Y1 = start.Y;

        WinLine.X2 = end.X;
        WinLine.Y2 = end.Y;

        WinLine.Visibility = Visibility.Visible;
    }

    private (Point, Point) FindLinePoints(WinInfo winInfo)
    {
        double squareSize = GameGrid.Width / 3;
        double margin = squareSize / 2;

        switch (winInfo.Type)
        {
            case WinType.Row:
                {
                    double y = winInfo.Number * squareSize + margin;
                    return (new Point(0, y), new Point(GameGrid.Width, y));
                }

            case WinType.Column:
                {
                    double x = winInfo.Number * squareSize + margin;
                    return (new Point(x, 0), new Point(x, GameGrid.Height));
                }

            case WinType.Diagonal:
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));

            default:
                return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }
    }

    private void TransitionToEndScreen(string? text, ImageSource? winnerImage)
    {
        TurnPanel.Visibility = Visibility.Hidden;
        GameCanvas.Visibility = Visibility.Hidden;
        EndScreen.Visibility = Visibility.Visible;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
    }

    private void TransitionToGameScreen()
    {
        EndScreen.Visibility = Visibility.Hidden;
        WinLine.Visibility = Visibility.Hidden;
        TurnPanel.Visibility = Visibility.Visible;
        GameCanvas.Visibility = Visibility.Visible;
    }

    private void ConnectEventHandlers()
    {
        _gameState.MoveMade += OnMoveMade;
        _gameState.GameEnded += OnGameEnded;
        _gameState.GameRestarted += OnGameRestart;
    }

    private void DisconnectEventHandlers()
    {
        _gameState.MoveMade -= OnMoveMade;
        _gameState.GameEnded -= OnGameEnded;
        _gameState.GameRestarted -= OnGameRestart;
    }
}