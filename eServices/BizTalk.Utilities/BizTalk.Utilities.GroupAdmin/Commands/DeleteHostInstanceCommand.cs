using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class DeleteHostInstanceCommand : AbstractCommand
    {
        internal DeleteHostInstanceCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host instance deletion by stopping the instance, deleting 
        /// the instance then deleting the mapping from the host to the server
        /// </summary>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            string serverNameParam = base._rawArgs[1];
            string hostNameParam = base._rawArgs[2];
            if (!serverNameParam.StartsWith("-Server:") || !hostNameParam.StartsWith("-HostName:"))
            {
                this.ReportUsage();
                return;
            }

            string serverName = serverNameParam.Split(':')[1];
            string hostName = hostNameParam.Split(':')[1];
            if (String.IsNullOrEmpty(serverName) || String.IsNullOrEmpty(hostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                string scope = "ROOT\\MicrosoftBizTalkServer";

                // Uninstall the instance
                using (ManagementClass hostInstanceManager = new ManagementClass(scope, "MSBTS_HostInstance", null))
                {
                    using (ManagementObject hostInstance = hostInstanceManager.CreateInstance())
                    {
                        // Yes, you have to use this format for the instance name...
                        hostInstance["Name"] = String.Format("Microsoft BizTalk Server {0} {1}", hostName, serverName);
                        hostInstance.InvokeMethod("Stop", null); // Won't fault if already stopped
                        hostInstance.InvokeMethod("UnInstall", null);
                    }
                }

                // Remove the server mapping
                using (ManagementClass hostManager = new ManagementClass(scope, "MSBTS_ServerHost", null))
                {
                    using (ManagementObject hostMapper = hostManager.CreateInstance())
                    {
                        hostMapper["ServerName"] = serverName;
                        hostMapper["HostName"] = hostName;
                        hostMapper.InvokeMethod("Unmap", null);
                    }
                }

                _output.Add(String.Format("Host instance: {0} deleted from server: {1} ", hostName, serverName));
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
            usage.Add(base.GetExeName() + " " + CommandType.DeleteHostInstance.ToString() +
                " -Server:<Server Name> -HostName:<Host Name>");
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.DeleteHostInstance.ToString() + " -Server:MyServer -HostName:MyNewInProcHost");
            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
