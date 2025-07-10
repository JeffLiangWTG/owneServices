using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryHeaderENS7501Bill : EntrySummary7501Bill, IObsoleteValidation
	{
		public EntryHeaderENS7501Bill(ZGuid entryPK, IBillDetails billDetails)
			: base(entryPK, billDetails.Factory)
		{
			this.billDetails = billDetails;
		}

		readonly IBillDetails billDetails;

		#region EntrySummary7501Bills methods

		public override ZDateTime ITDate
		{
			get { return billDetails.ITDate; }
		}

		public override ZString ITNO
		{
			get { return billDetails.ITNumber; }
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
			get { return billDetails.PackageQuantity; }
		}

		public override ZString PkgType
		{
			get { return billDetails.PackageType; }
		}

		#endregion
	}
}
