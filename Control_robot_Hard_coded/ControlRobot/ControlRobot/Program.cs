using System.IO;
using System.Net.Sockets;
using Jayrock.Json.Conversion;
using System;
using System.Text;
using System.Collections;
using System.IO.Ports;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            SerialPort comPort = new SerialPort();

            IDictionary eSense;
            IDictionary blinkStrength;
            TcpClient client;
            Stream stream;
            int Blink = 0;
            String meditate, atten;
            int count = 0;
            byte[] buffer = new byte[4096];
            int bytesRead; // Building command to enable JSON output from ThinkGear Connector (TGC) 

            byte[] forward = Encoding.ASCII.GetBytes("3");
            byte[] backward = Encoding.ASCII.GetBytes("4");
            byte[] left = Encoding.ASCII.GetBytes("1");
            byte[] right = Encoding.ASCII.GetBytes("2");
            byte[] stop = Encoding.ASCII.GetBytes("5");


            Console.WriteLine("Enter COM Port : ");
            var port = Console.ReadLine();

            try
            {
                //Starting connection on com port                
                comPort.PortName = port.ToString();
                comPort.BaudRate = 38400;
                comPort.Parity = Parity.None;
                comPort.StopBits = StopBits.One;
                comPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to port : " + port.ToString());
            }

            //1 for EEGsignal and any for raw data
            var com = @"{""enableRawOutput"": false, ""format"": ""Json""}";


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



                    //to check if device is ready
                    var ready = false;
                    var startRead = false;


                    //to note keyboard key press and note key press
                    Console.WriteLine("Enter any key to start.");
                    ConsoleKeyInfo key = Console.ReadKey(false);
                    Console.WriteLine("Reading bytes");

                    // This should really be in it's own thread  

                    while (true)
                    {
                        bytesRead = stream.Read(buffer, 0, 4096);


                        string[] packets = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split('\r');
                        foreach (string s in packets)
                        {
                            try
                            {

                                IDictionary data = (IDictionary)JsonConvert.Import(typeof(IDictionary), s);

                                Console.WriteLine(data);break;
                                //Check if device is ON/OFF
                                if (data.Contains("status"))
                                {

                                    Console.WriteLine("Device is Off.");
                                    ready = false;
                                    break;
                                }

                                //Check fitting (device on head or not)
                                if (data.Contains("eSense"))
                                    if (data["eSense"].ToString() == "{\"attention\":0,\"meditation\":0}")
                                    {
                                        Console.WriteLine("Check fitting.");
                                        ready = false;
                                        break;
                                    }

                                //check if device is ready
                                if ((data.Contains("eSense")) && (ready == false))
                                {
                                    IDictionary d = (IDictionary)data["eSense"];
                                    if ((d["attention"].ToString() != "0") && (d["meditation"].ToString() != "0"))
                                    {
                                        ready = true;
                                        Console.WriteLine("Device is ready.");

                                    }
                                }

                                //start data reading only when device is ready.
                                if (ready)
                                {

                                    eSense = (IDictionary)data["eSense"];
                                    // blinkStrength = (IDictionary)data["blinkStrength"];
                                    if ((data.Contains("eSense")))
                                    {

                                        Console.WriteLine("Attention : "+eSense["attention"].ToString() + ", Meditation : " + eSense["meditation"].ToString());
                                        
                                        atten = eSense["attention"].ToString();
                                        meditate = eSense["meditation"].ToString();
                                        if (count == 0)
                                        {
                                            if (Int32.Parse(atten) > 75)
                                            {
                                                //forward
                                                Console.WriteLine("Bot moving forward.");
                                                comPort.Write(forward, 0, 1);
                                            }
                                            else if( (Int32.Parse(atten) < 40) && (Int32.Parse(meditate) < 40))
                                            {
                                                
                                                //reverse
                                                Console.WriteLine("Bot moving reverse.");
                                                comPort.Write(backward, 0, 1);
                                            }
                                        }
                                        if (count > 0)
                                        {
                                            if (count == 1)
                                            {
                                                if (Blink >50 && Blink < 80)
                                                {
                                                    //left
                                                    Console.WriteLine("Bot moving left.");
                                                    comPort.Write(left, 0, 1);
                                                }
                                                else
                                                {
                                                    //right
                                                    Console.WriteLine("Bot moving right.");
                                                    comPort.Write(right, 0, 1);
                                                }
                                            }
                                            else if (count >= 2)
                                            {
                                                //stop
                                                Console.WriteLine("Bot stopped.");
                                                comPort.Write(stop, 0, 1);
                                            }
                                        }
                                        count = 0;
                                    }
                                    Console.WriteLine("Blink strength : " + data["blinkStrength"].ToString());
                                    int temp = Int32.Parse(data["blinkStrength"].ToString());
                                    if (temp > 30)
                                    {
                                        Blink = temp;
                                        count += 1;
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(500);
                client.Close();
                comPort.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error in data collection.");
                comPort.Close();
            }
        }

    }
}




