using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using BackGammonProjectFInalForm.Properties;
using BackGammonProject;
using System.Security.Cryptography;
using System.Timers;

namespace BackGammonProjectFInalForm
{
    public partial class BoardForm : Form
    {
        Dices gameDices = new Dices();
        Button lastDiceChoiseBtn;
        Board gameBoard = new Board();
        Player redPlayer = new Player(Colour.Red);
        Player blackPlayer = new Player(Colour.Black);
        Bar bar = new Bar();
        const int RED_DEAD_ZONE=0;
        const int BLACK_DEAD_ZONE = 25;
        bool isKilledSituationFound = false;
        int doubleDice = 0, lastDiceChoise, lastOriginZone;
        bool isEveryPieceIsInTheWinningZoneAndWinningDestination;
        public List<Label> ListOfLabels { get; set; }

        ResourceManager resourceManager = Resources.ResourceManager;

        public BoardForm()
        {
            InitializeComponent();
            InitializeBoardComponent();
            RefreshZones();
        }
        // Dice 1 click event
        private void Dice1Button_Click(object sender, EventArgs e)
        {
            lastDiceChoise = gameDices.Dice1;
            lastDiceChoiseBtn = (Button)sender;
        }
        // Dice 2 click event
        private void Dice2Button_Click(object sender, EventArgs e)
        {
            lastDiceChoise = gameDices.Dice2;
            lastDiceChoiseBtn = (Button)sender;
        }
        // Both dices click event
        private void DoubleDiceButton_Click(object sender, EventArgs e)
        {
            lastDiceChoise = gameDices.Dice1 + gameDices.Dice2;
            lastDiceChoiseBtn = (Button)sender;
        }
        // Randomizing the values of the dices and dispalying the dices images
        public void rollDices ()
        {
            Dices dices = new Dices();
            dices.RollDice();
            LoadDieImage(dices.Dice1, Dice1PictureBox);
            LoadDieImage(dices.Dice2, Dice2PictureBox);
            gameDices = dices;
        }
        // Roll dices button click even - does the rolling of the dices, managing the buttons, 
        // and swtiching player in case of 0 moves avaiiable and the double dice case.
        private void RollDiceButton_Click(object sender, EventArgs e)
        {
            rollDices();
            ButtonsEnabler();
            // if is there no optional moves after the dice roll -> switching players
            if (!isThereOptionalMoves(activePlayerFinder().PlayerColour,gameDices.Dice1,gameDices.Dice2))
            {
                if (!isThrereAWinner())
                {
                    MessageBox.Show("Your dices roll gave you 0 valid moves, Switching players!");
                    changePlayers();
                    return;
                }
            }
            ButtonsEnabler();
            RollDiceButton.Enabled = false;
            doubleDiceChecker();
            
        }
        // Checks if the dices are the same
        public void doubleDiceChecker ()
        {
            if (gameDices.DiceDouble)
            {
                doubleDice++;
            }
            else
            {
                doubleDice = 0;
            }
        }
        // Loads the correct dices images according to their values
        private void LoadDieImage(int firstDieNum, PictureBox diePictureBox)
        {
            Bitmap myImage = (Bitmap)resourceManager.GetObject("Dice" + firstDieNum);
            diePictureBox.Image = myImage;
        }
        // Where all the 'magic' happens -> Each zone click activates that function
        private void ZoneButton_Click(object sender, EventArgs e)
        {
            Player activePlayer = activePlayerFinder();
            int originZone = int.Parse((string)(((Label)sender).Tag));            
            int destinationZone = activePlayer.PlayerName == "Red" ? lastDiceChoise + originZone : originZone - lastDiceChoise;

            // Checks if the active player selected a dice or clicks without any chosen dices
            if (lastDiceChoise == -1)
            {
                MessageBox.Show("Please select a dice");
                return;
            }
            // Validated the validity of each requested move
            if (isLegalMove(activePlayer.PlayerColour,destinationZone,originZone))
            {
                // Validates if all the active player pieces are in the winning zone and the player tries to add a piece to the winning zone
                if (isEveryPieceIsInTheWinningZoneAndWinningDestination)
                {
                    addWinners(activePlayer, destinationZone);
                }
                // If the active player tires to kill an enemy player
                if (isKilledSituationFound)
                {
                    killPiece(activePlayer, destinationZone);
                }

                // Checks if every piece of the active player are in the winning zone and trying to 
                if (!isEveryPieceIsInTheWinningZoneAndWinningDestination)
                {
                    gameBoard.AddPieceToZone(destinationZone, activePlayer.PlayerColour);
                    RefreshZones(destinationZone);
                }

                gameBoard.RemovePieceFromZone(originZone);
                refreshAndRestartParams(originZone);

                // Checking if is there any Winner- if so the game ends here.
                if (isThrereAWinner())
                {
                    DialogResult dialog = MessageBox.Show(activePlayer.PlayerName + " player has won!!!", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dialog == DialogResult.OK)
                    {
                        this.Close();
                        return;
                    }
                }
                // Checking if is there any way to move with the other dices after the first pick
                if (!isThereOptionalMoves(activePlayerFinder().PlayerColour,gameDices.Dice1,gameDices.Dice2))
                {
                    if (!isThrereAWinner())
                    {
                        MessageBox.Show("You don't have any other possible moves, Switching Players");
                        changePlayers();
                    }
                }
                
            }
        }
        // Refreshing and restarting flags
        public void refreshAndRestartParams (int originZone)
        {
            RefreshZones(originZone);
            RefreshZones(int.Parse((string)labelBlackDead.Tag));
            RefreshZones(int.Parse((string)labelRedDead.Tag));
            refreshWinnginBar(Colour.Red, bar.BarWinningRedPiecesCount, labelRedWinnginZone);
            refreshWinnginBar(Colour.Black, bar.BarWinningBlackPiecesCount, labelBlackWinnginZone);
            dicesButtonDisabler();
            isKilledSituationFound = false;
            isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
            lastDiceChoise = -1;
        }
        // Adding a piece to the 'Dead' zone
        public void killPiece (Player activePlayer, int destinationZone)
        {
            gameBoard.RemovePieceFromZone(destinationZone);
            int deadZone = (activePlayer.PlayerColour == Colour.Red) ? BLACK_DEAD_ZONE : RED_DEAD_ZONE;
            Colour deadColor = (activePlayer.PlayerColour == Colour.Red) ? Colour.Black : Colour.Red;
            bar.AddDeadPieceToBar(deadColor);
            gameBoard.ZoneList[deadZone].ZonePiecesCount = ((activePlayer.PlayerColour == Colour.Red) ? bar.BarDeadBlackPiecesCount : bar.BarDeadRedPiecesCount);
            gameBoard.ZoneList[deadZone].ZoneColour = deadColor;
        }
        // Adding winners to the winning bar by player colour
        public void addWinners (Player activePlayer, int destinationZone)
        {
            if (activePlayer.PlayerColour == Colour.Red && destinationZone > 24)
            {
                bar.AddWinningPieceToBar(Colour.Red);
            }
            else if (activePlayer.PlayerColour == Colour.Black && destinationZone < 1)
            {
                bar.AddWinningPieceToBar(Colour.Black);
            }
        }
        // Controls the buttons activity
        public void dicesButtonDisabler ()
        {
            string tag = lastDiceChoiseBtn.Tag.ToString();
            // clicks on the double dice move
            if (tag=="3")
            {
                changePlayers();
                ButtonsEnabler(false);
                RollDiceButton.Enabled = true;
                // dices values are not same 
                if (doubleDice != 1)
                {
                    if (!isThrereAWinner())
                        MessageBox.Show("Switching players");
                }
                // dices values are the same
                else
                {
                    if (!isThrereAWinner())
                        MessageBox.Show("You rolled double! You won another move!");
                }
                lastOriginZone = -1;
            }
            // clicks on of the dices only 
            else
            {
                lastDiceChoiseBtn.Enabled = false;
                Dice3Button.Enabled = false;
                if (Dice1Button.Enabled == false && Dice2Button.Enabled == false)
                {
                    changePlayers();
                    ButtonsEnabler(false);
                    RollDiceButton.Enabled = true;
                    // dices values are not same 
                    if (doubleDice != 1)
                    {
                        if (!isThrereAWinner())
                            MessageBox.Show("Switching players");
                    }
                    // dices values are the same
                    else
                    {
                        if (!isThrereAWinner())
                            MessageBox.Show("You rolled double! You won another move!");
                    }
                    lastOriginZone = -1;
                }
            }
        }
        // Finds the active player 
        public Player activePlayerFinder ()
        {
            return ((redPlayer.PlayerIsActive) ? redPlayer : blackPlayer);
        }
        // Printing the pieces on each zone
        public string circleBuilder(uint counter)
        {
            string itemCounters = "";
            for (int ind = 0; ind < counter; ind++)
            {
                itemCounters = itemCounters + '⚫' + "\n";
            }
            return itemCounters;
        }
        // Print how many winners each player has
        public string WinnerBuilder(uint counter)
        {
            string itemCounters = counter+"";
            return itemCounters;
        }
        // Refresh the zones
        public void RefreshZones(int currentIndex = -1)
        {
            int firstIndex = 0;
            int lastIndex = gameBoard.ZoneList.Count - 1;
            if (currentIndex >= 0)
            {
                firstIndex = currentIndex;
                lastIndex = currentIndex;
            }
            else
            {
                firstIndex = 0;
            }
            for (int index = firstIndex; index <= lastIndex; index++)
            {
                uint pieceCount = gameBoard.ZoneList[index].ZonePiecesCount;
                Colour pieceColour = gameBoard.ZoneList[index].ZoneColour;
                string lbl = circleBuilder(pieceCount);
                Label lblZone = ListOfLabels[index];
                lblZone.Text = lbl;
                lblZone.ForeColor = (pieceColour == Colour.Black) ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 0, 0); /// change color 
                if (pieceCount >7 )
                {
                    Font font = new Font("Serif", 20, FontStyle.Bold);
                    //Microsoft Sans Serif, 27.75pt, style = Bold
                    lblZone.Font = font;
                    lblZone.Padding = new Padding(15,0,0,0);
                }
                else
                {
                    Font font = new Font("Serif", 28, FontStyle.Bold);
                    lblZone.Font = font;
                    lblZone.Padding = new Padding(10, 0, 0, 0);

                }

            }
            refreshWinnginBar(Colour.Red, 0, labelRedWinnginZone);
            refreshWinnginBar(Colour.Red, 0, labelBlackWinnginZone);
        }
        // Refresh the winning bar
        public void refreshWinnginBar(Colour pieceColour, uint counter, Label lblZone)
        {
            string lbl = WinnerBuilder(counter);
            lblZone.Text = lbl;
            lblZone.ForeColor = (pieceColour == Colour.Black) ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 0, 0); /// change colour 
        }
        // Initializing the board with all the pieces and all the components
        public void InitializeBoardComponent()
        {
            ListOfLabels = new List<Label>(new Label[26]);
            ListOfLabels[RED_DEAD_ZONE] = labelRedDead;
            ListOfLabels[1] = labelZone0;
            ListOfLabels[2] = labelZone1;
            ListOfLabels[3] = labelZone2;
            ListOfLabels[4] = labelZone3;
            ListOfLabels[5] = labelZone4;
            ListOfLabels[6] = labelZone5;
            ListOfLabels[7] = labelZone6;
            ListOfLabels[8] = labelZone7;
            ListOfLabels[9] = labelZone8;
            ListOfLabels[10] = labelZone9;
            ListOfLabels[11] = labelZone10;

            ListOfLabels[12] = labelZone11;
            ListOfLabels[13] = labelZone12;
            ListOfLabels[14] = labelZone13;
            ListOfLabels[15] = labelZone14;
            ListOfLabels[16] = labelZone15;
            ListOfLabels[17] = labelZone16;
            ListOfLabels[18] = labelZone17;
            ListOfLabels[19] = labelZone18;
            ListOfLabels[20] = labelZone19;
            ListOfLabels[21] = labelZone20;
            ListOfLabels[22] = labelZone21;
            ListOfLabels[23] = labelZone22;
            ListOfLabels[24] = labelZone23;
            ListOfLabels[BLACK_DEAD_ZONE] = labelBlackDead;

            LabelActivePlayer.Text = activePlayerFinder().PlayerName;
            Color colorPlayer = (activePlayerFinder().PlayerColour == Colour.Red) ? Color.Red : Color.Black;
            LabelActivePlayer.ForeColor = colorPlayer;
            labelCircle.ForeColor = colorPlayer;
            labelRedDead.Text = "";
            labelBlackDead.Text = "";

        }
        // Validate if the player has left and valid move after each move
        public bool isLegalMoveForLeftPieces(Colour movingPlayerColour, int destinationZone, int originZone)
        {
            isEveryPieceIsInTheWinningZoneAndWinningDestination = false;

            // Restarts the double dice flag if the double dice happens for the second time in a row
            if (doubleDice == 2)
            {
                doubleDice = 0;
            }

            // if every piece of the active player is in the winning zone and trying to reach a winning destination
            if (isEveryPieceIsInTheWinningZone(movingPlayerColour) ==true)
            {
                if ((movingPlayerColour == Colour.Red && destinationZone > 24 && bar.BarDeadRedPiecesCount == 0) ||
                    (movingPlayerColour == Colour.Black && destinationZone < 1 && bar.BarDeadRedPiecesCount == 0))
                {
                    isEveryPieceIsInTheWinningZoneAndWinningDestination = true;
                    return true;
                }
            }
            else
            {
                isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
            }

            // if the zone of the that we are trying to move is empty
            if (gameBoard.ZoneList[originZone].ZoneColour==Colour.Empty)
            {
                return false;
            }
            
            // if the player tries to move a wrong colour piece
            if (movingPlayerColour!= gameBoard.ZoneList[originZone].ZoneColour)
            {
                return false;
            }


            // If the destination zone is out of boundaries of the board - false
            if (!isEveryPieceIsInTheWinningZone(movingPlayerColour))
            {
                if (destinationZone > 24 || destinationZone < 1)
                {
                    return false;
                }
            }

            // If the destination zone has more than 1 enemy piece - false
            if (gameBoard.ZoneList[destinationZone].ZoneColour != movingPlayerColour)
            {
                if (gameBoard.ZoneList[destinationZone].ZonePiecesCount > 1)
                {
                    return false;
                }
            }
            // The direction of the move (0 - origin same as destination) (+1 foward for the Red player) (-1 forward for the black player)
            int direction = destinationZone - originZone;

            // If the destination zone is the same as the origin zone 
            if (direction == 0)
            {
                return false;
            }

            if (movingPlayerColour == Colour.Red)
            {
                // The Red player tries to move backwards
                if (direction < 0)
                {
                    return false;
                }
                // The Red player has dead pieces and he tries to move live pieces first
                if (bar.BarDeadRedPiecesCount>0)
                {
                    if (originZone != RED_DEAD_ZONE)
                    {
                        return false;
                    }
                }
            }
            else 
            {
                // The Black player tries to move backwards
                if (direction > 0)
                {
                    return false;
                }
                // The Black player has dead pieces and he tries to move live pieces first
                if (bar.BarDeadBlackPiecesCount > 0)
                {
                    if (originZone != BLACK_DEAD_ZONE)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        // Checks if the move is legal 
        public bool isLegalMove(Colour movingPlayerColour, int destinationZone, int originZone)
        {
            // checks if a double dice happen 2 times in a row
            if (doubleDice == 2)
            {
                doubleDice = 0;
            }
            // If every piece of the active player is in the winning zone and the destination zone is a winning zone 
            if (isEveryPieceIsInTheWinningZone(movingPlayerColour) == true)
            {
                //isEveryPieceIsInTheWinningZoneFlag = true;
                if ((movingPlayerColour == Colour.Red && destinationZone > 24 && bar.BarDeadRedPiecesCount==0) || 
                    (movingPlayerColour == Colour.Black && destinationZone < 1 && bar.BarDeadRedPiecesCount==0))
                {
                    isEveryPieceIsInTheWinningZoneAndWinningDestination = true;
                    return true;
                }
            }
            else
            {
                isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
            }

            // If the active player tries to move an empty cell
            if (gameBoard.ZoneList[originZone].ZoneColour == Colour.Empty)
            {
                MessageBox.Show("Invalid Move! You tried to move an empty cell, Please try again");
                return false;
            }
            if (movingPlayerColour != gameBoard.ZoneList[originZone].ZoneColour)
            {
                MessageBox.Show("Invalid Move! You tried to move the enemy pieces, Please try again");
                return false;
            }


            // If the destination zone is out of boundaries of the board - false
            if (!isEveryPieceIsInTheWinningZone(movingPlayerColour))
            {
                if (destinationZone > 24 || destinationZone < 1)
                {
                    MessageBox.Show("Invalid Move! You tried to move out of boundaries of the gaming board, Please try again");
                    return false;
                }
            }

            // If the destination zone has more than 1 enemy piece - false
            if (gameBoard.ZoneList[destinationZone].ZoneColour != movingPlayerColour)
            {
                if (gameBoard.ZoneList[destinationZone].ZonePiecesCount > 1)
                {
                    MessageBox.Show(string.Format("Invalid Move! The enemy pieces are blocking your destination zone, Please try again"));
                    return false;
                }
                else if (gameBoard.ZoneList[destinationZone].ZonePiecesCount == 1)
                {
                    isKilledSituationFound = true;
                }
            }
            // The direction of the move (0 - origin same as destination) (+1 foward for the Red player) (-1 forward for the black player)
            int direction = destinationZone - originZone;

            // If the destination zone is the same as the origin zone 
            if (direction == 0)
            {
                MessageBox.Show("Invalid Move! Please select a dice for your move and try again");
                return false;
            }

            if (movingPlayerColour == Colour.Red)
            {
                // The Red player tries to move backwards
                if (direction < 0)
                {
                    MessageBox.Show(string.Format("Invalid Move! You tried to move backwards, Please try again"));
                    return false;
                }
                // The Red player has dead pieces and he tries to move live pieces first
                if (bar.BarDeadRedPiecesCount > 0)
                {
                    if (originZone != RED_DEAD_ZONE)
                    {
                        MessageBox.Show("Invalid Move! You have dead pieces and you only move them, Please try again");
                        return false;
                    }
                    else
                    {
                        bar.RemoveDeadPieceFromBar(Colour.Red);
                    }
                }
            }
            else
            {
                // The Black player tries to move backwards
                if (direction > 0)
                {
                    MessageBox.Show(string.Format(("Invalid Move! You tried to move backwards, Please try again")));
                    return false;
                }
                // The Black player has dead pieces and he tries to move live pieces first
                if (bar.BarDeadBlackPiecesCount > 0)
                {
                    if (originZone != BLACK_DEAD_ZONE)
                    {
                        MessageBox.Show("Invalid Move! You have dead pieces and you only move them, Please try again");
                        return false;
                    }
                    else
                    {
                        bar.RemoveDeadPieceFromBar(Colour.Black);
                    }
                }
            }
            return true;
        }
        // The validation for each move to check if is there even 1 valid move
        public bool isThereOptionalMoves (Colour movingPlayerColour,int dice1, int dice2)
        {
            Player activePlayer = activePlayerFinder();
            bool doubleDiceSkipCheck=false;

            if ((gameDices.Dice1 == 0 && gameDices.Dice2 == 0))
                return true;

            if (doubleDice==1 && Dice1Button.Enabled == false && Dice2Button.Enabled == false)
            {
                doubleDiceSkipCheck = true;
                Dice1Button.Enabled = true;
                Dice2Button.Enabled = true;
            }
            // checks every piece of the active player and detemine if is there 1 valid move
            for (int originZone = 0; originZone < gameBoard.ZoneList.Count; originZone++)
            {
                if (gameBoard.ZoneList[originZone].ZoneColour==movingPlayerColour )
                {
                    int destinationZone1Dice = activePlayer.PlayerName == "Red" ? dice1 + originZone : originZone - dice1;
                    int destinationZone2Dice = activePlayer.PlayerName == "Red" ? dice2 + originZone : originZone - dice2;
                    int destinationZoneMixDice = activePlayer.PlayerName == "Red" ? dice1 + dice2 + originZone : originZone -(dice1 + dice2);

                    if (isLegalMoveForLeftPieces(movingPlayerColour, destinationZone1Dice, originZone) && Dice1Button.Enabled == true)
                    {
                        pramRestart();
                        /*isKilledSituationFound = false;
                        isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
                        if (doubleDiceSkipCheck)
                        {
                            Dice1Button.Enabled = false;
                            Dice2Button.Enabled = false;
                        }*/
                        return true;
                    }
                    if (isLegalMoveForLeftPieces(movingPlayerColour, destinationZone2Dice, originZone) && Dice2Button.Enabled == true)
                    {
                        pramRestart();
                        /*isKilledSituationFound = false;
                        isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
                        if (doubleDiceSkipCheck)
                        {
                            Dice1Button.Enabled = false;
                            Dice2Button.Enabled = false;
                        }*/
                        return true;
                    }
                    if (isLegalMoveForLeftPieces(movingPlayerColour, destinationZoneMixDice, originZone) && Dice3Button.Enabled == true)
                    {
                        pramRestart();
                        /*isKilledSituationFound = false;
                        isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
                        if (doubleDiceSkipCheck)
                        {
                            Dice1Button.Enabled = false;
                            Dice2Button.Enabled = false;
                        }*/
                        return true;
                    }
                }
                
            }
            // inner func to restart the values after double Dice is activated
            void pramRestart ()
            {
                isKilledSituationFound = false;
                isEveryPieceIsInTheWinningZoneAndWinningDestination = false;
                if (doubleDiceSkipCheck)
                {
                    Dice1Button.Enabled = false;
                    Dice2Button.Enabled = false;
                }
            }
            return false;
        }
        // Checks if is there a winner
        public bool isThrereAWinner ()
        {
            if (bar.BarWinningRedPiecesCount==15)
            {
                return true;
            }
            if (bar.BarWinningBlackPiecesCount==15)
            {
                return true;
            }
            return false;
        }
        // If every piece of a certain colour is in the winning zone (Red winning Zone : 1,2,3,4,5,6   Black winning Zone : 19,20,21,22,23,24)
        public bool isEveryPieceIsInTheWinningZone (Colour playerColour)
        {
            if (playerColour==Colour.Red)
            {
                for (int i = 0; i <= 18; i++)  // 19,20,21,22,23,24
                {
                    if (gameBoard.ZoneList[i].ZoneColour==Colour.Red)
                    {
                        return false;
                    }    
                }
                if (gameBoard.ZoneList[RED_DEAD_ZONE].ZonePiecesCount>0)
                {
                    return false;
                }
            }
            else if (playerColour==Colour.Black)
            {
                for (int i = 24; i > 6; i--)  // 1,2,3,4,5,6
                {
                    if (gameBoard.ZoneList[i].ZoneColour==Colour.Black)
                    {
                        return false;
                    }
                }
                if (gameBoard.ZoneList[BLACK_DEAD_ZONE].ZonePiecesCount > 0)
                {
                    return false;
                }
            }
            return true;
        }
        // Change the players + restarts the dices and buttons
        public void changePlayers()
        {
            // for double roll 5,5 1,1 2,2 3,3 4,4 6,6
            if (doubleDice != 1)
            {
                redPlayer.PlayerIsActive = !redPlayer.PlayerIsActive;
                blackPlayer.PlayerIsActive = !blackPlayer.PlayerIsActive;
                LabelActivePlayer.Text = activePlayerFinder().PlayerName;
                Color colorPlayer = (activePlayerFinder().PlayerColour == Colour.Red) ? Color.Red : Color.Black;
                LabelActivePlayer.ForeColor = colorPlayer;
                labelCircle.ForeColor = colorPlayer;
                gameDices.ResetDice1();
                gameDices.ResetDice2();
                ButtonsEnabler(true);
                Dice1Button.Enabled = false;
                Dice2Button.Enabled = false;
                Dice3Button.Enabled = false;
            }
        }
        // activate the buttons
        public void ButtonsEnabler(bool flag = true)
        {
            Dice1Button.Enabled = flag;
            Dice2Button.Enabled = flag;
            Dice3Button.Enabled = flag;
            RollDiceButton.Enabled = flag;
        }

    }
}
