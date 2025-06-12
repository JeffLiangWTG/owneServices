using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetOrchHostCommand: AbstractCommand
    {
        internal SetOrchHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host property update
        /// </summary>
        /// <remarks>Supports orchestration name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 4)
            {
                this.ReportUsage();
                return;
            }

            // Check params 2, 3 and 4 are -TypeName, -OldHost and -NewHost
            string typeNameMaskParam = base._rawArgs[1];
            string oldHostParam = base._rawArgs[2];
            string newHostParam = base._rawArgs[3];
            if (!typeNameMaskParam.StartsWith("-TypeName:") || 
                !oldHostParam.StartsWith("-OldHost:") ||
                !newHostParam.StartsWith("-NewHost:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2, 3, and 4 values are not blank
            string typeNameMask = typeNameMaskParam.Split(':')[1];
            string oldHostName = oldHostParam.Split(':')[1];
            string newHostName = newHostParam.Split(':')[1];
            if (String.IsNullOrEmpty(typeNameMask) ||
                String.IsNullOrEmpty(oldHostName) ||
                String.IsNullOrEmpty(newHostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool orchestrationSet = false;
                Regex expression = new Regex(typeNameMask, RegexOptions.None);

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
                        else if (newHost.Type != Microsoft.BizTalk.ExplorerOM.HostType.InProcess)
                        {
                            base.InitialiseOutput(OutputInitialiseOption.ForError);
                            _output.Add("New host is not an InProcess host");
                            return;
                        }

                        foreach (Application application in catalogue.Applications)
                        {
                            foreach (BtsOrchestration orchestration in application.Orchestrations)
                            {
                                Match matcher = expression.Match(orchestration.FullName);
                                if (matcher.Success && 
                                    orchestration.Host != null &&
                                    orchestration.Host.Name == oldHostName)
                                {
                                    orchestrationSet = true;
                                    orchestration.Host = newHost;
                                    _output.Add(String.Format("Application: {0}; Orchestration: {1}; OldHost: {2}; NewHost: {3}",
                                        application.Name, orchestration.FullName, oldHostName, newHostName));
                                }
                            }
                        }

                        // Commit the changes
                        if (orchestrationSet)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (orchestrationSet)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!orchestrationSet)
                {
                    _output.Add(String.Format("Orchestration {0} not updated from old host {1} to new host {2}", typeNameMask, oldHostName, newHostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchHost.ToString() +
                " -TypeName:<Orchestration Type Name RegEx> -OldHost:<Old Host Name> -NewHost:<New Host Name>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start orchestration type name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchHost.ToString() +
                " -TypeName:^Microsoft.BizTalk.Edi.RoutingOrchestration.BatchRoutingService$" +
                " -OldHost:BTSProcessingHost -NewHost:DRBTSProcessingHost");
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchHost.ToString() +
                " -TypeName:[a-zA-Z] -OldHost:BTSProcessingHost -NewHost:DRBTSProcessingHost");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
