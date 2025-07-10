using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX301GoodsShipmentGovernmentAgencyGoodsItem : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public NX301GoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override IPackaging GetPackagingCore() => InvoiceLine.JI_PackagingQTY.IsEmpty ? null : base.GetPackagingCore();
	}
}
