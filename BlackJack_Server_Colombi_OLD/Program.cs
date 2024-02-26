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
        public static bool libero=true;
        static int porta = 9000;
        public static int counter;
        public static List<int> punteggi=new List<int>();
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
    public static class Mazzo
    {

        public static List<string> mazzo;
        public static void CreaMazzo()
        {
            mazzo=new List<string>();
            int nCarta=1;
            string[] seme = new string[] { "C", "Q", "F", "P" };
            int contSeme = 0;
            for(int i=0; i < 52; i++)
            {
                
                string carta="";
                if (nCarta < 10)
                {
                    carta += "0";
                }
                carta += $"{nCarta}{seme[contSeme]}";
                nCarta++;
                if(nCarta > 13)
                {
                    nCarta = 1;
                    contSeme++;
                }
                mazzo.Add(carta);
            }
        }
        public static string EstraiCarta()
        {
            Random rnd = new Random();
            string carta = mazzo[rnd.Next(0, mazzo.Count)];
            mazzo.Remove(carta);
            return carta;
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
            int punteggio = 0;
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
                        } else if(dataFromClient=="DF")
                        {
                            if (Program.libero)
                            {
                                Program.libero = false;
                                serverResponse = "IN";
                            }
                            else serverResponse = "W";  
                        } else if (dataFromClient == "D")
                        {
                            string carta = Mazzo.EstraiCarta();
                            serverResponse = carta;
                            Program.punteggi[int.Parse(this.clNo)-1] += int.Parse(carta.Substring(0, 2));
                            if(punteggio > 21)
                            {
                                serverResponse = "HP";
                            } 
                            
                        } 
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
