using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryOriginalChargeValidation : CusCodeDataValidation
	{
		public ReconEntryOriginalChargeValidation(ReconEntryOriginalCharge charge)
			: base(charge)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCY_Amount();
		}

		new ReconEntryOriginalCharge Parent
		{
			get { return (ReconEntryOriginalCharge)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			if (ReconHeader != null && ReconHeader.OriginalCharges.HasDuplicate(Parent.CY_Code))
			{
				Parent.CY_CodeInfo.AddMessageError(CodeCannotBeDuplicated);
			}
		}
		internal const string CodeCannotBeDuplicated = "No Charge can be duplicated.";

		public void ValidateCY_Amount()
		{
			ValidateCalculatedProperty(Parent.CY_AmountInfo);
		}

		protected void CheckCY_Amount()
		{
			var originalFeeMatchesSumOfLines = true;

			if (ReconHeader != null && !Parent.CY_Code.IsEmpty && !ReconHeader.US_R_NoLineDetails && !ReconHeader.US_R_ChangedLinesOnly)
			{
				if (Parent.CY_Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
					Parent.CY_Code != Core.Constants.USCustoms.FeeCodes.MPC)
				{
					originalFeeMatchesSumOfLines = Parent.CY_Amount == ReconHeader.GetTotalOriginalCustomsFeesFromFeeCode(Parent.CY_Code);
				}
			}

			if (!originalFeeMatchesSumOfLines)
			{
				Parent.CY_AmountInfo.AddMessageError(TotalFeeDoesNotMatch);
			}

			if (ReconHeader != null && ReconHeader.US_R_ChangedLinesOnly &&
				Parent.CY_Code == Core.Constants.USCustoms.FeeCodes.MPC &&
				ReconHeader.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0 &&
				Parent.CY_Amount <= 0)
			{
				Parent.CY_AmountInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}
		internal const string TotalFeeDoesNotMatch = "The sum of all lines does not match the total for this fee.";
		internal const string EnterNumberGreaterThanZero = "Please enter an MPF as calculated and unadjusted for MPF calculation. This is required as this entry has only changed lines entered.";

		#region CY_SelectedRateType

		public void ValidateCY_SelectedRateType()
		{
			ValidateCalculatedProperty(Parent.CY_SelectedRateTypeInfo);
		}

		protected void CheckCY_SelectedRateType()
		{
			if (!Parent.CY_SelectedRateType_ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_SelectedRateTypeInfo, Parent.Lookups.CY_SelectedRateTypeList, "rate type");
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.CY_SelectedRateTypeInfo);
			}
		}

		#endregion

		ReconOriginalEntryHeader ReconHeader
		{
			get
			{
				var entryHeader = Parent?.Parent as CusEntryHeader;
				return entryHeader?.ReconOriginalEntry;
			}
		}
	}
}
