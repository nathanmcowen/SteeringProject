using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteeringProject;
using SOFT152SteeringLibrary;

namespace SOFT152Steering
{
    public partial class AntFoodForm : Form
    {
        // number of AntAgents that will be drawn
        static int numberOfAnts = 200;
        // Creates a list to store Nest objects created by user
        List<Nest> listOfNests = new List<Nest>();
        // Creates a list to store FoodPiles created by user
        List<FoodPile> listOfFoodPiles = new List<FoodPile>();
        // create a list to store deleted FoodPiles that have been deleted
        List<FoodPile> listOfDeletedFoodPiles = new List<FoodPile>();

        // Create list of Horizontal Lines
        //List<H_Line> ListOfHLines = new List<H_Line>();
        // Create list of Vertical Lines
        //List<V_Line> ListOfVLines = new List<V_Line>();

        // Creates an array to store AntAgents
        AntAgent[] Ants = new AntAgent[numberOfAnts];
        //threshold to reach for ant to forget memory
        int memoryThreshold = 600;
        // A timer that will begin to count when AntAgent is drawn
        int memoryTimer = 0;
        //ID for current ant
        int antID = 0;
        // the random object given to each Ant agent
        private Random randomGenerator;
        // A bitmap image used for double buffering
        private Bitmap backgroundImage;                                 
        // flag that will be set to determine if food pile or nest is to be added
        private string toAdd = "";                                      

        public AntFoodForm()
        {        
            InitializeComponent();
            //create back ground image
            CreateBackgroundImage();
            //create ants
            CreateAnts();                       
        }

        private void CreateAnts()
        {            
            Rectangle worldLimits;
            // create a random object to pass to the ants
            randomGenerator = new Random();                             

            // define some world size for the ants to move around on
            // assume the size of the world is the same size as the panel on which they are displayed
            worldLimits = new Rectangle(0, 0, drawingPanel.Width, drawingPanel.Height);

            // create all ant in the array
            for (int i = 0; i < numberOfAnts; i++)                      
            {
                Ants[i] = new AntAgent(new SOFT152Vector(100, 150), randomGenerator, worldLimits);
            }         
        }
        // Creates the background image to be used in double buffering
        private void CreateBackgroundImage()                             
        {
            int imageWidth;
            int imageHeight;

            // the backgroundImage can be any size. Assume it is the same size as the panel on which the AntAgents are drawn
            imageWidth = drawingPanel.Width;
            imageHeight = drawingPanel.Height;

            backgroundImage = new Bitmap(drawingPanel.Width, drawingPanel.Height);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            memoryTimer += 1;            

            if (memoryTimer == memoryThreshold)
            {
                MakeAntForgetMemory();
                memoryTimer = 0;
            }
            // a Foreach loop through the array of ants that have been created.
            foreach (AntAgent ant in Ants)
            {                
                // set AgentSpeed for AntAgent before it moves
                ant.AgentSpeed = 5;
                // set WanderLimt for AntAgent before it moves
                ant.WanderLimits = 0.25;
                // keep the agent within WorldBounds
                ant.ShouldStayInWorldBounds = true;

                // ----- decide if ant should wander -----

                // If AntAgent has FoodPile and Nest in memory and If ant has food, approach known Nest location
                if (ant.RememberFood == true && ant.RememberNest == true && ant.HasFood == true)
                {
                    ant.Approach(ant.NestLocation);
                }

                // If AntAgent has FoodPile and Nest in memory but doesn't have food, approach known FoodPile location
                else if (ant.RememberFood == true && ant.RememberNest == true && ant.HasFood == false)
                {
                    ant.Approach(ant.FoodLocation);
                }
                else
                {
                    //if the ant has forgetten food location or food has run out - but the ant has food and still knows the nest location THEN
                    if (ant.HasFood == true && ant.RememberNest == true)
                    {
                        //approach nest in memory
                        ant.Approach(ant.NestLocation);
                    }
                    else//OR
                    {
                        //ant is to wander
                        ant.Wander();

                    }
                }

                //loop through all nests that are contained within the nest list.
                for (int n = 0; n < listOfNests.Count; n++)
                {
                    // Checks to see if the AntAgent knows location of Nest.
                    if (ant.IsAntInRadius(listOfNests[n]) == true)
                    {
                        //does ant already have a nest location?
                        if (ant.RememberNest == true)
                        {
                            //storing nest location from memory
                            SOFT152Vector memory = ant.NestLocation;
                            //does memory match the nest the ant is near
                            if (memory == listOfNests[n].Location)
                            {
                                //store has food flag from ant
                                bool hasFood = ant.HasFood;
                                //does ant have food?
                                if (hasFood)
                                {
                                    //call ant arrived method and pass in the ant details
                                    listOfNests[n].AntArrived(ant);
                                }
                            }
                        }
                        //reached if any does not have any stored memory for a nest
                        else
                        {
                            //change remember nest flag of ant to true
                            ant.RememberNest = true;
                            //store nest location in ant memory
                            ant.NestLocation = listOfNests[n].Location;
                        }
                    }
                }                
                //loop through all food piles that are contained within the food pile list.
                for (int f = 0; f < listOfFoodPiles.Count; f++)
                {                    
                    // Checks to see if the AntAgent knows location of FoodPile.
                    if (ant.IsAntInRadius(listOfFoodPiles[f]) == true)
                    {
                        //does ant already have a food pile location? 
                        if (ant.RememberFood == true)
                        {
                            //store food location from memory of the ant
                            SOFT152Vector memory = ant.FoodLocation;
                            //does the memory match the food pile that is near the ant?
                            if (memory == listOfFoodPiles[f].Location)
                            {
                                //store hasfood flag
                                bool hasFood = ant.HasFood;
                                //if ant doesnt have food
                                if (!hasFood)
                                {
                                    //call ant arrived method and pass in the food pile and the ant in question
                                    listOfFoodPiles[f].AntArrived(listOfFoodPiles[f], ant);
                                    //call AnyFoodRemaining method to check if the food pile has any food left
                                    if (listOfFoodPiles[f].AnyFoodRemaining() == false)
                                    {
                                        //store the location of the foodpile
                                        FoodPile tempFood = listOfFoodPiles[f];                                         
                                        //removing foodpile from the list as there is no food remaining
                                        listOfFoodPiles.Remove(listOfFoodPiles[f]);
                                        //add tempFood to deleted food pile list
                                        listOfDeletedFoodPiles.Add(tempFood);
                                        //set ant food location to null
                                        ant.FoodLocation = null;
                                        //set remember food flag to false
                                        ant.RememberFood = false;
                                    }
                                }
                            }
                        }
                        //reached if ant is in range and doesnt have a food pile in memory
                        else
                        {
                            //set remember food flag to true
                            ant.RememberFood = true;
                            //store location of food pile in ant memory
                            ant.FoodLocation = listOfFoodPiles[f].Location;
                        }

                    }
                }
                //loop through all ants in ant array
                for (int c = 0; c < Ants.Length; c++)
                {
                    //check that current ant is not itself 
                    if (antID != c)
                    {
                        //check if current ant is in radius of selected ant
                        if (ant.IsAntInRadius(Ants[c]) == true)
                        {
                            //create hasFoodRunOut flag
                            bool hasFoodRunOut = false;
                            //loop through all deleted food piles
                            for (int deleted = 0; deleted < listOfDeletedFoodPiles.Count; deleted++)
                            {
                                //check if ant is in readius with a deleted food pile
                                if (ant.IsAntInRadius(listOfDeletedFoodPiles[deleted]) == true)
                                {
                                    //set hasFoodRunOut flag to true
                                    hasFoodRunOut = true;
                                    //set rememberFood flag to false
                                    ant.RememberFood = false;
                                    //remove food location from ants memory
                                    ant.FoodLocation = null;
                                }                                
                            }
                            //check if the food in ants memory has run out
                            if (!hasFoodRunOut)
                            {
                                //share memory with selected ant
                                ant.ShareMemory(ant, Ants[c]);
                            }
                        }
                    }
                }
                //increment the ant ID by 1
                antID += 1;
                //check if antID exceeds number of ants
                if (antID > numberOfAnts)
                {
                    //set antID back to 0
                    antID = 0;
                }
            }

            DrawAgentsDoubleBuffering();
        }
        //method to make a random ant forget its memory
        private void MakeAntForgetMemory()
        {
            //retrieve random number to use to select an ant
            int randomAntNumber = GetRandomNumber(0, numberOfAnts);
            //retrieve random number to use to determine if nest or food location will be forgotton
            int randomLocationToForget = GetRandomNumber(1, 2);
            //select random ant from ant array using randomAntNumber
            AntAgent selectedAnt = Ants[randomAntNumber];   
            //check if random number is 1
            if (randomLocationToForget == 1) //1 == nest and 2 == food
            {
                //check if ant has memory for nest
                if (selectedAnt.RememberNest == true)
                {
                    //make ant forget nest in memory 

                    //set rememberNest flag to false
                    selectedAnt.RememberNest = false;
                    //remove nest location from ant memory
                    selectedAnt.NestLocation = null;
                }
                //if ant does not have memory of a nest, check to see if it has memory of a food pile
                else if (selectedAnt.RememberFood == true)
                {
                    //make ant forget food pile in history

                    //set rememberFood flag to false
                    selectedAnt.RememberFood = false;
                    //remove food location from ant memory
                    selectedAnt.FoodLocation = null;
                }
            }
            else //reached if random number = 2(food)
            {
                //check if ant has memory for food
                if (selectedAnt.RememberFood == true)
                {
                    //make ant forget food pile in memory 

                    //set rememberFood flag to false
                    selectedAnt.RememberFood = false;
                    //remove food pile location from ant memory
                    selectedAnt.FoodLocation = null;
                }
                //if ant does not have memory of a food pile, check to see if it has memory of a nest
                else if (selectedAnt.RememberNest == true)
                {
                    //make ant forget nest in history

                    //set rememberNest flag to false
                    selectedAnt.RememberNest = false;
                    //remove nest location from ant memory
                    selectedAnt.NestLocation = null;
                }
            }
        }


        /// <summary>
        /// Draws the ants and any stationary objects using double buffering
        /// </summary>
        private void DrawAgentsDoubleBuffering()
        {            
            // some arbitary size to draw the Ant
            float antSize;

            antSize = 3.0f;

            Brush solidBrush;

            // get the graphics context of the background image
            using (Graphics backgroundGraphics =  Graphics.FromImage(backgroundImage))
            {
                //clear background graphics and change to the color white.
                backgroundGraphics.Clear(Color.LightSlateGray);
                //create a new brush of color black
                solidBrush = new SolidBrush(Color.Black);
                foreach (AntAgent ant in Ants)
                {        
                    //draw ants
                    backgroundGraphics.FillRectangle(solidBrush, (float)ant.AgentPosition.X, (float)ant.AgentPosition.Y, antSize, antSize);
                }
                //draw each nest that has been placed by the user from the nest list            
                foreach (Nest n in listOfNests)                             
                {
                    //create a new brush of color grey
                    solidBrush = new SolidBrush(Color.Orange);
                    //draw nests
                    backgroundGraphics.FillRectangle(solidBrush, (float)n.Location.X - 10, (float)n.Location.Y - 10, 20, 20);
                }
                //draw each food pile that has been placed by the user from the food pile list
                foreach (FoodPile food in listOfFoodPiles)                  
                {
                    //create a new brush of color red
                    solidBrush = new SolidBrush(Color.Red);
                    //draws food piles
                    backgroundGraphics.FillRectangle(solidBrush, (float)food.Location.X - 5, (float)food.Location.Y -5, 10, 10);
                }
            }
            // now draw the image on the panel
            using (Graphics g = drawingPanel.CreateGraphics())              
            {
                g.DrawImage(backgroundImage, 0, 0, drawingPanel.Width, drawingPanel.Height);
            }
            // dispose of resources
            solidBrush.Dispose();                                       
        }



        private void stopButton_Click(object sender, EventArgs e)
        {
            //stop timer
            timer.Stop();
        }

        private void startButton_Click(object sender, EventArgs e)
        {                        
            //start timer
            timer.Start();            
        }
        // called when add nest button is clicked
        private void btnAddNest_Click(object sender, EventArgs e)           
        {
            //set toAdd flag to "nest"
            toAdd = "nest";
            //disable addNest button
            btnAddNest.Enabled = false;
            //check if addFood button is disabled
            if (btnAddFood.Enabled == false)
            {
                //enable addFood button
                btnAddFood.Enabled = true;
            }
        }
        // When Nest button is clicked, disable FoodPile button
        private void btnAddFood_Click(object sender, EventArgs e)           
        {
            //set toAdd flag to "food"
            toAdd = "food";
            //disable addFood button
            btnAddFood.Enabled = false;
            //check is addNest button is disabled
            if (btnAddNest.Enabled == false)
            {
                //enable addNest button
                btnAddNest.Enabled = true;
            }
        }


        // When Nest button is clicked, disable FoodPile button
        private void btn_HLine_Click(object sender, EventArgs e)
        {
            //set toAdd flag to "food"
            toAdd = "food";
            //disable addFood button
            btnAddFood.Enabled = false;
            //check is addNest button is disabled
            if (btnAddNest.Enabled == false)
            {
                //enable addNest button
                btnAddNest.Enabled = true;
            }
        }


        //event called when user clicks on the drawing panel
        private void drawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            //store click location
            Point clickLocation = drawingPanel.PointToClient(Cursor.Position);            
            // check whether to add a Nest or a FoodPile
            if (toAdd == "nest")
            {
                // Creating new Nest and store it in listOfNests
                listOfNests.Add(new Nest(clickLocation.X, clickLocation.Y));            
            }  
            if (toAdd == "food")
            {
                // Create new FoodPile and store it in listOfFoodPiles
                listOfFoodPiles.Add(new FoodPile(clickLocation.X, clickLocation.Y));    
            }
            
        }
        //method to get a random number
        public int GetRandomNumber(int min, int max)
        {
            //create a random
            Random number = new Random();
            //store random number with a min and max value we pass to it
            int random = number.Next(min, max);
            //return random number
            return random;
        }
    }
}
