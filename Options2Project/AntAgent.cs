using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using SOFT152SteeringLibrary;
using SteeringProject;

namespace SOFT152Steering
{
    public class AntAgent
    {
        //flag to show if ant remembers food or not
        public bool RememberFood { get; set; }
        //location of food if the ant knows one
        public SOFT152Vector FoodLocation { get; set; }
        //flag to show if ant remembners food or not
        public bool RememberNest { get; set; }
        //location of nest if the ant knows one
        public SOFT152Vector NestLocation { get; set; }

        public bool HasFood { get; set; }

        public int FoodAmount = 0;
        /// <summary>
        /// The speed of the agent as used in all three movment methods 
        /// Ideal value depends on timer tick interval and realistic motion of
        /// agents needed. Suggest though in range 0 ... 2
        /// </summary>
        public double AgentSpeed { set; get; }  


        /// <summary>
        /// If the agent is using the the ApproachAgent() method, this property defines
        /// at what point the agent will reduce the speed of approach to miminic a 
        /// more relistic approach behaviour
        /// </summary>
        public double ApproachRadius { set; get; }    
        
        public double AvoidDistance { set; get; }      

        /// <summary>
        /// Property defines how 'random' the agent movement is whilst 
        /// the agent is using the Wander() method
        /// Suggest range of WanderLimits is 0 ... 1
        /// </summary>
        public double WanderLimits { set; get; }


        /// <summary>
        /// Used in conjunction worldBounds to determine if
        /// the agents position will stay within the world bounds 
        /// </summary>
        public bool ShouldStayInWorldBounds { set; get; }

        // --------------------------------------------
        // Private fields 

        /// <summary>
        /// Current postion of the agent, updated by the three
        /// movment methods
        /// </summary>
        private SOFT152Vector agentPosition;  

        /// <summary>
        /// used in conjunction with the Wander() method
        /// to detemin the next position an agent should be in 
        /// Should remain a private field and do not edit within this class
        /// </summary>
        private SOFT152Vector wanderPosition;


        /// <summary>
        /// The size of the world the agent lives on as a Rectangle object.
        /// Used in conjunction with ShouldStayInWorldBounds, which if true
        /// will mean the agents position will be kept within the world bounds 
        /// (i.e. the  world width or the world height)
        /// </summary>
        private Rectangle worldBounds;   // To keep track of the obejcts bounds i.e. ViewPort dimensions

        /// <summary>
        /// The random object passed to the agent. 
        /// Used only in the Wander() method to generate a 
        /// random direction to move in
        /// </summary>
        private Random randomNumberGenerator;              // random number used for wandering



        public AntAgent(SOFT152Vector position, Random random)
        {
           agentPosition = new SOFT152Vector(position.X, position.Y);

            randomNumberGenerator = random;

            InitialiseAgent();
        }

        public AntAgent(SOFT152Vector position, Random random, Rectangle bounds )
        {
            agentPosition = new SOFT152Vector(position.X, position.Y);

            worldBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            randomNumberGenerator = random;

            InitialiseAgent();
        }

        /// <summary>
        /// Initialises the Agents various fields
        /// with default values
        /// </summary>
        private void InitialiseAgent()
        {
            wanderPosition = new SOFT152Vector();

            ApproachRadius = 7;

            AvoidDistance = 25;

            AgentSpeed = 4.0;

            ShouldStayInWorldBounds = true;

            WanderLimits = 0.5;
        }

        /// <summary>
        /// Causes the agent to make one step towards the object at objectPosition
        /// The speed of approach will reduce one this agent is within
        /// an ApproachRadius of the objectPosition
        /// </summary>
        /// <param name="agentToApproach"></param>
        public void Approach(SOFT152Vector objectPosition)
        {
            Steering.MoveTo(agentPosition, objectPosition, AgentSpeed, ApproachRadius);

            StayInWorld();
        }

        /// <summary>
        /// Causes the agent to make one step away from  the objectPosition
        /// The speed of avoid is goverened by this agents speed
        /// </summary>
        public void FleeFrom(SOFT152Vector objectPosition)
        {

            Steering.MoveFrom(agentPosition, objectPosition, AgentSpeed, AvoidDistance);

            StayInWorld();
        }

        /// <summary>
        /// Causes the agent to make one random step.
        /// The size of the step determined by the value of WanderLimits
        /// and the agents speed
        /// </summary>
        public void Wander()
        {
            Steering.Wander(agentPosition, wanderPosition, WanderLimits, AgentSpeed, randomNumberGenerator);

           StayInWorld();
        }
  
        private void StayInWorld()
        {
            // if the agent should stay with in the world
            if (ShouldStayInWorldBounds == true)
            {
                // and the world has a positive width and height
                if (worldBounds.Width >= 0 && worldBounds.Height >= 0)
                {
                    // now adjust the agents position if outside the limits of the world
                    if (agentPosition.X < 0)
                        agentPosition.X = worldBounds.Width;

                    else if (agentPosition.X > worldBounds.Width)
                        agentPosition.X = 0;

                    if (agentPosition.Y < 0)
                        agentPosition.Y = worldBounds.Height;

                    else if (AgentPosition.Y > worldBounds.Height)
                        agentPosition.Y = 0;
                }
            }
        }
        
        
        public bool IsAntInRadius(Nest nest)
        {
        //find the location of a Nest compared to the location of an AntAgent
            //location of nest
            SOFT152Vector nestLocation = nest.Location;
            //nest X coordinate
            double distX = agentPosition.X - nestLocation.X;
            //nest Y coordinate
            double distY = agentPosition.Y - nestLocation.Y;

            //if the distance between the AntAgent and a Nest is less than the radius
            if (distX <= nest.radius && distX > -nest.radius && distY <= nest.radius && distY > -nest.radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAntInRadius(FoodPile food)
        {
        //find the location of a FoodPile compared to the location of an AntAgent
            //location of food
            SOFT152Vector foodLocation = food.Location;
            //food X coordinate
            double distX = agentPosition.X - foodLocation.X;
            //food Y coordinate
            double distY = agentPosition.Y - foodLocation.Y;

            //if the distance between the AntAgent and the FoodPile is less than the radius
            if (distX <= food.radius && distX > -food.radius && distY <= food.radius && distY > -food.radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAntInRadius(AntAgent selectedAnt)     
        {
            //find the location of current ant compared to the location of the selected ant
            //location of selected ant
            SOFT152Vector selectedAntLocation = selectedAnt.agentPosition;
            //store X coordinate after calculation 
            double distX = agentPosition.X - selectedAntLocation.X;
            //store Y coordinate after calculation 
            double distY = agentPosition.Y - selectedAntLocation.Y;

            //if the distance between the current ant and the selected ant is less than the radius
            if (distX <= selectedAnt.ApproachRadius && distX > -selectedAnt.ApproachRadius && distY <= selectedAnt.ApproachRadius && distY > -selectedAnt.ApproachRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public SOFT152Vector AgentPosition
        {
            set
            {               
                agentPosition = value;
            }

            get
            {
                return agentPosition;
            }
        } 
        //method to share memory when selected ant is in range of another ant
        public void ShareMemory(AntAgent CurrentAnt, AntAgent OtherAnt)
        {
            //check if current ant has memory of food and other ant does not
            if (CurrentAnt.RememberFood && !OtherAnt.RememberFood)
            {                
                //set other ant rememberfood flag to true;
                OtherAnt.RememberFood = true;
                //set other ant food location memory to current ant food location memory
                OtherAnt.FoodLocation = CurrentAnt.FoodLocation;
            }
            //check if current ant has memory of nest and other ant does not
            if (CurrentAnt.RememberNest && !OtherAnt.RememberNest)
            {
                //set other ant rememberNest flag to true
                OtherAnt.RememberNest = true;
                //set other ant nest location memory to current ant nest location memory
                OtherAnt.NestLocation = CurrentAnt.NestLocation;
            }
            //check if other ant has memory of food and current ant does not
            if (OtherAnt.RememberFood && !CurrentAnt.RememberFood)
            {
                //set current ant rememberFood flaf to true
                CurrentAnt.RememberFood = true;
                //set current ant food location memory to other ant food location memory
                CurrentAnt.FoodLocation = OtherAnt.FoodLocation;
            }
            //check if other ant has memory of nest and current ant does not
            if (OtherAnt.RememberNest && !CurrentAnt.RememberNest)
            {
                //set current ant rememberNest flag to true
                CurrentAnt.RememberNest = true;
                //set current ant nest location memory to other ant nest location memory
                CurrentAnt.NestLocation = OtherAnt.NestLocation;
            }
        }

    }  // end class AntAgent
}
