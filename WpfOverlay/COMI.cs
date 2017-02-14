using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.UDP;
using NetworkCommsDotNet.Connections;
using System.Net;

namespace WpfOverlay
{
    class COMI
    {
       public static void Intialize()
        {

            //Trigger the method PrintIncomingMessage when a packet of type 'Message' is received
            //We expect the incoming object to be a string which we state explicitly by using <string>
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", PrintIncomingMessage);
            //Start listening for incoming connections
            Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 1100));

            //Print out the IPs and ports we are now listening on
            Console.WriteLine("Server listening for TCP connection on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);

            //Let the user close the server
            Console.WriteLine("\nPress any key to close server.");
           // Console.ReadKey(true);

        }
        private static void PrintIncomingMessage(PacketHeader header, Connection connection, string message)
        {
            Console.WriteLine("\nA message was received from " + connection.ToString() + " which said '" + message + "'.");
        }

        public static void ShutDownCOM()
        {
            //We have used NetworkComms so we should ensure that we correctly call shutdown

            NetworkComms.Shutdown();
        }
    }
}
