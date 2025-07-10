using System;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer
{
	[ApplicableLoginCountry([CountryCodes.France])]
	public class FRH7DeclarationCommand : BaseH7DeclarationCommand
	{
		public FRH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.FRH7.IH7ManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new FRH7DeclarationConverter(this);
	}
}
