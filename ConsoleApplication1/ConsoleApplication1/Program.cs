using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace ConsoleAppFlightBooking
{
    // creates a delegate for event handling 
    public delegate void pricecutEvent(Int32 price);
    public class Airlines
    {
        static Random rng = new Random();
        public static event pricecutEvent priceCut; // Creates the event 
        public static Int32 FlightTicket = 900; // Intial value of Flight price
        public static Int32 NumOfPriceCut = 1; // variable for counting price cut 
        public static Int32 PriceCounter = 5;

        // Object of reader writer lock for order process
        public static ReaderWriterLock rwObj1 = new ReaderWriterLock();
        
        public Int32 getPrice() { return FlightTicket; }

        public static void changePrice(Int32 price) // changes the price of flight ticket after its updation
        {
            if (price < FlightTicket)
            {
                if (priceCut != null)
                {
                    priceCut(price); // calls the delegate if the new price is less than last price
                }
                FlightTicket = price;
            }
        }

        // Airline thread calls the function 
        public void UpdatePrice()
        {
            // No of Price cuts 
            while (NumOfPriceCut < 6)
            {
                if (PriceCounter == 5)
                {

                    Int32 p = rng.Next(100, 900);
                    Console.WriteLine("* * * * * * New Price Is : "+p+" * * * * * * * *");
                    PriceCounter = 0;

                    if (p < FlightTicket)
                    {
                        NumOfPriceCut++;
                    }
                    TravelAgents.oldTicket = FlightTicket;
                    TravelAgents.newTicket = p;
                    Airlines.changePrice(p);
                }


                rwObj1.AcquireWriterLock(1000);
                try
                {

                    String en_order = MCB.getOneCell();//gets the order string from the  buffer
                    PriceCounter++;

                    OrderClass orderObject = Decoder(en_order); //Convert the string order to order object

                    OrderProcessing opObj = new OrderProcessing();
                    //Console.WriteLine(orderObject.SenderId);
                    Thread orderProcessing = new Thread(() => opObj.orderP(orderObject));
                    orderProcessing.Start();
                   // orderProcessing.Join();
                }

                finally { rwObj1.ReleaseWriterLock(); }

                if (NumOfPriceCut > 5)
                {
                    Console.WriteLine("The Airlines thread will terminate - No of price cuts completed"); //Print before termination of  thread
                }
            }
        } //End of method
    

       /*
        public void GetTheOrder(String airline)
        {

            String order = MCB.GetOneCell(airline);
            Console.WriteLine(order);
            // OrderClass order = new OrderClass(Convert.ToInt32(Thread.CurrentThread.Name), 5555, 1 , p, 1);
            Object decoded = Decoder(order);
            // decoded = Decoder(order);
            Object OrderT = OrderProcessing(decoded);
        }
        
        */

        // Decodes the order string into order object
        public OrderClass Decoder(String order)
        {
            String[] s = order.Split(',');
            OrderClass orderOb2 = new OrderClass();

            orderOb2.SenderId = Convert.ToInt32(s[0]);
            orderOb2.CardNumber = Convert.ToInt32(s[1]);
           // orderOb2.ReceiverId = Convert.ToInt32(s[2]);
            orderOb2.Amount = Convert.ToInt32(s[2]);
            orderOb2.UnitPrice = Convert.ToInt32(s[3]);


            return orderOb2;
        }

        // Processes the order by counting the total amount of tickets 
        class OrderProcessing
        {
            public void orderP(OrderClass order)
            {
                Double tax = 0.10;
                Int32 LocationCharges = 5;
                Int32 NoTickets = order.Amount;
                Int32 PriceOfATicket = order.UnitPrice;
               
                
                    if (order.CardNumber >= 5000 && order.CardNumber <= 7000)
                    {
                        Int32 taxTotal = Convert.ToInt32(order.Amount * order.UnitPrice * tax);

                        Int32 totalPayableAmount = (order.Amount * order.UnitPrice) + taxTotal + LocationCharges;

                        
                        Console.WriteLine("Order Processed. Unit Price : {0} | No of tickets:{1} | Total Amount: {2} ",PriceOfATicket,NoTickets, totalPayableAmount);
                        Console.WriteLine("===========================================================================");
                        }

            }
        }

        public class TravelAgents
        {
            public static Int32 oldTicket;
            public static Int32 newTicket;
            static Random r2 = new Random();

            public void agentsFun()
            {
                // checks the number of price cuts
                while (Airlines.NumOfPriceCut < 21) 
                { 
               
                Airlines airline1 = new Airlines();

                    if(newTicket < oldTicket )
                    
                    {

                    Thread.Sleep(1000);

                        //Generates a Random credit card number between 5000 to 7000
                        Int32 creditCardNo = r2.Next(5000, 7000); 
                        //Taking random amount of flight tickets
                        Int32 numOfTicket = r2.Next(1, 20);     

                        OrderClass order = new OrderClass();

                        //Sets the values of object to send it to Encoder

                        order.SenderId=Convert.ToInt32(Thread.CurrentThread.Name);
                        order.CardNumber=creditCardNo;
                        order.Amount=numOfTicket;
                        order.UnitPrice=newTicket;
                       

                        String en_order = Encoder(order); // Converts the order object to string

                        MCB.setOneCell(en_order); //Adding the string to encoder
                        Console.WriteLine("Agency {0} has everyday low price : ${1} each", Thread.CurrentThread.Name, newTicket);
                    }
                        
                    else
                    {
                        Thread.Sleep(1000);
                        
                        Int32 creditCardNo = r2.Next(5000, 7000);//Taking a random credit card number
                        Int32 numOfTicket = r2.Next(1, 20);//Taking a random amount of Tickets

                        OrderClass order = new OrderClass();
                        //Setting the values of object for sending it to encoder

                        order.SenderId = Convert.ToInt32(Thread.CurrentThread.Name);
                        order.CardNumber = creditCardNo;
                        order.Amount = numOfTicket;
                        order.UnitPrice = newTicket;
                        String en_order = Encoder(order);// Converting the order object to string

                        MCB.setOneCell(en_order); //Adding the string to encoder
                        
                       Console.WriteLine("Agency {0} has everyday low price : ${1} each", Thread.CurrentThread.Name, newTicket);

                    } 
                
            } //End while
        }//End agentsFun


            

            public void FlightOnSale(Int32 p)
            {
                Console.WriteLine("Agent{0} tickets are on sale : as low as ${1} each", Thread.CurrentThread.Name, p);

            }

            public String Encoder(OrderClass order)
            {
                return (order.SenderId + "," + order.CardNumber + "," + order.Amount.ToString() + "," + order.UnitPrice.ToString());

            }



        }//end of TravelAgents class

        public class OrderClass
        {
            private Int32 _senderId;
            private Int32 _cardNo;
            //private Int32 _receiverId;
            private Int32 _amount;
            private Int32 _unitPrice;

            public Int32 SenderId
            {
                get { return _senderId; }

                set { _senderId = value; }
            }

            public Int32 CardNumber
            {
                get { return _cardNo; }

                set { _cardNo = value; }
            }
            /*
            public Int32 ReceiverId
            {
                get { return _receiverId; }

                set { _receiverId = value; }
            }
            */
            public Int32 Amount
            {
                get { return _amount; }

                set { _amount = value; }
            }

            public Int32 UnitPrice
            {
                get { return _unitPrice; }

                set { _unitPrice = value; }
            }


        }//end of OrderClass class

        public class myApp
        {
            static void Main(string[] args)
            {

                Airlines airline1 = new Airlines();
                Thread Airline = new Thread(new ThreadStart(airline1.UpdatePrice));
                Airline.Start();
                
                TravelAgents agent1 = new TravelAgents();
                Airlines.priceCut += new pricecutEvent(agent1.FlightOnSale);

                Thread[] agents = new Thread[5];

                for (int i = 0; i < 5; i++)
                {
                    agents[i] = new Thread(new ThreadStart(agent1.agentsFun));
                    agents[i].Name = (i + 1).ToString();
                    agents[i].Start();
                }


            }
        }//end of main class myApp

        public static class MCB
        {
                static Int32 readCell , writeCell = 0;
                static Semaphore maxSemaphore = new Semaphore(2, 2);
                static Semaphore minSemaphore = new Semaphore(0, 2);
                    
                const Int32 cellSize = 2; //Size of the Cell buffer

                static String[] cell = new String[cellSize];

                public static ReaderWriterLock rwObj2 = new ReaderWriterLock();  // ReaderWriter object Cell methods 


                public static String getOneCell()
                {
                    String order;
                    minSemaphore.WaitOne();
                    rwObj2.AcquireWriterLock(1000);
                    try
                    {
                        {
                            readCell = readCell % cell.Length;  
                            order = cell[readCell];
                            readCell++;
                        }
                        maxSemaphore.Release();    //releases the semaphore
                        return order;
                    }

                    finally { rwObj2.ReleaseWriterLock(); }
                } // End GetOneCell


                public static void setOneCell(String order)
                {

                    maxSemaphore.WaitOne();
                    rwObj2.AcquireWriterLock(1000);
                    try
                    {
                        {
                            writeCell = writeCell % cell.Length;
                            cell[writeCell] = order;    
                            writeCell++;
                        }
                        minSemaphore.Release();       // Releases the Semaphore
                    }

                    finally { rwObj2.ReleaseWriterLock(); }
                } // End SetOneCell


               // End MulticellBuffer

        }   //End MCB



    }//end of Airlines class


}//end of Namespace
