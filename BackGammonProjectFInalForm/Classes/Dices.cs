using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackGammonProject
{
    public class Dices
    {
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }
        public bool DiceDouble { get; set; }

        private Random random = new Random();
        
        // Dices Roll
        public void RollDice ()
        {
            Dice1 = random.Next(1, 7);
            Dice2 = random.Next(1, 7);

            //Dice1 = 4;
            //Dice2 = 4;
            if (Dice1 == Dice2)
                DiceDouble = true;
            else
                DiceDouble = false;
        }
        // Reset Dice1
        public void ResetDice1()
        {
            Dice1 = 0; 
        }
        // Reset Dice2
        public void ResetDice2()
        { 
            Dice2 = 0; 
        }

    }

}
