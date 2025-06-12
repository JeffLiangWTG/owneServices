using System;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public abstract class ChannelHolder : IDisposable
	{
		public ChannelHolder(IConnection connection)
		{
			Channel = connection.CreateModel();
			Channel.BasicQos(0, 1, false);
		}

		protected IModel Channel { get; private set; }

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Channel?.Dispose();
					Channel = null;
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
