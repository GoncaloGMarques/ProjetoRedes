﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Modelos;
using Common.Stores;
using Newtonsoft.Json;
using Servidor.Configuracoes;

namespace Servidor.Controller
{
    public class ServidorController
    {
        private JogoController _jogoController;

        /// <summary>
        /// Class responsible for controlling all communications with
        /// the server.
        /// Maybe we should consider refactoring some of this code
        /// and makeing the gamestate loop integrate the gamecontroller
        /// </summary>
        public ServidorController()
        {
            // Initiate an empty playerlist
            StoreJogo.Instance.Jogo.PlayerList = new List<Jogador>();
            // Indicate that the gamestate is ConnectionClosed
            // Meaning that we are currentlly not accepting any connections
            // to the server
            StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.ConnectionClosed;
            StoreJogo.Instance.Jogo.ConnectingPlayers = 0;
        }

        public void StartServer()
        {
            // Initiate an tcp Listener
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7777);
            tcpListener.Start();

            // Indicate that the gamestate is ConnectionOpen
            // Meaning that we now accept client connections
            StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.ConnectionOpen;
            Console.WriteLine("Server Connections Oppened");

            // this works like a gameloop making sure that the server 
            // never stops running
            while (true)
            {
                // Find which game state is the game running
                switch (StoreJogo.Instance.Jogo.EstadoJogo)
                {
                    case EstadoJogo.ConnectionClosed:
                        // Connections are closed
                        // TODO: The client should timeout trying
                        // TODO: to connect to this server.
                        // If we do accept a client connection
                        // we should consider changing the name of
                        // connectionclosed to some other name since
                        // we would need a connection to notify the
                        // client that the connections are closed
                        // Thread.Sleep makes sure the thread "sleeps" for 100ms
                        break;
                    case EstadoJogo.ConnectionOpen:
                        // Connection state is open, players may connect
                        // to the server and inform the server of their information
                        if (StoreJogo.Instance.Jogo.ConnectingPlayers < ServidorConfig.MaxPlayers)
                        {
                            if (tcpListener.Pending())
                            {
                                TcpClient tcpClient = tcpListener.AcceptTcpClient();

                                Thread newThread = new Thread(new ParameterizedThreadStart(ProcessClient));
                                newThread.Start(tcpClient);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                        break;
                    case EstadoJogo.JogoInitializing:
                        // In this state we should prepare everything for the game
                        // define the number that the player should try to guess
                        // Define who plays first
                        _jogoController = new JogoController();
                        Thread checlThread = new Thread(new ThreadStart(CheckConnections));
                        checlThread.Start();
                        break;
                    case EstadoJogo.JogoStarted:
                        // All the game process should be processed here
                        if (StoreJogo.Instance.Jogo.PlayerList.Last().ProntoJogador &&
                            StoreJogo.Instance.Jogo.PlayerList.First().ProntoJogador)
                        {
                            _jogoController.NextTurn();
                            _jogoController.AskPlayerToPlay();
                            _jogoController.ReceiveAnswer();
                        }
                        else
                        {
                            _jogoController.PlacingBoats();
                        }
                        break;
                    case EstadoJogo.JogoEnded:
                        // Notify all players that the game ended
                        // Show some stats?
                        // clear data models and repeat
                        // Maybe allow game to have a "payback" mode

                        Thread.Sleep(100);
                        Console.WriteLine("tada"); //todo acabar jogo
                        MensagemRede mensagemRede = new MensagemRede()
                        {
                            CampoJogador = StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida == 0).CampoJogador,
                            CampoInimigo = StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida != 0).CampoJogador,
                            Message =
                                "O jogador " + StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida != 0).PlayerName +
                                "ganhou!"
                        };
                        string memensagemRedeString = JsonConvert.SerializeObject(mensagemRede);
                        StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida == 0).BinaryWriter.Write(memensagemRedeString);

                        mensagemRede = new MensagemRede()
                        {
                            CampoJogador = StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida != 0).CampoJogador,
                            CampoInimigo = StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida == 0).CampoJogador,
                            Message =
                                "Ganhaste!"
                        };
                        memensagemRedeString = JsonConvert.SerializeObject(mensagemRede);
                        StoreJogo.Instance.Jogo.PlayerList.Find(j => j.Vida != 0).BinaryWriter.Write(memensagemRedeString);
                        break;
                }
                
            }
        }

        private void CheckConnections()
        {
            while (true)
            {
                foreach (Jogador jogador in StoreJogo.Instance.Jogo.PlayerList)
                {
                    if (jogador.TcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        try
                        {
                            jogador.TcpClient.Client.Receive(buff, SocketFlags.Peek);
                        }
                        catch (SocketException)
                        {
                            StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.ConnectionOpen;
                            StoreJogo.Instance.Jogo.ConnectingPlayers--;
                            MensagemRede mensagemRede = new MensagemRede()
                            {
                                Message =
                                    StoreJogo.Instance.Jogo.PlayerList.Find(j => !j.TcpClient.Connected).PlayerName +
                                    " fechou a conexao, por favor aguarde.",
                                NetworkInstruction = InstrucaoRede.WaitConnection
                            };
                            string mensagemRedeString = JsonConvert.SerializeObject(mensagemRede);
                            StoreJogo.Instance.Jogo.PlayerList.Find(j => j.TcpClient.Connected).BinaryWriter.Write(mensagemRedeString);
                            StoreJogo.Instance.Jogo.PlayerList.Remove(StoreJogo.Instance.Jogo.PlayerList.Find(j=> j== jogador));
                            Console.WriteLine("Players " + StoreJogo.Instance.Jogo.PlayerList.Count + "/" + ServidorConfig.MaxPlayers);
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }
                if (StoreJogo.Instance.Jogo.EstadoJogo == EstadoJogo.ConnectionOpen)
                {
                    break;
                }
            }
            
        }

        private void ProcessClient(object tcpClientObject)
        {
            TcpClient tcpClient = tcpClientObject as TcpClient;
            if (tcpClient == null)
            {
                return;
            }

            BinaryWriter binaryWriter = new BinaryWriter(tcpClient.GetStream());
            BinaryReader binaryReader = new BinaryReader(tcpClient.GetStream());


            // We know that the server will send a JSON string
            // so we prepare the statement for it
            string message = binaryReader.ReadString();

            // Unserialize the JSON string to the object NetworkMessage
            MensagemRede receivedNetworkMessage = JsonConvert.DeserializeObject<MensagemRede>(message);

            // We create a new player object with information
            // from the client message sush as its' name
            Jogador player = new Jogador()
            {
                Id = Guid.NewGuid(),
                PlayerName = receivedNetworkMessage.Jogador.PlayerName,
                Turn = false,
                TcpClient = tcpClient,
                BinaryReader = binaryReader,
                BinaryWriter = binaryWriter,
                Barcos = receivedNetworkMessage.Jogador.Barcos
            };

            // We add the player to the player list
            StoreJogo.Instance.Jogo.PlayerList.Add(player);

            // We create a new networkmessage object to
            // send back to the client notifying the client
            // that everything is ok
            MensagemRede networkMessageToSend = new MensagemRede()
            {
                Connected = true,
                Message = "Welcome to the game!",
                Jogador = player,
                NetworkInstruction = InstrucaoRede.WaitConnection
            };

            // Serialize the NetworkMessage object to a JSON string
            string networkMessageToSendJsonString = JsonConvert.SerializeObject(networkMessageToSend);

            binaryWriter.Write(networkMessageToSendJsonString);
            StoreJogo.Instance.Jogo.ConnectingPlayers++;
            // We check for the current number of players
            // to see if we can start the game
            if (StoreJogo.Instance.Jogo.ConnectingPlayers == ServidorConfig.MaxPlayers)
            {
                StoreJogo.Instance.Jogo.EstadoJogo = EstadoJogo.JogoInitializing;
            }

            // Display some information to the server console
            Console.WriteLine("Player" + receivedNetworkMessage.Jogador.PlayerName + " entered the game");
            Console.WriteLine("Players " + StoreJogo.Instance.Jogo.PlayerList.Count + "/" + ServidorConfig.MaxPlayers);
        }
    }
}
