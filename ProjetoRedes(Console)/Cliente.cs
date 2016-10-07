using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoRedes_Console_.Controller;

namespace ProjetoRedes_Console_
{
    class Cliente
    {
        static char[] campo = new char[100];
        static void Main(string[] args)
        {
            Console.WindowWidth = 100;
            ClienteController clienteController = new ClienteController();
            clienteController.StartClient();
            Console.ReadLine();
        }
    }
}
