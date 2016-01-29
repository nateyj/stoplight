using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace StopLightTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stoplight LightOne = new Stoplight("One");
        Stoplight LightTwo = new Stoplight("Two");
        Stoplight LightThree = new Stoplight("Three");
        Stoplight LightFour = new Stoplight("Four");
        
        System.Windows.Threading.DispatcherTimer fiveSecTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer thirtySecTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer oneSecTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer threeSecTimer = new System.Windows.Threading.DispatcherTimer();
        
        int iOneSecTimerCount = 1;
        bool bIs30SecProcessRunning;

        SolidColorBrush red = new SolidColorBrush(Colors.Red);
        SolidColorBrush green = new SolidColorBrush(Colors.Green);
        SolidColorBrush yellow = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush black = new SolidColorBrush(Colors.Black);

        Queue<Stoplight> qNextLightUp = new Queue<Stoplight>();

        public MainWindow()
        {      
            InitializeComponent();

            threeSecTimer.Tick += new EventHandler(threeSecTimer_Tick);
            threeSecTimer.Interval = new TimeSpan(0, 0, 3);

            fiveSecTimer.Tick += new EventHandler(fiveSecTimer_Tick);
            fiveSecTimer.Interval = new TimeSpan(0, 0, 5);
            fiveSecTimer.Start();    
            
            thirtySecTimer.Tick += new EventHandler(thirtySecTimer_Tick);
            thirtySecTimer.Interval = new TimeSpan(0, 0, 15);

            oneSecTimer.Tick += new EventHandler(oneSecTimer_Tick);
            oneSecTimer.Interval = new TimeSpan(0, 0, 1);
            oneSecTimer.Start();               
        }
        
        private void oneSecTimer_Tick(object sender, EventArgs e)
        {
            iOneSecTimerCount++;
        }

        //changes the yellow lights to red and turns on appropriate lights to green
        private void threeSecTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(iOneSecTimerCount);
            threeSecTimer.Stop();   //stops 3-sec timer

            //if there is a light in the queue, change yellow lights to red and red lights to green accordingly
            //if there are no more lights in the queue to change to green, then go back to default
            if (qNextLightUp.Count != 0)
            {
                if (qNextLightUp.Peek() == LightOne)
                    RedToGreenLightOne();
                else if (qNextLightUp.Peek() == LightTwo || qNextLightUp.Peek() == LightFour)
                    RedToGreenLightTwoAndFour();
                else if (qNextLightUp.Peek() == LightThree)
                    RedToGreenLightThree();
            }
            else
                RedToGreenLightThreeDefault();
        }

        private void fiveSecTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(iOneSecTimerCount);

            //find which lights are green
            //for those lights that are green, check to see if cars are still triggering the sensor at those lights (a.k.a. still passing through
            //  the intersection)
            if (OneGreen.Fill.ToString().Equals(green.ToString()))
            {
                //if no more cars are passing through in the past 5 seconds on the current green light, then change the lights
                if (LightOne.hasSensorTriggered == false)
                    TurnYellow();

                LightOne.hasSensorTriggered = false;
            }
            else if (TwoGreen.Fill.ToString().Equals(green.ToString()) || FourGreen.Fill.ToString().Equals(green.ToString()))
            {
                //both sensors for lights two and four have to be false, because if only one is false, then it means that a car is still
                //passing through the other light from the other direction. Lights two and four are always green together
                if (LightTwo.hasSensorTriggered == false && LightFour.hasSensorTriggered == false)
                    TurnYellow();

                //changes sensors to false so that they can keep track if more cars go through the lights in the next 30 seconds
                LightTwo.hasSensorTriggered = false;
                LightFour.hasSensorTriggered = false;
            }
            else if(ThreeGreen.Fill.ToString().Equals(green.ToString()))
            {               
                //if no more cars are passing through in the past 5 seconds on the current green light, then change the lights
                if (LightThree.hasSensorTriggered == false)
                {
                    //if there are lights in the queue, then change lights to yellow
                    //if there aren't any lights in the queue, doesn't do anything and stays default
                    if (qNextLightUp.Count != 0)
                        TurnYellow(); 
                }
                                    
                
                //changes sensor to false so that it can keep track if more cars go through the light in the next 30 seconds
                LightThree.hasSensorTriggered = false;
            }
        }

        //if thirty seconds ever elapses after the thirty-second timer starts, then it will change the lights it needs to based on which light started the thirty-second timer
        private void thirtySecTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(iOneSecTimerCount);
            TurnYellow();
            Console.WriteLine("Lights changed by default after 30 sec");
        }

        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            LightOne.hasSensorTriggered = true; //light one's sensor has been triggered

            //only put this light next on the queue if it's red
            //otherwise, it's green or yellow when the sensor is triggered, the car is passing through the light
            if (OneRed.Fill.ToString().Equals(red.ToString()))
                EnqueueLight(LightOne);

            //if there are no 30-sec processes running, start the 30-sec process with next light in queue
            if (bIs30SecProcessRunning == false)
                Start30SecProcess();
        }

        private void btnTwo_Click(object sender, RoutedEventArgs e)
        {
            LightTwo.hasSensorTriggered = true; //light two's sensor has been triggered

            //only put this light next on the queue if it's red
            //otherwise, it's green or yellow when the sensor is triggered, the car is passing through the light
            if (TwoRed.Fill.ToString().Equals(red.ToString()))
                EnqueueLight(LightTwo);

            //if there are no 30-sec processes running, start the 30-sec process with next light in queue
            if (bIs30SecProcessRunning == false)
                Start30SecProcess();
        }

        private void btnThree_Click(object sender, RoutedEventArgs e)
        {
            LightThree.hasSensorTriggered = true;   //light three's sensor has been triggered

            //only put this light next on the queue if it's red
            //otherwise, it's green or yellow when the sensor is triggered, the car is passing through the light
            if (ThreeRed.Fill.ToString().Equals(red.ToString()))
                EnqueueLight(LightThree);

            //if there are no 30-sec processes running, start the 30-sec process with next light in queue
            if (bIs30SecProcessRunning == false)
                Start30SecProcess();
        }

        private void btnFour_Click(object sender, RoutedEventArgs e)
        {
            //light four's sensor has been triggered
            LightFour.hasSensorTriggered = true;

            //only put this light next on the queue if it's red
            //otherwise, it's green or yellow when the sensor is triggered, the car is passing through the light
            if (FourRed.Fill.ToString().Equals(red.ToString()))
                EnqueueLight(LightFour);

            //if there are no 30-sec processes running, start the 30-sec process with next light in queue
            if (bIs30SecProcessRunning == false)
                Start30SecProcess();
        }

        //changes light one to green and everything else to red, including light four's right turn arrow
        private void RedToGreenLightOne()
        {
            //light one turns green
            OneGreen.Fill = green;
            OneYellow.Fill = black;
            OneRed.Fill = black;
            
            //light two turns red
            TwoGreen.Fill = black;
            TwoYellow.Fill = black;
            TwoRed.Fill = red;

            //light three turns red
            ThreeGreen.Fill = black;
            ThreeYellow.Fill = black;
            ThreeRed.Fill = red;

            //light four and its turn arrow turn red
            FourGreen.Fill = black;
            FourYellow.Fill = black;
            FourRed.Fill = red;
            FourRight.Fill = black;

            LightOne.hasSensorTriggered = false;    //resets sensor
            Console.WriteLine("Light one has changed to green.");

            End30SecProcess();
            Start30SecProcess();  //starts 30-sec process
        }

        //changes lights two and four to green (along with light four's right turn arrow) and lights one and three to red
        private void RedToGreenLightTwoAndFour()
        {
            //light one turns red
            OneGreen.Fill = black;
            OneYellow.Fill = black;
            OneRed.Fill = red;

            //light two turns green
            TwoGreen.Fill = green;
            TwoYellow.Fill = black;
            TwoRed.Fill = black;

            //light three turns red
            ThreeGreen.Fill = black;
            ThreeYellow.Fill = black;
            ThreeRed.Fill = red;

            //light four and its turn arrow turn green
            FourGreen.Fill = green;
            FourYellow.Fill = black;
            FourRed.Fill = black;
            FourRight.Fill = green;

            //resets sensors
            LightTwo.hasSensorTriggered = false;
            LightFour.hasSensorTriggered = false; 
            Console.WriteLine("Lights two and four have changed to green.");

            End30SecProcess();
            Start30SecProcess();  //starts 30-sec process
        }

        //changes light three and light four's right turn arrow to green, everything else to red
        private void RedToGreenLightThree()
        {
            //light one turns red
            OneGreen.Fill = black;
            OneYellow.Fill = black;
            OneRed.Fill = red;

            //light two turns red
            TwoGreen.Fill = black;
            TwoYellow.Fill = black;
            TwoRed.Fill = red;

            //light three turns green
            ThreeGreen.Fill = green;
            ThreeYellow.Fill = black;
            ThreeRed.Fill = black;

            //light four turns red, its right arrow turns green
            FourGreen.Fill = black;
            FourYellow.Fill = black;
            FourRed.Fill = red;
            FourRight.Fill = green;

            LightThree.hasSensorTriggered = false;    //resets sensor
            Console.WriteLine("Light three has changed to green.");

            End30SecProcess();
            Start30SecProcess();  //starts 30-sec process
        }

        //changes light three and light four's right turn arrow to green, everything else to red
        //omits using the Start30SecProcess and doesn't reset light three's sensor, because a sensor didn't trigger this process
        private void RedToGreenLightThreeDefault()
        {
            //light one goes red
            OneGreen.Fill = black;
            OneYellow.Fill = black;
            OneRed.Fill = red;

            //light two goes red
            TwoGreen.Fill = black;
            TwoYellow.Fill = black;
            TwoRed.Fill = red;

            //light three goes green
            ThreeGreen.Fill = green;
            ThreeYellow.Fill = black;
            ThreeRed.Fill = black;

            //light four goes red, right arrow goes green
            FourGreen.Fill = black;
            FourYellow.Fill = black;
            FourRed.Fill = red;
            FourRight.Fill = green;

            Console.WriteLine("Light three has changed to green by default.");

            End30SecProcess();
        }

        //changes light one to yellow
        private void GreenToYellowLightOne()
        {
            //light one turns yellow
            OneGreen.Fill = black;
            OneYellow.Fill = yellow;
            OneRed.Fill = black;
        }

        //changes lights two and four to yellow
        private void GreenToYellowLightTwoAndFour()
        {
            //light two turns yellow
            TwoGreen.Fill = black;
            TwoYellow.Fill = yellow;
            TwoRed.Fill = black;

            //light four turns yellow
            FourGreen.Fill = black;
            FourYellow.Fill = yellow;
            FourRed.Fill = black;           
        }

        //changes light three to yellow
        private void GreenToYellowLightThree()
        {
            //light one turns yellow
            ThreeGreen.Fill = black;
            ThreeYellow.Fill = yellow;
            ThreeRed.Fill = black;
        }

        //changes the lights based on who's next in the queue
        private void TurnYellow()
        {
            threeSecTimer.Start();  //start 3-sec timer for how long light will be yellow

            //if the queue is not empty, checks to see which light is next to change and changes lights to yellow accordingly
            if (qNextLightUp.Count != 0)
            {
                if (qNextLightUp.Peek() == LightOne)
                {
                    //check to see which lights are green to know which lights should go yellow
                    if (ThreeGreen.Fill.ToString().Equals(green.ToString()))    //if light three is green
                    {
                        GreenToYellowLightThree();
                        FourRight.Fill = yellow;    //light four's right turn arrow turns to yellow
                    }
                    else    //if lights two and four are green
                    {
                        GreenToYellowLightTwoAndFour();
                        FourRight.Fill = black; //light four right turn arrow turns off
                    }
                }
                else if (qNextLightUp.Peek() == LightTwo || qNextLightUp.Peek() == LightFour)
                {
                    //check to see which lights are green to know which lights should go yellow
                    if (ThreeGreen.Fill.ToString().Equals(green.ToString()))    //if light three is green
                        GreenToYellowLightThree();
                    else    //if light one is green
                        GreenToYellowLightOne();
                }
                else if (qNextLightUp.Peek() == LightThree)             
                    GreenToYellowWhenLightThreeIsNext();    //check to see which lights are green to know which lights should go yellow
            }
            else
                GreenToYellowWhenLightThreeIsNext();              
        }

        //changes appropriate lights to yellow when light three is next to go green
        private void GreenToYellowWhenLightThreeIsNext()
        {
            if (OneGreen.Fill.ToString().Equals(green.ToString()))  //if light one is green
                GreenToYellowLightOne();
            else    //if lights two and four are green
                GreenToYellowLightTwoAndFour();
        }

        //initiates the 30-second or less process of changing lights depending on how many cars are still passing through the green light
        private void Start30SecProcess()
        {

            bIs30SecProcessRunning = true;  //indicates that the 30-sec process has started and is currently running
            thirtySecTimer.Start(); //starts the 30-second timer

            //stops and restarts five-second timer to keep track if cars are continuing to pass through the green light in the allotted 30-second interval
            fiveSecTimer.Stop();
            fiveSecTimer.Start();
        }

        //ends the 30-sec or less process of changing lights depending on how many cars are still passing through the green light
        private void End30SecProcess()
        {
            bIs30SecProcessRunning = false;  //indicates that the 30-sec process has stopped
            thirtySecTimer.Stop(); //stops the 30-second timer

            //if there are stoplights in the queue, then take the light that started this process out of the queue
            if (qNextLightUp.Count != 0)
                qNextLightUp.Dequeue(); //takes the light that started this process out of the queue
        }

        //puts light next on the queue if it isn't already in the queue
        private void EnqueueLight(Stoplight stoplight)
        {
            bool IsFoundInQueue;
            
            //if the queue is not empty
            if (qNextLightUp.Count != 0)
            {
                if (stoplight == LightTwo || stoplight == LightFour)               
                    //search through the queue to see if lights two or four are already in the queue
                    IsFoundInQueue = SearchQueueForTwoOrFour();
                else
                    //search through the queue to see if the light we want to put in the queue is already in the queue
                    IsFoundInQueue = SearchQueueForOneOrThree(stoplight);

                //if the light is not already in the queue, put the light in the queue
                if (IsFoundInQueue == false)
                    qNextLightUp.Enqueue(stoplight);
            }
            else   //if the queue is empty
                qNextLightUp.Enqueue(stoplight);
        }

        //search through the queue to see if the light we want to put in the queue is already in the queue
        private bool SearchQueueForOneOrThree(Stoplight stoplight)
        {
            foreach (Stoplight oStoplight in qNextLightUp)
            {
                if (oStoplight == stoplight)
                    return true;              
            }

            return false;
        }

        //search through the queue to see if lights two and four are already in the queue
        private bool SearchQueueForTwoOrFour()
        {
            foreach (Stoplight oStoplight in qNextLightUp)
            {
                if (oStoplight == LightTwo || oStoplight == LightFour)
                    return true;
            }

            return false;
        }    
                
        class Stoplight
        {
            string Name;
            bool HasSensorTriggered;

            //getters and setters for properties
            public string name
            {
                get { return Name; }
                set { Name = value; }
            }

            public bool hasSensorTriggered
            {
                get { return HasSensorTriggered; }
                set { HasSensorTriggered = value; }
            }

            //constructor that sets the name of the stoplight
            public Stoplight(string strName)
            {
                this.Name = strName;
            }
        }
    }
}