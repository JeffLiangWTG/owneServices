using System;
using Renci.SshNet.Common;
namespace Renci.SshNet
{
	/// <customization>
	/// Added for testing.
	/// </customization>
    public interface IBaseClient
    {
        void Connect();
        ConnectionInfo ConnectionInfo { get; }
        void Disconnect();
        void Dispose();
        event EventHandler<ExceptionEventArgs> ErrorOccurred;
        event EventHandler<HostKeyEventArgs> HostKeyReceived;
        bool IsConnected { get; }
        TimeSpan KeepAliveInterval { get; set; }
        void SendKeepAlive();
    }
}
