using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SOFT152SteeringLibrary;
using SOFT152Steering;
using SteeringProject;

namespace SteeringProject
{
    public class FoodPile
    {
        /// <summary>
        /// Nest location
        /// </summary>
        public SOFT152Vector Location;

        /// <summary>
        /// Amount of food available in FoodPile.
        /// </summary>
        public int foodAvailable = 200;

        public double radius = 10;

        /// <summary>
        /// Set location
        /// </summary>
        /// <param name="XLocation"></param>
        /// <param name="YLocation"></param>
        public FoodPile(int XLocation, int YLocation)
        {
            //set location of food pile
            Location = new SOFT152Vector(XLocation, YLocation);
        }
        public void AntArrived(FoodPile food, AntAgent ant)
        {                 

            if (!ant.HasFood)
            {
                // FoodPile is decreased by 1
                foodAvailable -= 1;  
                //set has food flag to true
                ant.HasFood = true;
                // AntAgent is carrying 1 unit of food
                ant.FoodAmount = 1;                                 
            }          
        }
        //check if there is any food left in the food pile
        public bool AnyFoodRemaining()
        {
            if (foodAvailable > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
