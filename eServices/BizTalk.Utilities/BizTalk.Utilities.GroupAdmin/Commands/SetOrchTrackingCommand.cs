using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetOrchTrackingCommand : BaseTrackingCommand<SetOrchTrackingCommand>
    {
        internal SetOrchTrackingCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the orchestration tracking update command
        /// </summary>
        /// <remarks>Supports orchestration type name regular expression matching</remarks>
        internal override void Execute()
        {
            ValidatedTrackingArgs validArgs = base.ValidateSetterArguments<OrchestrationTrackingOption>();
            if (!validArgs.Valid)
            {
                this.ReportUsage();
                return;
            }

            try
            {
                // Map the internal tracking options to the ExplorerOM tracking types
                List<OrchestrationTrackingTypes> trackingOptions = MapInternalOrchestrationTrackingOptions(validArgs.Options);

                bool orchestrationFound = false;
                Regex expression = new Regex(validArgs.NameMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    try
                    {
                        foreach (Application application in catalogue.Applications)
                        {
                            foreach (BtsOrchestration orchestration in application.Orchestrations)
                            {
                                Match matcher = expression.Match(orchestration.FullName);
                                if (matcher.Success)
                                {
                                    orchestrationFound = true;
                                    orchestration.Tracking = OrchestrationTrackingTypes.None;
                                    foreach (OrchestrationTrackingTypes trackingOption in trackingOptions)
                                    {
                                        orchestration.Tracking = orchestration.Tracking | trackingOption;
                                    }
                                    _output.Add(String.Format("Orchestration: {0} updated", orchestration.FullName));
                                }
                            }
                        }

                        // Commit the changes
                        if (orchestrationFound)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (orchestrationFound)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
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
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchTracking.ToString() +
                " -Options:<Option1,Option2,Option3> -TypeName:<Orchestration Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE TRACKING OPTION PARAMETERS -");
            foreach (string option in Enum.GetNames(typeof(OrchestrationTrackingOption)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start orchestration type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchTracking.ToString() + 
                " -Options:" + OrchestrationTrackingOption.None.ToString() +
                " -TypeName:^Microsoft.BizTalk.Edi.RoutingOrchestration.BatchRoutingService$");
            usage.Add(base.GetExeName() + " " + CommandType.SetOrchTracking.ToString() +
                " -Options:" + OrchestrationTrackingOption.ServiceStartEnd.ToString() + "," +
                OrchestrationTrackingOption.InboundMessageBody.ToString() +
                " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        protected static List<OrchestrationTrackingTypes> MapInternalOrchestrationTrackingOptions(List<string> internalOptions)
        {
            List<OrchestrationTrackingOption> internalTrackingOptions = new List<OrchestrationTrackingOption>();
            foreach (string internalOption in internalOptions)
            {
                OrchestrationTrackingOption internalTrackingOption = (OrchestrationTrackingOption)Enum.Parse(typeof(OrchestrationTrackingOption), internalOption, false);
                internalTrackingOptions.Add(internalTrackingOption);
            }

            List<OrchestrationTrackingTypes> trackingOptions = new List<OrchestrationTrackingTypes>();
            foreach (OrchestrationTrackingOption internalOption in internalTrackingOptions)
            {
                switch (internalOption)
                {
                    case OrchestrationTrackingOption.InboundMessageBody: trackingOptions.Add(OrchestrationTrackingTypes.InboundMessageBody); break;
                    case OrchestrationTrackingOption.MessageSendReceive: trackingOptions.Add(OrchestrationTrackingTypes.MessageSendReceive); break;
                    case OrchestrationTrackingOption.None: trackingOptions.Add(OrchestrationTrackingTypes.None); break;
                    case OrchestrationTrackingOption.OrchestrationEvents: trackingOptions.Add(OrchestrationTrackingTypes.OrchestrationEvents); break;
                    case OrchestrationTrackingOption.OutboundMessageBody: trackingOptions.Add(OrchestrationTrackingTypes.OutboundMessageBody); break;
                    case OrchestrationTrackingOption.ServiceStartEnd: trackingOptions.Add(OrchestrationTrackingTypes.ServiceStartEnd); break;
                    case OrchestrationTrackingOption.TrackPropertiesForIncomingMessages: trackingOptions.Add(OrchestrationTrackingTypes.TrackPropertiesForIncomingMessages); break;
                    case OrchestrationTrackingOption.TrackPropertiesForOutgoingMessages: trackingOptions.Add(OrchestrationTrackingTypes.TrackPropertiesForOutgoingMessages); break;
                }
            }
            return trackingOptions;
        }
    }
}
