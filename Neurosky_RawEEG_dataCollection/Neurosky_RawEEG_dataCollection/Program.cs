using System.IO;
using System.Net;
using System.Net.Sockets;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            TcpClient client;
            Stream stream;
            byte[] buffer = new byte[4096];
            int bytesRead; // Building command to enable JSON output from ThinkGear Connector (TGC) 

            Console.WriteLine("Do you want raw EEG?(1/0) default is True.");
            var option = Console.ReadLine();
            var com = @"{""enableRawOutput"": true, ""format"": ""Json""}";
            if (option.ToString() == "0")
                com = @"{""enableRawOutput"": false, ""format"": ""Json""}";
            else
                com = @"{""enableRawOutput"": true, ""format"": ""Json""}";

            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(com);

            try
            {
                Console.WriteLine("Starting connection to Mindwave Mobile Headset.");
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();
                System.Threading.Thread.Sleep(500);
                client.Close();
                Console.WriteLine("Step 1 completed!!!");
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error connecting to device.");
            }


            try
            {
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();

                Console.WriteLine("Sending configuration packet to device.");              
                if (stream.CanWrite)
                    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                System.Threading.Thread.Sleep(500);
                client.Close();

                Console.WriteLine("Step 2 completed!!!");
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error sending configuration packet to TGC.");
            }


            try
            {
                Console.WriteLine("Starting data collection.");
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();

                // Sending configuration packet to TGC                
                if (stream.CanWrite)
                    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);


                if (stream.CanRead)
                {
                    Console.WriteLine("Reading bytes");

                    //to check if device is ready
                    var ready = false;
                    var startRead = false;

                    //to note keyboard key press and note key press
                    ConsoleKeyInfo key = Console.ReadKey(false);

                    // This should really be in it's own thread  
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(saveData);
                    while (true)
                    {
                        bytesRead = stream.Read(buffer, 0, 4096);

                        string[] packets = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split('\r');
                        foreach (string s in packets)
                        {
                            try
                            {
                                IDictionary data = (IDictionary)JsonConvert.Import(typeof(IDictionary), s);
                                
                                //Check if device is ON/OFF
                                if(data.Contains("status"))
                                {
                                    
                                    Console.WriteLine("Device is Off.");
                                    ready = false;
                                    break;
                                }

                                //Check fitting (device on head or not)
                                if(data.Contains("eSense"))
                                    if(data["eSense"].ToString() == "{\"attention\":0,\"meditation\":0}")
                                    {
                                        Console.WriteLine("Check fitting.");
                                        ready = false;
                                        break;
                                    }

                                //check if device is ready
                                if((data.Contains("eSense")) && (ready == false))
                                {
                                    IDictionary d = (IDictionary) data["eSense"];
                                    if ((d["attention"].ToString() != "0") && (d["meditation"].ToString() != "0"))
                                    {
                                        ready = true;
                                        Console.WriteLine("Device is ready.");
                                        Console.WriteLine("Enter F for FORWARD, B for BACKWARD, L for LEFT, R for RIGHT, S for STOP, CTRL + C to close readings and O for others.");
                                    }
                                }

                                if(ready)
                                {
                                    if (Console.KeyAvailable == true)
                                    {
                                        startRead = true;
                                        key = Console.ReadKey(true);
                                        break;
                                    }
                                    if(startRead)
                                    switch (key.Key)
                                    {
                                        case ConsoleKey.F:
                                            Console.WriteLine("F");
                                            break;
                                        case ConsoleKey.B:
                                            Console.WriteLine("B");
                                            break;
                                        case ConsoleKey.L:
                                            Console.WriteLine("L");
                                            break;
                                        case ConsoleKey.R:
                                            Console.WriteLine("R");
                                            break;
                                        case ConsoleKey.S:
                                            Console.WriteLine("S");
                                            break;
                                        default:
                                            Console.WriteLine("Wrong key");
                                            startRead = false;
                                            break;
                                    }
                                }
                            }
                            catch{ }
                        }
                    }                   
                }
                System.Threading.Thread.Sleep(500);
                client.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error in data collection.");
            }        
        }

        public static void saveData(object sender, ConsoleCancelEventArgs args)
        { 
            Console.WriteLine("Step 3 completed!!!");

            try
            {
                Console.WriteLine("Saving data to csv file.");
                Console.WriteLine("Step 4 completed!!! Enjoy!!!");
            }
            catch
            {
                Console.WriteLine("Error in data saving.");
            }

            
        }
    } 
}




