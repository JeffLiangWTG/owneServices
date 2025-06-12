using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetPipelineTrackingCommand : BaseTrackingCommand<GetPipelineTrackingCommand>
    {
        internal GetPipelineTrackingCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the pipeline tracking query
        /// </summary>
        /// <remarks>Supports pipeline type name regular expression matching</remarks>
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
                bool pipelineFound = false;
                Regex expression = new Regex(validArgs.NameMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    foreach (Pipeline pipeline in catalogue.Pipelines)
                    {
                        Match matcher = expression.Match(pipeline.FullName);
                        if (matcher.Success)
                        {
                            pipelineFound = true;
                            string optionsConcat = MapBtsPipelineTrackingTypes(pipeline.Tracking);
                            _output.Add(String.Format("Pipeline: {0}; Tracking Options: {1}",
                                pipeline.FullName, optionsConcat));
                        }
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
            usage.Add(base.GetExeName() + " " + CommandType.GetPipelineTracking.ToString() + " -TypeName:<Pipeline Type Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start pipeline type names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetPipelineTracking.ToString() + " -TypeName:^Microsoft.BizTalk.DefaultPipelines.XMLReceive$");
            usage.Add(base.GetExeName() + " " + CommandType.GetPipelineTracking.ToString() + " -TypeName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }

        /// <summary>
        /// Converts bitwise enum to the equivalent comma separated concatenation of GroupAdmin 
        /// pipeline tracking options
        /// </summary>
        /// <param name="btsTrackingTypes">BizTalk pipeline tracking types</param>
        protected static string MapBtsPipelineTrackingTypes(PipelineTrackingTypes pipelineTracking)
        {
            List<string> btsTrackingTypes = new List<string>();
            btsTrackingTypes.AddRange(pipelineTracking.ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));

            List<PipelineTrackingTypes> btsTrackingTypeEnums = new List<PipelineTrackingTypes>();
            btsTrackingTypeEnums.AddRange(btsTrackingTypes.ConvertAll<PipelineTrackingTypes>(
                new Converter<string, PipelineTrackingTypes>(ConvertBtsTrackingType<PipelineTrackingTypes>)));

            StringBuilder concat = new StringBuilder();
            foreach (PipelineTrackingTypes trackingType in btsTrackingTypeEnums)
            {
                if (concat.Length > 0)
                {
                    concat.Append(',');
                }
                switch (trackingType)
                {
                    case PipelineTrackingTypes.InboundMessageBody: concat.Append(PipelineTrackingOption.InboundMessageBody.ToString()); break;
                    case PipelineTrackingTypes.MessageSendReceive: concat.Append(PipelineTrackingOption.MessageSendReceive.ToString()); break;
                    case PipelineTrackingTypes.None: concat.Append(PipelineTrackingOption.None.ToString()); break;
                    case PipelineTrackingTypes.OutboundMessageBody: concat.Append(PipelineTrackingOption.OutboundMessageBody.ToString()); break;
                    case PipelineTrackingTypes.PipelineEvents: concat.Append(PipelineTrackingOption.PipelineEvents.ToString()); break;
                    case PipelineTrackingTypes.ServiceStartEnd: concat.Append(PipelineTrackingOption.ServiceStartEnd.ToString()); break;
                }
            }
            return concat.ToString();
        }
    }
}
