using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override ZInt GetSequenceNumericCore() => InvoiceLine.CusEntryLine?.CL_LineNumber ?? ZInt.Zero;
	}
}
