using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetAppStatusCommand: AbstractCommand
    {
        internal GetAppStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the status get
        /// </summary>
        /// <remarks>Supports application name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is -Application
            string applicationParam = base._rawArgs[1];
            if (!applicationParam.StartsWith("-Application:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is not blank
            string applicationName = applicationParam.Split(':')[1];
            if (String.IsNullOrEmpty(applicationName))
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

                    // Go through applications:
                    foreach (Application application in catalogue.Applications)
                    {
                        Match matcher = expression.Match(application.Name);
                        if (matcher.Success)
                        {
                            applicationFound = true;
                            _output.Add(String.Format("Application: {0}; Status: {1};",
                                application.Name, application.Status.ToString()));
                        }
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
            usage.Add(base.GetExeName() + " " + CommandType.GetAppStatus.ToString() +
                " -Application:<Application Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Example: Remember to start application name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetAppStatus.ToString() + " -Application:^BizTalk Application 1$");
            usage.Add(base.GetExeName() + " " + CommandType.GetAppStatus.ToString() + " -Application:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
