using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servidor.Controller;

namespace Servidor
{
    class Servidor
    {
        static void Main(string[] args)
        {
            ServidorController servidorController = new ServidorController();
            servidorController.StartServer();
        }
    }
}
