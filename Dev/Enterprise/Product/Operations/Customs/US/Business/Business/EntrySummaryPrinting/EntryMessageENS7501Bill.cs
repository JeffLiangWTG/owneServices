using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryMessageENS7501Bill : EntrySummary7501Bill, IObsoleteValidation
	{
		public EntryMessageENS7501Bill(ZGuid entryPK, BusinessObjectFactory factory, ZDate itDate, IBlock22BillDetails billDetails)
			: base(entryPK, factory)
		{
			this.billDetails = billDetails;
			this.itDate = itDate;
		}
		readonly IBlock22BillDetails billDetails;
		readonly ZDate itDate;

		public override ZDateTime ITDate
		{
			get { return itDate; }
		}

		public override ZString ITNO
		{
			get { return billDetails.ITNo; }
		}

		public override ZString EffectiveMasterBillIssuerSCAC
		{
			get { return billDetails.IssuerCodeOfMasterBillNumber; }
		}

		public override ZString MasterBill
		{
			get { return billDetails.MasterBillNumber; }
		}

		public override ZString EffectiveHouseBillIssuerSCAC
		{
			get { return billDetails.IssuerCodeOfHouseBillNumber; }
		}

		public override ZString HouseBill
		{
			get { return billDetails.HouseBillNumber; }
		}

		public override ZString EffectiveSubHouseBillIssuerSCAC
		{
			get { return billDetails.IssuerCodeOfSubHouseBillNumber; }
		}

		public override ZString SubHouseBill
		{
			get { return billDetails.SubHouseBillNumber; }
		}

		public override ZInt PkgQty
		{
			get { return billDetails.Quantity; }
		}

		public override ZString PkgType
		{
			get { return billDetails.Unit; }
		}
	}
}
