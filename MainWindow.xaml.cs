using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TicTacToe.Enums;
using TicTacToe.Game;

namespace TicTacToe;

public partial class MainWindow : Window
{
    private const string ASSETS_PATH = "pack://application:,,,/Assets/";

    private readonly Dictionary<Player, ImageSource> _imageSources = new()
    {
        { Player.X, new BitmapImage(new Uri(ASSETS_PATH + "X15.png")) },
        { Player.O, new BitmapImage(new Uri(ASSETS_PATH + "O15.png")) },
        { Player.None, null }
    };
    private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> _animations = new()
    {
        { Player.X, new ObjectAnimationUsingKeyFrames() },
        { Player.O, new ObjectAnimationUsingKeyFrames() }
    };

    private readonly Image[,] _imageControls = new Image[3, 3];
    private readonly GameState _gameState = new();


    public MainWindow()
    {
        InitializeComponent();
        SetupGameGrid();
        SetupAnimations();
        OnGameRestart();
    }


    private void GameGrid_Click(object sender, MouseButtonEventArgs e)
    {
        if (_gameState.CurrentPlayer != Player.O)
            return;

        double squareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squareSize);
        int column = (int)(clickPosition.X / squareSize);

        GameFlow(row, column);
    }


    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _gameState.Reset();
        OnGameRestart();
    }


    private void OnMoveMade(int row, int column)
    {
        Player player = _gameState.GameGrid[row, column];
        _imageControls[row, column].BeginAnimation(Image.SourceProperty, _animations[player]);
        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
    }


    private async void OnGameEnded(GameResult gameResult)
    {
        await Task.Delay(_gameState.GameOverDelay);

        if (gameResult.Winner == Player.None)
            TransitionToEndScreen("Tie!", null);
        else
        {
            await DrawLine(gameResult.WinInfo);
            await Task.Delay(_gameState.GameOverDelay);
            TransitionToEndScreen("Winner:", _imageSources[gameResult.Winner]);
        }
    }


    private async void OnGameRestart()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                _imageControls[row, column].BeginAnimation(Image.SourceProperty, null);
                _imageControls[row, column].Source = null;
            }
        }

        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];

        TransitionToGameScreen();

        _gameState.AIMove();

        await Task.Delay(80);
        OnMoveMade(_gameState.AIRow, _gameState.AIColumn);
    }


    private async void GameFlow(int row, int column)
    {
        if (_gameState.PlayerMove(row, column) && !_gameState.GameOver)
        {
            OnMoveMade(row, column);

            await Task.Delay(_gameState.StepDelay);

            _gameState.AIMove();

            if (!_gameState.GameOver)
            {
                OnMoveMade(_gameState.AIRow, _gameState.AIColumn);
            }
            else
            {
                OnMoveMade(_gameState.AIRow, _gameState.AIColumn);
                OnGameEnded(_gameState.GetGameResult());
            }
        }
        else if (_gameState.GameOver)
        {
            OnMoveMade(row, column);
            OnGameEnded(_gameState.GetGameResult());
        }
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


    private void SetupAnimations()
    {
        _animations[Player.X].Duration = TimeSpan.FromSeconds(.25);
        _animations[Player.O].Duration = TimeSpan.FromSeconds(.25);

        for (int i = 0; i < 16; i++)
        {
            Uri xUri = new($"{ASSETS_PATH}X{i}.png");
            Uri oUri = new($"{ASSETS_PATH}O{i}.png");

            BitmapImage xImage = new(xUri);
            BitmapImage oImage = new(oUri);

            DiscreteObjectKeyFrame xKeyFrame = new(xImage);
            DiscreteObjectKeyFrame oKeyFrame = new(oImage);

            _animations[Player.X].KeyFrames.Add(xKeyFrame);
            _animations[Player.O].KeyFrames.Add(oKeyFrame);
        }
    }


    private async Task DrawLine(WinInfo winInfo)
    {
        (Point start, Point end) = FindLinePoints(winInfo);

        Line.X1 = start.X;
        Line.Y1 = start.Y;

        DoubleAnimation xLineAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(.25),
            From = start.X,
            To = end.X
        };

        DoubleAnimation yLineAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(.25),
            From = start.Y,
            To = end.Y
        };

        Line.Visibility = Visibility.Visible;

        Line.BeginAnimation(Line.X2Property, xLineAnimation);
        Line.BeginAnimation(Line.Y2Property, yLineAnimation);

        await Task.Delay(xLineAnimation.Duration.TimeSpan);
    }


    private (Point, Point) FindLinePoints(WinInfo winInfo)
    {
        double squareSize = GameGrid.Width / 3;
        double margin = squareSize / 2;

        switch (winInfo.WinTypeKey)
        {
            case WinType.Row:
                {
                    double y = winInfo.WinTypeNum * squareSize + margin;

                    return (new Point(0, y), new Point(GameGrid.Width, y));
                }

            case WinType.Column:
                {
                    double x = winInfo.WinTypeNum * squareSize + margin;

                    return (new Point(x, 0), new Point(x, GameGrid.Height));
                }

            case WinType.Diagonal:
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));

            default:
                return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }
    }


    private void TransitionToEndScreen(string text, ImageSource winnerImage)
    {
        TurnPanel.Visibility = Visibility.Hidden;
        GameCanvas.Visibility = Visibility.Hidden;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        EndScreen.Visibility = Visibility.Visible;
    }


    private void TransitionToGameScreen()
    {
        Line.Visibility = Visibility.Hidden;
        EndScreen.Visibility = Visibility.Hidden;
        TurnPanel.Visibility = Visibility.Visible;
        GameCanvas.Visibility = Visibility.Visible;
    }
}