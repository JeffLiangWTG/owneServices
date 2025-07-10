using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ReferenceNumberWrapper))]
sealed class ReferenceNumberWrapperTest : TestCaseWithFactory
{
	public ReferenceNumberWrapper referenceNumberWrapper = new ReferenceNumberWrapper("1234567892023100511111101");

	public void TestIsValid()
	{
		CombineAssertions(() =>
		{
			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			AssertEquals("Splitting on a valid reference number", expected: true, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("123456789202310051111111");
			AssertEquals("Splitting on a too short reference number", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("12345678920231005111111001");
			AssertEquals("Splitting on a too long reference number", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.OrgCode = "12345678";
			AssertEquals("When setting a too short org no", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.OrgCode = "1234567890";
			AssertEquals("When setting a too long org no", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("-234567892023100511111101");
			AssertEquals("Splitting on a reference number with a negative org no", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.Date = "231005";
			AssertEquals("When setting a too short date", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.Date = "2023-10-05";
			AssertEquals("When setting a too long date", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.Date = "23/10/05";
			AssertEquals("When setting an unsupported date format", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.DateTime = ZDateTime.Empty;
			AssertEquals("When setting an empty datetime", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023022911111101");
			AssertEquals("Splitting on a reference number with an invalid date", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.Sequence = "11111";
			AssertEquals("When setting a too short sequence", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111101");
			referenceNumberWrapper.Sequence = "1111111";
			AssertEquals("When setting a too long sequence", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("12345678920231005-1111101");
			AssertEquals("Splitting on a reference number with a negative sequence", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111199");
			AssertEquals("When version number 99", expected: true, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.TryIncrementVersion();
			AssertEquals("When rolling over to version number 100", expected: false, referenceNumberWrapper.IsValid);

			referenceNumberWrapper.SetSplitValue("12345678920231005111111-1");
			AssertEquals("Splitting on a reference number with a negative version", expected: false, referenceNumberWrapper.IsValid);
		});
	}

	public void TestReferenceNumberSplitValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Org no", "123456789", referenceNumberWrapper.OrgCode);
			AssertEquals("Date", "20231005", referenceNumberWrapper.Date);
			AssertEquals("Sequence", "111111", referenceNumberWrapper.Sequence);
			AssertEquals("Version", "01", referenceNumberWrapper.VersionAsString);

			referenceNumberWrapper.SetSplitValue("1234567892023100511111199");
			AssertEquals("Splitting on a valid number with a version of 99", expected: true, referenceNumberWrapper.IsValid);
			referenceNumberWrapper.TryIncrementVersion();
			AssertEquals("Number should be invalid after increasing from 99", expected: false, referenceNumberWrapper.IsValid);
			AssertEquals("Pick fallback value when IsValid is false", "1234567892023100511111199", referenceNumberWrapper.ReferenceNumber);
		});
	}

	[TestDate(2023, 10, 19, 09, 30, 00)]
	public void TestGenerateFrom()
	{
		const string expectedOrgCode1 = "111222333";
		const string expectedOrgCode2 = "444555666";
		const string expectedOrgCode3 = "555566666";
		const string invalidOrgCode = "1234567890";
		const string expectedFallback = "1234567892023100511111101";
		var declarantDateTime = ZDateTime.Now;
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.OrganizationNumber;
		orgCusCode.OK_CustomsRegNo = expectedOrgCode1;
		CombineAssertions(() =>
		{
			referenceNumberWrapper.GenerateFrom(Factory, orgHeader, declarantDateTime, expectedFallback);
			AssertEquals("Org no", expectedOrgCode1, referenceNumberWrapper.OrgCode);
			AssertEquals("Date", "20231019", referenceNumberWrapper.Date);
			AssertEquals("Sequence", "000001", referenceNumberWrapper.Sequence);
			AssertEquals("Version", "01", referenceNumberWrapper.VersionAsString);
			AssertEquals("FallbackValue", expectedFallback, referenceNumberWrapper.FallbackValue);
			AssertEquals("IsValid", expected: true, referenceNumberWrapper.IsValid);
			AssertEquals("ReferenceNumber", "1112223332023101900000101", referenceNumberWrapper.ReferenceNumber);

			referenceNumberWrapper.GenerateFrom(null, orgHeader, declarantDateTime, expectedFallback);
			AssertEquals("when Factory is null, IsValid", expected: false, referenceNumberWrapper.IsValid);
			AssertEquals("when Factory is null, ReferenceNumber", expectedFallback, referenceNumberWrapper.ReferenceNumber);

			orgCusCode.OK_CustomsRegNo = invalidOrgCode;
			referenceNumberWrapper.GenerateFrom(Factory, orgHeader, declarantDateTime, expectedFallback);
			AssertEquals("when OrgCode is invalid, IsValid", expected: false, referenceNumberWrapper.IsValid);
			AssertEquals("when OrgCode is invalid, ReferenceNumber", expectedFallback, referenceNumberWrapper.ReferenceNumber);

			orgCusCode.OK_CustomsRegNo = expectedOrgCode1;
			referenceNumberWrapper.GenerateFrom(Factory, orgHeader, declarantDateTime, expectedFallback);
			AssertEquals("when gen on same Declarant, Org no", expectedOrgCode1, referenceNumberWrapper.OrgCode);
			AssertEquals("when gen on same Declarant, Date", "20231019", referenceNumberWrapper.Date);
			AssertEquals("when gen on same Declarant, Sequence", "000002", referenceNumberWrapper.Sequence);
			AssertEquals("when gen on same Declarant, Version", "01", referenceNumberWrapper.VersionAsString);
			AssertEquals("when gen on same Declarant, IsValid", expected: true, referenceNumberWrapper.IsValid);
			AssertEquals("when gen on same Declarant, ReferenceNumber", "1112223332023101900000201", referenceNumberWrapper.ReferenceNumber);

			orgCusCode.OK_CustomsRegNo = expectedOrgCode2;
			referenceNumberWrapper.GenerateFrom(Factory, orgHeader, declarantDateTime, expectedFallback);
			AssertEquals("when gen on another Declarant, Org no", expectedOrgCode2, referenceNumberWrapper.OrgCode);
			AssertEquals("when gen on another Declarant, Date", "20231019", referenceNumberWrapper.Date);
			AssertEquals("when gen on another Declarant, Sequence", "000001", referenceNumberWrapper.Sequence);
			AssertEquals("when gen on another Declarant, Version", "01", referenceNumberWrapper.VersionAsString);
			AssertEquals("when gen on another Declarant, IsValid", expected: true, referenceNumberWrapper.IsValid);
			AssertEquals("when gen on another Declarant, ReferenceNumber", "4445556662023101900000101", referenceNumberWrapper.ReferenceNumber);

			orgCusCode.OK_CustomsRegNo = expectedOrgCode3;
			referenceNumberWrapper.GenerateFrom(Factory, orgHeader, declarantDateTime, ZString.Empty);
			AssertEquals("when fallback is empty, IsValid", expected: true, referenceNumberWrapper.IsValid);
			AssertEquals("when fallback is empty, ReferenceNumber", $"{expectedOrgCode3}2023101900000101", referenceNumberWrapper.ReferenceNumber);
		});
	}
}
