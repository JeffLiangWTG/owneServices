using System;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	[ApplicableLoginCountry(new[] { CountryCodes.Ireland })]
	public class IEH7DeclarationCommand : BaseH7DeclarationCommand
	{
		public IEH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.IEH7.IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new IEH7DeclarationConverter(this);
	}
}
