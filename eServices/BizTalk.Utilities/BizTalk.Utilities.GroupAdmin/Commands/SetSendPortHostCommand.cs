using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetSendPortHostCommand: AbstractCommand
    {
        internal SetSendPortHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host property update
        /// </summary>
        /// <remarks>Supports send port regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 4)
            {
                this.ReportUsage();
                return;
            }

            // Check params 2, 3 and 4 are -SendPort, -OldHost and -NewHost
            string sendPortMaskParam = base._rawArgs[1];
            string oldHostParam = base._rawArgs[2];
            string newHostParam = base._rawArgs[3];
            if (!sendPortMaskParam.StartsWith("-SendPort:") ||
                !oldHostParam.StartsWith("-OldHost:") ||
                !newHostParam.StartsWith("-NewHost:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2, 3, and 4 values are not blank
            string sendPortMask = sendPortMaskParam.Split(':')[1];
            string oldHostName = oldHostParam.Split(':')[1];
            string newHostName = newHostParam.Split(':')[1];
            if (String.IsNullOrEmpty(sendPortMask) ||
                String.IsNullOrEmpty(oldHostName) ||
                String.IsNullOrEmpty(newHostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool sendPortSet = false;
                Regex expression = new Regex(sendPortMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    try
                    {
                        // Validate and retrieve the new and old host objects
                        Host newHost = null; Host oldHost = null;
                        foreach (Host host in catalogue.Hosts)
                        {
                            if (host.Name == newHostName)
                            {
                                newHost = host;
                            }
                            else if (host.Name == oldHostName)
                            {
                                oldHost = host;
                            }
                        }

                        if (newHost == null || oldHost == null)
                        {
                            base.InitialiseOutput(OutputInitialiseOption.ForError);
                            _output.Add("Old or new host does not exist");
                            return;
                        }
                        else if (newHost.Type != oldHost.Type)
                        {
                            base.InitialiseOutput(OutputInitialiseOption.ForError);
                            _output.Add("Old and new host types are different");
                            return;
                        }

                        foreach (SendPort sendPort in catalogue.SendPorts)
                        {
                            Match matcher = expression.Match(sendPort.Name);
                            if (matcher.Success && !sendPort.IsDynamic && 
                                sendPort.PrimaryTransport.SendHandler != null &&
                                sendPort.PrimaryTransport.SendHandler.Host.Name == oldHostName)
                            {
                                foreach (SendHandler handler in catalogue.SendHandlers)
                                {
                                    if (handler.Host.Name == newHostName &&
                                        handler.TransportType == sendPort.PrimaryTransport.SendHandler.TransportType)
                                    {
                                        sendPortSet = true;
                                        sendPort.PrimaryTransport.SendHandler = handler;
                                        _output.Add(String.Format(
                                            "SendPort: {0}; OldHost: {1}; NewHost: {2}",
                                            sendPort.Name, oldHostName, newHostName));
                                        break;
                                    }
                                }
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
                    _output.Add(String.Format("Send port {0} not updated from old host {1} to new host {2}", sendPortMask, oldHostName, newHostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortHost.ToString() +
                " -SendPort:<Send Port RegEx> -OldHost:<Old Host Name> -NewHost:<New Host Name>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start send port name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortHost.ToString() +
                " -SendPort:^HSL_Request_SendPort$ -OldHost:BTSSendHost -NewHost:DRBTSSendHost");
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortHost.ToString() +
                " -SendPort:[a-zA-Z] -OldHost:BTSSendHost -NewHost:DRBTSSendHost");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
