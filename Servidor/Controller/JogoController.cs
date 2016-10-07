using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.JogoPlacingBoats;
        }

        public void PlacingBoats()
        {
            foreach (Jogador jogador in StoreJogo.Instance.Jogo.PlayerList)
            {
                string message = jogador.BinaryReader.ReadString();
                while (message == null)
                {
                    message = jogador.BinaryReader.ReadString();
                    Thread.Sleep(100);
                }


                // Unserialize the JSON string to the object NetworkMessage
                MensagemRede receivedNetworkMessage = JsonConvert.DeserializeObject<MensagemRede>(message);
                Console.Write(receivedNetworkMessage.Coordenadas[0]);
                Console.WriteLine(receivedNetworkMessage.Coordenadas[1]);
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

        //public void ReceiveAnswer()
        //{
        //    // We know that the server will send a JSON string
        //    // so we prepare the statement for it
        //    int answer = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryReader.Read();
        //    while(answer == 0)
        //    {
        //        answer = StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).BinaryReader.Read();
        //        Thread.Sleep(100);
        //    }

        //    string hint = string.Empty;
        //    InstrucaoRede targetNetworkInstruction = InstrucaoRede.Wait;
        //    if (answer == StoreJogo.Instance.Jogo.TargetNumber)
        //    {
        //        hint = "O jogador acertou no número correto!";
        //        StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.JogoEnded;
        //        targetNetworkInstruction = InstrucaoRede.JogoEnded;
        //    } else if(answer < StoreJogo.Instance.Jogo.TargetNumber)
        //    {
        //        hint = "O número é superior ao valor introduzido";
        //    }
        //    else if (answer > StoreJogo.Instance.Jogo.TargetNumber)
        //    {
        //        hint = "O número é inferior ao valor introduzido";
        //    }


        //    MensagemRede networkMessageToSend = new MensagemRede()
        //    {
        //        Message = "O jogador " + StoreJogo.Instance.Jogo.PlayerList.Find(p => p.Turn).PlayerName + " tentou "+ answer + "\n" + hint,

        //        NetworkInstruction = targetNetworkInstruction
        //    };

        //    string networkMessageToSenndJsonString = JsonConvert.SerializeObject(networkMessageToSend);
            
        //    foreach (Jogador player in StoreJogo.Instance.Jogo.PlayerList)
        //    {
        //        player.BinaryWriter.Write(networkMessageToSenndJsonString);
        //    }

        //    //TODO: Log player moves
        //}
    }
}
