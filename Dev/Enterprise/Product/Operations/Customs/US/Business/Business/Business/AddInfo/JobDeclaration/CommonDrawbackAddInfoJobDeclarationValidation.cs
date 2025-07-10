using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CommonDrawbackAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public CommonDrawbackAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EntryTypeInfo);
			if (Parent.Lookups.US_EntryTypeList.ContainsCode(Parent.US_EntryType))
			{
				var declaration = Parent.Declaration;
				if (declaration.IsACEDrawback && !declaration.IsDrawbackTFTEA)
				{
					Parent.US_EntryTypeInfo.AddMessageError(OlnyTFTEADrawbackProvisionsAreAllowed);
				}
			}
		}
		const string OlnyTFTEADrawbackProvisionsAreAllowed = "Only TFTEA Drawback Provisions are allowed.";

		protected override void CheckUS_ClaimPort()
		{
			base.CheckUS_ClaimPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ClaimPortInfo, Parent.Lookups.US_ClaimPortCodeList);
			if (ClaimPortAndTeamNoIsNotValid)
			{
				Parent.US_ClaimPortInfo.AddMessageError(InconsistentClaimPortAndTeamNoMessage);
			}
			Parent.Validation.ValidateUS_TeamNo();
		}

		protected override void CheckUS_TeamNo()
		{
			base.CheckUS_TeamNo();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TeamNoInfo, Parent.Lookups.US_TeamNoForDrawbackCodeList);
			if (ClaimPortAndTeamNoIsNotValid)
			{
				Parent.US_TeamNoInfo.AddMessageError(InconsistentClaimPortAndTeamNoMessage);
			}
			Parent.Validation.ValidateUS_ClaimPort();
		}

		bool ClaimPortAndTeamNoIsNotValid
		{
			get
			{
				bool result = false;
				if (!Parent.US_ClaimPort.IsEmpty && !Parent.US_TeamNo.IsEmpty)
				{
					ZString teamNo = "";
					result = !Parent.Lookups.ValidClaimPortTeamNos.TryGetValue(Parent.US_ClaimPort, out teamNo) || teamNo != Parent.US_TeamNo;
				}
				return result;
			}
		}
		internal const string InconsistentClaimPortAndTeamNoMessage = "The entered Team Number is not consistent with the entered Claim Port.";

		protected override void CheckUS_BondType()
		{
			base.CheckUS_BondType();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondTypeInfo, Parent.Lookups.US_BondTypeList);

			if (BondTypeRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_BondTypeInfo);
			}

			if (Parent.US_BondType == BondTypeList.Codes.NoBondRequired && IsBondTypeRequired)
			{
				Parent.US_BondTypeInfo.AddMessageError(BondTypeRequiredMessageError);
			}

			ValidateUS_SuretyCode();
			ValidateUS_BondWaiverCode();
		}

		protected virtual ZBool BondTypeRequired
		{
			get { return true; }
		}

		protected virtual ZBool IsBondTypeRequired
		{
			get { return Parent.US_AcceleratedClaimInd || Parent.US_ExporterSummaryInd; }
		}

		protected virtual ZString BondTypeRequiredMessageError
		{
			get { return BondTypeRequiredMessage; }
		}
		internal const string BondTypeRequiredMessage = "A Bond Type is required if Accelerated Drawback or Exporter Summary Procedure is claimed.";

		protected override void CheckUS_AcceleratedClaimInd()
		{
			base.CheckUS_AcceleratedClaimInd();
			Parent.Validation.ValidateUS_BondType();
		}

		protected override void CheckUS_ExporterSummaryInd()
		{
			base.CheckUS_ExporterSummaryInd();
			Parent.Validation.ValidateUS_BondType();
		}

		protected override void CheckUS_SuretyCode()
		{
			base.CheckUS_SuretyCode();
			if (Parent.US_BondType == BondTypeList.Codes.NoBondRequired)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.US_SuretyCodeInfo, "Surety Code");
			}
			else
			{
				if (!Parent.US_SuretyCode.IsEmpty)
				{
					ZString messageError = SuretyCodeValidator.Validate(Parent.US_SuretyCode);
					if (!messageError.IsEmpty)
					{
						Parent.US_SuretyCodeInfo.AddMessageError(messageError);
					}
				}
			}
		}

		protected override void CheckUS_BondType2()
		{
			base.CheckUS_BondType2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BondType2Info, Parent.Lookups.AdditionBondTypesList);
		}

		protected override void CheckUS_EstimatedEntryDate()
		{
			base.CheckUS_EstimatedEntryDate();
			if (!Parent.US_EstimatedEntryDate.IsEmpty && Parent.US_EstimatedEntryDate.IsValid && Parent.US_EstimatedEntryDate.AddDays(30) < ZDateTime.Now)
			{
				Parent.US_EstimatedEntryDateInfo.AddMessageError(EstimatedClaimDateMessage);
			}
			Parent.Validation.ValidateUS_EarliestExportDate();
		}
		internal const string EstimatedClaimDateMessage = "The Estimated Claim Date must be within 30 days of the ABI transmission.";

		protected override void CheckUS_EarliestExportDate()
		{
			base.CheckUS_EarliestExportDate();
			if (!Parent.US_EarliestExportDate.IsEmpty && !Parent.US_EstimatedEntryDate.IsEmpty && Parent.US_EarliestExportDate.IsValid && Parent.US_EstimatedEntryDate.IsValid && Parent.US_EarliestExportDate.AddYears(5) < Parent.US_EstimatedEntryDate)
			{
				Parent.US_EarliestExportDateInfo.AddMessageError(EarliestExportDateMessage);
			}
		}
		internal const string EarliestExportDateMessage = "The Earliest Export Date (if entered) must be within 5 years of the Estimated Claim Date.";

		void DatesBothEnteredAndConsistent(ZPropertyInfo datePeriodInfo)
		{
			if (!Parent.US_DRWDatePeriodFrom.IsEmpty || !Parent.US_DRWDatePeriodTo.IsEmpty)
			{
				if (Parent.US_DRWDatePeriodFrom.IsEmpty || Parent.US_DRWDatePeriodTo.IsEmpty || Parent.US_DRWDatePeriodTo < Parent.US_DRWDatePeriodFrom)
				{
					datePeriodInfo.AddMessageError(DrawbackPeriodDatesMessage);
				}
			}
		}
		internal const string DrawbackPeriodDatesMessage = "Both dates must be enter, and the 'To' date may not be before the 'From' date.";

		protected override void CheckUS_DRWDatePeriodFrom()
		{
			base.CheckUS_DRWDatePeriodFrom();
			DatesBothEnteredAndConsistent(Parent.US_DRWDatePeriodFromInfo);
			Parent.Validation.ValidateUS_DRWDatePeriodTo();
		}

		protected override void CheckUS_DRWDatePeriodTo()
		{
			base.CheckUS_DRWDatePeriodTo();
			DatesBothEnteredAndConsistent(Parent.US_DRWDatePeriodToInfo);
			Parent.Validation.ValidateUS_DRWDatePeriodFrom();
		}
	}
}
