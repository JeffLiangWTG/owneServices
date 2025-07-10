using System;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.GB;

namespace Enterprise.eTail.DataTransfer
{
	[ApplicableLoginCountry(new[] { CountryCodes.UnitedKingdom })]
	public class GBH7DeclarationCommand : BaseH7DeclarationCommand
	{
		public GBH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("8d731c93-8fb4-455a-8014-66e979790986", "Low Value (H7/BIRDS)");

		protected override Type RelatedCustomsJobType => ObjectFactory.GetType<GBH7.IAsycudaManifestHeader>();

		public override CustomsRelatedBusinessObjectConverter Converter => new GBH7DeclarationConverter(this);
	}
}
