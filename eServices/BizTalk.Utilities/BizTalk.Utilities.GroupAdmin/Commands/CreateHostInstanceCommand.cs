using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class CreateHostInstanceCommand : AbstractCommand
    {
        internal CreateHostInstanceCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host instance creation
        /// </summary>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 5)
            {
                this.ReportUsage();
                return;
            }

            string serverNameParam = base._rawArgs[1];
            string hostNameParam = base._rawArgs[2];
            string userNameParam = base._rawArgs[3];
            string passwordParam = base._rawArgs[4];
            if (!serverNameParam.StartsWith("-Server:") || !hostNameParam.StartsWith("-HostName:") ||
                !userNameParam.StartsWith("-UserName:") || !passwordParam.StartsWith("-Password:"))
            {
                this.ReportUsage();
                return;
            }

            string serverName = serverNameParam.Split(':')[1];
            string hostName = hostNameParam.Split(':')[1];
            string userName = userNameParam.Split(':')[1];
            string password = passwordParam.Split(':')[1];
            if (String.IsNullOrEmpty(serverName) || String.IsNullOrEmpty(hostName) ||
                String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                string scope = "ROOT\\MicrosoftBizTalkServer";
                using (ManagementClass hostManager = new ManagementClass(scope, "MSBTS_ServerHost", null))
                {
                    using (ManagementObject hostMapper = hostManager.CreateInstance())
                    {
                        hostMapper["ServerName"] = serverName;
                        hostMapper["HostName"] = hostName;
                        hostMapper.InvokeMethod("Map", null);
                    }
                }
                try
                {
                    using (ManagementClass hostInstanceManager = new ManagementClass(scope, "MSBTS_HostInstance", null))
                    {
                        using (ManagementObject hostInstanceCreator = hostInstanceManager.CreateInstance())
                        {
                            // Yes, you have to use this format for the instance name...
                            hostInstanceCreator["Name"] = String.Format("Microsoft BizTalk Server {0} {1}", hostName, serverName);
                            hostInstanceCreator.InvokeMethod("Install", new object[] { userName, password });
                            _output.Add(String.Format("Host instance: {0} created on server: {1} ", hostName, serverName));
                        }
                    }
                }
                catch (Exception)
                {
                    // Try and rollback the mapping
                    using (ManagementClass hostManager = new ManagementClass(scope, "MSBTS_ServerHost", null))
                    {
                        using (ManagementObject hostMapper = hostManager.CreateInstance())
                        {
                            hostMapper["ServerName"] = serverName;
                            hostMapper["HostName"] = hostName;
                            hostMapper.InvokeMethod("Unmap", null);
                        }
                    }
                    throw;
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
            usage.Add(base.GetExeName() + " " + CommandType.CreateHostInstance.ToString() +
                " -Server:<Server Name> -HostName:<Host Name> -UserName:<Domain Account> -Password:<Domain Account Password>");
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.CreateHostInstance.ToString() +
                @" -Server:MyServer -HostName:MyNewInProcHost -UserName:MyDomain\Me -Password:P@ssw0rd");
            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
