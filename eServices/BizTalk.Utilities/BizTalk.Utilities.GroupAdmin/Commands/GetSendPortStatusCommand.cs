using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetSendPortStatusCommand: AbstractCommand
    {
        internal GetSendPortStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the status query
        /// </summary>
        /// <remarks>Supports send port name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is -SendPort
            string sendPortMaskParam = base._rawArgs[1]; 
            if (!sendPortMaskParam.StartsWith("-SendPort:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is not blank
            string sendPortMask = sendPortMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(sendPortMask))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool sendPortFound = false;
                Regex expression = new Regex(sendPortMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    foreach (SendPort sendPort in catalogue.SendPorts)
                    {
                        Match matcher = expression.Match(sendPort.Name);
                        if (matcher.Success)
                        {
                            sendPortFound = true;
                            SendPortStatus currentStatus = SendPortStatus.Unenlisted;
                            switch (sendPort.Status)
                            {
                                case PortStatus.Bound: currentStatus = SendPortStatus.Unenlisted; break;
                                case PortStatus.Started: currentStatus = SendPortStatus.Started; break;
                                case PortStatus.Stopped: currentStatus = SendPortStatus.Stopped; break;
                                default: throw new NotSupportedException(String.Format("Send port status {0} could not be resolved", sendPort.Status.ToString()));
                            }
                            _output.Add(String.Format("SendPort: {0}; Status: {1}",
                                sendPort.Name, currentStatus.ToString()));
                        }
                    }
                }

                if (!sendPortFound)
                {
                    _output.Add(sendPortMask + " not found!");
                }
            }
            catch (Exception ex)
            {
                base.InitialiseOutput(OutputInitialiseOption.ForError);
                _output.Add(ex.ToString());
            }
        }

        /// <summary>
        /// Generate the usage to display to the user
        /// </summary>
        protected override void ReportUsage()
        {
            List<string> usage = new List<string>();
            usage.Add(base.GetExeName() + " " + CommandType.GetSendPortStatus.ToString() +
                " -SendPort:<Send Port Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start send port name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetSendPortStatus.ToString() + " -SendPort:^HSL_Request_SendPort$");
            usage.Add(base.GetExeName() + " " + CommandType.GetSendPortStatus.ToString() + " -SendPort:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
