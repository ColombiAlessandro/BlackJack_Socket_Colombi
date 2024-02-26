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
                            
                        } while (stringa_ricevuta != "S");
                        Console.WriteLine("Gioco iniziato");
                        string scelta = "";
                        int punteggio = 0;
                        do
                        {

                            Console.WriteLine($"Il tuo punteggio attuale è:{punteggio}");
                            Console.WriteLine("Vuoi pescare un'altra carta?[Y/N]");
                            scelta = Console.ReadLine();
                            if(scelta== "Y" || scelta == "y")
                            {
                                Byte[]bytes_da_inviare = Encoding.ASCII.GetBytes("D" + "\n");
                                stream.Write(bytes_da_inviare, 0, bytes_da_inviare.Length);
                                numero_bytes = stream.Read(bytes, 0, cliente.ReceiveBufferSize);
                                stringa_ricevuta = Encoding.ASCII.GetString(bytes, 0, numero_bytes);
                                if (stringa_ricevuta != "HP")
                                {
                                    Console.WriteLine($"La carta estratta è {stringa_ricevuta}");
                                    punteggio += int.Parse(stringa_ricevuta.Substring(0, 2));
                                }
                                else
                                {
                                    Console.WriteLine("Hai perso");
                                    return;
                                }
                            }
                        } while (scelta != "N" || scelta != "n");
                        Console.WriteLine("Ora tocca al banco");
                        Byte[] bytes_da_inviare1 = Encoding.ASCII.GetBytes("H" + "\n");
                        stream.Write(bytes_da_inviare1, 0, bytes_da_inviare1.Length);
                        do
                        {
                            numero_bytes = stream.Read(bytes, 0, cliente.ReceiveBufferSize);
                            stringa_ricevuta = Encoding.ASCII.GetString(bytes, 0, numero_bytes);
                            Console.WriteLine(stringa_ricevuta);
                        } while (stringa_ricevuta != "HV" || stringa_ricevuta != "HP");
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
