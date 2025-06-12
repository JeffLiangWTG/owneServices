using System;
using System.Collections.Generic;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class HelpCommand : AbstractCommand
    {
        internal HelpCommand(string[] args) : base(args) { }

        /// <summary>
        /// Execute the help command
        /// </summary>
        internal override void Execute()
        {
            this.ReportUsage();
            return;
        }

        /// <summary>
        /// Generate the usage to display to the user
        /// </summary>
        protected override void ReportUsage()
        {
            List<string> usage = new List<string>();
            usage.Add(base.GetExeName() + " <command> [ -<Parameter1>:<value1> ... ]");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE COMMANDS -");
            List<string> validCommands = new List<string>(Enum.GetNames(typeof(CommandType)));
            foreach (string validCommand in validCommands)
            {
                usage.Add(validCommand);
            }

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
