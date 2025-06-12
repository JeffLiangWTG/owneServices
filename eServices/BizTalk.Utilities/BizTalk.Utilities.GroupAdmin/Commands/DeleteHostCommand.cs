using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class DeleteHostCommand : AbstractCommand
    {
        internal DeleteHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host deletion
        /// </summary>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            string hostNameParam = base._rawArgs[1];
            if (!hostNameParam.StartsWith("-HostName:"))
            {
                this.ReportUsage();
                return;
            }

            string hostName = hostNameParam.Split(':')[1];
            if (String.IsNullOrEmpty(hostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                string scope = "ROOT\\MicrosoftBizTalkServer";
                string query = "MSBTS_HostSetting.Name='" + hostName + "'";
                using (ManagementObject host = new ManagementObject())
                {
                    host.Scope = new ManagementScope(scope);
                    host.Path = new ManagementPath(query);
                    host.Delete();
                }
                _output.Add(String.Format("Host: {0} deleted", hostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.DeleteHost.ToString() + " -HostName:<Host Name>");
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.DeleteHost.ToString() + " -HostName:MyNewInProcHost");
            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
