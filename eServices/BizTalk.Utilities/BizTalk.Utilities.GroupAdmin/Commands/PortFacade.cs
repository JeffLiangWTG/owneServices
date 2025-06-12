using System.Collections.Generic;
using System.Text.RegularExpressions;
using BizTalk.Utilities.GroupAdmin.Options;
using Microsoft.BizTalk.ExplorerOM;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal class PortFacade
    {
        private ReceivePort _receivePort;
        private SendPort _sendPort;

        /// <summary>
        /// Iterate through the port collection and return the port facade for the port name regular expression match
        /// </summary>
        internal static List<PortFacade> NewCollection(BtsCatalogExplorer catalogue, CommandType command, string portNameMask)
        {
            List<PortFacade> ports = new List<PortFacade>();
            Regex expression = new Regex(portNameMask, RegexOptions.None);

            if (command == CommandType.SetReceivePortTracking || command == CommandType.GetReceivePortTracking)
            {
                foreach (ReceivePort receiver in catalogue.ReceivePorts)
                {
                    Match matcher = expression.Match(receiver.Name);
                    if (matcher.Success)
                    {
                        ports.Add(new PortFacade(receiver));
                    }
                }
            }
            else if (command == CommandType.SetSendPortTracking || command == CommandType.GetSendPortTracking)
            {   
                foreach (SendPort sender in catalogue.SendPorts)
                {
                    Match matcher = expression.Match(sender.Name);
                    if (matcher.Success)
                    {
                        ports.Add(new PortFacade(sender));
                    }
                }
            }
            return ports;
        }

        /// <summary>
        /// Receive port facage constructir
        /// </summary>
        private PortFacade(ReceivePort receivePort)
        {
            _receivePort = receivePort;
        }

        /// <summary>
        /// Send port facage constructir
        /// </summary>
        private PortFacade(SendPort sendPort)
        {
            _sendPort = sendPort;
        }

        /// <summary>
        /// Get/set the tracking options on the receive or send port
        /// </summary>
        internal TrackingTypes Tracking
        {
            get
            {
                if (_receivePort != null)
                {
                    return _receivePort.Tracking;
                }
                else
                {
                    return _sendPort.Tracking;
                }
            }
            set
            {
                if (_receivePort != null)
                {
                    _receivePort.Tracking = value;
                }
                else
                {
                    _sendPort.Tracking = value;
                }
            }
        }

        /// <summary>
        /// Get the underlying object's name property
        /// </summary>
        internal string Name
        {
            get
            {
                if (_receivePort != null)
                {
                    return _receivePort.Name;
                }
                else
                {
                    return _sendPort.Name;
                }
            }
        }
    }
}
