using TicTacToe.Enums;

namespace TicTacToe.Game;

public class GameResult()
{
    public Player Winner { get; set; }
    public WinInfo? WinInfo { get; set; }
}