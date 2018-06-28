using System.IO;
using System.Net.Sockets;
using Jayrock.Json.Conversion;
using System;
using System.Text;
using System.Collections;

namespace ConsoleApp1
{
    class Program
    {
        //rawDataFile
        public static StreamWriter rawdata = new StreamWriter(@"rawData_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now) + ".csv", true);

        //EEG data
        public static StreamWriter eegdata = new StreamWriter(@"EEGData_" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now) + ".csv", true);

        static void Main(string[] args)
        {
            rawdata.WriteLine("rawData,class");
            eegdata.WriteLine("attention,meditation,delta,theta,lowAplha,highAlpha,lowBeta,highBeta,lowGamma,highGamma,class");
            IDictionary eegPower;
            IDictionary eSense;
            TcpClient client;
            Stream stream;
            byte[] buffer = new byte[4096];
            int bytesRead; // Building command to enable JSON output from ThinkGear Connector (TGC) 

            Console.WriteLine("Do you want normal EEG Signal data?(1/0) default is raw data.");
            var option = Console.ReadLine();//1 for EEGsignal and any for raw data
            var com = @"{""enableRawOutput"": true, ""format"": ""Json""}";
            if (option.ToString() == "1")
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
                    
                  

                    //to check if device is ready
                    var ready = false;
                    var startRead = false;

                    //to note keyboard key press and note key press
                    Console.WriteLine("Enter any key to start.");
                    ConsoleKeyInfo key = Console.ReadKey(false);
                    Console.WriteLine("Reading bytes");

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
                                       // Console.WriteLine("Enter F for FORWARD, B for BACKWARD, L for LEFT, R for RIGHT, S for STOP and CTRL + C to close readings");
                                    }
                                }

                                //start data reading only when device is ready.
                                if(ready)
                                {
                                    if (Console.KeyAvailable == true)
                                    {
                                        startRead = true;
                                        key = Console.ReadKey(true);
                                        
                                        break;
                                    }

                                    //read data only when key press has been noted
                                    if (startRead)
                                    {
                                        Console.WriteLine(data);
                                        switch (key.Key)
                                        {
                                            case ConsoleKey.F:
                                               // Console.WriteLine("F readings...");
                                                if (option.ToString() == "1")
                                                {
                                                    eSense = (IDictionary)data["eSense"];
                                                    eegPower = (IDictionary)data["eegPower"];                                                

                                                    eegdata.WriteLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                                                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                                                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                                                        eegPower["highGamma"].ToString() + ",0");
                                                }
                                                else
                                                    rawdata.WriteLine(data["rawEeg"].ToString() + ",0");//0 for Forward
                                                break;
                                            case ConsoleKey.B:
                                                //Console.WriteLine("B readings...");
                                                if (option.ToString() == "1")
                                                {
                                                    eSense = (IDictionary)data["eSense"];
                                                    eegPower = (IDictionary)data["eegPower"];
                                                    eegdata.WriteLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                                                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                                                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                                                        eegPower["highGamma"].ToString() + ",1");
                                                }
                                                else
                                                    rawdata.WriteLine(data["rawEeg"].ToString() + ",1");//1 for Backward                      
                                                break;
                                            case ConsoleKey.L:
                                                //Console.WriteLine("L readings...");
                                                if (option.ToString() == "1")
                                                {
                                                    eSense = (IDictionary)data["eSense"];
                                                    eegPower = (IDictionary)data["eegPower"];
                                                    eegdata.WriteLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                                                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                                                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                                                        eegPower["highGamma"].ToString() + ",2");
                                                }
                                                else
                                                    rawdata.WriteLine(data["rawEeg"].ToString() + ",2");//2 for Left           
                                                break;
                                            case ConsoleKey.R:
                                                //Console.WriteLine("R readings...");
                                                if (option.ToString() == "1")
                                                {
                                                    eSense = (IDictionary)data["eSense"];
                                                    eegPower = (IDictionary)data["eegPower"];
                                                    eegdata.WriteLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                                                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                                                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                                                        eegPower["highGamma"].ToString() + ",3");

                                                }
                                                else
                                                    rawdata.WriteLine(data["rawEeg"].ToString() + ",3");//3 for Right
                                                
                                                
                                                break;
                                            case ConsoleKey.S:
                                                //Console.WriteLine("S readings...");
                                                if (option.ToString() == "1")
                                                {
                                                    eSense = (IDictionary)data["eSense"];
                                                    eegPower = (IDictionary)data["eegPower"];
                                                    eegdata.WriteLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                                                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                                                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                                                        eegPower["highGamma"].ToString() + ",4");
                                                }
                                                else
                                                    rawdata.WriteLine(data["rawEeg"].ToString() + ",4");//4 for Stop
                                                
                                                
                                                break;
                                           
                                            default:
                                              //  Console.WriteLine("Wrong key");
                                                startRead = false;
                                                break;
                                        }
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                            }
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
                rawdata.Flush();
                rawdata.Close();
                rawdata.Dispose();
                eegdata.Flush();
                eegdata.Close();
                eegdata.Dispose();
                Console.WriteLine("Step 4 completed!!! Enjoy!!!");
            }
            catch
            {
                Console.WriteLine("Error in data saving.");
            }

            
        }
    } 
}




