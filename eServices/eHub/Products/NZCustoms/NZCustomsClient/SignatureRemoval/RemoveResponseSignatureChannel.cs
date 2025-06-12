using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.NZCustoms.Client.SignatureRemoval
{
	public class RemoveResponseSignatureChannel : IRequestChannel
	{
		readonly IRequestChannel innerChannel;

		public RemoveResponseSignatureChannel(IRequestChannel innerchannel)
		{
			this.innerChannel = innerchannel;
			innerchannel.Closed += (sender, e) => { if (Closed != null) Closed(sender, e); };
			innerchannel.Closing += (sender, e) => { if (Closing != null) Closing(sender, e); };
			innerchannel.Faulted += (sender, e) => { if (Faulted != null) Faulted(sender, e); };
			innerchannel.Opened += (sender, e) => { if (Opened != null) Opened(sender, e); };
			innerchannel.Opening += (sender, e) => { if (Opening != null) Opening(sender, e); };
		}

		#region IRequestChannel Members

		public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return innerChannel.BeginRequest(message, timeout, callback, state);
		}

		public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
		{
			return innerChannel.BeginRequest(message, callback, state);
		}

		public Message EndRequest(IAsyncResult result)
		{
			return innerChannel.EndRequest(result);
		}

		public EndpointAddress RemoteAddress
		{
			get { return innerChannel.RemoteAddress; }
		}

		public Message Request(Message message, TimeSpan timeout)
		{
			Message ret = innerChannel.Request(message, timeout);

			if (ret != null)
			{
				int index = ret.Headers.FindHeader("Security", @"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

				if (index >= 0)
				{
					ret.Headers.RemoveAt(index);
				}
			}

			return ret;
		}

		public Message Request(Message message)
		{
			Message ret = Request(message, new TimeSpan(0, 1, 30));
			return ret;
		}

		public Uri Via
		{
			get { return innerChannel.Via; }
		}

		#endregion

		#region IChannel Members

		public T GetProperty<T>() where T : class
		{
			return innerChannel.GetProperty<T>();
		}

		#endregion

		#region ICommunicationObject Members

		public void Abort()
		{
			innerChannel.Abort();
		}

		public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
		{
			return innerChannel.BeginClose(timeout, callback, state);
		}

		public IAsyncResult BeginClose(AsyncCallback callback, object state)
		{
			return innerChannel.BeginClose(callback, state);
		}

		public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
		{
			return innerChannel.BeginOpen(timeout, callback, state);
		}

		public IAsyncResult BeginOpen(AsyncCallback callback, object state)
		{
			return innerChannel.BeginOpen(callback, state);
		}

		public void Close(TimeSpan timeout)
		{
			innerChannel.Close(timeout);
		}

		public void Close()
		{
			innerChannel.Close();
		}

		public event EventHandler Closed;

		public event EventHandler Closing;

		public void EndClose(IAsyncResult result)
		{
			innerChannel.EndClose(result);
		}

		public void EndOpen(IAsyncResult result)
		{
			innerChannel.EndOpen(result);
		}

		public event EventHandler Faulted;

		public void Open(TimeSpan timeout)
		{
			innerChannel.Open(timeout);
		}

		public void Open()
		{
			innerChannel.Open();
		}

		public event EventHandler Opened;

		public event EventHandler Opening;

		public CommunicationState State
		{
			get { return innerChannel.State; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			IDisposable d = innerChannel as IDisposable;
			if (d != null)
			{
				d.Dispose();
			}
		}

		#endregion
	}
}
