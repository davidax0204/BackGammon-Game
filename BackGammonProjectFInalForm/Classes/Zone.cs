using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackGammonProject
{
    public class Zone
    {
        public uint ZonePiecesCount { get; set; }
        public Colour ZoneColour { get; set; }
        public Zone() 
        {
            ZoneColour = Colour.Empty;
        }
        public Zone (uint NumberOfPiecesInZone, Colour ColourZone)
        {
            this.ZonePiecesCount = NumberOfPiecesInZone;
            this.ZoneColour = ColourZone;
        }
    }
}
