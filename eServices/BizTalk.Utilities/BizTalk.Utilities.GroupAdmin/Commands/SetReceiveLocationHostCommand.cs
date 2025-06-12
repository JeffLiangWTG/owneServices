using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetReceiveLocationHostCommand: AbstractCommand
    {
        internal SetReceiveLocationHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host property update
        /// </summary>
        /// <remarks>Supports receive location regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 4)
            {
                this.ReportUsage();
                return;
            }

            // Check params 2, 3 and 4 are -ReceiveLocation, -OldHost and -NewHost
            string receiveLocationMaskParam = base._rawArgs[1];
            string oldHostParam = base._rawArgs[2];
            string newHostParam = base._rawArgs[3];
            if (!receiveLocationMaskParam.StartsWith("-ReceiveLocation:") ||
                !oldHostParam.StartsWith("-OldHost:") ||
                !newHostParam.StartsWith("-NewHost:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2, 3, and 4 values are not blank
            string receiveLocationMask = receiveLocationMaskParam.Split(':')[1];
            string oldHostName = oldHostParam.Split(':')[1];
            string newHostName = newHostParam.Split(':')[1];
            if (String.IsNullOrEmpty(receiveLocationMask) ||
                String.IsNullOrEmpty(oldHostName) ||
                String.IsNullOrEmpty(newHostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool receiveLocationSet = false;
                Regex expression = new Regex(receiveLocationMask, RegexOptions.None);

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

                        foreach (ReceivePort receivePort in catalogue.ReceivePorts)
                        {
                            foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                            {
                                Match matcher = expression.Match(receiveLocation.Name);
                                if (matcher.Success && receiveLocation.ReceiveHandler != null && 
                                    receiveLocation.ReceiveHandler.Host.Name == oldHostName)
                                {
                                    foreach (ReceiveHandler handler in catalogue.ReceiveHandlers)
                                    {
                                        if (handler.Host.Name == newHostName &&
                                            handler.TransportType == receiveLocation.ReceiveHandler.TransportType)
                                        {
                                            receiveLocationSet = true;
                                            receiveLocation.ReceiveHandler = handler;
                                            _output.Add(String.Format(
                                                "ReceiveLocation: {0}; OldHost: {1}; NewHost: {2}",
                                                receiveLocation.Name, oldHostName, newHostName));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Commit the changes
                        if (receiveLocationSet)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (receiveLocationSet)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!receiveLocationSet)
                {
                    _output.Add(String.Format("Receive location {0} not updated from old host {1} to new host {2}", receiveLocationMask, oldHostName, newHostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationHost.ToString() +
                " -ReceiveLocation:<Receive Location Name RegEx> -OldHost:<Old Host Name> -NewHost:<New Host Name>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start receive location name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationHost.ToString() +
                " -ReceiveLocation:^pControl_Services_AckNack$" +
                " -OldHost:BTSReceiveHost -NewHost:DRBTSReceiveHost");
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationHost.ToString() +
                " -ReceiveLocation:[a-zA-Z] -OldHost:BTSReceiveHost -NewHost:DRBTSReceiveHost");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
