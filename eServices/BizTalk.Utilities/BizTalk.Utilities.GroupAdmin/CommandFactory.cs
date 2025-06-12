using System;
using System.Collections.Generic;
using BizTalk.Utilities.GroupAdmin.Commands;
using BizTalk.Utilities.GroupAdmin.Options;

namespace BizTalk.Utilities.GroupAdmin
{
    internal static class CommandFactory
    {
        /// <summary>
        /// Resolve the command required or show all commands as help
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns></returns>
        internal static AbstractCommand GenerateInstance(string[] args)
        {
            List<string> validCommands = new List<string>(Enum.GetNames(typeof(CommandType)));
            if (args.Length < 1 || !validCommands.Contains(args[0]))
            {
                return new HelpCommand(args);
            }

            AbstractCommand commandRequested = null;
            CommandType actionRequested = (CommandType)Enum.Parse(typeof(CommandType), args[0], false);

            switch (actionRequested)
            {
                case CommandType.Help: commandRequested = new HelpCommand(args); break;
                case CommandType.CreateHost: commandRequested = new CreateHostCommand(args); break;
                case CommandType.CreateHostInstance: commandRequested = new CreateHostInstanceCommand(args); break;
                case CommandType.CreateReceiveHandler: commandRequested = new CreateReceiveHandlerCommand(args); break;
                case CommandType.CreateSendHandler: commandRequested = new CreateSendHandlerCommand(args); break;
                case CommandType.DeleteReceiveHandler: commandRequested = new DeleteReceiveHandlerCommand(args); break;
                case CommandType.DeleteSendHandler: commandRequested = new DeleteSendHandlerCommand(args); break;
                case CommandType.DeleteHost: commandRequested = new DeleteHostCommand(args); break;
                case CommandType.DeleteHostInstance: commandRequested = new DeleteHostInstanceCommand(args); break;
                case CommandType.GetAppStatus: commandRequested = new GetAppStatusCommand(args); break;
                case CommandType.GetHostProperty: commandRequested = new GetHostPropertyCommand(args); break;
                case CommandType.GetOrchHost: commandRequested = new GetOrchHostCommand(args); break;
                case CommandType.GetOrchStatus: commandRequested = new GetOrchStatusCommand(args); break;
                case CommandType.GetOrchTracking: commandRequested = new GetOrchTrackingCommand(args); break;
                case CommandType.GetReceiveLocationHost: commandRequested = new GetReceiveLocationHostCommand(args); break;
                case CommandType.GetReceiveLocationStatus: commandRequested = new GetReceiveLocationStatusCommand(args); break;
                case CommandType.GetReceivePortTracking: commandRequested = new GetPortTrackingCommand(args, CommandType.GetReceivePortTracking); break;
                case CommandType.GetSendPortHost: commandRequested = new GetSendPortHostCommand(args); break;
                case CommandType.GetSendPortStatus: commandRequested = new GetSendPortStatusCommand(args); break;
                case CommandType.GetSendPortTracking: commandRequested = new GetPortTrackingCommand(args, CommandType.GetSendPortTracking); break;
                case CommandType.GetPipelineTracking: commandRequested = new GetPipelineTrackingCommand(args); break;
                case CommandType.GetRegistryKeyValue: commandRequested = new GetRegistryKeyValueCommand(args); break;
                case CommandType.SetAppStatus: commandRequested = new SetAppStatusCommand(args); break;
                case CommandType.SetApplHostsAndHandlers: commandRequested = new SetApplHostsAndHandlersCommand(args); break;
                case CommandType.SetHostInstanceState: commandRequested = new SetHostInstanceStateCommand(args); break;
                case CommandType.SetHostProperty: commandRequested = new SetHostPropertyCommand(args); break;
                case CommandType.SetOrchHost: commandRequested = new SetOrchHostCommand(args); break;
                case CommandType.SetOrchStatus: commandRequested = new SetOrchStatusCommand(args); break;
                case CommandType.SetOrchTracking: commandRequested = new SetOrchTrackingCommand(args); break;
                case CommandType.SetReceiveLocationHost: commandRequested = new SetReceiveLocationHostCommand(args); break;
                case CommandType.SetReceiveLocationStatus: commandRequested = new SetReceiveLocationStatusCommand(args); break;
                case CommandType.SetReceivePortTracking: commandRequested = new SetPortTrackingCommand(args, CommandType.SetReceivePortTracking); break;
                case CommandType.SetRegistryKeyValue: commandRequested = new SetRegistryKeyValueCommand(args); break;
                case CommandType.SetSendPortHost: commandRequested = new SetSendPortHostCommand(args); break;
                case CommandType.SetSendPortStatus: commandRequested = new SetSendPortStatusCommand(args); break;
                case CommandType.SetSendPortTracking: commandRequested = new SetPortTrackingCommand(args, CommandType.SetSendPortTracking); break;
                case CommandType.SetPipelineTracking: commandRequested = new SetPipelineTrackingCommand(args); break;
            }

            return commandRequested;
        }
    }
}
