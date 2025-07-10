using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public static class OrgHeaderExtension
{
	public static ZString GetDefermentApprovalNumberCodeOrEmpty(this OrgHeader organization)
		=> GetDANCustomsRegistrationNumber(organization,
			getValueWithLengthIsSeven: v => v,
			getValueWhenLengthIsEight: v => v,
			getValueWhenLengthIsMoreOrLessThanExpected: v => v);

	public static ZString GetMVACodeOrEmpty(this OrgHeader organization)
	{
		return organization == null
			? ZString.Empty
			: organization.CustomsCodes.GetCustomsRegNo(OrgCusCode.NorwayCodeTypes.MVA, Core.Constants.CountryCodes.Norway);
	}

	public static ZString GetOrganizationNumberOrEmpty(this OrgHeader organization)
	{
		return organization == null
			? ZString.Empty
			: organization.CustomsCodes.GetCustomsRegNo(UniversalReferenceConstants.OrgCodeType.OrganizationNumber, Core.Constants.CountryCodes.Norway);
	}

	public static ZString GetSocialSecurityNumberOrEmpty(this OrgHeader naturalPerson)
	{
		return naturalPerson == null
			? ZString.Empty
			: naturalPerson.CustomsCodes.GetCustomsRegNo(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, Core.Constants.CountryCodes.Norway);
	}

	public static ZString GetDANCustomsRegNumberOmitLastTwo(this OrgHeader orgHeader) => GetDANCustomsRegistrationNumber(orgHeader,
		getValueWithLengthIsSeven: v => v.SubstringSafe(0, 5),
		getValueWhenLengthIsEight: v => v.SubstringSafe(0, 6),
		getValueWhenLengthIsMoreOrLessThanExpected: v => v);

	public static ZString GetDANCustomsRegNumberTakeLastTwo(this OrgHeader orgHeader)
		=> GetDANCustomsRegistrationNumber(orgHeader,
			getValueWithLengthIsSeven: v => v.SubstringSafe(5, 2),
			getValueWhenLengthIsEight: v => v.SubstringSafe(6, 2),
			getValueWhenLengthIsMoreOrLessThanExpected: _ => ZString.Empty);

	public static string GetCusRegNoEMD(this OrgHeader orgHeader)
	{
		return orgHeader == null
			? ZString.Empty
			: orgHeader.CustomsCodes.GetCustomsRegNo(UniversalReferenceConstants.OrgCodeType.EmmaSystemIdentifier, Core.Constants.CountryCodes.Norway);
	}

	public static ZString GetCustomsRegNoOfTypeMVAOrOrgOrSSN(this OrgHeader orgHeader)
	{
		if (orgHeader is null)
		{
			return ZString.Empty;
		}

		return orgHeader.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Norway,
		[
					OrgCusCode.NorwayCodeTypes.MVA,
					UniversalReferenceConstants.OrgCodeType.OrganizationNumber,
					UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber
		]);
	}

	public static bool IsPrivatePerson(this OrgHeader orgHeader) => (orgHeader?.OH_Category ?? ZString.Empty) == OrgConstants.Category.NaturalPersonIndividual;

	static ZString GetDANCustomsRegistrationNumber(OrgHeader orgHeader,
		Func<ZString, ZString> getValueWithLengthIsSeven,
		Func<ZString, ZString> getValueWhenLengthIsEight,
		Func<ZString, ZString> getValueWhenLengthIsMoreOrLessThanExpected)
	{
		if (orgHeader is null)
		{
			return ZString.Empty;
		}

		var approvalNumber = orgHeader.CustomsCodes.GetCustomsRegNo(UniversalReferenceConstants.OrgCodeType.DefermentApprovalNumber, Core.Constants.CountryCodes.Norway);
		var danValueLength = approvalNumber.Length;
		return danValueLength switch
		{
			_ when danValueLength == 7 => getValueWithLengthIsSeven(approvalNumber),
			_ when danValueLength == 8 => getValueWhenLengthIsEight(approvalNumber),
			_ => getValueWhenLengthIsMoreOrLessThanExpected(approvalNumber)
		};
	}
}
