using TicTacToe.Enums;

namespace TicTacToe.Game;

public class WinInfo
{
    public Player Winner { get; set; }
    public int WinTypeNum { get; set; }
    public WinType WinTypeKey { get; set; }
}