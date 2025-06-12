using System;
using System.Collections.Generic;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetPortTrackingCommand : BaseTrackingCommand<SetPortTrackingCommand>
    {
        CommandType _command;

        internal SetPortTrackingCommand(string[] args, CommandType command) : base(args)
        {
            _command = command;
        }

        /// <summary>
        /// Validate the arguments and then execute the port tracking option update
        /// </summary>
        /// <remarks>Supports port name regular expression matching</remarks>
        internal override void Execute()
        {
            ValidatedTrackingArgs validArgs = base.ValidateSetterArguments<PortTrackingOption>();
            if (!validArgs.Valid)
            {
                this.ReportUsage();
                return;
            }

            // Map the internal tracking options to the ExplorerOM tracking types
            List<TrackingTypes> trackingOptions = MapInternalPortTrackingOptions(validArgs.Options, _command);

            try
            {
                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;
                    try
                    {
                        List<PortFacade> ports = PortFacade.NewCollection(catalogue, _command, validArgs.NameMask);
                        if (ports.Count > 0)
                        {
                            foreach (PortFacade port in ports)
                            {
                                port.Tracking = 0; // None
                                foreach (TrackingTypes trackingOption in trackingOptions)
                                {
                                    port.Tracking = port.Tracking | trackingOption;
                                }
                                _output.Add(String.Format("Port: {0} updated", port.Name));
                            }
                        }
                        else
                        {
                            _output.Add(String.Format("Port: {0} not found!", validArgs.NameMask));
                        }

                        // Commit the changes
                        catalogue.SaveChanges();
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        catalogue.DiscardChanges();
                        throw;
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
            usage.Add(base.GetExeName() + " " + _command.ToString() + 
                " -Options:<Option1,Option2,Option3> -PortName:<Port Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("- CASE-SENSITIVE TRACKING OPTION PARAMETERS -");
            foreach (string option in Enum.GetNames(typeof(PortTrackingOption)))
            {
                usage.Add(option);
            }
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start port names with ^ and end names with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + _command.ToString() + 
                " -Options:" + PortTrackingOption.None.ToString() + " -PortName:^ReceivePort1$");
            usage.Add(base.GetExeName() + " " + CommandType.SetReceivePortTracking.ToString() +
                " -Options:" + PortTrackingOption.RequestMessageBodyBeforePortProcessing.ToString() + "," +
                PortTrackingOption.RequestMessageBodyAfterPortProcessing.ToString() + " -PortName:^ReceivePort2$");
            usage.Add(base.GetExeName() + " " + CommandType.SetSendPortTracking.ToString() +
                " -Options:" + PortTrackingOption.RequestMessageBodyBeforePortProcessing.ToString() + "," +
                PortTrackingOption.ResponseMessageBodyBeforePortProcessing.ToString() + " -PortName:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
