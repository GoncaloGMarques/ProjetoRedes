using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.Modelos
{
    public class MensagemRede
    {
        [JsonProperty("Message")]
        public string Message { get; set; }
        [JsonProperty("Connected")]
        public bool Connected { get; set; }
        [JsonProperty("Player")]
        public Jogador Jogador { get; set; }

        [JsonProperty("Coordenadas")]
        public int[][] Coordenadas { get; set; }
        public InstrucaoRede NetworkInstruction { get; set; }
    }
}
