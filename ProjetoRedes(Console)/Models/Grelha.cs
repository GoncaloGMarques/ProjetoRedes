using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoRedes_Console_.Models
{
    class Grelha
    {
        private static char[] base26Chars = "ABCDEFGHIJ".ToCharArray();
        public static void DesenharGrelha(char[] campo)
        {
            for (int j = 0; j < 2; j++)
            {
                Console.Write("_");
                Console.Write("| ");
                for (int i = 0; i < 10; i++)
                {
                    Console.Write(i);
                    Console.Write(" | ");
                }
                Console.Write("     ");
            }
            Console.Write("\n");

            string returnValue = null;
            for (int i = 0; i < 10; i++)
            {
                returnValue = base26Chars[i % 20].ToString();
                for (int j = 0; j < 2; j++)
                {
                    int campoIt = 0;
                    Console.Write(returnValue + "|");
                    for (int y = 0; y < 10; y++)
                    {
                        Console.Write(" " + campo[campoIt] + " |");
                        campoIt++;
                    }
                    Console.Write("      ");
                }
                Console.Write("\n");
                for (int j = 0; j < 2; j++)
                {
                    for (int y = 0; y < 42; y++)
                    {
                        Console.Write("-");
                    }
                    Console.Write("      ");
                }
                Console.Write("\n");

            }
        }
    }
}
