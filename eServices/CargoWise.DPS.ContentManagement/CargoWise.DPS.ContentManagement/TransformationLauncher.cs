using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using Parsing.CommandLineParsing;


namespace CargoWise.DPS.ContentManagement
{
    public class TransformationLauncher
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineDictionary d = CommandLineDictionary.FromArguments(args);

                Console.WriteLine(args[0]);
                string cmdName = "CargoWise.DPS.ContentManagement." + d["cmdName"];

                Assembly assembly = Assembly.GetExecutingAssembly();
                Command c = null;

                c = (Command)assembly.CreateInstance(cmdName);
                c.ParseArguments(args.Skip(1));
                c.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();

            }

        }
    }
}
