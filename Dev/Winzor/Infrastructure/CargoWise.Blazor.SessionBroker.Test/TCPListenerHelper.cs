using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	internal static class TCPListenerHelper
	{
		internal static async Task WaitReadySignal(IPAddress localAddress, int port, TimeSpan timeout,CancellationTokenSource cancellationTokenSource)
		{
			using var server = new TcpListener(localAddress, port);
			server.Start();
			using var timeoutCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
			timeoutCancellationSource.CancelAfter(timeout);
			try
			{
				await server.AcceptTcpClientAsync(timeoutCancellationSource.Token);
			}
			finally
			{
				server.Stop();
			}
		}
	}
}
