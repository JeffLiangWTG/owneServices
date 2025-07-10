using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching.Testing
{
	public enum logMessageTypes
	{
		Unmatched,
		NoMatchByTaxRegistrationCodeType,
		NoTaxRegistrationNumberFound,
		MatchedAddressOnRegistrationDetails,
		MatchedOrgOnRegistrationDetails,
		MultipleEligibleOrganisationsToMatch,
		AssignedToUnmatchedOrg,
		AssignedToUnmatchedOrgWithCode
	}

	public static class OrganisationMatcherLoggingTestHelper
	{
		public static string GetLogForInactiveOrg(OrgHeader org)
		{
			return $"Warning - Unable to use Organization (Code: {org.OH_Code}) as it is marked as inactive.";
		}

		public static string GetLogWhenMultipleOrgsMatchRegistrationDetails(OrgCusCode code)
		{
			return $"Warning - Unable to determine a match as multiple organizations were matched with registration detail (Country/Region='{code.OK_RN_NKCodeCountry}', Type='{code.OK_CodeType}', Number='{code.OK_CustomsRegNo}').";
		}

		public static string GetLogForMatchedOrgOnRegistrationDetails(OrgHeader org, OrgCusCode code)
		{
			return $"Information - Matched to '{org.OH_Code}' by registration detail (Country/Region='{code.OK_RN_NKCodeCountry}', Type='{code.OK_CodeType}', Number='{code.OK_CustomsRegNo}').";
		}

		public static string GetLogForMatchedAddressOnRegistrationDetails(OrgAddress address, OrgCusCode code)
		{
			return $"Information - Matched to address '{address.OA_Address1}' on '{address.Header.OH_Code}' by registration detail (Country/Region='{code.OK_RN_NKCodeCountry}', Type='{code.OK_CodeType}', Number='{code.OK_CustomsRegNo}').";
		}

		public static string GetLogWhenNoMatchByTaxRegistrationCodeType(IOrgHeaderForMatching orgMatchingData, IOrgCusCodeForMatching matchingOrgCusCode)
		{
			return $"Warning - No organization could be matched with tax registration number (Country='{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}', Organization Name='{orgMatchingData.OH_FullName}', Type='{matchingOrgCusCode.OK_CodeType}', Number='{matchingOrgCusCode.OK_CustomsRegNo}').";
		}

		public static string GetLogWhenNoTaxRegistrationNumberFound(IOrgHeaderForMatching orgMatchingData, IOrgCusCodeForMatching matchingOrgCusCode)
		{
			return $"Warning - No tax registration number found for organization matching (Country='{GlbCompany.CurrentCompany.GC_RN_NKCountryCode}', Organization Name='{orgMatchingData.OH_FullName}', Type='{matchingOrgCusCode.OK_CodeType}').";
		}

		public static string GetLogWhenMultipleEligibleOrganisationsToMatch(OrgCusCode cusCode)
		{
			return $"Warning - Multiple records were found with the {cusCode.OK_CodeType} registration number {cusCode.OK_CustomsRegNo}. Organization could not been matched.";
		}

		public static string GetLogForAssignedToUnmatched(IOrgHeaderForMatching orgMatchingData)
		{
			return $"Warning - No match found for '[]'.";
		}

		public static string GetLogForAssignedToUnmatchedOrg(IOrgHeaderForMatching orgMatchingData)
		{
			return $"Warning - No match found for '[Company Name: {orgMatchingData.OH_FullName}]'.";
		}

		public static string GetLogForAssignedToUnmatchedOrgWithCode(IOrgHeaderForMatching orgMatchingData)
		{
			return $"Warning - No match found for '[Org. Code: {orgMatchingData.OH_Code}; Company Name: {orgMatchingData.OH_FullName}]'.";
		}

		public static string GetLogForAssignedToUnmatchedOrg()
		{
			return "No match found - Assigned to UNMATCHED organization";
		}

		public static string GetExpectedLogMessage(logMessageTypes messageCode, OrgHeaderForMatching orgMatchingData, OrgCusCode orgCusCode, OrgAddress matchedAddress, OrgHeader matchedOrganization, IOrgCusCodeForMatching matchingOrgCusCode = null)
		{
			switch (messageCode)
			{
				case logMessageTypes.Unmatched:
					return OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatched(orgMatchingData);
				case logMessageTypes.NoMatchByTaxRegistrationCodeType:
					return OrganisationMatcherLoggingTestHelper.GetLogWhenNoMatchByTaxRegistrationCodeType(orgMatchingData, matchingOrgCusCode);
				case logMessageTypes.NoTaxRegistrationNumberFound:
					return OrganisationMatcherLoggingTestHelper.GetLogWhenNoTaxRegistrationNumberFound(orgMatchingData, matchingOrgCusCode);
				case logMessageTypes.MatchedAddressOnRegistrationDetails:
					return OrganisationMatcherLoggingTestHelper.GetLogForMatchedAddressOnRegistrationDetails(matchedAddress, orgCusCode);
				case logMessageTypes.MatchedOrgOnRegistrationDetails:
					return OrganisationMatcherLoggingTestHelper.GetLogForMatchedOrgOnRegistrationDetails(matchedOrganization, orgCusCode);
				case logMessageTypes.MultipleEligibleOrganisationsToMatch:
					return OrganisationMatcherLoggingTestHelper.GetLogWhenMultipleEligibleOrganisationsToMatch(orgCusCode);
				case logMessageTypes.AssignedToUnmatchedOrg:
					return OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrg(orgMatchingData);
				case logMessageTypes.AssignedToUnmatchedOrgWithCode:
					return OrganisationMatcherLoggingTestHelper.GetLogForAssignedToUnmatchedOrgWithCode(orgMatchingData);
				default:
					return string.Empty;
			}
		}
	}
}
