using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class GetReceiveLocationStatusCommand: AbstractCommand
    {
        internal GetReceiveLocationStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then query the receive location status
        /// </summary>
        /// <remarks>Supports receive location name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 2)
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is -ReceiveLocation
            string locationMaskParam = base._rawArgs[1]; 
            if (!locationMaskParam.StartsWith("-ReceiveLocation:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is not blank
            string locationMask = locationMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(locationMask))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool locationFound = false;
                Regex expression = new Regex(locationMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    foreach (ReceivePort receivePort in catalogue.ReceivePorts)
                    {
                        foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                        {
                            Match matcher = expression.Match(receiveLocation.Name);
                            if (matcher.Success)
                            {
                                locationFound = true;
                                _output.Add(String.Format("ReceiveLocation: {0}; Enabled: {1}",
                                    receiveLocation.Name, receiveLocation.Enable));
                            }
                        }
                    }
                }

                if (!locationFound)
                {
                    _output.Add(locationMask + " not found!");
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
            usage.Add(base.GetExeName() + " " + CommandType.GetReceiveLocationStatus.ToString() +
                " -ReceiveLocation:<Receive Location Name RegEx>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start receive location name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.GetReceiveLocationStatus.ToString() + " -ReceiveLocation:^pControl_Services_AckNack$");
            usage.Add(base.GetExeName() + " " + CommandType.GetReceiveLocationStatus.ToString() + " -ReceiveLocation:[a-zA-Z]");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
