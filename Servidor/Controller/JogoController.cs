using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common.Modelos;
using Common.Stores;
using Newtonsoft.Json;

namespace Servidor.Controller
{
    public class JogoController
    {
        public JogoController()
        {
            StoreJogo.Instance.Jogo.PlayerList.Last().Turn = true;
            StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.JogoStarted;
            foreach (Jogador jogador in StoreJogo.Instance.Jogo.PlayerList)
            {
                MensagemRede networkMessageToSend = new MensagemRede()
                {
                    NetworkInstruction = InstrucaoRede.PlacingBoats
                };

                // Serialize the NetworkMessage object to a JSON string
                string networkMessageToSendJsonString = JsonConvert.SerializeObject(networkMessageToSend);

                jogador.BinaryWriter.Write(networkMessageToSendJsonString);
            }
        }

        public void PlacingBoats()
        {
            foreach (Jogador jogador in StoreJogo.Instance.Jogo.PlayerList)
            {
                if (jogador.TcpClient.GetStream().DataAvailable)
                {
                    string message = jogador.BinaryReader.ReadString();

                    // Unserialize the JSON string to the object NetworkMessage
                    MensagemRede receivedNetworkMessage = JsonConvert.DeserializeObject<MensagemRede>(message);
                    jogador.CampoJogador = receivedNetworkMessage.CampoJogador;
                    if (receivedNetworkMessage.Pronto)
                    {
                        jogador.ProntoJogador = true;
                        jogador.Barcos = receivedNetworkMessage.Jogador.Barcos;
                    }
                }
            }
        }

        public void NextTurn()
        {
            if (StoreJogo.Instance.Jogo.PlayerList.FindIndex(p => p.Turn) + 1 ==
                StoreJogo.Instance.Jogo.PlayerList.Count)
            {
                StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).Turn = false;
                StoreJogo.Instance.Jogo.PlayerList.First().Turn = true;
            }
            else
            {
                StoreJogo.Instance.Jogo.PlayerList[StoreJogo.Instance.Jogo.PlayerList.FindIndex(p => p.Turn) + 1].Turn = true;
                StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).Turn = false;
            }
        }

        public void AskPlayerToPlay()
        {
            MensagemRede networkMessageToSend = new MensagemRede()
            {
                Message = "Please make a guess!",
                NetworkInstruction = InstrucaoRede.MakeMove
            };

            string networkMessageToSenndJsonString = JsonConvert.SerializeObject(networkMessageToSend);

            StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryWriter.Write(networkMessageToSenndJsonString);
        }

        public void ReceiveAnswer()
        {
            // We know that the server will send a JSON string
            // so we prepare the statement for it
            string answer = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryReader.ReadString();
            while (answer.Length == 0)
            {
                answer = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryReader.ReadString();
                Thread.Sleep(100);
            }
            Console.WriteLine(answer);
            MensagemRede MensagemRecebida = JsonConvert.DeserializeObject<MensagemRede>(answer);
            string CoordAtaque = MensagemRecebida.Message;

            Regex re1 = new Regex("(?<Alpha>[a-jA-J]+)(?<Numeric>[0-9]+)");
            Regex re2 = new Regex("(?<Numeric>[0-9]+)(?<Alpha>[a-jA-J]+)");
            Match result1 = re1.Match(CoordAtaque);
            Match result2 = re2.Match(CoordAtaque);
            string alphaPart = null;
            string NumPart = null;

            if (result1.Success)
            {
                alphaPart = result1.Groups["Alpha"].Value;
                NumPart = result1.Groups["Numeric"].Value;
            }
            else
            {
                alphaPart = result2.Groups["Alpha"].Value;
                NumPart = result2.Groups["Numeric"].Value;
            }
            string Mensagem = null;
            if (
                StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).CampoJogador[
                    Int32.Parse(NumPart), ContarLetras(alphaPart)] == char.Parse("+"))
            {
                Console.WriteLine("PUMBA NA BEIÇA");
                StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).CampoJogador[
                    Int32.Parse(NumPart), ContarLetras(alphaPart)] = char.Parse("X");
                StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).CampoInimigo[
                    Int32.Parse(NumPart), ContarLetras(alphaPart)] = char.Parse("X");
                Mensagem = CheckPlayerLife(Int32.Parse(NumPart), ContarLetras(alphaPart));
            }
            else
            {
                StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).CampoJogador[
                    Int32.Parse(NumPart), ContarLetras(alphaPart)] = char.Parse("O");
                StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).CampoInimigo[
                    Int32.Parse(NumPart), ContarLetras(alphaPart)] = char.Parse("O");
                Mensagem = "Falhou!";
            }
            Console.WriteLine(StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).PlayerName);
            MensagemRede networkMessageToSend = new MensagemRede()
            {
                Message = Mensagem,
                CampoJogador = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).CampoJogador,
                CampoInimigo = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).CampoInimigo
            };
            string networkMessageToSenndJsonString = JsonConvert.SerializeObject(networkMessageToSend);

            StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryWriter.Write(networkMessageToSenndJsonString);

            networkMessageToSend = new MensagemRede()
            {
                Message = Mensagem,
                CampoJogador = StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).CampoJogador,
                CampoInimigo = StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).CampoInimigo
            };
            networkMessageToSenndJsonString = JsonConvert.SerializeObject(networkMessageToSend);

            StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).BinaryWriter.Write(networkMessageToSenndJsonString);
            
            // TODO continuar esta parte
        }

        private string CheckPlayerLife(int x, int y)
        {
            for (int n = 0; n < 5; n++)
            {
                for (int i = 0; i < StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Vida; i++)
                {
                    if (StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Coordenadas[i,0] == x && StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Coordenadas[i,1] == y)
                    {
                        StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Vida--;
                        if (StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Vida == 0)
                        {
                            return "Afundou o " + StoreJogo.Instance.Jogo.PlayerList.Find(p => !p.Turn).Barcos[n].Nome;
                        }
                    }
                }
            }
            return "Acertou num navio"; //TODO VErificar se isto funciona!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        private int ContarLetras(string letra)
        {
            int i;
            char[] base26CharsM = "ABCDEFGHIJ".ToCharArray();
            char[] base26Charsm = "abcdefghij".ToCharArray();
            for (i = 0; i < 10; i++)
            {
                if (letra == base26CharsM[i % 10].ToString() || letra == base26Charsm[i % 10].ToString())
                {
                    return i;
                }
            }
            return i;
        }
    }
}
