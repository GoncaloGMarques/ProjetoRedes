using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Modelos;

namespace Common.Stores
{
    public sealed class StoreJogo
    {
         /// <summary>
        /// This is a singleton allowing us to access the Jogo datamodel.
        /// With this, we can make sure that we do not loose information
        /// because of the garbage collector, and we also make sure that
        /// there is not more than one instances of the same object
        /// </summary>
        private static StoreJogo _instance;
        public Jogo Jogo { get; set; }

        private StoreJogo()
        {
            Jogo = new Jogo();
        }

        public static StoreJogo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StoreJogo();
                }
                return _instance;
            }
        }

        // TODO: Implement data model dispose method.
        // TODO: We should be able to clear all data saved in this
        // TODO: singleton with this method. Might be important later.
        public void Dispose()
        {
            
        }
    }
}
