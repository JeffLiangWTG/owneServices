using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class AgencyRequirementsValidator
	{
		#region Constants

		public static class RequirementConstants
		{
			public static class PGA
			{
				public const string PGARequiredButBlank = "{0} Indicator is required when tariff is an {0} tariff.";
				public const string PGAPilotApproved = "You have elected to send PGA data. Before sending PGA data, please make sure that you are approved by CBP in the PGA pilot program at this port of entry.";
				public const string PGALineRequired = "At least one {0} Line is required when {0} Indicator is 'Declared'.";
				public const string PGADisclaimedButEntered = "{0} Lines may not be entered if {0} Indicator is blank or 'Disclaimed'.";
				public const string PGADisclaimedReason = "You have not entered a disclaim reason.";
				public const string PGADisclaimedIsNotAllowedForExportMadatoryValue = "Disclaim NOT allowed for this tariff.";
				public const string PGADisclaimedIsNotAllowedForRequiredTariff = "Disclaim is generally NOT allowed if the HTS tariff is flagged as 'Must Be provided'.";
				public const string PGANotRequired = "The tariff does not indicate that {0} reporting is required.";
				public const string PGANotApplicableForCertificationMode = "PGA reporting is not allowed if cargo is certified in ACS.";
				public const string PGAIsNotEffective = "You have elected to send PGA data. Please check with the PGA regarding their process for accepting the PGA data and any follow up that you might have to do with them or CBP.";
				public const string PGALineRequiredWhenndicatorIsNotBlank = "At least one {0} Line is required when {0} Indicator is not blank.";
				public const string PGALineNotRequiredButEntered = "{0} Lines may not be entered if {0} Indicator is blank.";
				public const string PGANotRequiredIfNotApplicable = "{0} Indicator is not allowed for the selected entry type, {1}.";
				public const string PGASubmittedBy = "FSIS may be submitted electronically via ACE by indicating “D” here, otherwise FSIS data may be submitted via the FSIS 9540-1 document.";
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public static void ValidatePGAIndicatorAndData(ZPropertyInfo propertyInfo, string agencyCode, IEnumerable<IPGADataCorrection> pgas)
		{
			var value = (ZString)propertyInfo.Value;
			var hasValidPGA = pgas.Any(x => x.IncludedInMessage());
			if (OGAIndicatorList.IsToBeDeclared(value) && !hasValidPGA)
			{
				propertyInfo.AddMessageError(string.Format(RequirementConstants.PGA.PGALineRequired, agencyCode));
			}
			else if ((OGAIndicatorList.IsToBeDisclaimed(value) || value.IsEmpty) && hasValidPGA)
			{
				propertyInfo.AddMessageError(string.Format(RequirementConstants.PGA.PGADisclaimedButEntered, agencyCode));
			}
		}

		public static void ValidatePGAIndicatorAndDataForExport(ZPropertyInfo propertyInfo, ZString agencyCode, int pgaLinesCount, bool isRequired = false)
		{
			var value = (ZString)propertyInfo.Value;
			if (OGAIndicatorList.IsToBeDeclared(value))
			{
				if (pgaLinesCount == 0)
				{
					propertyInfo.AddMessageError(ZString.Format(RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, agencyCode));
				}
			}
			else if (isRequired && OGAIndicatorList.IsToBeDisclaimed(value))
			{
				propertyInfo.AddMessageError(RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			}
			else if (value.IsEmpty && pgaLinesCount > 0)
			{
				propertyInfo.AddMessageError(ZString.Format(RequirementConstants.PGA.PGALineNotRequiredButEntered, agencyCode));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public static void ValidatePGA(ZPropertyInfo propertyInfo, string agencyCode, bool isEffective, bool hasRequirement)
		{
			var value = (ZString)propertyInfo.Value;
			if (hasRequirement)
			{
				if (isEffective && value.IsEmpty)
				{
					propertyInfo.AddMessageError(string.Format(RequirementConstants.PGA.PGARequiredButBlank, agencyCode));
				}
			}
			else if (OGAIndicatorList.IsToBeDeclaredOrDisclaimed(value) && !(agencyCode.StartsWith("TSCA", StringComparison.OrdinalIgnoreCase) && OGAIndicatorList.IsToBeDisclaimed(value)))
			{
				propertyInfo.AddWarning(string.Format(RequirementConstants.PGA.PGANotRequired, agencyCode));
			}
		}

		public static void ValidatePGADisclaimedIndicator(ZPropertyInfo propertyInfo, bool isRequired)
		{
			var value = (ZString)propertyInfo.Value;

			ValidatePGADisclaimedIndicator(propertyInfo, value, isRequired);
		}

		public static void ValidatePGADisclaimedIndicator(ZPropertyInfo propertyInfo, ZString pgaIndicator, bool isRequired)
		{
			if (OGAIndicatorList.IsToBeDisclaimed(pgaIndicator) && isRequired)
			{
				propertyInfo.AddMessageError(RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
			}
		}

		public static void ValidateOGAAgencyRequirements(string agencyCode, OGAAgencyRequirementCollection requirementColletion)
		{
			var requirement = requirementColletion.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == agencyCode);
			if (requirement != null)
			{
				requirement.ValidateIndicator();
				requirement.RefreshBinding();
			}
		}

		public static void ValidateOGAAgencyDisclaimReason(string agencyCode, OGAAgencyRequirementCollection requirementColletion)
		{
			var agencyRequirement = requirementColletion.Cast<OGAAgencyRequirement>().FirstOrDefault(x => x.AgencyCode == agencyCode);
			if (agencyRequirement != null)
			{
				agencyRequirement.ValidateDisclaimReason();
			}
		}

		public static void ValidatePGADisclaimed(ZPropertyInfo propertyInfo, ZString indicator, CodeDescriptionPairList codeDescList)
		{
			var value = (ZString)propertyInfo.Value;

			ListValidation.MessageErrorIfInvalidCode(propertyInfo, codeDescList);
			if (value.IsEmpty && OGAIndicatorList.IsToBeDisclaimed(indicator))
			{
				propertyInfo.AddMessageError(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			}
		}

		public static void ValidatePGAIsDisallowed(ZString agencyCode, ZBool isEntrySummary, ZBool isCargoRelease, ZString entryType, ZBool isCertified, ZBool isExpeditedRelease, ZBool isWeeklyEstimateFiling, ZPropertyInfo propertyInfo)
		{
			var value = (ZString)propertyInfo.Value;
			if (!GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, agencyCode) && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(value))
			{
				propertyInfo.AddMessageError(ZString.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, agencyCode, entryType));
			}
		}
	}
}
