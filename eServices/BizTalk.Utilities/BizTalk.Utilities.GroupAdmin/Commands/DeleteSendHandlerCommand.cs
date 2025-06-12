using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class DeleteSendHandlerCommand : AbstractCommand
    {
        internal DeleteSendHandlerCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the handler removal
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

            string adapterNameParam = base._rawArgs[1];
            string hostNameParam = base._rawArgs[2];

            if (!adapterNameParam.StartsWith("-AdapterName:") || !hostNameParam.StartsWith("-HostName:"))
            {
                this.ReportUsage();
                return;
            }

            string adapterName = adapterNameParam.Split(':')[1];
            string hostName = hostNameParam.Split(':')[1];
            if (String.IsNullOrEmpty(adapterName) || String.IsNullOrEmpty(hostName))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                string scope = "ROOT\\MicrosoftBizTalkServer";
                using (ManagementClass handlerManager = new ManagementClass(scope, "MSBTS_SendHandler2", null))
                {
                    using (ManagementObject handlerCreator = handlerManager.CreateInstance())
                    {
                        handlerCreator["AdapterName"] = adapterName;
                        handlerCreator["HostName"] = hostName;
                        handlerCreator.Delete();
                    }
                    _output.Add(String.Format("Send handler deleted.  Adapter: {0} no longer associated with Host: {1}", adapterName, hostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.DeleteSendHandler.ToString() + " -AdapterName:<Registered Adapter Name> -HostName:<Host Name>");
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.DeleteSendHandler.ToString() + " -AdapterName:WCF-SQL -HostName:SendHost_WCFSQL");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
