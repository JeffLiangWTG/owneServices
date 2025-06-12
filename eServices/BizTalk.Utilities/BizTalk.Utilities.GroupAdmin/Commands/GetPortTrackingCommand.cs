using System;
using System.Collections.Generic;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetPortTrackingCommand : BaseTrackingCommand<GetPortTrackingCommand>
    {
        CommandType _command;

        internal GetPortTrackingCommand(string[] args, CommandType command) : base(args)
        {
            _command = command;
        }

        /// <summary>
        /// Validate the arguments and then execute the port tracking query
        /// </summary>
        /// <remarks>Supports port name regular expression matching</remarks>
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
                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    List<PortFacade> ports = PortFacade.NewCollection(catalogue, _command, validArgs.NameMask);
                    if (ports.Count > 0)
                    {
                        foreach (PortFacade port in ports)
                        {
                            string optionsConcat = MapBtsPortTrackingTypes(port.Tracking, _command);
                            _output.Add(String.Format("Port: {0}; Tracking Options: {1}", port.Name, optionsConcat));
                        }
                    }
                    else
                    {
                        _output.Add(String.Format("Port: {0} not found!", validArgs.NameMask));
                    }
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
            usage.Add(base.GetExeName() + " " + _command.ToString() + " -PortName:<Port Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start port names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + _command.ToString() + " -PortName:^ReceivePort1$");
            usage.Add(base.GetExeName() + " " + _command.ToString() + " -PortName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
