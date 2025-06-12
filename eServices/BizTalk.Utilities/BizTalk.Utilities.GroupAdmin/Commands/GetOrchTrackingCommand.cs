using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetOrchTrackingCommand : BaseTrackingCommand<GetOrchTrackingCommand>
    {
        internal GetOrchTrackingCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the orchestration tracking query
        /// </summary>
        /// <remarks>Supports orchestration type name regular expression matching</remarks>
        internal override void Execute()
        {
            ValidatedTrackingArgs validArgs = base.ValidateGetterArguments();
            if (!validArgs.Valid)
            {
                this.ReportUsage();
                return;
            }

            try
            {
                bool orchestrationFound = false;
                Regex expression = new Regex(validArgs.NameMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status query
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    foreach (Application application in catalogue.Applications)
                    {
                        foreach (BtsOrchestration orchestration in application.Orchestrations)
                        {
                            Match matcher = expression.Match(orchestration.FullName);
                            if (matcher.Success)
                            {
                                orchestrationFound = true;
                                string optionsConcat = MapBtsOrchestrationTrackingTypes(orchestration.Tracking);
                                _output.Add(String.Format("Application: {0}; Orchestration: {1}; Tracking Options: {2}",
                                    application.Name, orchestration.FullName, optionsConcat));
                            }
                        }
                    }
                }

                if (!orchestrationFound)
                {
                    _output.Add(validArgs.NameMask + " not found!");
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
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchTracking.ToString() + " -TypeName:<Orchestration Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start orchestration type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchTracking.ToString() + " -TypeName:^Microsoft.BizTalk.Edi.RoutingOrchestration.BatchRoutingService$");
            usage.Add(base.GetExeName() + " " + CommandType.GetOrchTracking.ToString() + " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        /// <summary>
        /// Converts bitwise enum to the equivalent comma separated concatenation of GroupAdmin 
        /// orchestration tracking options
        /// </summary>
        /// <param name="btsTrackingTypes">BizTalk orchestration tracking types</param>
        protected static string MapBtsOrchestrationTrackingTypes(OrchestrationTrackingTypes orchestrationTracking)
        {
            List<string> btsTrackingTypes = new List<string>();
            btsTrackingTypes.AddRange(orchestrationTracking.ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));

            List<OrchestrationTrackingTypes> btsTrackingTypeEnums = new List<OrchestrationTrackingTypes>();
            btsTrackingTypeEnums.AddRange(btsTrackingTypes.ConvertAll<OrchestrationTrackingTypes>(
                new Converter<string, OrchestrationTrackingTypes>(ConvertBtsTrackingType<OrchestrationTrackingTypes>)));

            StringBuilder concat = new StringBuilder();
            foreach (OrchestrationTrackingTypes trackingType in btsTrackingTypeEnums)
            {
                if (concat.Length > 0)
                {
                    concat.Append(',');
                }
                switch (trackingType)
                {
                    case OrchestrationTrackingTypes.InboundMessageBody: concat.Append(OrchestrationTrackingOption.InboundMessageBody.ToString()); break;
                    case OrchestrationTrackingTypes.MessageSendReceive: concat.Append(OrchestrationTrackingOption.MessageSendReceive.ToString()); break;
                    case OrchestrationTrackingTypes.None: concat.Append(OrchestrationTrackingOption.None.ToString()); break;
                    case OrchestrationTrackingTypes.OrchestrationEvents: concat.Append(OrchestrationTrackingOption.OrchestrationEvents.ToString()); break;
                    case OrchestrationTrackingTypes.OutboundMessageBody: concat.Append(OrchestrationTrackingOption.OutboundMessageBody.ToString()); break;
                    case OrchestrationTrackingTypes.ServiceStartEnd: concat.Append(OrchestrationTrackingOption.ServiceStartEnd.ToString()); break;
                    case OrchestrationTrackingTypes.TrackPropertiesForIncomingMessages: concat.Append(OrchestrationTrackingOption.TrackPropertiesForIncomingMessages.ToString()); break;
                    case OrchestrationTrackingTypes.TrackPropertiesForOutgoingMessages: concat.Append(OrchestrationTrackingOption.TrackPropertiesForOutgoingMessages.ToString()); break;
                }
            }
            return concat.ToString();
        }
    }
}
