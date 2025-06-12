using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetReceiveLocationStatusCommand: AbstractCommand
    {
        internal SetReceiveLocationStatusCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the status property update
        /// </summary>
        /// <remarks>Supports receive location regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validate Arguments

            // Check all the parameters are present
            if (base._rawArgs.Count != 3)
            {
                this.ReportUsage();
                return;
            }

            // Check params 2 and 3 are -ReceiveLocation, -Enable or -Disable
            string receiveLocationMaskParam = base._rawArgs[1];
            string stateParam = base._rawArgs[2];
            if (!receiveLocationMaskParam.StartsWith("-ReceiveLocation:") ||
                (stateParam != "-Enable" && stateParam != "-Disable"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 value is not blank
            string receiveLocationMask = receiveLocationMaskParam.Split(':')[1];
            if (String.IsNullOrEmpty(receiveLocationMask))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool receiveLocationSet = false;
                Regex expression = new Regex(receiveLocationMask, RegexOptions.None);

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                // Perform the status update
                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    catalogue.ConnectionString = connectionString;

                    try
                    {
                        foreach (ReceivePort receivePort in catalogue.ReceivePorts)
                        {
                            foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                            {
                                Match matcher = expression.Match(receiveLocation.Name);
                                if (matcher.Success)
                                {
                                    receiveLocationSet = true;
                                    receiveLocation.Enable = (stateParam == "-Enable");
                                    _output.Add(String.Format("ReceiveLocation: {0}; Enabled: {1}",
                                            receiveLocation.Name, receiveLocation.Enable));
                                 }
                            }
                        }

                        // Commit the changes
                        if (receiveLocationSet)
                        {
                            catalogue.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        if (receiveLocationSet)
                        {
                            catalogue.DiscardChanges();
                        }
                        throw;
                    }
                }

                if (!receiveLocationSet)
                {
                    _output.Add(String.Format("Receive location {0} not updated", receiveLocationMask));
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
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationStatus.ToString() +
                " -ReceiveLocation:<Receive Location Name RegEx> [-Enable][-Disable]");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start receive location name with ^ and end with $ if matching a specific name");
            usage.Add(string.Empty);
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationStatus.ToString() +
                " -ReceiveLocation:^pControl_Services_AckNack$ -Enable");
            usage.Add(base.GetExeName() + " " + CommandType.SetReceiveLocationStatus.ToString() +
                " -ReceiveLocation:[a-zA-Z] -Disable");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }
    }
}
