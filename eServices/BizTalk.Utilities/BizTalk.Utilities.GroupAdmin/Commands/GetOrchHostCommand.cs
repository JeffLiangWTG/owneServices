using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetOrchHostCommand: AbstractCommand
    {
        internal GetOrchHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then query the host name properties
        /// </summary>
        /// <remarks>Supports orchestration name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is -TypeName
            string typeNameMaskParam = base._rawArgs[1]; 
            if (!typeNameMaskParam.StartsWith("-TypeName:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is not blank
            string typeNameMask = typeNameMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(typeNameMask))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool orchestrationFound = false;
                Regex expression = new Regex(typeNameMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    foreach (Application application in catalogue.Applications)
                    {
                        foreach (BtsOrchestration orchestration in application.Orchestrations)
                        {
                            Match matcher = expression.Match(orchestration.FullName);
                            if (matcher.Success)
                            {
                                orchestrationFound = true;
                                string hostName = (orchestration.Host != null ? orchestration.Host.Name : "Not Set");
                                _output.Add(String.Format("Application: {0}; Orchestration: {1}; Host: {2}",
                                    application.Name, orchestration.FullName, hostName));
                            }
                        }
                    }
                }

                if (!orchestrationFound)
                {
                    _output.Add(typeNameMask + " not found!");
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
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchHost.ToString() + " -TypeName:<Orchestration Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start orchestration type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchHost.ToString() + " -TypeName:^Microsoft.BizTalk.Edi.RoutingOrchestration.BatchRoutingService$");
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchHost.ToString() + " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
