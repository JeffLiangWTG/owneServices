using System;
using System.Collections.Generic;
using System.Management;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class CreateHostCommand : AbstractCommand
    {
        internal CreateHostCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host creation
        /// </summary>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 7)
            {
                this.ReportUsage();
                return;
            }

            string hostNameParam = base._rawArgs[1];
            string hostTypeParam = base._rawArgs[2];
            string ntGroupParam = base._rawArgs[3];
            string forTrackingParam = base._rawArgs[4];
            string trustedParam = base._rawArgs[5];
            string is32BitParam = base._rawArgs[6];

            if (!hostNameParam.StartsWith("-HostName:") ||
                !hostTypeParam.StartsWith("-HostType:") ||
                !ntGroupParam.StartsWith("-NTGroup:") ||
                !forTrackingParam.StartsWith("-ForTracking:") ||
                !trustedParam.StartsWith("-Trusted:") ||
                !is32BitParam.StartsWith("-32Bit:"))
            {
                this.ReportUsage();
                return;
            }

            string hostName = hostNameParam.Split(':')[1];
            string ntGroup = ntGroupParam.Split(':')[1];
            if (String.IsNullOrEmpty(hostName) || String.IsNullOrEmpty(ntGroupParam))
            {
                this.ReportUsage();
                return;
            }

            string hostTypeName = hostTypeParam.Split(':')[1];
            List<string> validHostTypes = new List<string>(Enum.GetNames(typeof(HostType)));
            if (!validHostTypes.Contains(hostTypeName))
            {
                this.ReportUsage();
                return;
            }

            bool forTracking = false;
            bool trusted = false;
            bool is32Bit = false;
            if (!Boolean.TryParse(forTrackingParam.Split(':')[1], out forTracking) ||
                !Boolean.TryParse(trustedParam.Split(':')[1], out trusted) ||
                !Boolean.TryParse(is32BitParam.Split(':')[1], out is32Bit))
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
                using (ManagementClass hostManager = new ManagementClass(scope, "MSBTS_HostSetting", null))
                {
                    using (ManagementObject hostCreator = hostManager.CreateInstance())
                    {
                        hostCreator["Name"] = hostName;
                        hostCreator["HostType"] = (int)(HostType)Enum.Parse(typeof(HostType), hostTypeName);
                        hostCreator["NTGroupName"] = ntGroup;
                        hostCreator[HostProperty.HostTracking.ToString()] = forTracking;
                        hostCreator[HostProperty.AuthTrusted.ToString()] = trusted;
                        hostCreator[HostProperty.IsHost32BitOnly.ToString()] = is32Bit;
                        hostCreator.Put(createIt);
                    }
                    _output.Add(String.Format("Host: {0} created", hostName));
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
            usage.Add(base.GetExeName() + " " + CommandType.CreateHost.ToString() +
                " -HostName:<Host Name> -HostType:<Host Type> -NTGroup:<Windows Group>" +
                " -ForTracking:<true/false> -Trusted:<true/false> -32Bit:<true/false>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE HOST TYPES -");
            foreach (string hostType in Enum.GetNames(typeof(HostType)))
            {
                usage.Add(hostType);
            }
            usage.Add(string.Empty);
            usage.Add("Example:");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.CreateHost.ToString() +
                " -HostName:MyNewInProcHost -HostType:" + HostType.InProcess.ToString() +
                " -NTGroup:\"BizTalk Application Users\" -ForTracking:false -Trusted:false -32Bit:false");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
