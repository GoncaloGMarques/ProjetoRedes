using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.Modelos
{
    public class Jogador
    {
        public Guid Id { get; set; }
        public string PlayerName { get; set; }
        public bool Turn { get; set; }
        public Barcos[] Barcos = new Barcos[4];
        public char[,] CampoJogador = new char[10,10];
        public char[,] CampoInimigo = new char[10,10];
        public bool ProntoJogador = false;
        [JsonIgnore]
        public TcpClient TcpClient { get; set; }
        [JsonIgnore]
        public BinaryWriter BinaryWriter { get; set; }
        [JsonIgnore]
        public BinaryReader BinaryReader { get; set; }
    }
}
