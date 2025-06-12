using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.NZCustoms.Client.SignatureRemoval
{
	public class RemoveResponseSignatureChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		IChannelFactory<TChannel> innerChannelFactory;

		public IChannelFactory<TChannel> InnerChannelFactory
		{
			get { return innerChannelFactory; }
			set { innerChannelFactory = value; }
		}

		protected override void OnOpen(TimeSpan timeout)
		{
			innerChannelFactory.Open(timeout);
		}

		protected override TChannel OnCreateChannel(EndpointAddress to, Uri via)
		{
			TChannel innerchannel = innerChannelFactory.CreateChannel(to, via);
			if (typeof(TChannel) == typeof(IRequestChannel))
			{
				RemoveResponseSignatureChannel cachereqCnl = new RemoveResponseSignatureChannel((IRequestChannel)innerchannel);
				return (TChannel)(object)cachereqCnl;
			}
			return innerchannel;
		} 

		protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		protected override void OnEndOpen(IAsyncResult result)
		{
			throw new NotImplementedException();
		}
	}
}