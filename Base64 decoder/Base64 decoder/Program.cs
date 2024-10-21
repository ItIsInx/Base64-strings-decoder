using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Text;
using System.Linq;

namespace Base64_decoder
{
    internal static class Utils
    {
        public static bool IsBase64String(this string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length % 4 != 0)
                return false;

            foreach (char c in s)
            {
                if (!char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '=')
                    return false;
            }

            return true;
        }
    }
    internal class Program
    {
        private static readonly Logger logger = new Logger();
        static void Main(string[] args)
        {
            Console.Title = "Base64 strings encoding by itisinx";
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(
             @" 


           ____    _    ____  _____ __   _  _     ____  _____ ____ ___  ____  _____ ____
          | __ )  / \  / ___|| ____/ /_ | || |   |  _ \| ____/ ___/ _ \|  _ \| ____|  _ \
          |  _ \ / _ \ \___ \|  _|| '_ \| || |_  | | | |  _|| |  | | | | | | |  _| | |_) |
          | |_) / ___ \ ___) | |__| (_) |__   _| | |_| | |__| |__| |_| | |_| | |___|  _ <
          |____/_/   \_\____/|_____\___/   |_|   |____/|_____\____\___/|____/|_____|_| \_\

           using dnlib --By itisinx
             "
            );

            if (args.Length != 1)
            {
                logger.Log(LogLevel.Warn, "=> Drag n drop assembly!");
            }
            else
            {
                ModuleDefMD module = ModuleDefMD.Load(args[0]);
                logger.Log(LogLevel.Info, "=> Decoding strings....");
                Console.WriteLine("");
                int decodedStringCount = 0;
                foreach (TypeDef type in module.Types)
                {
                    foreach (MethodDef method in type.Methods.Where(m => m.HasBody))
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            var instructions = method.Body.Instructions;
                            Instruction instruction = method.Body.Instructions[i];
                            if (instruction.OpCode == OpCodes.Ldstr)
                            {
                                string originalString = instruction.Operand as string;
                                if (originalString.IsBase64String())
                                {
                                    try
                                    {
                                        byte[] base64Bytes = Convert.FromBase64String(originalString);
                                        string decodedString = Encoding.UTF8.GetString(base64Bytes);
                                        instructions[i - 1].OpCode = OpCodes.Ldstr;
                                        instructions[i - 1].Operand = decodedString;
                                        instructions.RemoveAt(i);
                                        instructions.RemoveAt(i);
                                        instructions.RemoveAt(i);
                                        decodedStringCount++;
                                        logger.Log(LogLevel.Success, $"=> Decoded: {originalString}");
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Log(LogLevel.Failure, $"=> Error decoding string: {originalString} - {ex.Message}");
                                        Console.ReadKey();
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("");
                logger.Log(LogLevel.Info, $"=> Decoded: {decodedStringCount} string, File saved!");
                module.Write(args[0].Replace(".exe", "-decoded.exe"), null);

            }
            Console.ReadKey();
        }
    }
}