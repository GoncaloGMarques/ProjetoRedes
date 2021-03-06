﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoRedes_Console_.Models
{
    class Grelha
    {
        private static char[] base26Chars = "ABCDEFGHIJ".ToCharArray();
        public static void DesenharGrelha(char[,] campo, char[,] campoInimigo)
        {
            for (int j = 0; j < 2; j++)
            {
                Console.Write("_");
                Console.Write("| ");
                for (int i = 0; i < 10; i++)
                {   Console.ForegroundColor = ConsoleColor.DarkGreen; //todo dar cor as coisas e preencher o "mar"
                    Console.Write(i);
                    Console.ResetColor();
                    Console.Write(" | ");
                }
                Console.Write("     ");
            }
            Console.Write("\n");

            string returnValue = null;
            for (int i = 0; i < 10; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                returnValue = base26Chars[i % 20].ToString();
                Console.ResetColor();
                for (int j = 0; j < 2; j++)
                {
                    if (j == 0)
                    {
                        int campoIt = 0;
                        Console.Write(returnValue);
                        for (int y = 0; y < 10; y++)
                        {
                            Console.Write("| ");
                            if (campo[y, i] == Char.Parse("+"))
                            { Console.ForegroundColor = ConsoleColor.Red; }
                            if (campo[y, i] == Char.Parse("~"))
                            { Console.ForegroundColor = ConsoleColor.Blue; }
                            if (campo[y, i] == Char.Parse("X"))
                            { Console.ForegroundColor = ConsoleColor.Red; }
                            Console.Write(campo[y, i]);
                            Console.ResetColor();
                            Console.Write(" ");
                            campoIt++;
                        }
                        Console.Write("      ");

                    }
                    if (j == 1)
                    {
                        int campoIt = 0;
                        Console.Write(returnValue + "|");
                        for (int y = 0; y < 10; y++)
                        {
                            Console.Write("| ");
                            if (campoInimigo[y, i] == Char.Parse("X"))
                            { Console.ForegroundColor = ConsoleColor.Red; }
                            if (campoInimigo[y, i] == Char.Parse("~"))
                            { Console.ForegroundColor = ConsoleColor.Blue; }
                            Console.Write(campoInimigo[y, i]);
                            Console.ResetColor();
                            Console.Write(" ");
                            campoIt++;
                        }
                    }

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

        public static char[,] InitCampoJogador()
        {
            char[,] campo = new char[10,10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    campo[i, j] = Char.Parse("~");
                }
            }
            return campo;
        }
        public static char[,] InitCampoInimigo()
        {
            char[,] campo = new char[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    campo[i, j] = Char.Parse("~");
                }
            }
            return campo;
        }
    }
}
