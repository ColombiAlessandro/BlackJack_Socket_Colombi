using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackJack_Client_Colombi
{
    class Program
    {

        static void Main(string[] args)
        {
            
            TcpClient cliente = new TcpClient();
            NetworkStream stream;
            Boolean ok = true;
            int porta = 9000;
            string nome_server = "127.0.0.1";
            try
            {
                Console.Write("Vuoi iniziare la partita? [Y/N]");
                string start = Console.ReadLine();
                if (start == "Y" || start == "y")
                {
                    // tenta di connettersi al server specificato
                    cliente.Connect(nome_server, porta);
                    // Delega la gestione del flusso dati all'oggetto networkstream, in modo
                    // identico a quanto già visto per il Server.
                    stream = cliente.GetStream();
                    // Verifica che il flusso sia di lettura e scrittura
                    if (!stream.CanRead)
                    {
                        Console.WriteLine("Non posso leggere dallo Stream");
                        ok = false;
                    }
                    else
                        if (!stream.CanWrite)
                    {
                        Console.WriteLine("Non posso scrivere nello Stream");
                        ok = false;
                    }
                    if (ok)
                    {
                        Byte[] bytes = new Byte[cliente.ReceiveBufferSize];

                        int numero_bytes; //numero di byte letti o scritti
                        String stringa_inviata;
                        String stringa_ricevuta;
                        Console.WriteLine("Connesso al Server!");
                        // I flussi in uscita e in ingresso sono gestiti esattamente come
                        // già visto nell'applicazione Server...
                        do
                        {
                            Byte[] bytes_da_inviare = Encoding.ASCII.GetBytes("S" + "\n");
                            stream.Write(bytes_da_inviare, 0, bytes_da_inviare.Length);
                            numero_bytes = stream.Read(bytes, 0, cliente.ReceiveBufferSize);
                            stringa_ricevuta = Encoding.ASCII.GetString(bytes, 0, numero_bytes);
                            Console.WriteLine("Ricevuto: " + stringa_ricevuta);
                            if (stringa_ricevuta == "W")
                            {
                                Thread.Sleep(1000);
                            }
                        } while (stringa_ricevuta != "S");
                        Console.WriteLine("Gioco iniziato");
                        Console.ReadLine();
                    }
                    cliente.Close();
                }
            }
            catch (Exception eccezione)
            {
                Console.WriteLine(eccezione.ToString());
            }

        }
    }
}
