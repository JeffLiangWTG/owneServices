using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentScanning;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing.DocumentSending
{
	class CustomsGuaranteeEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadGuaranteeFromCode()
		{
			AssertEquals(guarantee1.PK, loader.LoadBusinessObjectFromCode(Factory, "P-12345678|GB|HYECMRLON|ABC|DEF|19000101|20790606")?.PK);
			AssertEquals(guarantee2.PK, loader.LoadBusinessObjectFromCode(Factory, "P-87654321|GB|HYECMRLON|ABC||19000101|")?.PK);
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "P-87654321|AU|HYECMRLON|AAA|BBB|19000101|20790606")?.PK);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Code should contain \"PermitNumber|CountryCode|PermitHolder|PermitType|PermitSubType|StartDate|EndDate\"")]
		public void TestLoadGuaranteeFromCodeWithMissingParts()
		{
			_ = loader.LoadBusinessObjectFromCode(Factory, "P-12345678|GB|HYECMRLON|ABC|DEF");
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Invalid empty values (PermitType, StartDate)")]
		public void TestLoadGuaranteeFromCodeWithEmptyParts()
		{
			_ = loader.LoadBusinessObjectFromCode(Factory, "P-12345678|GB|HYECMRLON||DEF||");
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "No matching permit holder found (DUMMY)")]
		public void TestLoadGuaranteeFromCodeWithInvalidPermitHolder()
		{
			_ = loader.LoadBusinessObjectFromCode(Factory, "P-12345678|GB|DUMMY|ABC|DEF|19000101|20790606");
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Invalid date format (yyyyMMdd)")]
		public void TestLoadGuaranteeFromCodeWithInvalidDate()
		{
			_ = loader.LoadBusinessObjectFromCode(Factory, "P-12345678|GB|HYECMRLON|ABC|DEF|12345678|20790606");
		}

		BaseCusGuaranteeHeader guarantee1, guarantee2;
		CustomsGuaranteeEDocsViaUniversalXmlSupport loader;

		protected override void SetUp()
		{
			var orgheader = Factory.New<OrgHeader>();
			orgheader.OH_Code = "HYECMRLON";

			guarantee1 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee1.CPH_Number = "P-12345678";
			guarantee1.CPH_RN_NKCountryCode = "GB";
			guarantee1.CPH_OH_PermitHolder = orgheader.PK;
			guarantee1.CPH_Type = "ABC";
			guarantee1.CPH_SubType = "DEF";
			guarantee1.CPH_StartDate = new ZDate(1900, 1, 1);
			guarantee1.CPH_EndDate = new ZDate(2079, 6, 6);

			guarantee2 = Factory.New<BaseCusGuaranteeHeader>();
			guarantee2.CPH_Number = "P-87654321";
			guarantee2.CPH_RN_NKCountryCode = "GB";
			guarantee2.CPH_OH_PermitHolder = orgheader.PK;
			guarantee2.CPH_Type = "ABC";
			guarantee2.CPH_StartDate = new ZDate(1900, 1, 1);

			loader = new CustomsGuaranteeEDocsViaUniversalXmlSupport();
		}
	}
}
