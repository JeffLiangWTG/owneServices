using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetHostInstanceStateCommand : AbstractCommand
    {
        internal SetHostInstanceStateCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host instance state update
        /// </summary>
        /// <remarks>Supports host instance name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 4)
            {
                this.ReportUsage();
                return;
            }

            string stateParam = base._rawArgs[1];
            string nameMaskParam = base._rawArgs[2];
            string serverNameParam = base._rawArgs[3];

            // Check param 2, 3 and 4 are -State, -HostInstanceName and -Server
            if (!stateParam.StartsWith("-State:") ||
                !nameMaskParam.StartsWith("-HostInstanceName:") ||
                !serverNameParam.StartsWith("-Server:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is a valid property
            string stateName = stateParam.Split(':')[1];
            List<string> validStates = new List<string>(Enum.GetNames(typeof(HostInstanceState)));
            if (!validStates.Contains(stateName))
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

            // Check param 4 is not blank
            string serverName = serverNameParam.Split(':')[1];
            if (String.IsNullOrEmpty(serverName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool hostInstanceFound = false;
                HostInstanceState stateRequired = (HostInstanceState)Enum.Parse(typeof(HostInstanceState), stateName);
                Regex hostNameExpression = new Regex(nameMask, RegexOptions.None);
                Regex serverNameExpression = new Regex(serverName, RegexOptions.None);

                // Use WMI to get the value
                string scope = "ROOT\\MicrosoftBizTalkServer";
                string query = "SELECT * FROM MSBTS_HostInstance";
                using (ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query))
                {
                    using (ManagementObjectCollection results = search.Get())
                    {
                        foreach (ManagementObject hostInstance in results)
                        {
                            string hostServerName = hostInstance["RunningServer"].ToString();
                            Match serverMatcher = serverNameExpression.Match(hostServerName);
                            if (serverMatcher.Success)
                            {
                                string hostInstanceName = hostInstance["HostName"].ToString();
                                Match hostMatcher = hostNameExpression.Match(hostInstanceName);
                                if (hostMatcher.Success)
                                {
                                    hostInstanceFound = true;
                                    if (stateRequired == HostInstanceState.Started)
                                    {
                                        hostInstance.InvokeMethod("Start", new object[0]);
                                    }
                                    else if (stateRequired == HostInstanceState.Stopped)
                                    {
                                        hostInstance.InvokeMethod("Stop", new object[0]);
                                    }
                                    _output.Add(String.Format("HostInstance: {0}; Server: {1}; State: {2}", hostInstanceName, hostServerName, stateName));
                                }
                            }
                        }
                    }
                }

                if (!hostInstanceFound)
                {
                    _output.Add(String.Format("HostInstance: {0}; Server: {1} not found!", nameMask, serverName));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetHostInstanceState.ToString() +
                " -State:<State Required> -HostInstanceName:<Host Instance Name RegEx> -Server:<Server Name>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE PROPERTY NAMES -");
            foreach (string state in Enum.GetNames(typeof(HostInstanceState)))
            {
                usage.Add(state);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start server and host instance names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetHostInstanceState.ToString() + 
                " -State:" + HostInstanceState.Started.ToString() + " -HostInstanceName:^ReceiveHost$ -Server:^NET-BIOS-1$");
            usage.Add(base.GetExeName() + " " + CommandType.SetHostInstanceState.ToString() +
                " -State:" + HostInstanceState.Stopped.ToString() + " -HostInstanceName:[a-zA-Z] -Server:[A-Za-z0-9]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
