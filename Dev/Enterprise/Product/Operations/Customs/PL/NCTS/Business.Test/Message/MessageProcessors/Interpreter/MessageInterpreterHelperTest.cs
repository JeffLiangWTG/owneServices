using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

public class MessageInterpreterHelperTest : TestCaseWithFactory
{
	public void TestGetOfficeCodeWithDescription()
	{
		const string testOfficeCode = $"{CountryCodes.Poland}2233";
		const string testOfficeCodeWithMissingGrouping = "DE2233";
		const string testOfficeDescription = "Test Office Of Departure";

		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, codeType, testOfficeCode, testOfficeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();

		CombineAssertions(() => {
			var expectedOffice = $"{testOfficeCode} - {testOfficeDescription}";
			var actualOffice = MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, testOfficeCode);
			AssertEquals("Office Of Departure includes description if office code has valid grouping", expectedOffice, actualOffice);

			actualOffice = MessageInterpreterHelper.GetOfficeCodeWithDescription(Factory, testOfficeCodeWithMissingGrouping);
			expectedOffice = testOfficeCodeWithMissingGrouping;
			AssertEquals("Office Of Departure doesn't include description if office code has not existing grouping", expectedOffice, actualOffice);
		});
	}

	[TestDate(2021, 03, 01)]

	public void TestGetTypeOfControlDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("PL");
		helper.CreateNewOrGetExistingCusCodeType("CL716", "Test list CL716.");
		helper.CreateCusCodeList("PL", "CL716", "T1", "Type Of Control 1", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 01, 31));
		helper.CreateCusCodeList("PL", "CL716", "T2", "Type Of Control 2", new ZDateTime(2021, 02, 01), new ZDateTime(2021, 12, 31));
		Factory.Save();

		CombineAssertions(() =>
		{
			var typesOfControl = Factory.GetTypeOfControlDescription("T1", date: new ZDateTime(2021, 01, 10));
			AssertEquals("Filtered by date.", "Type Of Control 1", typesOfControl);

			typesOfControl = Factory.GetTypeOfControlDescription("T2");
			AssertEquals("Filtered by default PL grouping.", "Type Of Control 2", typesOfControl);
		});
	}
}
