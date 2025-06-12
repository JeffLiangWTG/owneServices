using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class ConcurrentTransferrerPool : IDisposable
	{
		readonly ITransferrerFactory transferrerFactory;
		readonly ConcurrentStack<ITransferrer> transferrers;
		readonly TransferrerProperties.Receive.Location location;
		readonly XmlDocument locationConfigDom;
		readonly CancellationToken cancellationToken;
		readonly TransferrerProperties.Receive properties;

		public ConcurrentTransferrerPool(ITransferrerFactory transferrerFactory, TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, CancellationToken cancellationToken, XmlDocument locationConfigDom)
		{
			this.transferrerFactory = transferrerFactory;
			transferrers = new ConcurrentStack<ITransferrer>();
			this.location = location;
			this.properties = properties;
			this.cancellationToken = cancellationToken;
			this.locationConfigDom = locationConfigDom;
		}

		public void Dispose()
		{
			var exceptions = new List<Exception>();

			foreach (var transferrer in transferrers)
			{
				InvokeAndCaptureException(exceptions, () => transferrer.Dispose());
			}
			transferrers.Clear();

			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions.ToArray());
			}
		}

		static void InvokeAndCaptureException(List<Exception> exceptions, Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				exceptions.Add(ex);
			}
		}

		public void InvokeActionWithTransferrer(Action<ITransferrer> transferrerAction)
		{
			var transferrer = Pop();

			try
			{
				transferrerAction(transferrer);
			}
			catch
			{
				transferrer.Dispose();
				throw;
			}

			Push(transferrer);
		}

		ITransferrer Pop()
		{
			if (transferrers.TryPop(out var transferrer))
			{
				return transferrer;
			}

			transferrer = transferrerFactory.CreateTransferrer();
			TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Transferrer created. Hash Code = {0}", transferrer.GetHashCode());
			transferrer.Server = location.Server;
			transferrer.Port = location.Port;
			transferrer.UserName = location.UserName;
			transferrer.Password = location.Password;
			transferrer.Timeout = properties.Timeout;
			transferrer.Logger = properties.Logger;
			transferrer.CancelToken = cancellationToken;
			transferrer.ReadLocationConfiguration(locationConfigDom);
			transferrer.Open();
			return transferrer;
		}

		void Push(ITransferrer transferrer)
		{
			transferrers.Push(transferrer);
		}
	}
}
