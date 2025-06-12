using System;
using System.Collections.Generic;
using System.Reflection;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal abstract class AbstractCommand
    {
        protected List<string> _rawArgs;
        protected List<string> _output;
        protected const string NOTICE_HEADING_FORMAT = "******************************************\r\n{0}\r\n******************************************\r\n";

        internal AbstractCommand(string[] rawArgs)
        {
            _rawArgs = new List<string>();
            _rawArgs.AddRange(rawArgs);
            InitialiseOutput(OutputInitialiseOption.None);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        internal abstract void Execute();

        /// <summary>
        /// Generate the usage to display to the user
        /// </summary>
        protected abstract void ReportUsage();

        /// <summary>
        /// Console lines to write out at the end of the <see cref="Execute"/> method call
        /// </summary>
        internal virtual List<string> Output
        {
            get
            {
                return _output;
            }
        }

        /// <summary>
        /// Assist with formatting the output line array
        /// </summary>
        /// <param name="option">Format for what output type</param>
        protected void InitialiseOutput(OutputInitialiseOption option)
        {
            _output = new List<string>();

            switch (option)
            {
                case OutputInitialiseOption.ForError: _output.Add(String.Format(NOTICE_HEADING_FORMAT, "Error:")); break;
                case OutputInitialiseOption.ForUsage: _output.Add(String.Format(NOTICE_HEADING_FORMAT, "Usage:")); break;
            }
        }

        /// <summary>
        /// Get the executable's name for use in usage displays
        /// </summary>
        /// <returns></returns>
        protected string GetExeName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }

        /// <summary>
        /// Output type formats
        /// </summary>
        protected enum OutputInitialiseOption
        {
            None,
            ForError,
            ForUsage
        }
    }
}
