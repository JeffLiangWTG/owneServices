using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACSDrawbackAddInfoJobDeclarationValidation : CommonDrawbackAddInfoJobDeclarationValidation
	{
		public ACSDrawbackAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckUS_DRWPurpose()
		{
			base.CheckUS_DRWPurpose();
			if (Parent.US_DRWPurpose.IsEmpty)
			{
				Parent.US_DRWPurposeInfo.AddError(DrawbackPurposeIsMandatory);
			}
			else
			{
				var lookups = Parent.Lookups;
				if (lookups != null)
				{
					ListValidation.ErrorIfInvalidCode(Parent.US_DRWPurposeInfo, lookups.US_DRWPurposeCodeList);
				}
			}
		}
		internal const string DrawbackPurposeIsMandatory = "Please enter Drawback Purpose.";

		protected override void CheckUS_DRWTransferee()
		{
			base.CheckUS_DRWTransferee();
			if (Is7552)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWTransfereeInfo);
			}
		}

		protected override void CheckUS_DRWCertOfManufacture()
		{
			base.CheckUS_DRWCertOfManufacture();
			if (Is7552 && !Parent.US_DRWCertOfManufacture.IsEmpty && !Regex.IsMatch(Parent.US_DRWCertOfManufacture, "^[0-9]{6}$"))
			{
				Parent.US_DRWCertOfManufactureInfo.AddMessageError(CertOfManufactureFormatIsInvalid);
			}
		}
		internal const string CertOfManufactureFormatIsInvalid = "Certificate of Manufacturer should be 6 digits.";

		protected override void CheckUS_DRWRulingNo()
		{
			base.CheckUS_DRWRulingNo();
			if (Is7552 && !Parent.US_DRWRulingNo.IsEmpty && !Regex.IsMatch(Parent.US_DRWRulingNo, "^[0-9]{2}-[0-9]{5}-[0-9]{3}$") && Parent.US_DRWRulingNo != "pending")
			{
				Parent.US_DRWRulingNoInfo.AddMessageError(RulingNoFormatIsInvalid);
			}
		}
		internal const string RulingNoFormatIsInvalid = "RulingNo should 12 character field in the format of NN-NNNNN-NNN whereas N is a digit OR the word 'pending'";

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();
			if (Parent.US_EntryType == EntryTypeList.Codes.DirectIdentificationManufacturingDrawback ||
					Parent.US_EntryType == EntryTypeList.Codes.SubstitutionManufacturerDrawback)
			{
				ZString contractMumber = "";
				if (Parent.Declaration != null && Parent.Declaration.ContractNumbers.Count > 0)
				{
					contractMumber = Parent.Declaration.ContractNumbers[0].CY_Data;
				}

				if (contractMumber.IsEmpty)
				{
					Parent.US_EntryTypeInfo.AddMessageError(ContractNumberRequiredMessage);
				}
			}
		}
		internal const string ContractNumberRequiredMessage = "A Drawback Contract Number is required for this Claim Type.";

		protected override void CheckUS_EstimatedEntryDate()
		{
			base.CheckUS_EstimatedEntryDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EstimatedEntryDateInfo, "Estimated Claim Date");
		}

		protected override void CheckUS_NAFTADrawbackCountry()
		{
			base.CheckUS_NAFTADrawbackCountry();
			if (Parent.US_NAFTAClaimInd)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_NAFTADrawbackCountryInfo, Parent.Lookups.US_NAFTACountryCodeList);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.US_NAFTADrawbackCountryInfo, "NAFTA Drawback Country");
			}
		}

		protected override void CheckUS_NAFTAClaimInd()
		{
			base.CheckUS_NAFTAClaimInd();
			Parent.Validation.ValidateUS_NAFTADrawbackCountry();
		}

		protected override void CheckUS_PreparerDistrictPort()
		{
			base.CheckUS_PreparerDistrictPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_PreparerDistrictPortInfo, Parent.Lookups.RegionDistrictPorts);
		}

		protected override void CheckUS_DRWTotalPRDC()
		{
			base.CheckUS_DRWTotalPRDC();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.US_DRWTotalPRDCInfo);
		}

		protected override void CheckUS_DRWFilingMethod()
		{
			base.CheckUS_DRWFilingMethod();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_DRWFilingMethodInfo, Parent.Lookups.US_DRWFilingMethodCodeList);
			if (Declaration.US_DRWFilingMethod == DrawbackMethodOfFilingList.Codes.Manual && !Declaration.JE_MessageStatus.IsEmpty
				&& Declaration.JE_MessageStatus != DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryDelete)
			{
				Declaration.US_DRWFilingMethodInfo.AddError(CannotSetMethodOfFilingAsManual);
			}
		}
		internal const string CannotSetMethodOfFilingAsManual = "Please delete this job's message from ABI first before filing as manual.";

		ZBool Is7552
		{
			get { return Declaration != null && Declaration.Is7552; }
		}
	}
}
