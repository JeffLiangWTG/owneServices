using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants.LogsScreeningStatus;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Organization Related Party Types Report")]
	public class TestRelatedPartyTypesReport : TemplateTestCase
	{
		public void TestRelatedPartyTypesReportAllColumnHeadingsShow()
		{
			var shownHeadings = new List<string>()
			{
				"Organization Code",
				"Organization Name",
				"Related UNLOCO",
				"Related Country/Region",
				"Party Type",
				"Party Type Description",
				"Freight Direction",
				"Transport Mode",
				"Container Mode",
				"Related Party Organization Code",
				"Related Party Organization Name",
				"Company Level",
				"Company Level Description"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in columnHeadings)
			{
				Assert(shownHeadings.Contains(heading.DisplayLabel));
			}
		}

		public void TestRelatedPartyReportHasDataCanFilter()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TEST$CODE";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TEST$O1";
			org2.OH_Code = "TEST$O2";
			org1.OH_RL_NKClosestPort = "USCHI";
			org2.OH_RL_NKClosestPort = "AUSYD";
			org1.MainAddress.OA_RN_NKCountryCode = "US";
			org2.MainAddress.OA_RN_NKCountryCode = "AU";

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var orgRelatedParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty1.PR_OH_Parent = orgHeader.PK;
			orgRelatedParty1.PR_OH_RelatedParty = org1.PK;
			orgRelatedParty1.PR_PartyType = "CAB";
			orgRelatedParty1.PR_FreightDirection = "DLV";
			orgRelatedParty1.PR_FreightTransportMode = "AIR";
			orgRelatedParty1.PR_SystemCreateUser = staff1.GS_Code;
			orgRelatedParty1.PR_SystemCreateTimeUtc = new ZDateTime("2018-09-03 12:00:00");

			var orgRelatedParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty2.PR_OH_Parent = orgHeader.PK;
			orgRelatedParty2.PR_OH_RelatedParty = org2.PK;
			orgRelatedParty2.PR_PartyType = "LTT";
			orgRelatedParty2.PR_FreightDirection = "PIC";
			orgRelatedParty2.PR_FreightTransportMode = "SEA";
			orgRelatedParty2.PR_SystemCreateUser = staff2.GS_Code;
			orgRelatedParty2.PR_SystemCreateTimeUtc = new ZDateTime("2018-09-05 12:00:00");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			staff1.GS_GB_HomeBranch = branch1.PK;
			staff2.GS_GB_HomeBranch = branch2.PK;

			Factory.Save();

			((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList.Add(orgHeader);

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals("TEST$CODE", excelInterface.WorkSheets[0][7, 1].ToString());
				AssertEquals(21, excelInterface.WorkSheets[0].RowCount);

				Report.ResetCachedExcelFileForTesting();
				((DateRangeField)Report.FilterCollection["Create Date"]).ValueLow = new ZDateTime("2018-09-03 00:00:00");
				((DateRangeField)Report.FilterCollection["Create Date"]).ValueHigh = new ZDateTime("2018-09-04 00:00:00");
				((LookupField)Report.FilterCollection["Related Party Create Branch"]).ZValue = branch1.PK;
				((LookupField)Report.FilterCollection["Related UNLOCO"]).ZValue = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USCHI").PK;
				((LookupField)Report.FilterCollection["Related Country/Region"]).ZValue = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "US").PK;
				((CodeListMultipleChoice)Report.FilterCollection["Party Type"]).Value = "CAB";
				((CodeListMultipleChoice)Report.FilterCollection["Direction"]).Value = "DLV";
				((CodeListMultipleChoice)Report.FilterCollection["Transport Mode"]).Value = "AIR";

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals(org1.OH_Code, excelInterface.WorkSheets[0][8, 10].ToString());
				Assert(!excelInterface.WorkSheets[0].ToString().Contains(org2.OH_Code));
				AssertEquals(20, excelInterface.WorkSheets[0].RowCount);

				foreach (FilterFieldWithUTSupport filter in Report.FilterCollection)
				{
					if (!(filter.DisplayName == "Organization" || filter.DisplayName == "Configurations"))
					{
						filter.ClearValueForUnitTest();
					}
				}

				Report.ResetCachedExcelFileForTesting();
				((DateRangeField)Report.FilterCollection["Create Date"]).ValueLow = new ZDateTime("2018-09-05 00:00:00");
				((DateRangeField)Report.FilterCollection["Create Date"]).ValueHigh = new ZDateTime("2018-09-06 00:00:00");
				((LookupField)Report.FilterCollection["Related Party Create Branch"]).ZValue = branch2.PK;
				((LookupField)Report.FilterCollection["Related UNLOCO"]).ZValue = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD").PK;
				((LookupField)Report.FilterCollection["Related Country/Region"]).ZValue = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "AU").PK;
				((CodeListMultipleChoice)Report.FilterCollection["Party Type"]).Value = "LTT";
				((CodeListMultipleChoice)Report.FilterCollection["Direction"]).Value = "PIC";
				((CodeListMultipleChoice)Report.FilterCollection["Transport Mode"]).Value = "SEA";

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals(org2.OH_Code, excelInterface.WorkSheets[0][8, 10].ToString());
				Assert(!excelInterface.WorkSheets[0].ToString().Contains(org1.OH_Code));
				AssertEquals(20, excelInterface.WorkSheets[0].RowCount);
			}
		}
	}

	[TemplateName("Address Profile Report")]
	public class TestAddressProfileReport : TemplateTestCase
	{
		public void TestReportShouldHasAtLeastOneFieldFilled()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var groupFilters = new List<FilterField>() {
				Report.FilterCollection["Country/Region"],
				Report.FilterCollection["Temp. Accounts"],
				Report.FilterCollection["Organizations"],
				Report.FilterCollection["UNLOCO"],
				Report.FilterCollection["Only Main"],
				Report.FilterCollection["Organization Type"]
			};
			var otherFilters = Report.FilterCollection.ToList().Cast<FilterField>().Where(x => !groupFilters.Contains(x)).ToList();

			Report.RunPreSaveValidation();
			groupFilters.ForEach(x => AssertContains("at least one", x.ValidationError, true));
			otherFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
			groupFilters[0].FillWithValidTestData();
			groupFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
			otherFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
		}

		public void TestReportShouldNotHasAllColumnHeadingsShow()
		{
			var shownHeadings = new List<string>()
			{
				"Organisation Code",
				"Organisation Full Name",
				"Is Address Active",
				"Address Related Port",
				"Address Company Name Override",
				"Additional Address Information",
				"Address Line 1",
				"Address Line 2",
				"City",
				"Post Code",
				"State",
				"Phone Number",
				"Mobile Number",
				"Fax Number",
				"Email Address",
				"Language",
				"Latitude",
				"Longitude"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in columnHeadings)
			{
				AssertEquals(shownHeadings.Contains(heading.DisplayLabel), !heading.Hidden);
			}
		}
	}

	[TemplateName("AP Profile Report")]
	public class TestAPProfileReport : TemplateTestCase
	{
	}

	[TemplateName("AR Profile Report")]
	public class TestARProfileReport : TemplateTestCase
	{
	}

	[TemplateName("ARAP Organisation By Consolidation Category")]
	public class TestARAPOrganisationByConsolidationCategory : TemplateTestCase
	{
	}

	[TemplateName("Carrier Profile Report")]
	public class TestCarrierProfileReport : TemplateTestCase
	{
	}

	[TemplateName("Competitor Trade Profile Report")]
	public class TestCompetitorTradeProfileReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Consignee Profile Report")]
	public class TestConsigneeProfileReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Consignee Supplier Report")]
	public class TestConsigneeSupplierReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Consignor Profile Report")]
	public class TestConsignorProfileReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Contact Birthday Report")]
	public class TestContactBirthdayReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Exchange Rate Report")]
	public class TestExchangeRateReport : TemplateTestCase
	{
	}

	[TemplateName("Organisation - Main Details Profile")]
	public class TestOrganisationMainDetailsProfile : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		public void TestReportShouldHasAtLeastOneFieldFilled()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var groupFilters = new List<FilterField>() {
				Report.FilterCollection["Country/Region"],
				Report.FilterCollection["Temp. Accounts"],
				Report.FilterCollection["Organizations"],
				Report.FilterCollection["UNLOCO"],
			};
			var otherFilters = Report.FilterCollection.ToList().Cast<FilterField>().Where(x => !groupFilters.Contains(x)).ToList();

			Report.RunPreSaveValidation();
			groupFilters.ForEach(x => AssertContains("at least one", x.ValidationError, true));
			otherFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
			groupFilters[0].FillWithValidTestData();
			groupFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
			otherFilters.ForEach(x => AssertNotContains("at least one", x.ValidationError, true));
		}

		public void TestReportShouldNotHasAllColumnHeadingsShow()
		{
			var shownHeadings = new List<string>()
			{
				"Organization Code",
				"Organization Name",
				"Company Name",
				"National",
				"Active",
				"Temporary",
				"CargoWise branch overseeing relationship",
				"Receivable",
				"Payable",
				"Consignor",
				"Consignee",
				"Transport Client",
				"Warehouse Client",
				"Carrier",
				"Forwarder",
				"Broker",
				"Services",
				"Competitor",
				"Sales",
				"Controlling Customer",
				"Controlling Agent"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in columnHeadings)
			{
				AssertEquals(shownHeadings.Contains(heading.DisplayLabel), !heading.Hidden);
			}
		}
	}

	[TemplateName("Organisation Competitor Details Profile Report")]
	public class TestOrganisationCompetitorDetailsProfileReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		public void TestAllCompetitorsAreReturned()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var org1 = GetNewOrgHeader("TEST01", true);
			CreateOrgCompanyData(org1.PK, GlbCompany.CurrentCompany.PK);

			var org2 = GetNewOrgHeader("TEST02", true);
			CreateOrgCompanyData(org2.PK, company1.PK);
			CreateOrgCompanyData(org2.PK, company2.PK);

			var org3 = GetNewOrgHeader("TEST03", false);
			CreateOrgCompanyData(org3.PK, company1.PK);
			CreateOrgCompanyData(org3.PK, company2.PK);

			var org4 = GetNewOrgHeader("TEST04", true);
			CreateOrgCompanyData(org4.PK, company1.PK);
			CreateOrgCompanyData(org4.PK, GlbCompany.CurrentCompany.PK);

			var org5 = GetNewOrgHeader("TEST05", false);

			var org6 = GetNewOrgHeader("TEST06", true);

			Factory.Save();

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);
			orgFilterList.Add(org3);
			orgFilterList.Add(org4);
			orgFilterList.Add(org5);
			orgFilterList.Add(org6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals("TEST01", excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals("TEST02", excelInterface.WorkSheets[0][8, 2].ToString());
				AssertEquals("TEST04", excelInterface.WorkSheets[0][9, 2].ToString());
				AssertEquals("TEST06", excelInterface.WorkSheets[0][10, 2].ToString());
				AssertEquals(22, excelInterface.WorkSheets[0].RowCount);
			}
		}

		public void TestCountryMultipleSelectionFilter()
		{
			//Add numerous records with unique data
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var countryA = Factory.NewWithValidTestData<RefCountry>();
			var countryB = Factory.NewWithValidTestData<RefCountry>();
			var countryC = Factory.NewWithValidTestData<RefCountry>();
			countryA.Code = "01";
			countryB.Code = "02";
			countryC.Code = "03";

			var org1 = GetNewOrgHeader("TEST01", true);
			org1.OH_RL_NKClosestPort = "01";

			var org2 = GetNewOrgHeader("TEST02", true);
			org2.OH_RL_NKClosestPort = "02";

			var org3 = GetNewOrgHeader("TEST03", true);
			org3.OH_RL_NKClosestPort = "03";

			var org4 = GetNewOrgHeader("TEST04", true);

			Factory.Save();

			//Filter by country
			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				var countryFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Country/Region"]).BindToList;
				countryFilterList.Add(countryA);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert only record with that country are shown
				AssertEquals("TEST01", excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("TEST02"));
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("TEST03"));
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST04"));

				//reset report
				Report.ResetCachedExcelFileForTesting();

				countryFilterList.Clear();
				countryFilterList.Add(countryA);
				countryFilterList.Add(countryB);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Filter by multiple countries
				//Assert only records with selected country are shown
				AssertEquals("TEST01", excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals("TEST02", excelInterface.WorkSheets[0][8, 2].ToString());
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("TEST03"));
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST04"));

				//clear report
				Report.ResetCachedExcelFileForTesting();

				//Filter by no country
				countryFilterList.Clear();

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert all records are shown
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST01"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST02"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST03"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST04"));
			}
		}

		public void TestSWOTRowHeight()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			//Generate records with various SWOT lengths (ensure that each column has at least one entry with length that is far too long)
			var shortStrength = "Short";
			var longStrength = "Really long: AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";

			var org1 = GetNewOrgHeader("TEST01", true);
			CreateOrgCompanyData(org1.PK, company1.PK);
			org1.MiscServ.OM_CIStrength = longStrength;

			var org2 = GetNewOrgHeader("TEST02", true);
			CreateOrgCompanyData(org2.PK, company2.PK);
			org2.MiscServ.OM_CIStrength = shortStrength;

			Factory.Save();

			//Generate report
			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
				orgFilterList.Add(org1);
				orgFilterList.Add(org2);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert that each record only displays a single row
				AssertEquals(longStrength, excelInterface.WorkSheets[0][7, 16].ToString());
				AssertEquals(shortStrength, excelInterface.WorkSheets[0][8, 16].ToString());
			}
		}

		public void TestSalesClientCategoryColumn()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			//Generate Records with Sales Category data
			var org1 = GetNewOrgHeader("TEST01", true);
			org1.MiscServ.OM_CMSalesCategory = "A01";

			var org2 = GetNewOrgHeader("TEST02", true);
			org2.MiscServ.OM_CMSalesCategory = "A02";

			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				//Generate Report
				var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
				orgFilterList.Add(org1);
				orgFilterList.Add(org2);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert that SCC column exists
				AssertEquals("Sales Cat.", excelInterface.WorkSheets[0][6, 12].ToString());

				//Assert correct data in each row of column
				AssertEquals("A01", excelInterface.WorkSheets[0][7, 12].ToString());
				AssertEquals("A02", excelInterface.WorkSheets[0][8, 12].ToString());
			}
		}

		public void TestUNLOCOMultipleSelectFilter()
		{
			//Add numerous records with unique data
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco1.Code = "UNLO1";
			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco2.Code = "UNLO2";
			var unloco3 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco3.Code = "UNLO3";
			var unloco4 = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco4.Code = "UNLO4";

			var org1 = GetNewOrgHeader("TEST01", true);
			org1.OH_RL_NKClosestPort = "UNLO1";

			var org2 = GetNewOrgHeader("TEST02", true);
			org2.OH_RL_NKClosestPort = "UNLO2";

			var org3 = GetNewOrgHeader("TEST03", true);
			org3.OH_RL_NKClosestPort = "UNLO3";

			var org4 = GetNewOrgHeader("TEST04", true);
			org4.OH_RL_NKClosestPort = "UNLO4";

			Factory.Save();

			//Filter by UNLOCO
			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				var unlocoFilterList = ((MultipleSelectionLookup)Report.FilterCollection["UNLOCO"]).BindToList;
				unlocoFilterList.Add(unloco1);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert only record with that UNLOCO are shown
				AssertEquals("UNLO1", excelInterface.WorkSheets[0][7, 4].ToString());
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("UNLO2"));
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("UNLO3"));
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("UNLO4"));

				//reset report
				Report.ResetCachedExcelFileForTesting();

				unlocoFilterList.Clear();
				unlocoFilterList.Add(unloco2);
				unlocoFilterList.Add(unloco3);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Filter by multiple UNLOCOs
				//Assert only records with selected UNLOCO are shown
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("UNLO1"));
				AssertEquals("UNLO2", excelInterface.WorkSheets[0][7, 4].ToString());
				AssertEquals("UNLO3", excelInterface.WorkSheets[0][8, 4].ToString());
				AssertEquals(false, excelInterface.WorkSheets[0].ToString().Contains("UNLO4"));

				//clear report
				Report.ResetCachedExcelFileForTesting();

				//Filter by no UNLOCO
				unlocoFilterList.Clear();

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert all records are shown
				Assert(excelInterface.WorkSheets[0].ToString().Contains("UNLO1"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("UNLO2"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("UNLO3"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("UNLO4"));
			}
		}

		public void TestCountryColumn()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			//Generate Records with Country data
			var org1 = GetNewOrgHeader("TEST01", true);
			org1.MainAddressCollection[0].OA_RN_NKCountryCode = "AU";
			org1.OH_RL_NKClosestPort = "AUAAA";
			var org2 = GetNewOrgHeader("TEST02", true);
			org2.OH_RL_NKClosestPort = "BMBBB";

			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				//Generate Report
				var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
				orgFilterList.Add(org1);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert that Country column exists
				AssertEquals("Country /Region", excelInterface.WorkSheets[0][6, 5].ToString());

				//Assert correct data in each row of column
				AssertEquals("AU", excelInterface.WorkSheets[0][7, 5].ToString());
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("BM"));
			}
		}

		public void TestSalesClientCategoryMultipleSelectionFilter()
		{
			//Add numerous records with unique data
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var org1 = GetNewOrgHeader("TEST01", true);
			org1.MiscServ.OM_CMSalesCategory = "AAA";

			var org2 = GetNewOrgHeader("TEST02", true);
			org2.MiscServ.OM_CMSalesCategory = "BBB";

			var org3 = GetNewOrgHeader("TEST03", true);
			org3.MiscServ.OM_CMSalesCategory = "CCC";

			var org4 = GetNewOrgHeader("TEST04", true);
			org4.MiscServ.OM_CMSalesCategory = "DDD";

			Factory.Save();

			//Filter by Sales Category
			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				var salesCatFilterList = ((OptionGroup)Report.FilterCollection["Sales Category"]);
				salesCatFilterList.AddOption("A", "AAA", true);
				salesCatFilterList.AddOption("B", "BBB", true);
				salesCatFilterList.AddOption("C", "CCC", false);
				salesCatFilterList.AddOption("D", "DDD", false);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert only record with those categories are shown
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST01"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST02"));
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST03"));
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST04"));

				Report.ResetCachedExcelFileForTesting();

				salesCatFilterList.DescriptionCodePairList["A"].Value = false;
				salesCatFilterList.DescriptionCodePairList["B"].Value = true;
				salesCatFilterList.DescriptionCodePairList["C"].Value = true;
				salesCatFilterList.DescriptionCodePairList["D"].Value = false;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				//Assert only record with those categories are shown
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST01"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST02"));
				Assert(excelInterface.WorkSheets[0].ToString().Contains("TEST03"));
				Assert(!excelInterface.WorkSheets[0].ToString().Contains("TEST04"));
			}
		}

		OrgHeader GetNewOrgHeader(string code, bool isCompetitor)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			org.OH_IsCompetitor = isCompetitor;
			org.CompanyDataCollection.RemoveAndDeleteAll();
			return org;
		}

		void CreateOrgCompanyData(ZGuid orgPK, ZGuid companyPK)
		{
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_GC = companyPK;
			orgCompanyData.OB_OH = orgPK;
		}
	}

	[TemplateName("Organisation Document Recipient Contacts Report")]
	public class TestOrganisationDocumentRecipientContactsReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Organisation Contact List" };
		}
	}

	[TemplateName("Organisation Contact Report")]
	public class TestOrganisationContactReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Organisation Contact" };
		}
	}

	[TemplateName("Organisation Contact List Report For Mail Merge")]
	public class TestOrganisationContactListReportForMailMerge : TemplateTestCase
	{
	}

	[TemplateName("Organisation Customs Codes")]
	public class TestOrganisationCustomsCodes : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		public void TestCountryRegionHeaderDisplayedCorrect()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals("Country/Region", excelInterface.WorkSheets[0][7, 4].ToString());
			}
		}
	}

	[TemplateName("Organisation Notes Profile")]
	public class TestOrganisationNotesProfile : TemplateTestCase
	{
		public void TestOrganisationNotesProfileReportAllColumnHeadingsShow()
		{
			var shownHeadings = new List<string>()
			{
				"Organization Code",
				"Organization Name",
				"UNLOCO",
				"Note Type",
				"Visibility",
				"Module",
				"Direction",
				"Freight",
				"Company",
				"Note"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;

			AssertContainsExactElementsInAnyOrder(shownHeadings, columnHeadings.Cast<ColumnHeading>().Select(heading => heading.DisplayLabel));
		}

		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Organisation Profile Counts")]
	public class TestOrganisationProfileCounts : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}

	[TemplateName("Organisation with Missing Contact Types")]
	public class TestOrganisationwithMissingContactTypes : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		protected override void ApplyNonClearableFiltersValues()
		{
			((LookupField)Report.FilterCollection["UNLOCO"]).ValueAsStringForSerialisation = "AUSYD";
		}
	}

	[TemplateName("US IRS 1099-MISC and 1099-NEC Forms")]
	public class TestOrganisationUSIRS1099MISCFormReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			((NumberField)Report.FilterCollection["Calendar Year"]).DefaultExpression = "2006";
		}
	}

	[TemplateName("US IRS 1099-MISC and 1099-NEC Forms 2021")]
	public class TestOrganisationUSIRS1099MISCFormReport2021 : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			((NumberField)Report.FilterCollection["Calendar Year"]).DefaultExpression = "2006";
		}
	}

	[TemplateName("US IRS 1099-MISC and 1099-NEC Forms 2020")]
	public class TestOrganisationUSIRS1099MISCFormReport2020 : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			((NumberField)Report.FilterCollection["Calendar Year"]).DefaultExpression = "2006";
		}
	}

	[TemplateName("Sales Client Relationship Profile Report")]
	public class TestSalesClientRelationshipProfileReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Sales - Missing Client Representatives Report")]
	public class TestSalesMissingClientRepresentativesReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Sales - Prospective Trade Lane Value")]
	public class TestSalesProspectiveTradeLaneValueReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Sales Trade Profile Report")]
	public class TestSalesTradeProfileReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Sales Missing Trade Profiles Report")]
	public class TestSalesMissingTradeProfilesReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Sales - Unseen Shipments Report")]
	public class TestSalesUnseenShipmentsReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	[TemplateName("Services Profile Report")]
	public class TestServicesProfileReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}

		public void TestReportContainsCorrectColumn()
		{
			var shownHeadings = new List<string>()
			{
				"Organization Code",
				"Organization Name",
				"UNLOCO",
				"Preference for Use",
				"Packing Depot",
				"Unpacking Depot",
				"Air CTO",
				"Sea CTO",
				"Road Depot",
				"Container Park",
				"Fumigation Contractor",
				"Rail Head",
				"Distribution Center"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			AssertEquals(shownHeadings.Count, columnHeadings.Count);

			foreach (ColumnHeading heading in columnHeadings)
			{
				Assert(shownHeadings.Contains(heading.DisplayLabel));
			}
		}
	}

	[TemplateName("Organisation - Similar Organisations Report (Legacy)")]
	public class TestOrganisationSimilarOrganisationsReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			((TextField)Report.FilterCollection["Org. Name Starts With"]).ValueAsStringForSerialisation = "AB";
		}

		protected override void ApplyNonClearableFiltersValues()
		{
			((TextField)Report.FilterCollection["Org. Name Starts With"]).ValueAsStringForSerialisation = "AB";
		}
	}

	[TemplateName("Voting Campaign Result Report")]
	public class TestVotingCampaignReport : TemplateTestCase
	{
	}

	[TemplateName("Address Edit Record")]
	public class TestAddressEditRecord : TemplateTestCase
	{
	}

	[TemplateName("Organization - Denied Party Screening Log")]
	public class TestOrganisationDeniedPartyScreeningLogReport : TemplateTestCase
	{
		public void TestStatusFilter()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals("There are 4 filters.", 4, Report.FilterCollection.Count);

				var filterNames = new List<string>();
				Report.FilterCollection.Cast<FilterFieldWithUTSupport>().ForEach(f => filterNames.Add(f.DisplayName));
				AssertContainsExactElementsInAnyOrder(new[] { "Configurations", "Organizations", "Date UTC", "Status" }, filterNames);

				Assert(excelInterface.WorkSheets[1].ToString().Contains($"{NoScreeningPerformed}, {ScreenedPermanentClear}, {ScreenedClear}, {MatchedDeniedParty}, {PotentialMatchesFound}, {InvalidatedByLocalDataChanges}, {InvalidatedByContentUpdate}, {ScreenedCanceled}, {UpdateRelatedJobs}"));
			}
		}
	}

	[TemplateName("Organization - Address Additional Info Override Usage Report")]
	public class TestAdditionalAddressInformationOverrideUsageReport : TemplateTestCase
	{
		public void TestAdditionalAddressInformationOverrideUsageReportAllColumnHeadingsShow()
		{
			var shownHeadings = new List<string>()
			{
				"Organization Code",
				"Organization Name",
				"Additional Address Info",
				"User",
				"Date"
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			AssertContainsExactElementsInAnyOrder(shownHeadings, columnHeadings.Cast<ColumnHeading>().Select(heading => heading.DisplayLabel));
		}

		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Template" };
		}
	}
}
