using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackGammonProject
{
    class Bar
    {
        public uint BarWinningRedPiecesCount { get; set; }
        public uint BarWinningBlackPiecesCount { get; set; }
        public uint BarDeadRedPiecesCount { get; set; }
        public uint BarDeadBlackPiecesCount { get; set; }

        // Adds a piece to the 'dead' pieces bar by colour
        public void AddDeadPieceToBar(Colour playerColour)
        {
            if (playerColour == Colour.Red)
            {
                BarDeadRedPiecesCount++;
            }
            else
            {
                BarDeadBlackPiecesCount++;
            }
        }

        // Removes a piece from the 'dead' pieces bar by colour
        public void RemoveDeadPieceFromBar(Colour playerColour)
        {
            if (playerColour == Colour.Red)
            {
                if (BarDeadRedPiecesCount > 0)
                {
                    BarDeadRedPiecesCount--;
                }
            }
            else
            {
                if (BarDeadBlackPiecesCount > 0)
                {
                    BarDeadBlackPiecesCount--;
                }
            }
        }

        // Adds a Red piece to the 'winning' pieces bar
        public void AddWinningPieceToBar(Colour playerColour) 
        {
            if (playerColour == Colour.Red)
            {
                BarWinningRedPiecesCount++;
            }
            else
            {
                BarWinningBlackPiecesCount++;
            }
        }

    }
}
