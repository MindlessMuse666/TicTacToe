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
        { Player.O, new BitmapImage(new Uri(ASSETS_PATH + "O15.png")) }
    };

    private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> _animations = new()
    {
        { Player.X, new ObjectAnimationUsingKeyFrames() },
        { Player.O, new ObjectAnimationUsingKeyFrames() }
    };

    private readonly DoubleAnimation _fadeOutAnimation = new()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 1,
        To = 0
    };

    private readonly DoubleAnimation _fadeInAnimation = new()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 0,
        To = 1
    };

    private readonly Image[,] _imageControls = new Image[3, 3];
    private readonly GameState _gameState = new();


    public MainWindow()
    {
        InitializeComponent();
        ConnectEventHandlers();
        SetupAnimations();
        SetupGameGrid();
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_gameState.CurrentPlayer != Player.X)
            return;

        double squareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squareSize);
        int column = (int)(clickPosition.X / squareSize);

        _gameState.MakeMove(row, column);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (_gameState.GameOver)
        {
            _gameState.ResetGame();
        }
    }

    private void OnMoveMade(int row, int column)
    {
        if (_gameState == null)
            return;

        Player player = _gameState.GameGrid[row, column];

        _imageControls[row, column].BeginAnimation(Image.SourceProperty, _animations[player]);
        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
    }

    private async void OnGameEnded(GameResult gameResult)
    {
        await Task.Delay(1000);

        if (gameResult.Winner == Player.None)
        {
            await TransitionToEndScreen("Tie!", null);
        }
        else
        {
            await DrawLine(gameResult.WinInfo);
            await Task.Delay(1000);
            await TransitionToEndScreen("Winner:", _imageSources[gameResult.Winner]);
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

        await TransitionToGameScreen();
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

        WinLine.X1 = start.X;
        WinLine.Y1 = start.Y;

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

        WinLine.Visibility = Visibility.Visible;

        WinLine.BeginAnimation(Line.X2Property, xLineAnimation);
        WinLine.BeginAnimation(Line.Y2Property, yLineAnimation);

        await Task.Delay(xLineAnimation.Duration.TimeSpan);
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

    private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
    {
        await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));

        ResultText.Text = text;
        WinnerImage.Source = winnerImage;

        await FadeIn(EndScreen);
    }

    private async Task TransitionToGameScreen()
    {
        await FadeOut(EndScreen);

        WinLine.Visibility = Visibility.Hidden;

        await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
    }

    private async Task FadeOut(UIElement uIElement)
    {
        uIElement.BeginAnimation(OpacityProperty, _fadeOutAnimation);

        await Task.Delay(_fadeOutAnimation.Duration.TimeSpan);

        uIElement.Visibility = Visibility.Hidden;
    }

    private async Task FadeIn(UIElement uIElement)
    {
        uIElement.Visibility = Visibility.Visible;

        uIElement.BeginAnimation(OpacityProperty, _fadeInAnimation);

        await Task.Delay(_fadeInAnimation.Duration.TimeSpan);
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

    protected override void OnClosed(EventArgs e)
    {
        DisconnectEventHandlers();

        base.OnClosed(e);
    }
}