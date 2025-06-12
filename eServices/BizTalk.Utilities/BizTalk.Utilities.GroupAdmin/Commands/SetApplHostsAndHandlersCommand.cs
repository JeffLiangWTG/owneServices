using System;
using System.Collections.Generic;
using BizTalk.Utilities.Common;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class SetApplHostsAndHandlersCommand : AbstractCommand
    {
        internal SetApplHostsAndHandlersCommand(string[] args) : base(args) { }

        /// <summary>
        /// Validate the arguments and then execute the host property update
        /// </summary>
        /// <remarks>Supports host name regular expression matching.</remarks>
        internal override void Execute()
        {
            #region Validation

            // Check all the parameters are present
            if (base._rawArgs.Count != 5)
            {
                this.ReportUsage();
                return;
            }

            string applicationParam = base._rawArgs[1];
            string receiveHandlerParam = base._rawArgs[2];
            string sendHandlerParam = base._rawArgs[3];
            string processingHostParam = base._rawArgs[4];

            // Check param 2, 3, 4 and 5 are -Application, -ReceiveHost, -SendHost, -ProcessingHost
            if (!applicationParam.StartsWith("-Application:") ||
                !receiveHandlerParam.StartsWith("-ReceiveHandler:") ||
                !sendHandlerParam.StartsWith("-SendHandler:") ||
                !processingHostParam.StartsWith("-ProcessingHost:"))
            {
                this.ReportUsage();
                return;
            }

            // Check param 2 is not blank
            string applicationName = applicationParam.Split(':')[1];
            if (String.IsNullOrEmpty(applicationName))
            {
                this.ReportUsage();
                return;
            }

            // Check param 3 is not blank
            string[] receiveHandlerArr = receiveHandlerParam.Split(':')[1].Split(',');
            string receiveHandler = receiveHandlerArr[0];
            if (String.IsNullOrEmpty(receiveHandler))
            {
                this.ReportUsage();
                return;
            }
            string receiveHandlerIsolated = String.Empty;
            if (receiveHandlerArr.Length > 1)
            {
                receiveHandlerIsolated = receiveHandlerArr[1];
            }

            // Check param 4 is not blank
            string[] sendHandlerArr = sendHandlerParam.Split(':')[1].Split(',');
            string sendHandler = sendHandlerArr[0];
            if (String.IsNullOrEmpty(sendHandler))
            {
                this.ReportUsage();
                return;
            }
            string sendHandlerIsolated = String.Empty;
            if (sendHandlerArr.Length > 1)
            {
                sendHandlerIsolated = sendHandlerArr[1];
            }

            // Check param 5 is not blank
            string processingHost = processingHostParam.Split(':')[1];
            if (String.IsNullOrEmpty(processingHost))
            {
                this.ReportUsage();
                return;
            }

            #endregion

            try
            {
                bool applicationFound = false;

                // Get the connection string using WMI to assist
                string connectionString = ExplorerOMHelper.GetConnectionString();

                using (BtsCatalogExplorer catalogue = new BtsCatalogExplorer())
                {
                    try
                    {
                        catalogue.ConnectionString = connectionString;

                        #region Get requested Host
                        ReceiveHandler newReceiveHandler = null;
                        SendHandler newSendHandler = null;
                        Host newHost = GetHost(catalogue, processingHost);

                        if (newHost == null)
                        {
                            base.InitialiseOutput(OutputInitialiseOption.ForError);
                            _output.Add("The following host does not exist: " + processingHost);
                            return;
                        }
                        #endregion

                        _output.Add(String.Empty);

                        //Go through applications:
                        foreach (Application application in catalogue.Applications)
                        {
                            if (application.Name.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                applicationFound = true;

                                if (application.Status == Status.Started || application.Status == Status.PartiallyStarted)
                                {
                                    application.Stop(ApplicationStopOption.StopAll);
                                }

                                #region Replace Processing Hosts
                                foreach (BtsOrchestration orchestration in application.Orchestrations)
                                {
                                    orchestration.Host = newHost;
                                    _output.Add(String.Format("Application: {0}, Orchestration: {1}, Host changed to: {2} ", application.Name, orchestration.FullName, newHost.Name));
                                    _output.Add(String.Empty);
                                }
                                _output.Add(String.Empty);
                                #endregion

                                #region Replace Receive Handlers
                                foreach (ReceivePort receivePort in application.ReceivePorts)
                                {
                                    foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                                    {
                                        newReceiveHandler = GetReceiveHandler(catalogue, receiveHandler, receiveHandlerIsolated, receiveLocation.TransportType.Name);

                                        if (newReceiveHandler != null)
                                        {
                                            receiveLocation.ReceiveHandler = newReceiveHandler;
                                            _output.Add(String.Format("Application: {0}, Receive Location: {1}, Handler changed to: {2}", application.Name, receiveLocation.Name, newReceiveHandler.Name));
                                            _output.Add(String.Empty);
                                        }
                                        else
                                        {
                                            //throw new Exception(String.Format("Application: {0}, Receive Location: {1}, Handler {2} was NOT foud for Transport Type: {3}", new object[] { application.Name, receiveLocation.Name, receiveHandler, receiveLocation.TransportType.Name }));
                                            _output.Add(String.Format("ERROR: Application: {0}, Receive Location: {1}, In-Process Handler {2} and Isolated Handler {3} were NOT foud for Transport Type: {4}", new object[] { application.Name, receiveLocation.Name, receiveHandler, receiveHandlerIsolated, receiveLocation.TransportType.Name }));
                                            _output.Add(String.Empty);
                                        }
                                    }
                                }
                                _output.Add(String.Empty);
                                #endregion

                                #region Replace Send Handlers
                                foreach (SendPort sendPort in application.SendPorts)
                                {
                                    if (sendPort.PrimaryTransport != null && sendPort.PrimaryTransport.TransportType != null)
                                    {
                                        newSendHandler = GetSendHandler(catalogue, sendHandler, sendHandlerIsolated, sendPort.PrimaryTransport.TransportType.Name);

                                        if (newSendHandler != null)
                                        {
                                            sendPort.PrimaryTransport.SendHandler = newSendHandler;
                                            _output.Add(String.Format("Application: {0}, Send Port: {1}, Handler changed to: {2} for primary transport", application.Name, sendPort.Name, newSendHandler.Name));
                                            _output.Add(String.Empty);
                                        }
                                        else
                                        {
                                            //throw new Exception(String.Format("Application: {0}, Send Port: {1}, Handler {2} was NOT foud for Transport Type: {3}", new object[] { application.Name, sendPort.Name, sendHandler, sendPort.PrimaryTransport.TransportType.Name }));
                                            _output.Add(String.Format("ERROR: Application: {0}, Send Port: {1}, In-Process Handler {2} and Isolated Handler {3} were NOT foud for Transport Type: {4}", new object[] { application.Name, sendPort.Name, sendHandler, sendHandlerIsolated, sendPort.PrimaryTransport.TransportType.Name }));
                                            _output.Add(String.Empty);
                                        }
                                    }
                                    if (sendPort.SecondaryTransport != null && sendPort.SecondaryTransport.TransportType != null)
                                    {
                                        newSendHandler = GetSendHandler(catalogue, sendHandler, sendHandlerIsolated, sendPort.SecondaryTransport.TransportType.Name);

                                        if (newSendHandler != null)
                                        {
                                            sendPort.SecondaryTransport.SendHandler = newSendHandler;
                                            _output.Add(String.Format("Application: {0}, Send Port: {1}, Handler changed to: {2} for secondary transport \n\r", application.Name, sendPort.Name, newSendHandler.Name));
                                            _output.Add(String.Empty);
                                        }
                                        else
                                        {
                                            //throw new Exception(String.Format("Application: {0}, Send Port: {1}, Handler {2} was NOT foud for Transport Type: {3}", new object[] { application.Name, sendPort.Name, sendHandler, sendPort.SecondaryTransport.TransportType.Name }));
                                            _output.Add(String.Format("ERROR: Application: {0}, Send Port: {1}, In-Process Handler {2} and Isolated Handler {3} were NOT foud for Transport Type: {4}", new object[] { application.Name, sendPort.Name, sendHandler, sendHandlerIsolated, sendPort.SecondaryTransport.TransportType.Name }));
                                            _output.Add(String.Empty);
                                        }
                                    }
                                }
                                _output.Add(String.Empty);
                                #endregion

                                //Do not start application. Will be done manually!
                                //application.Start(ApplicationStartOption.StartAll);

                            }

                        }

                        // Commit changes:
                        catalogue.SaveChanges();

                    }
                    catch (Exception)
                    {
                        // Rollback the change
                        catalogue.DiscardChanges();
                        throw;
                    }
                }

                if (!applicationFound)
                {
                    _output.Add("The following application is not found: " + applicationName);
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
            usage.Add(base.GetExeName() + " " + CommandType.SetHostProperty.ToString() +
                " -Application:<Application Name RegEx> -ReceiveHandler:<Receive Handler Name> -SendHandler:<Send Handler Name> -ProcessingHost:<Processing Host Name>");
            usage.Add(string.Empty);
            usage.Add("Examples: Remember to start application name with ^ and end with $ if matching a specific name");

            base.InitialiseOutput(OutputInitialiseOption.ForUsage);
            _output.AddRange(usage);
        }


        private Host GetHost(BtsCatalogExplorer catalogue, string hostName)
        {
            Host retHost = null;

            foreach (Host host in catalogue.Hosts)
            {
                if (host.Name.Equals(hostName, StringComparison.InvariantCultureIgnoreCase))
                {
                    retHost = host;
                    break;
                }
            }
            return retHost;
        }

        private ReceiveHandler GetReceiveHandler(BtsCatalogExplorer catalogue, string receiveHandlerName, string receiveIsolatedHandlerName, string transportTypeName)
        {
            ReceiveHandler retHandler = null;

            foreach (ReceiveHandler receiveHandler in catalogue.ReceiveHandlers)
            {
                if ( (receiveHandler.Name.Equals(receiveHandlerName, StringComparison.InvariantCultureIgnoreCase) || receiveHandler.Name.Equals(receiveIsolatedHandlerName, StringComparison.InvariantCultureIgnoreCase) ) &&
                    receiveHandler.TransportType.Name.Equals(transportTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    retHandler = receiveHandler;
                    break;
                }
            }
            return retHandler;
        }

        private SendHandler GetSendHandler(BtsCatalogExplorer catalogue, string sendHandlerName, string sendIsolatedHandlerName, string transportTypeName)
        {
            SendHandler retHandler = null;


            foreach (SendHandler sendHandler in catalogue.SendHandlers)
            {
                if ((sendHandler.Name.Equals(sendHandlerName, StringComparison.InvariantCultureIgnoreCase) || sendHandler.Name.Equals(sendIsolatedHandlerName, StringComparison.InvariantCultureIgnoreCase)) &&
                    sendHandler.TransportType.Name.Equals(transportTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    retHandler = sendHandler;
                    break;
                }
            }
            return retHandler;
        }
    }
}
