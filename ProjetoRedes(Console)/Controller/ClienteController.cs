﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Modelos;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Common.Stores;

namespace ProjetoRedes_Console_.Controller
{
    public class ClienteController
    {
        private EstadoJogador _estadoJogador;
        private InstrucaoRede _instrucaoRede;
        public ClienteController()
        {
            _estadoJogador = EstadoJogador.ReceivePlayerInformation;
            _instrucaoRede = InstrucaoRede.Wait;
        }
            
        public void StartClient()
        {
            // Start udp client
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 7777);
            BinaryWriter binaryWriter = new BinaryWriter(tcpClient.GetStream());
            BinaryReader binaryReader = new BinaryReader(tcpClient.GetStream());

            string MensagemRedeRecebidaJsonString;
            MensagemRede MensagemRedeRecebida;
            Jogador jogador = null;
            // This while::cycle works like a game cycle.
            // A boolean should take it's place so that the client
            // can exit the cycle when it wants.
            // It is here to make sure that the client can make
            // several communications with the server.
            while (true)
            {
                switch (_estadoJogador)
                {  
                    case EstadoJogador.ReceivePlayerInformation:
                        Console.WriteLine("Input player name:");
                        // Here we initiate a new Player class with 
                        // information from the player input (player name)
                        jogador = new Jogador()
                        {
                            PlayerName = Console.ReadLine()
                        };

                        // Check player name for null
                        if (!string.IsNullOrEmpty(jogador.PlayerName))
                        {
                            // Create a new instance of a class
                            // called networkMessage and set the current
                            // player in that model so that we can send
                            // this object to the server
                            MensagemRede mensagemRede = new MensagemRede()
                            {
                                Jogador = jogador
                            };

                            // Serialize the NetworkMessage object to a JSON string
                            string mensagemRedeJsonStrong = JsonConvert.SerializeObject(mensagemRede);
                            binaryWriter.Write(mensagemRedeJsonStrong);

                            // We know that the server will send a JSON string
                            // so we prepare the statement for it
                            MensagemRedeRecebidaJsonString = binaryReader.ReadString();

                            // Unserialize the JSON string to the object NetworkMessage
                            MensagemRedeRecebida = JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);

                            // Temporary and simple validation indicating 
                            // that we received a positive connected state
                            // from the server
                            if (MensagemRedeRecebida.Connected)
                            {
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                jogador = MensagemRedeRecebida.Jogador;
                                _estadoJogador = EstadoJogador.JogoPlacingBoats;
                                InitBoats(jogador);
                            }
                            else
                            {
                                Console.WriteLine("Not Connected");
                            }
                        }
                        break;
                    case EstadoJogador.JogoPlacingBoats:
                        for (int i = 0; i < 4; i++)
                        {
                            bool final = false;
                            while (jogador.Barcos[i].Colocado == false)
                            {
                                if (!final)
                                {
                                    Regex re1 = new Regex("(?<Alpha>[a-jA-J]+)(?<Numeric>[0-9]+)");
                                    Regex re2 = new Regex("(?<Numeric>[0-9]+)(?<Alpha>[a-jA-J]+)");
                                    string coord;
                                    Console.WriteLine("Introduza as coordenadas iniciais do " + jogador.Barcos[i].Nome);
                                    coord = Console.ReadLine();
                                    Match result1 = re1.Match(coord);
                                    Match result2 = re2.Match(coord);
                                    string alphaPart = null;
                                    string NumPart = null;
                                    if (result2.Success || result1.Success)
                                    {
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
                                        jogador.Barcos[i].Coordenadas[0, 0] = contarLetras(alphaPart);
                                        jogador.Barcos[i].Coordenadas[0, 1] = Int32.Parse(NumPart);
                                        MensagemRede networkMessageToSend = new MensagemRede()
                                        {
                                            Coordenadas = new[] { jogador.Barcos[i].Coordenadas[0, 0], jogador.Barcos[i].Coordenadas[0, 1] }
                                        };

                                        // Serialize the NetworkMessage object to a JSON string
                                        string networkMessageToSendJsonString = JsonConvert.SerializeObject(networkMessageToSend);

                                        binaryWriter.Write(networkMessageToSendJsonString);
                                        final = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Essa coordenada nao existe");
                                    }
                                }
                                else
                                {
                                    Regex re1 = new Regex("(?<Alpha>[a-jA-J]+)(?<Numeric>[0-9]+)");
                                    Regex re2 = new Regex("(?<Numeric>[0-9]+)(?<Alpha>[a-jA-J]+)");
                                    string coord;
                                    Console.WriteLine("Introduza as coordenadas do ultimo bloco do " + jogador.Barcos[i].Nome);
                                    coord = Console.ReadLine();
                                    Match result1 = re1.Match(coord);
                                    Match result2 = re2.Match(coord);
                                    string alphaPart = null;
                                    string NumPart = null;
                                    if (result2.Success || result1.Success)
                                    {
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
                                        int posicaoDesejadaX = contarLetras(alphaPart);
                                        int posicaoDesejadaY = Int32.Parse(NumPart);
                                        if ((posicaoDesejadaX == jogador.Barcos[i].Coordenadas[0, 0] &&
                                             (posicaoDesejadaY - (jogador.Barcos[i].Vida - 1) ==
                                              jogador.Barcos[i].Coordenadas[0, 1] ||
                                              posicaoDesejadaY + (jogador.Barcos[i].Vida - 1) ==
                                              jogador.Barcos[i].Coordenadas[0, 1])))
                                        {
                                            for (int y = 1; y < jogador.Barcos[i].Vida; y++)
                                            {
                                                jogador.Barcos[i].Coordenadas[y, 0] = posicaoDesejadaX;
                                                jogador.Barcos[i].Coordenadas[y, 1] = jogador.Barcos[i].Coordenadas[0, 1] + y;

                                            }
                                            jogador.Barcos[i].Colocado = true;
                                        }
                                        else if ((posicaoDesejadaY == jogador.Barcos[i].Coordenadas[0, 1] &&
                                             (posicaoDesejadaX - (jogador.Barcos[i].Vida - 1) ==
                                              jogador.Barcos[i].Coordenadas[0, 0] ||
                                              posicaoDesejadaX + (jogador.Barcos[i].Vida - 1) ==
                                              jogador.Barcos[i].Coordenadas[0, 0])))
                                        {
                                            for (int x = 1; x < jogador.Barcos[i].Vida; x++)
                                            {
                                                jogador.Barcos[i].Coordenadas[x, 0] = jogador.Barcos[i].Coordenadas[0, 0] + x;
                                                jogador.Barcos[i].Coordenadas[x, 1] = posicaoDesejadaY;
                                            }
                                            jogador.Barcos[i].Colocado = true;
                                        }
                                        else
                                        {
                                            
                                            Console.WriteLine("Nao e possivel colocar o navio nessa posicao, tenha em atencao que o navio tem "+jogador.Barcos[i].Vida+" blocos de tamanho.");
                                        }
                                        
                                    }
                                    else
                                    {
                                        Console.WriteLine("Essa coordenada nao existe");
                                    }
                                }

                            }
                        }
                        break;
                    case EstadoJogador.JogoStarted:
                        switch (_instrucaoRede)
                        {
                            case InstrucaoRede.Wait:
                                // We know that the server will send a JSON string
                                // so we prepare the statement for it
                                MensagemRedeRecebidaJsonString = binaryReader.ReadString();

                                // Unserialize the JSON string to the object NetworkMessage
                                MensagemRedeRecebida =
                                    JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);

                                _instrucaoRede = MensagemRedeRecebida.NetworkInstruction;
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                break;
                            case InstrucaoRede.MakeMove:
                                int answer = int.Parse(Console.ReadLine());
                                binaryWriter.Write(answer);
                                _instrucaoRede = InstrucaoRede.Wait;
                                break;
                            case InstrucaoRede.JogoEnded:
                                _estadoJogador = EstadoJogador.JogoEnded;
                                break;
                        }

                        break;
                    case EstadoJogador.JogoEnded:
                        Thread.Sleep(100);
                        break;
                }
            }
        }

        private void InitBoats(Jogador jogador)
        {
            jogador.Barcos[0].Nome = "Porta Aviões";
            jogador.Barcos[0].Vida = 4;
            jogador.Barcos[0].Coordenadas = new int[jogador.Barcos[0].Vida,2];//TODO fix this shit
            jogador.Barcos[0].Colocado = false;
            jogador.Barcos[1].Nome = "Fragata";
            jogador.Barcos[1].Vida = 3;
            jogador.Barcos[1].Coordenadas = new int[jogador.Barcos[1].Vida, 2]; 
            jogador.Barcos[1].Colocado = false;
            jogador.Barcos[2].Nome = "Submarino";
            jogador.Barcos[2].Vida = 2;
            jogador.Barcos[2].Coordenadas = new int[jogador.Barcos[2].Vida, 2]; 
            jogador.Barcos[2].Colocado = false;
            jogador.Barcos[3].Nome = "Patrulha";
            jogador.Barcos[3].Vida = 1;
            jogador.Barcos[3].Coordenadas = new int[jogador.Barcos[3].Vida, 2];
            jogador.Barcos[3].Colocado = false;
        }

        private int contarLetras(string letra)
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
