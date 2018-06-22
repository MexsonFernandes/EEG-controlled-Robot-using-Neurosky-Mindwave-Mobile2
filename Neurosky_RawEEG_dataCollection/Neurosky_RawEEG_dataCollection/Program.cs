using System.IO;
using System.Net;
using System.Net.Sockets;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;

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
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes(@"{""enableRawOutput"": false, ""format"": ""Json""}");

            try
            {
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();

                System.Threading.Thread.Sleep(5000);
                client.Close();
            }
            catch (SocketException se) { }

            try
            {
                client = new TcpClient("127.0.0.1", 13854);
                stream = client.GetStream();

                // Sending configuration packet to TGC                
                if (stream.CanWrite)
                {
                    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }

                System.Threading.Thread.Sleep(5000);
                client.Close();
            }
            catch (SocketException se) { }

            try
            {
                client = new TcpClient("127.0.0.1", 13854); stream = client.GetStream();

                // Sending configuration packet to TGC                
                if (stream.CanWrite)
                {
                    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
                }


                if (stream.CanRead)
                {
                    Console.WriteLine("reading bytes");

                    // This should really be in it's own thread                    
                    while (true)
                    {
                        bytesRead = stream.Read(buffer, 0, 2048);
                        string[] packets = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split('\r');
                        //foreach (string s in packets)
                        //{

                        //var json = System.IO.File.ReadAllText(s.Trim());
                        //var objects = Newtonsoft.Json.Linq.JArray.Parse(s); // parse as array
                        Console.WriteLine(packets.GetValue(0));
                        //parseJSON(s.Trim());
                        //}
                    }
                }
                System.Threading.Thread.Sleep(5000);

                client.Close();

            }
            catch (SocketException se) { }
        }
    }
}



