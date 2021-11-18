using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackGammonProject
{
    public class Player
    {
        public bool PlayerIsActive { get; set; }
        public uint PlayerDeadPieces { get; set; }
        public uint PlayerWinningPieces { get; set; }
        public string PlayerName { get; private set; }
        public Colour PlayerColour { get; private set; }
        public Player (Colour PlayerColour)
        {
            this.PlayerColour = PlayerColour;
            PlayerName = (PlayerColour == Colour.Red) ? "Red" : "Black";
            PlayerIsActive = (PlayerColour == Colour.Red) ? true : false;
        }
    }
}
