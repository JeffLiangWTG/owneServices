using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class BillImportAction : SailingBillImportAction
	{
		public BillImportAction(CusInBondBill bill)
			: base(bill)
		{
		}

		internal new CusInBondBill Bill
		{
			get { return base.Bill as CusInBondBill; }
		}

		public ZString IssuerCode
		{
			get { return Bill.B0_IssuerCode; }
		}

		public override ZString BillNumber
		{
			get { return Bill.B0_MasterBillNumber; }
		}

		protected override bool IsSourceBillOfLadingValid
		{
			get { return base.IsSourceBillOfLadingValid && Bill.Header.BillsOfLading.Contains(((ISailingSynchronisationTarget<BillOfLading>)Bill).Source); }
		}
	}
}
