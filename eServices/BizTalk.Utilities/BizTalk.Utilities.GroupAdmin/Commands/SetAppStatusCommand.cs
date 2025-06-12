using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetAppStatusCommand: AbstractCommand
    {
        internal SetAppStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the status update
        /// </summary>
        /// <remarks>Supports application name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 and 3 are -Application: and -Option, respectively
            string applicationParam = base._rawArgs[1];
            string optionParam = base._rawArgs[2];
            if (!applicationParam.StartsWith("-Application:") || !optionParam.StartsWith("-Option:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 and 3 are not blank
            string applicationName = applicationParam.Split(':')[1];
            string optionName = optionParam.Split(':')[1];
            if (String.IsNullOrEmpty(applicationName) || String.IsNullOrEmpty(optionName))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 is a valid option
            List<string> validOptions = new List<string>(Enum.GetNames(typeof(AppStatusOption)));
            if (!validOptions.Contains(optionName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool applicationFound = false;
                Regex expression = new Regex(applicationName, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    try
                    {
                        // Go through applications
                        foreach (Application application in catalogue.Applications)
                        {
                            Match matcher = expression.Match(application.Name);
                            if (matcher.Success)
                            {
                                applicationFound = true;

                                if (optionName.StartsWith("Start"))
                                {
                                    ApplicationStartOption startOption = MapInternalStartOption(optionName);
                                    application.Start(startOption);
                                }
                                else
                                {
                                    ApplicationStopOption stopOption = MapInternalStopOption(optionName);
                                    application.Stop(stopOption);
                                }

                                _output.Add(String.Format("Application: {0}; Status: {1};",
                                    application.Name, application.Status.ToString()));
                            }
                        }

                        // Commit the changes
                        if (applicationFound)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (applicationFound)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!applicationFound)
                {
                    _output.Add("The following application is not found: " + applicationName);
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
            usage.Add(base.GetExeName() + " " + CommandType.SetAppStatus.ToString() +
                " -Application:<Application Name RegEx> -Option:<Status Option>");
            usage.Add(string.Empty);
            usage.Add("Example: Remember to start application name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE STOP OPTION PARAMETERS -");
            foreach (string option in Enum.GetNames(typeof(AppStatusOption)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetAppStatus.ToString() + 
                " -Application:^BizTalk Application 1$ -Option:" + AppStatusOption.StopAllArtefactsAndTerminate.ToString());
            usage.Add(base.GetExeName() + " " + CommandType.SetAppStatus.ToString() +
                " -Application:[a-zA-Z] -Option:" + AppStatusOption.StartAllSendPorts.ToString());

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        protected static ApplicationStartOption MapInternalStartOption(string internalOption)
        {
            AppStatusOption appStatusOption = (AppStatusOption)Enum.Parse(typeof(AppStatusOption), internalOption, false);
            switch (appStatusOption)
            {
                case AppStatusOption.StartAllArtefacts: return ApplicationStartOption.StartAll;
                case AppStatusOption.StartAllOrchestrations: return ApplicationStartOption.StartAllOrchestrations;
                case AppStatusOption.StartAllSendPortGroups: return ApplicationStartOption.StartAllSendPortGroups;
                case AppStatusOption.StartAllSendPorts: return ApplicationStartOption.StartAllSendPorts;
                case AppStatusOption.StartByDeployingAllPolicies: return ApplicationStartOption.DeployAllPolicies;
                case AppStatusOption.StartByEnablingAllReceiveLocations: return ApplicationStartOption.EnableAllReceiveLocations;
                case AppStatusOption.StartReferencedApplications: return ApplicationStartOption.StartReferencedApplications;
                default: throw new NotSupportedException();
            }
        }

        protected static ApplicationStopOption MapInternalStopOption(string internalOption)
        {
            AppStatusOption appStatusOption = (AppStatusOption)Enum.Parse(typeof(AppStatusOption), internalOption, false);
            switch (appStatusOption)
            {
                case AppStatusOption.StopAllArtefactsAndTerminate: return ApplicationStopOption.StopAll;
                case AppStatusOption.StopByDisablingAllReceiveLocations: return ApplicationStopOption.DisableAllReceiveLocations;
                case AppStatusOption.StopByUndeployingAllPolicies: return ApplicationStopOption.UndeployAllPolicies;
                case AppStatusOption.StopByUnenlistingAllOrchestrations: return ApplicationStopOption.UnenlistAllOrchestrations;
                case AppStatusOption.StopByUnenlistingAllSendPortGroups: return ApplicationStopOption.UnenlistAllSendPortGroups;
                case AppStatusOption.StopByUnenlistingAllSendPorts: return ApplicationStopOption.UnenlistAllSendPorts;
                case AppStatusOption.StopReferencedApplications: return ApplicationStopOption.StopReferencedApplications;
                default: throw new NotSupportedException();
            }
        }
    }
}
