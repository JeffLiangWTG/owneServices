using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace CargoWise.eHub.Products.NZCustoms.Client.SignatureRemoval
{
	public class RemoveResponseSignatureChannelBindingElementExtensionElement : BindingElementExtensionElement
	{
		public override Type BindingElementType
		{
			get { return typeof(RemoveResponseSignatureChannelBindingElement); }
		}

		protected override BindingElement CreateBindingElement()
		{
			return new RemoveResponseSignatureChannelBindingElement();
		}
	}
}