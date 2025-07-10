using System;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	[ApplicableLoginCountry([CountryCodes.Spain])]
	public class ESH7DeclarationCommand : BaseH7DeclarationCommand
	{
		public ESH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.ESH7.IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new ESH7DeclarationConverter(this);
	}
}
