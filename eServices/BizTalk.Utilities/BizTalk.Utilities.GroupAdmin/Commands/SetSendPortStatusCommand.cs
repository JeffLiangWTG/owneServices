using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetSendPortStatusCommand: AbstractCommand
    {
        internal SetSendPortStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the status property update
        /// </summary>
        /// <remarks>Supports send port regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            // Check params 2 and 3 are -SendPort and -Status
            string sendPortMaskParam = base._rawArgs[1];
            string statusParam = base._rawArgs[2];
            if (!sendPortMaskParam.StartsWith("-SendPort:") || !statusParam.StartsWith("-Status:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 and 3 values are not blank
            string sendPortMask = sendPortMaskParam.Split(':')[1];
            string statusRequested = statusParam.Split(':')[1];
            if (String.IsNullOrEmpty(sendPortMask) || String.IsNullOrEmpty(statusRequested))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 value is a valid status
            List<string> validStatuses = new List<string>(Enum.GetNames(typeof(SendPortStatus)));
            if (!validStatuses.Contains(statusRequested))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool sendPortSet = false;
                Regex expression = new Regex(sendPortMask, RegexOptions.None);

                // Work out the Explorer OM status required 
                SendPortStatus typedStatusRequested = (SendPortStatus)Enum.Parse(typeof(SendPortStatus), statusRequested);
                PortStatus statusRequired = PortStatus.Bound;
                switch (typedStatusRequested)
                {
                    case SendPortStatus.Started: statusRequired = PortStatus.Started; break;
                    case SendPortStatus.Stopped: statusRequired = PortStatus.Stopped; break;
                    case SendPortStatus.Unenlisted: statusRequired = PortStatus.Bound; break;
                    default: throw new NotSupportedException("Port status requested cannot be resolved");
                }

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    try
                    {
                        foreach (SendPort sendPort in catalogue.SendPorts)
                        {
                            Match matcher = expression.Match(sendPort.Name);
                            if (matcher.Success && !sendPort.IsDynamic)
                            {
                                sendPortSet = true;
                                sendPort.Status = statusRequired;
                                _output.Add(String.Format("SendPort: {0}; Status: {1}", 
                                    sendPort.Name, statusRequested));
                            }
                        }

                        // Commit the changes
                        if (sendPortSet)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (sendPortSet)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!sendPortSet)
                {
                    _output.Add(String.Format("Send port {0} not updated to status {1}", sendPortMask, statusRequested));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortStatus.ToString() +
                " -SendPort:<Send Port RegEx> -Status:<Status>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE STATUSES -");
            foreach (string option in Enum.GetNames(typeof(SendPortStatus)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start send port name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortStatus.ToString() +
                " -SendPort:^HSL_Request_SendPort$ -Status:" + SendPortStatus.Started.ToString());
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortStatus.ToString() +
                " -SendPort:[a-zA-Z] -Status:" + SendPortStatus.Unenlisted.ToString());

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
