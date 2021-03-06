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
using Newtonsoft.Json.Serialization;
using ProjetoRedes_Console_.Models;

namespace ProjetoRedes_Console_.Controller
{
    public class ClienteController
    {
        private EstadoJogador _estadoJogador;
        private InstrucaoRede _instrucaoRede;

        public ClienteController()
        {
            _estadoJogador = EstadoJogador.ReceivePlayerInformation;
            _instrucaoRede = InstrucaoRede.WaitConnection;
        }

        private TcpClient tcpClient;
        private BinaryReader binaryReader;
        private BinaryWriter binaryWriter;

        public void StartClient()
        {
            // Start udp client
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 7777);
            binaryWriter = new BinaryWriter(tcpClient.GetStream());
            binaryReader = new BinaryReader(tcpClient.GetStream());
            Console.SetWindowSize(Console.WindowWidth, 50);

            string MensagemRedeRecebidaJsonString;
            Jogador jogador = null;
            MensagemRede MensagemRedeRecebida = null;
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
                            InitBoats(jogador);
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
                            MensagemRedeRecebida =
                                JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);

                            // Temporary and simple validation indicating 
                            // that we received a positive connected state
                            // from the server
                            if (MensagemRedeRecebida.Connected)
                            {
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                jogador = MensagemRedeRecebida.Jogador;
                                _estadoJogador = EstadoJogador.JogoStarted;
                                ultimoMapaJogador = Grelha.InitCampoJogador();
                                ultimoMapaInimigo = Grelha.InitCampoInimigo();
                            }
                            else
                            {
                                Console.WriteLine("Not Connected");
                            }
                        }
                        break;
                    case EstadoJogador.JogoStarted:
                        switch (_instrucaoRede)
                        {
                            case InstrucaoRede.WaitConnection:
                                // Unserialize the JSON string to the object NetworkMessage
                                MensagemRedeRecebidaJsonString = binaryReader.ReadString();
                                if (pronto == true)
                                {
                                    Console.WriteLine("Queres barcos novos?\n 1- Sim\n 2- Escolher barcos novos.");
                                    string answer= null;
                                    while (answer != "1" || answer != "2")
                                    {
                                        answer = Console.ReadLine();
                                        if (answer == "1")
                                        {
                                            pronto = false;
                                        }
                                        else if(answer == "2")
                                        {
                                            pronto = true;
                                        }
                                    }
                                }
                                MensagemRedeRecebida =
                                    JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);
                                _instrucaoRede = MensagemRedeRecebida.NetworkInstruction;
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                break;
                            case InstrucaoRede.Wait:
                                // We know that the server will send a JSON string
                                // so we prepare the statement for it
                                MensagemRedeRecebidaJsonString = binaryReader.ReadString();
                                // Unserialize the JSON string to the object NetworkMessage
                                Console.Clear();
                                MensagemRedeRecebida =
                                    JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);
                                if (MensagemRedeRecebida.CampoJogador != null)
                                {
                                    ultimoMapaJogador = MensagemRedeRecebida.CampoJogador;
                                    ultimoMapaInimigo = MensagemRedeRecebida.CampoInimigo;
                                    
                                    Grelha.DesenharGrelha(ultimoMapaJogador, ultimoMapaInimigo);
                                }
                                else
                                {
                                    _instrucaoRede = MensagemRedeRecebida.NetworkInstruction;
                                }
                             
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                break;

                            case InstrucaoRede.PlacingBoats:
                                PlacingBoats(jogador);
                                _instrucaoRede = InstrucaoRede.Wait;
                                break;
                            case InstrucaoRede.MakeMove:
                                bool enviado = false;
                                while (!enviado)
                                {
                                    Console.WriteLine("Onde quer atacar?");
                                    Regex re1 = new Regex("(?<Alpha>[a-jA-J]+)(?<Numeric>[0-9]+)");

                                    Regex re2 = new Regex("(?<Numeric>[0-9]+)(?<Alpha>[a-jA-J]+)");
                                    string coord;
                                    coord = Console.ReadLine();
                                    Match result1 = re1.Match(coord);
                                    Match result2 = re2.Match(coord);
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
                                    if ((result2.Success || result1.Success) &&
                                        ultimoMapaInimigo[Int32.Parse(NumPart), ContarLetras(alphaPart)] ==
                                        char.Parse("~"))
                                    {
                                        MensagemRede mensagemRede = new MensagemRede()
                                        {
                                            Message = coord
                                        };
                                        string mensagemRedeJsonStrong = JsonConvert.SerializeObject(mensagemRede);
                                        _instrucaoRede = InstrucaoRede.Wait;

                                        binaryWriter.Write(mensagemRedeJsonStrong);
                                        enviado = true;
                                    }
                                    else if (_instrucaoRede != InstrucaoRede.MakeMove)
                                    {
                                        Console.WriteLine("Já tentou essa coordenada. Tente outra vez.");
                                    }
                                }
                                // Unserialize the JSON string to the object NetworkMessage
                                MensagemRedeRecebidaJsonString = binaryReader.ReadString();
                                // Unserialize the JSON string to the object NetworkMessage
                                MensagemRedeRecebida =
                                    JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);
                                if (MensagemRedeRecebida.CampoJogador != null)
                                {
                                    Console.Clear();
                                    ultimoMapaJogador = MensagemRedeRecebida.CampoJogador;
                                    ultimoMapaInimigo = MensagemRedeRecebida.CampoInimigo;
                                    Grelha.DesenharGrelha(ultimoMapaJogador, ultimoMapaInimigo);
                                }
                                _instrucaoRede = MensagemRedeRecebida.NetworkInstruction;
                                Console.WriteLine(MensagemRedeRecebida.Message);
                                break;

                            case InstrucaoRede.JogoEnded:
                                _estadoJogador = EstadoJogador.JogoEnded;
                                break;

                        }

                        break;
                    case EstadoJogador.JogoEnded: //TODO acabar o jogo
                    {
                        MensagemRedeRecebidaJsonString = binaryReader.ReadString();

                        // Unserialize the JSON string to the object NetworkMessage
                        MensagemRedeRecebida =
                            JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);
                        ultimoMapaJogador = MensagemRedeRecebida.CampoJogador;
                        ultimoMapaInimigo = MensagemRedeRecebida.CampoInimigo;
                        Grelha.DesenharGrelha(ultimoMapaJogador, ultimoMapaInimigo);
                        Console.WriteLine(MensagemRedeRecebida.Message);
                        
                        break;
                    }
                }
            }
        }

        private int ContarLetras(string letra)
        {
            int i;
            char[] base26CharsM = "ABCDEFGHIJ".ToCharArray();
            char[] base26Charsm = "abcdefghij".ToCharArray();
            for (i = 0; i < 10; i++)
            {
                if (letra == base26CharsM[i%10].ToString() || letra == base26Charsm[i%10].ToString())
                {
                    return i;
                }
            }
            return i;
        }

        private void InitBoats(Jogador jogador)
        {
            jogador.Barcos[0].Nome = "Porta Aviões";
            jogador.Barcos[0].Vida = 5;
            jogador.Barcos[0].Coordenadas = new int[jogador.Barcos[0].Vida, 2]; 
            jogador.Barcos[0].Colocado = false;
            jogador.Barcos[1].Nome = "Fragata";
            jogador.Barcos[1].Vida = 4;
            jogador.Barcos[1].Coordenadas = new int[jogador.Barcos[1].Vida, 2];
            jogador.Barcos[1].Colocado = false;
            jogador.Barcos[2].Nome = "Submarino";
            jogador.Barcos[2].Vida = 3;
            jogador.Barcos[2].Coordenadas = new int[jogador.Barcos[2].Vida, 2];
            jogador.Barcos[2].Colocado = false;
            jogador.Barcos[3].Nome = "Patrulha";
            jogador.Barcos[3].Vida = 3;
            jogador.Barcos[3].Coordenadas = new int[jogador.Barcos[3].Vida, 2];
            jogador.Barcos[3].Colocado = false;
            jogador.Barcos[4].Nome = "Torpedeiro";
            jogador.Barcos[4].Vida = 2;
            jogador.Barcos[4].Coordenadas = new int[jogador.Barcos[4].Vida, 2];
            jogador.Barcos[4].Colocado = false;
        }

        private void InitBoatsOnlyLife(Jogador jogador)
        {
            jogador.Barcos[0].Vida = 5;
            jogador.Barcos[1].Vida = 4;
            jogador.Barcos[2].Vida = 3;
            jogador.Barcos[3].Vida = 3;
            jogador.Barcos[4].Vida = 2;
        }


        private char[,] ultimoMapaJogador = new char[10, 10];
        private char[,] ultimoMapaInimigo = new char[10, 10];
        private bool pronto = false;

        private void PlacingBoats(Jogador jogador) 
        {
            Console.WriteLine("Para cancelar a posicao inicial de um barco, escreva 'Cancelar'");
            binaryWriter = new BinaryWriter(tcpClient.GetStream());
            binaryReader = new BinaryReader(tcpClient.GetStream());
            bool final;
            if (!pronto)
            {
                for (int i = 0; i < 5; i++)
                {
                    final = false;
                    if (i != 0)
                    {
                        if (jogador.Barcos[i - 1].Colocado)
                        {
                            Grelha.DesenharGrelha(ultimoMapaJogador, ultimoMapaInimigo);
                        }
                    }

                    while (jogador.Barcos[i].Colocado == false && _instrucaoRede != InstrucaoRede.WaitConnection)
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
                                if (ultimoMapaJogador[
                                        Int32.Parse(NumPart), ContarLetras(alphaPart)] != char.Parse("+"))
                                {
                                    jogador.Barcos[i].Coordenadas[0, 0] = Int32.Parse(NumPart);
                                    jogador.Barcos[i].Coordenadas[0, 1] = ContarLetras(alphaPart);
                                    ultimoMapaJogador[
                                            jogador.Barcos[i].Coordenadas[0, 0], jogador.Barcos[i].Coordenadas[0, 1]] =
                                        char.Parse("+");
                                    final = true;
                                }
                                else
                                {
                                    Console.WriteLine("Essa posicao ja está ocupada");
                                }
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
                                int posicaoDesejadaX = Int32.Parse(NumPart);
                                int posicaoDesejadaY = ContarLetras(alphaPart);
                                bool possivel = true;
                                if ((posicaoDesejadaX == jogador.Barcos[i].Coordenadas[0, 0] &&
                                     (posicaoDesejadaY - (jogador.Barcos[i].Vida - 1) ==
                                      jogador.Barcos[i].Coordenadas[0, 1] ||
                                      posicaoDesejadaY + (jogador.Barcos[i].Vida - 1) ==
                                      jogador.Barcos[i].Coordenadas[0, 1])))
                                {
                                    for (int y = 1; y < jogador.Barcos[i].Vida; y++)
                                    {
                                        if (posicaoDesejadaY > jogador.Barcos[i].Coordenadas[0, 1])
                                        {
                                            if (ultimoMapaJogador[
                                                    posicaoDesejadaX, jogador.Barcos[i].Coordenadas[0, 1] + y] ==
                                                char.Parse("+"))
                                            {
                                                possivel = false;
                                            }
                                        }
                                        else
                                        {
                                            if (ultimoMapaJogador[
                                                    posicaoDesejadaX, jogador.Barcos[i].Coordenadas[0, 1] - y] ==
                                                char.Parse("+"))
                                            {
                                                possivel = false;
                                            }
                                        }

                                    }
                                    if (possivel)
                                    {
                                        for (int y = 1; y < jogador.Barcos[i].Vida; y++)
                                        {
                                            if (posicaoDesejadaY > jogador.Barcos[i].Coordenadas[0, 1])
                                            {
                                                jogador.Barcos[i].Coordenadas[y, 0] = posicaoDesejadaX;
                                                jogador.Barcos[i].Coordenadas[y, 1] =
                                                    jogador.Barcos[i].Coordenadas[0, 1] +
                                                    y;
                                                ultimoMapaJogador[
                                                        jogador.Barcos[i].Coordenadas[y, 0],
                                                        jogador.Barcos[i].Coordenadas[y, 1]
                                                    ] =
                                                    char.Parse("+");
                                            }
                                            else
                                            {

                                                jogador.Barcos[i].Coordenadas[y, 0] = posicaoDesejadaX;
                                                jogador.Barcos[i].Coordenadas[y, 1] =
                                                    jogador.Barcos[i].Coordenadas[0, 1] -
                                                    y;
                                                ultimoMapaJogador[
                                                        jogador.Barcos[i].Coordenadas[y, 0],
                                                        jogador.Barcos[i].Coordenadas[y, 1]
                                                    ] =
                                                    char.Parse("+");

                                            }
                                            jogador.Barcos[i].Colocado = true;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Essa posicao ja está ocupada");
                                    }
                                }
                                else if ((posicaoDesejadaY == jogador.Barcos[i].Coordenadas[0, 1] &&
                                          (posicaoDesejadaX - (jogador.Barcos[i].Vida - 1) ==
                                           jogador.Barcos[i].Coordenadas[0, 0] ||
                                           posicaoDesejadaX + (jogador.Barcos[i].Vida - 1) ==
                                           jogador.Barcos[i].Coordenadas[0, 0])))
                                {

                                    for (int x = 1; x < jogador.Barcos[i].Vida; x++)
                                    {
                                        if (posicaoDesejadaX > jogador.Barcos[i].Coordenadas[0, 0])
                                        {
                                            if (ultimoMapaJogador[
                                                    jogador.Barcos[i].Coordenadas[0, 0] + x, posicaoDesejadaY] ==
                                                char.Parse("+"))
                                            {
                                                possivel = false;
                                            }
                                        }
                                        else
                                        {
                                            if (ultimoMapaJogador[
                                                    jogador.Barcos[i].Coordenadas[0, 0] - x, posicaoDesejadaY] ==
                                                char.Parse("+"))
                                            {
                                                possivel = false;
                                            }
                                        }
                                    }
                                    if (possivel)
                                    {
                                        for (int x = 1; x < jogador.Barcos[i].Vida; x++)
                                        {
                                            if (posicaoDesejadaX > jogador.Barcos[i].Coordenadas[0, 0])
                                            {
                                                jogador.Barcos[i].Coordenadas[x, 0] =
                                                    jogador.Barcos[i].Coordenadas[0, 0] +
                                                    x;
                                                jogador.Barcos[i].Coordenadas[x, 1] = posicaoDesejadaY;
                                                ultimoMapaJogador[
                                                        jogador.Barcos[i].Coordenadas[x, 0],
                                                        jogador.Barcos[i].Coordenadas[x, 1]
                                                    ] =
                                                    char.Parse("+");
                                            }
                                            else
                                            {
                                                jogador.Barcos[i].Coordenadas[x, 0] =
                                                    jogador.Barcos[i].Coordenadas[0, 0] -
                                                    x;
                                                jogador.Barcos[i].Coordenadas[x, 1] = posicaoDesejadaY;
                                                ultimoMapaJogador[
                                                        jogador.Barcos[i].Coordenadas[x, 0],
                                                        jogador.Barcos[i].Coordenadas[x, 1]
                                                    ] =
                                                    char.Parse("+");
                                            }

                                        }
                                        jogador.Barcos[i].Colocado = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Essa posicao ja está ocupada");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(
                                        "Nao e possivel colocar o navio nessa posicao, tenha em atencao que o navio tem " +
                                        jogador.Barcos[i].Vida + " blocos de tamanho.");
                                }

                            }
                            else if (coord == "cancelar" || coord == "Cancelar")
                            {
                                ultimoMapaJogador[
                                        jogador.Barcos[i].Coordenadas[0, 0], jogador.Barcos[i].Coordenadas[0, 1]] =
                                    char.Parse("-");
                                final = false;
                            }
                            else
                            {
                                Console.WriteLine("Essa coordenada nao existe");
                            }
                        }
                    }
                    pronto = true;
                }
            }
            Console.WriteLine("Passou");
            string MensagemRedeRecebidaJsonString;
            MensagemRede MensagemRedeRecebida;
            
            MensagemRedeRecebida = new MensagemRede()
            {
                CampoJogador = ultimoMapaJogador, 
                CampoInimigo = ultimoMapaInimigo,
                Pronto = true,
                Jogador = jogador
            };
            // Serialize the NetworkMessage object to a JSON string
            MensagemRedeRecebidaJsonString =
                JsonConvert.SerializeObject(MensagemRedeRecebida);

            binaryWriter.Write(MensagemRedeRecebidaJsonString);
            
            _instrucaoRede = InstrucaoRede.WaitConnection;

            //MensagemRedeRecebidaJsonString = binaryReader.ReadString();

            //// Unserialize the JSON string to the object NetworkMessage
            //MensagemRedeRecebida =
            //    JsonConvert.DeserializeObject<MensagemRede>(MensagemRedeRecebidaJsonString);
            //ultimoMapaJogador = MensagemRedeRecebida.CampoJogador;
            //ultimoMapaInimigo = MensagemRedeRecebida.CampoInimigo;

        }
    }
}
