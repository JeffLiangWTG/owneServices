using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetOrchStatusCommand : AbstractCommand
    {
        internal SetOrchStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the orchestration status update command
        /// </summary>
        /// <remarks>Supports orchestration type name regular expression matching</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            string statusParam = base._rawArgs[1];
            string typeNameMaskParam = base._rawArgs[2];

            // Check param 2 and 3 are -Status: and -TypeName, respectively
            if (!statusParam.StartsWith("-Status:") || !typeNameMaskParam.StartsWith("-TypeName:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is a valid status
            string newStatusRequired = statusParam.Split(':')[1];
            List<string> validStatuses = new List<string>(Enum.GetNames(typeof(OrchestrationStatus)));
            if (!validStatuses.Contains(newStatusRequired))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 value is not blank
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
                OrchestrationStatus statusRequired = (OrchestrationStatus)Enum.Parse(typeof(OrchestrationStatus), newStatusRequired, false);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    try
                    {
                        foreach (Application application in catalogue.Applications)
                        {
                            foreach (BtsOrchestration orchestration in application.Orchestrations)
                            {
                                Match matcher = expression.Match(orchestration.FullName);
                                if (matcher.Success)
                                {
                                    orchestrationFound = true;
                                    OrchestrationStatus oldStatus = orchestration.Status;
                                    if (orchestration.Status != statusRequired)
                                    {
                                        orchestration.Status = statusRequired;
                                        _output.Add(String.Format("Old Status: {0}; New Status: {1}; Application: {2}; {3}",
                                            oldStatus, newStatusRequired, application.Name, orchestration.FullName));
                                    }
                                    else
                                    {
                                        _output.Add(String.Format("{0} already {1} in Application {2}",
                                            orchestration.FullName, newStatusRequired, application.Name));
                                    }
                                }
                            }
                        }

                        // Commit the changes
                        if (orchestrationFound)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (orchestrationFound)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
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
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchStatus.ToString() + 
                " -Status:<Status Required> -TypeName:<Orchestration Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE STATUS PARAMETERS -");
            foreach (string status in Enum.GetNames(typeof(OrchestrationStatus)))
            {
                usage.Add(status);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start orchestration type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchStatus.ToString() + 
                " -Status:" + OrchestrationStatus.Started.ToString() +
                " -TypeName:^Microsoft.BizTalk.Edi.RoutingOrchestration.BatchRoutingService$");
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchStatus.ToString() +
                " -Status:" + OrchestrationStatus.Enlisted.ToString() +
                " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
