using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEAffirmationCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public ACEAffirmationCodeValidation(ACEAffirmationCode parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			var code = (ACEAffirmationCode)Parent;
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo, code.AOCList);

			var fda = Parent.Factory.Load<ACEFDA>(Parent.CY_ParentID);
			if (fda != null && fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice)
			{
				if (Parent.CY_Code.IsEmpty)
				{
					Parent.CY_CodeInfo.AddMessageError(CodeRequired);
				}

				if (Parent.CY_Code == ACE_AffirmationOfComplianceList.Codes.RB1 &&
					!fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.ACC ||
								x.CY_Code == ACE_AffirmationOfComplianceList.Codes.ANC))
				{
					Parent.CY_CodeInfo.AddMessageError(ZString.Format(AdditionalCodeRequired, "ACC", " or 'ANC'", Parent.CY_Code));
				}
				else if ((Parent.CY_Code == ACE_AffirmationOfComplianceList.Codes.ACC || Parent.CY_Code == ACE_AffirmationOfComplianceList.Codes.ANC)
					&& !fda.AffirmationCodes.OfType<ACEAffirmationCode>().Any(x => x.CY_Code == ACE_AffirmationOfComplianceList.Codes.RB1))
				{
					Parent.CY_CodeInfo.AddMessageError(ZString.Format(AdditionalCodeRequired, "RB1", ZString.Empty, Parent.CY_Code));
				}

				if (IsExcludedAffirmationCode(Parent.CY_Code))
				{
					Parent.CY_CodeInfo.AddMessageError(AffirmationCodeExcluded);
				}
			}
		}
		internal const string CodeRequired = "Affirmation Code is mandatory.";
		internal const string AdditionalCodeRequired = "Affirmation of Compliance '{0}'{1} is required if Affirmation of Compliance '{2}' is present.";
		internal const string AffirmationCodeExcluded = "This affirmation code should not be entered here, it is determined from other data in the declaration.";

		bool IsExcludedAffirmationCode(string code)
		{
			var strings = new List<ZString>
			{
				ACE_AffirmationOfComplianceList.Codes.VES,
				ACE_AffirmationOfComplianceList.Codes.VFT,
				ACE_AffirmationOfComplianceList.Codes.PFR,
				ACE_AffirmationOfComplianceList.Codes.CFR,
				ACE_AffirmationOfComplianceList.Codes.GFR,
				ACE_AffirmationOfComplianceList.Codes.FME
			};
			return strings.Contains(code);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var fda = Parent.Factory.Load<ACEFDA>(Parent.CY_ParentID);
			var programCode = fda?.US_ProgramCode ?? ZString.Empty;
			var codeType = ACEAffirmationCode.AoCProgramCodePrefix + programCode;
			var maskAttributeName = Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask;
			var errorTextAttributeName = Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText;

			var (mask, errorText) = UniversalReferenceDataHelper.GetRefCusCodeListAttribute(Parent.Factory, Parent.CY_Code, codeType, maskAttributeName, errorTextAttributeName);

			if (!mask.IsEmpty && !errorText.IsEmpty && !Regex.IsMatch(Parent.CY_Data, $@"^{mask}$"))
			{
				if (mask == "(?![\\s\\S])")
				{
					Parent.CY_DataInfo.AddMessageError(ZString.Format(AoCDataMessageErrorPrefix, Parent.CY_Code, errorText));
				}
				else
				{
					Parent.CY_DataInfo.AddMessageError(ZString.Format(AoCDataMessageErrorPrefix, Parent.CY_Code, ZString.Format(AoCDataFormatIsIncorrect, errorText)));
				}
			}
			else if (!mask.IsEmpty && errorText.IsEmpty && !Regex.IsMatch(Parent.CY_Data, $@"^{mask}$"))
			{
				Parent.CY_DataInfo.AddMessageError(ZString.Format(AoCDataMessageErrorPrefix, Parent.CY_Code, AoCDataNoErrorTextInRefDb));
			}
		}
		internal const string AoCDataMessageErrorPrefix = "For Affirmation of Compliance Code '{0}', {1}";
		internal const string AoCDataFormatIsIncorrect = "the following is acceptable: '{0}'.";
		internal const string AoCDataNoErrorTextInRefDb = "the data is empty or invalid. Please refer to the FDA Implementation Guide in the CATAIR for format details.";
	}
}
