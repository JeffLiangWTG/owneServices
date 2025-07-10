using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public class FormalImportAddInfoBillValidation : CommonImportAddInfoBillValidation
	{
		public FormalImportAddInfoBillValidation(AddInfoBill addInfoHouseBill)
			: base(addInfoHouseBill)
		{
		}

		protected override void CheckUS_SESplitShip()
		{
			base.CheckUS_SESplitShip();
			if (Parent.US_SESplitShip && !Bill.IsLowestBill)
			{
				Parent.US_SESplitShipInfo.AddMessageError(SplitForParentBill);
			}

			var declaration = Bill.Declaration;
			if (Parent.US_SESplitShip && declaration != null && declaration.US_NonAMS)
			{
				Parent.US_SESplitShipInfo.AddMessageError(NonAMSBillCannotBeSplit);
			}
		}
		internal const string SplitForParentBill = "The Split shipment flag should be indicated on lowest bills.";
		internal const string NonAMSBillCannotBeSplit = "Bills cannot be split when job is flagged as Non-AMS.";
		internal const string NonTransportNeverBeNonAMS = "The non-transport job cannot be flagged as Non-AMS (Entry type is 06 or 22).";

		protected override bool ShouldValidateUS_UI_NKBillIssuerSCAC
		{
			get { return IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		protected override void CheckIssuerSCACNotAllowed()
		{
			base.CheckIssuerSCACNotAllowed();
			var declaration = Parent.Declaration;
			if (declaration != null && declaration.IsACECargoReleaseValidationMode && TransportTypeList.IsIssuerSCACNotAllowed(declaration.JE_TransportMode) && !Parent.US_UI_NKBillIssuerSCAC.IsEmpty)
			{
				Parent.US_UI_NKBillIssuerSCACInfo.AddMessageError(IssuerSCACiSNotPermitted);
			}
		}

		protected override void CheckUS_ExpressTracking()
		{
			base.CheckUS_ExpressTracking();

			var declaration = Parent.Declaration;
			if (Parent.US_ExpressTracking && declaration != null && declaration.IsExpressTrackingNumberRelevant)
			{
				if (declaration.US_EnableENS)
				{
					Parent.US_ExpressTrackingInfo.AddMessageError(ExpressTrackingNotAllowedForENS);
				}

				var hasOtherTrackingNumber = declaration.Bills.OfType<Bill>().Any(bill => bill.PK != Bill.PK && bill.US_ExpressTracking);
				if (hasOtherTrackingNumber)
				{
					Parent.US_ExpressTrackingInfo.AddMessageError(OnlyOneExpressTrackingNumberAllowed);
				}
			}
		}
		internal const string ExpressTrackingNotAllowedForENS = "Express Tracking Number is not allowed for entry summary.";
		internal const string OnlyOneExpressTrackingNumberAllowed = "Only one master bill can be marked as express carrier tracking number.";

		#region Boolean Flags

		bool IsEntrySummaryValidationMode
		{
			get { return Parent.Declaration != null && Parent.Parent.Declaration.IsEntrySummaryValidationMode; }
		}

		bool IsCargoReleaseValidationMode
		{
			get { return Parent.Declaration != null && Parent.Parent.Declaration.IsCargoReleaseValidationMode; }
		}

		internal bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return IsEntrySummaryValidationMode || IsCargoReleaseValidationMode; }
		}

		#endregion
	}
}
