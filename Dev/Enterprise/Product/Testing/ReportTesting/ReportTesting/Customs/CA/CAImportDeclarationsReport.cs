namespace Enterprise.ReportTesting.Customs.CA
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.Universal;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("CA Import Declaration Report")]
	public class TestCAImportDeclarationsReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public void TestReportRunsWithNoExceptionWithAllColumnHeadings()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var headings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in headings)
			{
				heading.Hidden = false;
			}
			RunReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			cusCodeList.ZZD_CodeType = "SUBLC";
			cusCodeList.ZZD_Code = "0001";
			cusCodeList.ZZD_Description = "DESC";
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);

			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "3001";
			carrier.ZZ4_Description = "3001 Desc";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "Toronto International Airport (Pearson)", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);

			Factory.Save();
		}

		public void TestBondTypeFilter()
		{
			PrepareReportForRender();
			var bondType = (CodeListMultipleChoice)Report.FilterCollection["Bond Type"];
			AssertEquals(CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider(CodeListChoiceCodeDescriptionList.Codes.CABondTypeList), bondType.List);
		}

		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public void TestBondInfoColumnHeading()
		{
			var shownHeadings = new List<string>()
			{
				"Bond Type",
				"Bond Number",
				"Bond Surety"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			FillReportWithDefaultValues();
			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings.Cast<ColumnHeading>().Where(headings => shownHeadings.Contains(headings.DisplayLabel));
			AssertEquals(3, columnHeadings.Count());
			foreach (var heading in columnHeadings)
			{
				AssertEquals($"{heading.DisplayLabel} expects hidden", true, heading.Hidden);
			}
			RunReport();
		}
	}

	public class TestCAImportDeclarationsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "CA Import Declaration Report"; }
		}

		public override string Hint
		{
			get { return "This report makes import (B2, IMP, LVS, LVX, MSC) declaration information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCAImportDeclarationsReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}
