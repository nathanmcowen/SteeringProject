using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SOFT152Steering;
using System.Windows.Forms;
using SOFT152SteeringLibrary;
using SteeringProject;

namespace SteeringProject
{
    public class Nest
    {
        /// <summary>
        /// Nest location
        /// </summary>
        public SOFT152Vector Location;

        /// <summary>
        /// Amount of food deposited
        /// </summary>
        public int foodDeposit = 0;

        public double radius = 10;

        /// <summary>
        /// Set location
        /// </summary>
        /// <param name="XLocation"></param>
        /// <param name="YLocation"></param>
        public Nest (int XLocation, int YLocation)
        {
            Location = new SOFT152Vector(XLocation, YLocation);            
        }
        public void AntArrived(AntAgent ant)
        {
            if (ant.HasFood)
            {
                // Nest FoodAmount in increased by 1
                foodDeposit += ant.FoodAmount;  
                //set has food flag to false
                ant.HasFood = false;
                // Food carried by AnyAgent is removed
                ant.FoodAmount = 0;                                     
            }            
        }
    }
}
