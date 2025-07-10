using System;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ExportLicensingMessageGoodsShipment))]
	sealed class NX201_01ExportLicensingMessageGoodsShipmentTest : ExportLicensingMessageGoodsShipmentTest<NX201_01ExportLicensingMessageGoodsShipment>
	{
		protected override Type ExpectedGovernmentAgencyGoodsItemsType => typeof(NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem);

		protected override NX201_01ExportLicensingMessageGoodsShipment GetLicensingMessageGoodsShipment(CusTWControllingMessageHeader header)
		{
			return new NX201_01ExportLicensingMessageGoodsShipment(header);
		}
	}
}
