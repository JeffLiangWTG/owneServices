using System;
using System.Collections.Generic;
using System.Text;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class BaseTrackingCommand<TSuperClass> : AbstractCommand
    {
        private TrackedObject _objectForTracking;
        private enum TrackedObject
        {
            Orchestration,
            Pipeline,
            Port
        }

        protected BaseTrackingCommand(string[] args) : base(args)
        {
            if (typeof(TSuperClass) == typeof(GetOrchTrackingCommand) || typeof(TSuperClass) == typeof(SetOrchTrackingCommand))
            {
                _objectForTracking = TrackedObject.Orchestration;
            }
            else if (typeof(TSuperClass) == typeof(GetPipelineTrackingCommand) || typeof(TSuperClass) == typeof(SetPipelineTrackingCommand))
            {
                _objectForTracking = TrackedObject.Pipeline;
            }
            else if (typeof(TSuperClass) == typeof(GetPortTrackingCommand) || typeof(TSuperClass) == typeof(SetPortTrackingCommand))
            {
                _objectForTracking = TrackedObject.Port;
            }
            else
            {
                throw new ArgumentException("Super class not supported", "superClass");
            }
        }

        internal override void Execute()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void ReportUsage()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Delegate for validating the arguments passed in by the user on a set command
        /// </summary>
        /// <typeparam name="T">Internal tracking option enum type</typeparam>
        protected ValidatedTrackingArgs ValidateSetterArguments<T>()
        {
            ValidatedTrackingArgs trackingArgs = new ValidatedTrackingArgs();
            trackingArgs.Valid = false;

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                return trackingArgs;
            }

            string optionsParam = base._rawArgs[1];
            string nameMaskParam = base._rawArgs[2];

            // Check param 2 and 3 are -Options: and -PortName, -TypeName or -Name
            if (!optionsParam.StartsWith("-Options:"))
            {
                return trackingArgs;
            }
            else if (
                (_objectForTracking == TrackedObject.Port && !nameMaskParam.StartsWith("-PortName:")) ||
                (_objectForTracking == TrackedObject.Orchestration && !nameMaskParam.StartsWith("-TypeName:")) ||
                (_objectForTracking == TrackedObject.Pipeline && !nameMaskParam.StartsWith("-TypeName:")))
            {
                return trackingArgs;
            }

            // Check param 2 is a valid option
            string optionsConcat = optionsParam.Split(':')[1];
            List<string> requiredInternalOptions = new List<string>(optionsConcat.Split(','));
            List<string> validInternalOptions = new List<string>(Enum.GetNames(typeof(T)));
            foreach (string requiredInternalOption in requiredInternalOptions)
            {
                if (!validInternalOptions.Contains(requiredInternalOption))
                {
                    return trackingArgs;
                }
            }

            // Check param 3 value is not blank
            string nameMask = nameMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(nameMask))
            {
                return trackingArgs;
            }

            // Successfully passed validation
            trackingArgs.Valid = true;
            trackingArgs.NameMask = nameMask;
            trackingArgs.Options = requiredInternalOptions;
            return trackingArgs;
        }

        /// <summary>
        /// Delegate for validating the arguments passed in by the user on a get command
        /// </summary>
        protected ValidatedTrackingArgs ValidateGetterArguments()
        {
            ValidatedTrackingArgs trackingArgs = new ValidatedTrackingArgs();
            trackingArgs.Valid = false;

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                return trackingArgs;
            }

            string nameMaskParam = base._rawArgs[1];

            // Check param 2 is -PortName, -TypeName or -Name
            if ((_objectForTracking == TrackedObject.Port && !nameMaskParam.StartsWith("-PortName:")) ||
                (_objectForTracking == TrackedObject.Orchestration && !nameMaskParam.StartsWith("-TypeName:")) ||
                (_objectForTracking == TrackedObject.Pipeline && !nameMaskParam.StartsWith("-TypeName:")))
            {
                return trackingArgs;
            }

            // Check param 2 value is not blank
            string nameMask = nameMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(nameMask))
            {
                return trackingArgs;
            }

            // Successfully passed validation
            trackingArgs.Valid = true;
            trackingArgs.NameMask = nameMask;
            trackingArgs.Options = new List<string>();
            return trackingArgs;
        }

        protected static List<TrackingTypes> MapInternalPortTrackingOptions(List<string> internalOptions, CommandType command)
        {
            List<PortTrackingOption> trackingOptions = new List<PortTrackingOption>();
            foreach (string internalOption in internalOptions)
            {
                PortTrackingOption option = (PortTrackingOption)Enum.Parse(typeof(PortTrackingOption), internalOption, false);
                trackingOptions.Add(option);
            }
            return MapInternalPortTrackingOptions(trackingOptions, command);
        }

        private static List<TrackingTypes> MapInternalPortTrackingOptions(List<PortTrackingOption> internalOptions, CommandType command)
        {
            List<TrackingTypes> trackingOptions = new List<TrackingTypes>();
            foreach (PortTrackingOption option in internalOptions)
            {
                if (command == CommandType.SetReceivePortTracking)
                {
                    switch (option)
                    {
                        case PortTrackingOption.RequestMessageBodyAfterPortProcessing: trackingOptions.Add(TrackingTypes.AfterReceivePipeline); break;
                        case PortTrackingOption.RequestMessageBodyBeforePortProcessing: trackingOptions.Add(TrackingTypes.BeforeReceivePipeline); break;
                        case PortTrackingOption.RequestMessagePropsAfterPortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesAfterReceivePipeline); break;
                        case PortTrackingOption.RequestMessagePropsBeforePortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesBeforeReceivePipeline); break;
                        case PortTrackingOption.ResponseMessageBodyAfterPortProcessing: trackingOptions.Add(TrackingTypes.AfterSendPipeline); break;
                        case PortTrackingOption.ResponseMessageBodyBeforePortProcessing: trackingOptions.Add(TrackingTypes.BeforeSendPipeline); break;
                        case PortTrackingOption.ResponseMessagePropsAfterPortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesAfterSendPipeline); break;
                        case PortTrackingOption.ResponseMessagePropsBeforePortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesBeforeSendPipeline); break;
                    }
                }
                else if (command == CommandType.SetSendPortTracking)
                {
                    switch (option)
                    {
                        case PortTrackingOption.RequestMessageBodyAfterPortProcessing: trackingOptions.Add(TrackingTypes.AfterSendPipeline); break;
                        case PortTrackingOption.RequestMessageBodyBeforePortProcessing: trackingOptions.Add(TrackingTypes.BeforeSendPipeline); break;
                        case PortTrackingOption.RequestMessagePropsAfterPortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesAfterSendPipeline); break;
                        case PortTrackingOption.RequestMessagePropsBeforePortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesBeforeSendPipeline); break;
                        case PortTrackingOption.ResponseMessageBodyAfterPortProcessing: trackingOptions.Add(TrackingTypes.AfterReceivePipeline); break;
                        case PortTrackingOption.ResponseMessageBodyBeforePortProcessing: trackingOptions.Add(TrackingTypes.BeforeReceivePipeline); break;
                        case PortTrackingOption.ResponseMessagePropsAfterPortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesAfterReceivePipeline); break;
                        case PortTrackingOption.ResponseMessagePropsBeforePortProcessing: trackingOptions.Add(TrackingTypes.TrackPropertiesBeforeReceivePipeline); break;
                    }
                }
                else
                {
                    throw new NotSupportedException("Port tracking mapping for command " + command.ToString() + " not supported");
                }
            }
            return trackingOptions;
        }

        /// <summary>
        /// Converts bitwise enum to the equivalent comma separated concatenation of GroupAdmin port tracking options
        /// </summary>
        /// <param name="portTracking">BizTalk port tracking types</param>
        protected static string MapBtsPortTrackingTypes(TrackingTypes portTracking, CommandType command)
        {
            List<string> btsTrackingTypes = new List<string>();
            btsTrackingTypes.AddRange(portTracking.ToString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));

            List<TrackingTypes> btsTrackingTypeEnums = new List<TrackingTypes>();
            btsTrackingTypeEnums.AddRange(btsTrackingTypes.ConvertAll<TrackingTypes>(
                new Converter<string, TrackingTypes>(ConvertBtsTrackingType<TrackingTypes>)));

            StringBuilder concat = new StringBuilder();
            foreach (TrackingTypes trackingType in btsTrackingTypeEnums)
            {
                if (concat.Length > 0)
                {
                    concat.Append(',');
                }
                if (command == CommandType.GetReceivePortTracking)
                {
                    switch (trackingType)
                    {
                        case TrackingTypes.AfterReceivePipeline: concat.Append(PortTrackingOption.RequestMessageBodyAfterPortProcessing.ToString()); break;
                        case TrackingTypes.BeforeReceivePipeline: concat.Append(PortTrackingOption.RequestMessageBodyBeforePortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesAfterReceivePipeline: concat.Append(PortTrackingOption.RequestMessagePropsAfterPortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesBeforeReceivePipeline: concat.Append(PortTrackingOption.RequestMessagePropsBeforePortProcessing.ToString()); break;
                        case TrackingTypes.AfterSendPipeline: concat.Append(PortTrackingOption.ResponseMessageBodyAfterPortProcessing.ToString()); break;
                        case TrackingTypes.BeforeSendPipeline: concat.Append(PortTrackingOption.ResponseMessageBodyBeforePortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesAfterSendPipeline: concat.Append(PortTrackingOption.ResponseMessagePropsAfterPortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesBeforeSendPipeline: concat.Append(PortTrackingOption.ResponseMessagePropsBeforePortProcessing.ToString()); break;
                        default: concat.Append(PortTrackingOption.None.ToString()); break;
                    }
                }
                else if (command == CommandType.GetSendPortTracking)
                {
                    switch (trackingType)
                    {
                        case TrackingTypes.AfterSendPipeline: concat.Append(PortTrackingOption.RequestMessageBodyAfterPortProcessing.ToString()); break;
                        case TrackingTypes.BeforeSendPipeline: concat.Append(PortTrackingOption.RequestMessageBodyBeforePortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesAfterSendPipeline: concat.Append(PortTrackingOption.RequestMessagePropsAfterPortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesBeforeSendPipeline: concat.Append(PortTrackingOption.RequestMessagePropsBeforePortProcessing.ToString()); break;
                        case TrackingTypes.AfterReceivePipeline: concat.Append(PortTrackingOption.ResponseMessageBodyAfterPortProcessing.ToString()); break;
                        case TrackingTypes.BeforeReceivePipeline: concat.Append(PortTrackingOption.ResponseMessageBodyBeforePortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesAfterReceivePipeline: concat.Append(PortTrackingOption.ResponseMessagePropsAfterPortProcessing.ToString()); break;
                        case TrackingTypes.TrackPropertiesBeforeReceivePipeline: concat.Append(PortTrackingOption.ResponseMessagePropsBeforePortProcessing.ToString()); break;
                        default: concat.Append(PortTrackingOption.None.ToString()); break;
                    }
                }
                else
                {
                    throw new NotSupportedException("Port tracking mapping for command " + command.ToString() + " not supported");
                }
            }
            return concat.ToString();
        }

        /// <summary>
        /// Converts an enum name to the enum type
        /// </summary>
        protected static TTrackingType ConvertBtsTrackingType<TTrackingType>(string name)
        {
            return (TTrackingType)Enum.Parse(typeof(TTrackingType), name);
        }
    }
}
