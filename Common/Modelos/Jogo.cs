using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Modelos
{
    public class Jogo
    {
        public List<Jogador> PlayerList { get; set; }
        public int ConnectingPlayers { get; set; }
        public List<Jogada> MoveList { get; set; }
        public EstadoJogo EstadoJogo { get; set; }
    }
}
