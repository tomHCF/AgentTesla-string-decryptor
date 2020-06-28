using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using System.Security.Cryptography;
using System.IO;

namespace AgentTeslaStringDecryptor
{
    class Program
    {
        public static string decryptString(byte[] encData)
        {

            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            int encStrLen = encData.Length - 48;
            byte[] encStr = new byte[encStrLen];
            Array.Copy(encData, 0, key, 0, 32);
            Array.Copy(encData, 32, iv, 0, 16);
            Array.Copy(encData, 48, encStr, 0, encStrLen);
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = key;
            rijndael.IV = iv;
            return Encoding.UTF8.GetString(rijndael.CreateDecryptor().TransformFinalBlock(encStr, 0, encStrLen));
        }
        public static TypeDef FindMaxTypeByFields(List<TypeDef> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("Empty List");
            }
            int maxFieldCount = 0;
            foreach (TypeDef type in list)
            {
                maxFieldCount = Math.Max(maxFieldCount, type.Fields.Count);
            }
            return list.FirstOrDefault(TypeDef => TypeDef.Fields.Count == maxFieldCount);
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: AgentTeslaStringDecryptor.exe <pathToFile>");
                System.Environment.Exit(1);
            }
            string filename = args[0];
            Console.WriteLine("Dercypting strings from binary: {0}", filename);
            try
            {
                ModuleDefMD mod = ModuleDefMD.Load(filename);
                TypeDef typeDefMD = FindMaxTypeByFields(mod.GetTypes().ToList());
                Console.WriteLine("Module name with max nuber of fields: {0}, numbers of fields: {1}", typeDefMD.Name, typeDefMD.Fields.Count);
                StreamWriter sw = File.CreateText("decStringsOut.txt");
                for (int i = 1; i < typeDefMD.Fields.Count; i++)
                {
                    if (typeDefMD.Fields.ElementAt(i).InitialValue is System.Byte[])
                        sw.WriteLine("{0}  {1}", i, decryptString(typeDefMD.Fields.ElementAt(i).InitialValue));
                }
                sw.Close();
                Console.WriteLine("Done, enter key ...");
                Console.ReadKey();
            }
            catch (BadImageFormatException e)
            {
                Console.WriteLine("Load binary error: {0}", e.Message);
                Console.WriteLine(".Net binary is required");
            }

        }
    }
}
