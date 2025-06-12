using System;
using System.Collections.Generic;
using BizTalk.Utilities.GroupAdmin.Commands;

namespace BizTalk.Utilities.GroupAdmin
{
    class Program
    {
        /// <summary>
        /// Can be run without params to show the help (ie) commands available.  Then, should be 
        /// able to show help for each command by only passing in the command type
        /// </summary>
        /// <param name="args">Command line arguments (case-sensitive)</param>
        static void Main(string[] args)
        {
            try
            {
                AbstractCommand command = CommandFactory.GenerateInstance(args);

                command.Execute();

                List<string> output = command.Output;
                foreach (string outputLine in output)
                {
                    Console.WriteLine(outputLine);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
