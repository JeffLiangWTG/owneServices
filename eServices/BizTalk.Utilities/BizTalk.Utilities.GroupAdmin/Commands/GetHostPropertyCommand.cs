using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetHostPropertyCommand : AbstractCommand
    {
        internal GetHostPropertyCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host property query
        /// </summary>
        /// <remarks>Supports host name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            string propertyParam = base._rawArgs[1];
            string nameMaskParam = base._rawArgs[2];

            // Check param 2 and 3 are -Property, and -HostName
            if (!propertyParam.StartsWith("-Property:") ||
                !nameMaskParam.StartsWith("-HostName:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is a valid property
            string propertyName = propertyParam.Split(':')[1];
            List<string> validPropertyNames = new List<string>(Enum.GetNames(typeof(HostProperty)));
            if (!validPropertyNames.Contains(propertyName))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 is not blank
            string nameMask = nameMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(nameMask))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool hostFound = false;
                Regex expression = new Regex(nameMask, RegexOptions.None);

                // Use WMI to get the value
                string scope = "ROOT\\MicrosoftBizTalkServer";
                string query = "SELECT * FROM MSBTS_HostSetting";
                using (ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query))
                {
                    using (ManagementObjectCollection results = search.Get())
                    {
                        foreach (ManagementObject host in results)
                        {
                            string hostName = host["Name"].ToString();
                            Match matcher = expression.Match(hostName);
                            if (matcher.Success)
                            {
                                hostFound = true;
                                string value = host[propertyName].ToString();
                                _output.Add(String.Format("Host: {0}; Property: {1}; Value: {2}",
                                    hostName, propertyName, value));
                            }
                        }
                    }
                }

                if (!hostFound)
                {
                    _output.Add(nameMask + " not found!");
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
            usage.Add(base.GetExeName() + " " + CommandType.GetHostProperty.ToString() +
                " -Property:<Property Name> -HostName:<Host Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE PROPERTY NAMES -");
            foreach (string option in Enum.GetNames(typeof(HostProperty)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start host names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetHostProperty.ToString() + 
                " -Property:" + HostProperty.HostTracking.ToString() + " -HostName:^ReceiveHost$");
            usage.Add(base.GetExeName() + " " + CommandType.GetHostProperty.ToString() +
                " -Property:" + HostProperty.DeliveryQueueSize.ToString() + " -HostName:^TrackingHost$");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
