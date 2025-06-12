using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class CreateReceiveHandlerCommand : AbstractCommand
    {
        internal CreateReceiveHandlerCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the handler association
        /// </summary>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count < 3 || base._rawArgs.Count > 4)
            {
                this.ReportUsage();
                return;
            }

            string adapterNameParam = base._rawArgs[1];
            string hostNameParam = base._rawArgs[2];
            string adapterConfigParam = base._rawArgs.Count == 4 ? base._rawArgs[3] : String.Empty;

            if (!adapterNameParam.StartsWith("-AdapterName:") ||
                !hostNameParam.StartsWith("-HostName:") ||
                (!String.IsNullOrEmpty(adapterConfigParam) && !adapterConfigParam.StartsWith("-AdapterConfig")))
            {
                this.ReportUsage();
                return;
            }

            string adapterName = adapterNameParam.Split(':')[1];
            string hostName = hostNameParam.Split(':')[1];
            string adapterConfig = !String.IsNullOrEmpty(adapterConfigParam) ? adapterConfigParam.Split(':')[1] : String.Empty;
            if (String.IsNullOrEmpty(adapterName) ||
                String.IsNullOrEmpty(hostName) ||
                (!String.IsNullOrEmpty(adapterConfigParam) && String.IsNullOrEmpty(adapterConfig)))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                PutOptions createIt = new PutOptions();
                createIt.Type = PutType.CreateOnly;

                string scope = "ROOT\\MicrosoftBizTalkServer";
                using (ManagementClass handlerManager = new ManagementClass(scope, "MSBTS_ReceiveHandler", null))
                {
                    using (ManagementObject handlerCreator = handlerManager.CreateInstance())
                    {
                        handlerCreator["AdapterName"] = adapterName;
                        handlerCreator["HostName"] = hostName;
                        handlerCreator["CustomCfg"] = adapterConfig;
                        handlerCreator.Put(createIt);
                    }
                    _output.Add(String.Format("Receive handler created.  Adapter: {0} associated with Host: {1}", adapterName, hostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.CreateReceiveHandler.ToString() +
                " -AdapterName:<Registered Adapter Name> -HostName:<Host Name> -AdapterConfig:[optional]<Custom Configuration (Escaped XML)>");
            usage.Add(string.Empty);
            usage.Add("Examples:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.CreateReceiveHandler.ToString() + " -AdapterName:WCF-SQL -HostName:SendHost_WCFSQL");
            usage.Add(base.GetExeName() + " " + CommandType.CreateReceiveHandler.ToString() + " -AdapterName:FTP -HostName:SendHost_FTP " +
                "-AdapterConfig:<CustomProps><AdapterConfig vt='8'>&lt;Config xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'&gt;&lt;userName&gt;Username&lt;/userName&gt;&lt;password&gt;Password&lt;/password&gt;&lt;/Config&gt;</AdapterConfig></CustomProps>");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
