using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.NZCustoms.Client.SignatureRemoval
{
	public class RemoveResponseSignatureChannelBindingElement : BindingElement
	{
		public RemoveResponseSignatureChannelBindingElement()
		{
		}

		protected RemoveResponseSignatureChannelBindingElement(RemoveResponseSignatureChannelBindingElement other)
			: base(other)
		{
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
		{
			return new RemoveResponseSignatureChannelFactory<TChannel> { InnerChannelFactory = context.BuildInnerChannelFactory<TChannel>() };
		}

		public override BindingElement Clone()
		{
			return new RemoveResponseSignatureChannelBindingElement(this);
		}

		public override T GetProperty<T>(BindingContext context)
		{
			return context.GetInnerProperty<T>();
		}
	}
}