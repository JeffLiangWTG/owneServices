using System;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	[ApplicableLoginCountry([CountryCodes.Italy])]
	public class ITH7DeclarationCommand : BaseH7DeclarationCommand
	{
		public ITH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.ITH7.IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new ITH7DeclarationConverter(this);
	}
}
