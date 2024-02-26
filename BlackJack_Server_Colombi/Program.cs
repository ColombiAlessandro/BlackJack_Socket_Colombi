using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack_Server_Colombi
{
    class Program
    {

        static void Main(string[] args)
        {
            int porta = 9000;
            // Istanzia un oggetto "ascoltatore" (un Listener, appunto) in grado
            // di accettare connessioni su qualunque interfaccia ip, sulla porta definita dalla variabile porta
            TcpListener server = new TcpListener(IPAddress.Any, porta);
            // Istanzia un oggetto Client
            TcpClient tcpClient;
            // Crea un array di bytes
            Byte[] sendBytes;
            // In modo analogo alla lettura di un file, leggere via rete è come leggere un "flusso" (stream)
            NetworkStream networkStream;
            Boolean connesso;
            // Avvia il server di ascolto
            server.Start();
            int punteggio=0;
            Mazzo.CreaMazzo();
            while (true)
            {
                Console.WriteLine("In attesa di connessione sulla porta " + porta + " ...");
                // Il costrutto try...catch server per il "trapping" degli errori...
                try
                {
                    // Accetta la richiesta di connessione di un client
                    tcpClient = server.AcceptTcpClient();
                    Console.WriteLine("Connessione accettata!");
                    // Associa la connessione all'oggetto di stream in grado di gestire il flusso dei dati
                    networkStream = tcpClient.GetStream();
                    // Crea dinamicamente un array della stessa dimensione del buffer di ricezione
                    Byte[] bytes = new Byte[tcpClient.ReceiveBufferSize];
                    int numero_bytes; //numero di byte letti o scritti

                    String dati_client; // dati ricevuti
                    String risposta; //dati da inviare

                    connesso = true;

                    while (connesso)
                    {
                        dati_client = "";
                        // Legge fino al terminatore "\n"
                        do
                        {
                            
                            numero_bytes = networkStream.Read(bytes, 0, tcpClient.ReceiveBufferSize);
                            // Aggiunge alla stringa dati_client quanto letto, interpretandolo come sequenza di codici ascii
                            dati_client += Encoding.ASCII.GetString(bytes, 0, numero_bytes);
                            // ...il tutto fino a quando non viene incontrato il carattere "a capo"
                        } while (!dati_client.Contains("\n"));
                        // Toglie di mezzo lo "a capo"
                        dati_client = dati_client.Replace("\n", "");

                        Console.WriteLine(("Ricevuto: " + dati_client));

                        if (!dati_client.Equals("QUIT"))
                        {
                            risposta = "";
                            if (dati_client == "S") risposta = "S";
                            if(dati_client=="D")
                            {
                                string carta = Mazzo.EstraiCarta();
                                risposta = carta;
                                punteggio += int.Parse(carta.Substring(0, 2));
                                if (punteggio > 21) risposta = "HP";

                            }
                            if (dati_client == "H")
                            {
                                int punteggio2 = 0;
                                do
                                {
                                    string carta = Mazzo.EstraiCarta();
                                    risposta = carta;
                                    sendBytes = Encoding.ASCII.GetBytes(risposta);
                                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                                    punteggio2 += int.Parse(carta.Substring(0, 2));
                                    if (punteggio2 > 21) risposta = "HV";
                                } while (punteggio2 < 15);
                                if (punteggio > punteggio2) risposta = "HV"; else risposta = "HP";
                            }
                            //risposta = elaborazioneDati(dati_client);
                            sendBytes = Encoding.ASCII.GetBytes(risposta);
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            Console.WriteLine("Inviato: " + risposta + "\n");

                        }
                        else
                        {
                            connesso = false;
                        }
                    }

                    //Chiude
                    Console.WriteLine("Connessione chiusa!\n");
                    tcpClient.Close();
                }
                catch (Exception eccezione)
                {
                    Console.WriteLine(eccezione.ToString());
                }
            }
            server.Stop();
            Console.WriteLine("Uscita");
            Console.ReadLine();
        }


        // Questa funzione esegue una banalissima elaborazione della stringa ricevuta
        static String elaborazioneDati(String clientdata)
        {

            String risposta = "";
            risposta = "Mi hai inviato: " + clientdata + " - alle ore: " + DateTime.Now;
            return risposta;
        }

    }
    public static class Mazzo
    {

        public static List<string> mazzo;
        public static void CreaMazzo()
        {
            mazzo = new List<string>();
            int nCarta = 1;
            string[] seme = new string[] { "C", "Q", "F", "P" };
            int contSeme = 0;
            for (int i = 0; i < 52; i++)
            {

                string carta = "";
                if (nCarta < 10)
                {
                    carta += "0";
                }
                carta += $"{nCarta}{seme[contSeme]}";
                nCarta++;
                if (nCarta > 13)
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
}
