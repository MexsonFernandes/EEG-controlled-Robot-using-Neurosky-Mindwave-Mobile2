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
            byte[] buffer = new byte[2048];
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
                System.Threading.Thread.Sleep(5000);
                client.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error connecting to device.");
            }

            Console.WriteLine("Step 1 completed!!!");

            try
            {
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();

                Console.WriteLine("Sending configuration packet to device.");              
                if (stream.CanWrite)
                    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

                System.Threading.Thread.Sleep(5000);
                client.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error sending configuration packet to TGC.");
            }

            Console.WriteLine("Step 2 completed!!!");

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
                    
                    // This should really be in it's own thread  
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(saveData);
                    while (true)
                    {
                        bytesRead = stream.Read(buffer, 0, 2048);

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
                                    break;
                                }

                                //Check fitting (device on head or not)
                                //if(data["eSense"].ToString() == "0" && data["eSense"].ToString()["meditation"].ToString() == "0")

                                //Console.WriteLine("Raw data: " + data["rawEeg"]);
                                Console.WriteLine("fefej" + s);
                               // if(data["poorSignalLevel"])
                            }
                            catch{ }
                        }
                    }                   
                }
                System.Threading.Thread.Sleep(5000);
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
            }
            catch
            {
                Console.WriteLine("Error in data saving.");
            }

            Console.WriteLine("Step 4 completed!!! Enjoy!!!");
        }
    } 
}




