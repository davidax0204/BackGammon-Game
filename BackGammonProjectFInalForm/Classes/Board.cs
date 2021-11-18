using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackGammonProject
{
    public class Board
    {
        public List <Zone> ZoneList { get; set; }
        public Board ()
        {
            ZoneList = new List<Zone>(new Zone[26]);
            InitializeBoard();
        }
        // Board Creation
        private void InitializeBoard ()
        {
            for (int i = 0; i < 26; i++)
            {
                switch(i)
                {
                    case 1:  ZoneList[i] = new Zone(2, Colour.Red);   break;
                    case 6:  ZoneList[i] = new Zone(5, Colour.Black); break;
                    case 8:  ZoneList[i] = new Zone(3, Colour.Black); break;
                    case 12: ZoneList[i] = new Zone(5, Colour.Red);   break;
                    case 13: ZoneList[i] = new Zone(5, Colour.Black); break;
                    case 17: ZoneList[i] = new Zone(3, Colour.Red);   break;
                    case 19: ZoneList[i] = new Zone(5, Colour.Red);   break;
                    case 24: ZoneList[i] = new Zone(2, Colour.Black); break;
                    default: ZoneList[i] = new Zone(0,Colour.Empty);  break;
                }
            }
        }
        // Adding a piece to a zone 
        public void AddPieceToZone (int destinationZone, Colour movingPlayerColour)
        {
            if (ZoneList[destinationZone].ZoneColour==movingPlayerColour || ZoneList[destinationZone].ZoneColour==Colour.Empty)
            {
                ZoneList[destinationZone].ZonePiecesCount++;
                ZoneList[destinationZone].ZoneColour = movingPlayerColour;
            }
        }
        // Removing a piece from zone 
        public void RemovePieceFromZone(int originZone)
        {
            if (ZoneList[originZone].ZonePiecesCount > 0)
            {
                ZoneList[originZone].ZonePiecesCount--;
            }
            if (ZoneList[originZone].ZonePiecesCount == 0)
            {
                ZoneList[originZone].ZoneColour = Colour.Empty;
            }

        }
    }
}
