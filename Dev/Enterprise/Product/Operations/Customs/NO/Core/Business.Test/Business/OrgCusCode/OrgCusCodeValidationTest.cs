using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(OrgCusCodeValidation))]
sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOK_RegistrationNumberNotEntered()
	{
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "";
			Assert("OK_CustomsRegNo is mandatory", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "a12dwer";
			Assert("No Error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());
		});
	}

	public void TestCheckOK_EMDRegistrationNumber()
	{
		cusCode.OK_CodeType = OrgCusCode.NorwayCodeTypes.EMD;
		const string message = "The length of Registration Number / Code for Type 'EMD' cannot exceed 10 characters.";
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "12345678910";
			AssertHasMessageError("When length of Registration Number / Code > 10", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageError("When length of Registration Number / Code <= 10", cusCode.OK_CustomsRegNoInfo, message);
		});
	}

	public void TestCheckOK_GovBusinessCodeRegistrationNumber()
	{
		cusCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
		const string message = "Registration Number / Code: Norway Government Business Code should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.";
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasError("When Registration Number Length < 9", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "123456ab";
			AssertHasError("When Registration Number conatins non-numeric characters", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasErrorContaining("When Registration Number is missing the checksum digit.", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "123456785";
			AssertNoErrors("Valid Registration Number", cusCode.OK_CustomsRegNoInfo);
		});
	}

	public void TestCheckOK_MVARegistrationNumber()
	{
		cusCode.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
		const string message = "Registration Number / Code: Norway MVA (VAT Tax ID) should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.";
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasError("When Registration Number Length < 9", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "123456ab";
			AssertHasError("When Registration Number conatins non-numeric characters", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasErrorContaining("When Registration Number is missing the checksum digit.", cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "123456785";
			AssertNoErrors("Valid Registration Number", cusCode.OK_CustomsRegNoInfo);
		});
	}

	public void TestCheckOK_SSNRegistrationNumber()
	{
		cusCode.OK_CodeType = NorwayOrgCusCodeInfo.OrgCusCodes.SSN;
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "12345678901";
			AssertHasError("When Invalid Registration Number", cusCode.OK_CustomsRegNoInfo, "Not a valid Norwegian Social Security Number.");
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasError("When Registration Number Length < 11", cusCode.OK_CustomsRegNoInfo, "Wrong length. Norwegian Social Security Numbers should be 11 digits.");
			cusCode.OK_CustomsRegNo = "123456789012";
			AssertHasError("When Registration Number Length > 11", cusCode.OK_CustomsRegNoInfo, "Wrong length. Norwegian Social Security Numbers should be 11 digits.");
			cusCode.OK_CustomsRegNo = "ABC12345678";
			AssertHasError("When Registration Number conatins non-numeric characters", cusCode.OK_CustomsRegNoInfo, "Norwegian Social Security Numbers can only contain digits.");
			cusCode.OK_CustomsRegNo = "08052621187";
			AssertNoErrors("Valid Registration Number", cusCode.OK_CustomsRegNoInfo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		org = Factory.NewWithValidTestData<OrgHeader>();
		cusCode = org.CustomsCodes.AddNew();
	}

	OrgCusCode cusCode;
	OrgHeader org;
}
