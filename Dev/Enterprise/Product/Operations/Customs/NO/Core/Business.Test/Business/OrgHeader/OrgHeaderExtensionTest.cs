using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(OrgHeaderExtension))]
sealed class OrgHeaderExtensionTest : TestCaseWithFactory
{
	public void TestGetOrganizationNumberOrEmpty()
		=> AssertSingleCusRegNumber(UniversalReferenceConstants.OrgCodeType.OrganizationNumber, h => h.GetOrganizationNumberOrEmpty());

	public void TestGetMVACodeOrEmpty()
		=> AssertSingleCusRegNumber(OrgCusCode.NorwayCodeTypes.MVA, h => h.GetMVACodeOrEmpty());

	public void GetSocialSecurityNumberOrEmpty()
		=> AssertSingleCusRegNumber(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, h => h.GetOrganizationNumberOrEmpty());

	public void TestGetDANCustomsRegNumberOmitLastTwo() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals("When DefermentApprovalNumber is not present", ZString.Empty, orgHeader.GetDANCustomsRegNumberOmitLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "12345678");
		AssertEquals("When DefermentApprovalNumber is of 8 digits", "123456", orgHeader.GetDANCustomsRegNumberOmitLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "1234567");
		AssertEquals("When DefermentApprovalNumber is of 7 digits", "12345", orgHeader.GetDANCustomsRegNumberOmitLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "12367");
		AssertEquals("When DefermentApprovalNumber less than 7", "12367", orgHeader.GetDANCustomsRegNumberOmitLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "123678989");
		AssertEquals("When DefermentApprovalNumber less more than 8", "123678989", orgHeader.GetDANCustomsRegNumberOmitLastTwo());

		AssertEquals("When OrgHeader is null", ZString.Empty, ((OrgHeader)null).GetDANCustomsRegNumberOmitLastTwo());
	});

	public void TestGetDANCustomsRegNumberTakeLastTwo() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals("When DefermentApprovalNumber is not present", ZString.Empty, orgHeader.GetDANCustomsRegNumberTakeLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "12345678");
		AssertEquals("When DefermentApprovalNumber is of 8 digits", "78", orgHeader.GetDANCustomsRegNumberTakeLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "1234567");
		AssertEquals("When DefermentApprovalNumber is of 7 digits", "67", orgHeader.GetDANCustomsRegNumberTakeLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "12367");
		AssertEquals("When DefermentApprovalNumber less than 7", ZString.Empty, orgHeader.GetDANCustomsRegNumberTakeLastTwo());

		AddNewDefermentApprovalNumber(orgHeader, "123678989");
		AssertEquals("When DefermentApprovalNumber less more than 8", ZString.Empty, orgHeader.GetDANCustomsRegNumberTakeLastTwo());

		AssertEquals("When OrgHeader is null", ZString.Empty, ((OrgHeader)null).GetDANCustomsRegNumberTakeLastTwo());
	});

	public void TestGetCusRegNoEMD()
		=> AssertSingleCusRegNumber(UniversalReferenceConstants.OrgCodeType.EmmaSystemIdentifier, h => h.GetCusRegNoEMD());

	public void TestGetCustomsRegNoOfTypeMVAOrOrgOrSSN() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals("When no CusCodes are present", ZString.Empty, orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.MVARegistrationNumber, "V12345");
		AssertEquals("When MVA is present", "V12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, "S12345");
		AssertEquals("When SSN is present", "S12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.OrganizationNumber, "O12345");
		AssertEquals("When ORG is present", "O12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode("EMD", "12345678");
		AssertEquals("When none from ORG/SSN/MVA is present", ZString.Empty, orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.OrganizationNumber, "O12345");
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, "S12345");
		AssertEquals("When SSN and ORG are present, should return ORG", "O12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.MVARegistrationNumber, "M12345");
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, "S12345");
		AssertEquals("When MVA and SSN are present, should return MVA", "M12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());

		RemoveAndDeleteAllCusCodes(orgHeader);
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.MVARegistrationNumber, "M12345");
		orgHeader.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.OrganizationNumber, "O12345");
		AssertEquals("When MVA and ORG are present, should return MVA", "M12345", orgHeader.GetCustomsRegNoOfTypeMVAOrOrgOrSSN());
	});

	public void TestIsPrivatePerson() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals("When OH_Category is empty", false, orgHeader.IsPrivatePerson());

		orgHeader.OH_Category = UniversalReferenceConstants.OrgHeaderType.NaturalPerson;
		AssertEquals("When OH_Category is NAT", true, orgHeader.IsPrivatePerson());

		orgHeader.OH_Category = "OTH";
		AssertEquals("When OH_Category is OTH", false, orgHeader.IsPrivatePerson());
	});

	void AssertSingleCusRegNumber(string type, Func<OrgHeader, ZString> invokeMethodUnderTest) => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals($"When {type} is not present", ZString.Empty, invokeMethodUnderTest(orgHeader));

		orgHeader.WithOrgCusCode(type, "12345678");
		AssertEquals($"When {type} is present", "12345678", invokeMethodUnderTest(orgHeader));

		AssertEquals("When OrgHeader is null", ZString.Empty, invokeMethodUnderTest(null));
	});

	static void AddNewDefermentApprovalNumber(OrgHeader header, string number)
	{
		RemoveAndDeleteAllCusCodes(header);
		header.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.DefermentApprovalNumber, number);
	}

	static void RemoveAndDeleteAllCusCodes(OrgHeader header) => header.CustomsCodes.RemoveAndDeleteAll();
}
