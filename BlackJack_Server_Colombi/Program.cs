using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BlackJack_Server_Colombi
{
    
    class Program
    {
        static int porta = 9000;
        public static int counter;
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, porta);
            TcpClient clientSocket = default(TcpClient);

            serverSocket.Start();
            Console.WriteLine("Server avviato - Porta: " + porta);

            counter = 0;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Numero:" + Convert.ToString(counter) + " avviato!");
                var port = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Port;
                Console.WriteLine("Il client lavora sulla porta " + port);
                handleClient client = new handleClient();
                client.startClient(clientSocket, Convert.ToString(counter));
            }

            serverSocket.Stop();
            Console.WriteLine("Esco (in realtà non avviene mai, per come è scritto questo codice)");
            Console.ReadLine();
        }
    }

    //Classe che gestisce ogni client connesso separatamente
    public class handleClient
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;

            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
            Console.WriteLine("La connessione è attiva all'indirizzo/porta: " + clientSocket.Client.RemoteEndPoint.ToString());
            int num_bytesFrom; //numero di byte letti o scritti
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            requestCount = 0;
            bool connesso = true;

            while (connesso)
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    // Legge fino al terminatore "\n"
                    dataFromClient = "";
                    do
                    {
                        num_bytesFrom = networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                        dataFromClient += Encoding.ASCII.GetString(bytesFrom, 0, num_bytesFrom);
                    } while (!dataFromClient.Contains("\n"));
                    dataFromClient = dataFromClient.Replace("\n", "");
                    Console.WriteLine(DateTime.Now + " - Ricevuto dal Client numero: " + clNo + " il messaggio: [" + dataFromClient + "]");

                    if (!dataFromClient.Equals("QUIT"))
                    {
                        if (dataFromClient == "S")
                        {
                            if (Program.counter - 1 < 2)
                            {
                                serverResponse = "W";
                            } else
                            {
                                serverResponse = "S";
                            }
                        } else serverResponse = "[ok]";
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(DateTime.Now + " - Invio messaggio di conferma ricezione al Client numero: " + clNo);
                    }
                    else
                    {
                        Console.WriteLine("Il Client numero: " + clNo + " si è disconnesso");
                        connesso = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore: " + ex.Message);
                    Console.WriteLine("Il Client numero: " + clNo + " si è disconnesso");
                    connesso = false;
                }
            }
        }
    }
}
