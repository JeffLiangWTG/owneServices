using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetPipelineTrackingCommand : BaseTrackingCommand<SetPipelineTrackingCommand>
    {
        internal SetPipelineTrackingCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the pipeline tracking option update
        /// </summary>
        /// <remarks>Supports pipeline type name regular expression matching</remarks>
        internal override void Execute()
        {
            ValidatedTrackingArgs validArgs = base.ValidateSetterArguments<PipelineTrackingOption>();
            if (!validArgs.Valid)
            {
                this.ReportUsage();
                return;
            }

            try
            {
                // Map the internal tracking options to the ExplorerOM tracking types
                List<PipelineTrackingTypes> trackingOptions = MapInternalPipelineTrackingOptions(validArgs.Options);

                bool pipelineFound = false;
                Regex expression = new Regex(validArgs.NameMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    try
                    {
                        foreach (Pipeline pipeline in catalogue.Pipelines)
                        {
                            Match matcher = expression.Match(pipeline.FullName);
                            if (matcher.Success)
                            {
                                pipelineFound = true;
                                pipeline.Tracking = PipelineTrackingTypes.None;
                                foreach (PipelineTrackingTypes trackingOption in trackingOptions)
                                {
                                    pipeline.Tracking = pipeline.Tracking | trackingOption;
                                }
                                _output.Add(String.Format("Pipeline: {0} updated", pipeline.FullName));
                            }
                        }

                        // Commit the changes
                        if (pipelineFound)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (pipelineFound)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!pipelineFound)
                {
                    _output.Add("Pipeline: " + validArgs.NameMask + " not found!");
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
            usage.Add(base.GetExeName() + " " + CommandType.SetPipelineTracking.ToString() +
                " -Options:<Option1,Option2,Option3> -TypeName:<Pipeline Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE TRACKING OPTION PARAMETERS -");
            foreach (string option in Enum.GetNames(typeof(PipelineTrackingOption)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start pipeline type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetPipelineTracking.ToString() +
                " -Options:" + PipelineTrackingOption.None.ToString() + " -TypeName:^Microsoft.BizTalk.DefaultPipelines.XMLReceive$");
            usage.Add(base.GetExeName() + " " + CommandType.SetPipelineTracking.ToString() +
                " -Options:" + PipelineTrackingOption.PipelineEvents.ToString() + "," +
                PipelineTrackingOption.InboundMessageBody.ToString() + "," +
                PipelineTrackingOption.OutboundMessageBody.ToString() + " -TypeName:^Microsoft.BizTalk.DefaultPipelines.PassThruTransmit$");
            usage.Add(base.GetExeName() + " " + CommandType.SetPipelineTracking.ToString() +
                " -Options:" + PipelineTrackingOption.None.ToString() + " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        protected static List<PipelineTrackingTypes> MapInternalPipelineTrackingOptions(List<string> internalOptions)
        {
            List<PipelineTrackingOption> internalTrackingOptions = new List<PipelineTrackingOption>();
            foreach (string internalOption in internalOptions)
            {
                PipelineTrackingOption internalTrackingOption = (PipelineTrackingOption)Enum.Parse(typeof(PipelineTrackingOption), internalOption, false);
                internalTrackingOptions.Add(internalTrackingOption);
            }

            List<PipelineTrackingTypes> trackingOptions = new List<PipelineTrackingTypes>();
            foreach (PipelineTrackingOption internalOption in internalTrackingOptions)
            {
                switch (internalOption)
                {
                    case PipelineTrackingOption.InboundMessageBody: trackingOptions.Add(PipelineTrackingTypes.InboundMessageBody); break;
                    case PipelineTrackingOption.MessageSendReceive: trackingOptions.Add(PipelineTrackingTypes.MessageSendReceive); break;
                    case PipelineTrackingOption.None: trackingOptions.Add(PipelineTrackingTypes.None); break;
                    case PipelineTrackingOption.OutboundMessageBody: trackingOptions.Add(PipelineTrackingTypes.OutboundMessageBody); break;
                    case PipelineTrackingOption.PipelineEvents: trackingOptions.Add(PipelineTrackingTypes.PipelineEvents); break;
                    case PipelineTrackingOption.ServiceStartEnd: trackingOptions.Add(PipelineTrackingTypes.ServiceStartEnd); break;
                }
            }
            return trackingOptions;
        }
    }
}
