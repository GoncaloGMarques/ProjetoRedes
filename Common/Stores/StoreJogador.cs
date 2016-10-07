using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Modelos;

namespace Common.Stores
{
    public sealed class StoreJogador
    {
        /// <summary>
        /// I do not know the exact purpose of this class for now. 
        /// It is a singleton to the player class, but I might
        /// have lost track of what I was thinking.
        /// Might get back to it later.
        /// </summary>
        private static StoreJogador _instance;
        public Jogador Jogador { get; set; }

        private StoreJogador()
        {
            Jogador = new Jogador();
        }

        public static StoreJogador Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StoreJogador();
                }
                return _instance;
            }
        }

        // TODO: Implement data model dispose method.
        // TODO: We should be able to clear all data saved in this
        // TODO: singleton with this method. Might be important later.
        public void Dispose()
        {
            // eg: player = null; playlist.cler(); etc...
        }
    }
}
