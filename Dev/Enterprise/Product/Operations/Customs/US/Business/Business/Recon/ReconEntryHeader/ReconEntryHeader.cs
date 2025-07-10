using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryHeader : NonPersistentBusinessObjectWithLogsAndNotes
	{
		public ReconEntryHeader(CusEntryHeader entry) : base(entry.Factory)
		{
			this.entry = Argument.NotNull(entry, "entry");
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}
		}
		readonly CusEntryHeader entry;

		public ZString EntryNumber
		{
			get { return entry.EntryNumber; }
		}

		public ZString CH_Status
		{
			get { return entry.CH_Status; }
			set { entry.CH_Status = value; }
		}

		public ZDateTime US_CollectionDate
		{
			get { return entry.US_CollectionDate; }
		}

		public ZDateTime US_AnticipatedLiquidationDate
		{
			get { return entry.US_ALDate; }
		}

		public ZDecimal US_AnticipatedLiquidatedDuty
		{
			get { return entry.US_ALDuty; }
		}

		public ZDecimal CH_TotalPaid
		{
			get { return entry.CH_TotalPaid; }
			set { entry.CH_TotalPaid = value; }
		}

		public CusEntryHeaderChargesCollection Charges
		{
			get { return entry.Charges; }
		}

		public ZDecimal DutyAmount
		{
			get { return entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZDecimal TaxPaymentAmount
		{
			get { return entry.Charges.GetTotalAmount(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public ZDecimal FeePaymentAmount
		{
			get { return entry.Charges.GetTotalAmount(EntryChargeTypeList.GetFeeCodes()); }
		}

		public ZDecimal InterestPaymentAmount
		{
			get { return entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public CusEntryHeader GetEntry()
		{
			return entry;
		}

		[ChildEditable(false)]
		public EDIMessageCollection Messages
		{
			get { return entry.Messages; }
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return entry; }
		}

		#region Business Object Overrides

		public override bool IsInDatabase
		{
			get { return entry.IsInDatabase; }
		}

		public override void Delete()
		{
			base.Delete();
			entry.Delete();
		}

		public override bool IsDeleted
		{
			get { return entry.IsDeleted; }
		}

		public override bool IsSavedByFactory
		{
			get { return true; }
		}

		public ReconEntryHeaderValidation Validation
		{
			get { return new ReconEntryHeaderValidation(this); }
		}

		protected override void AddToFactoryCache()
		{
			// should be called after entry is set
		}

		protected override ZGuid GetPK()
		{
			return entry.PK;
		}

		#endregion
	}
}
