using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Communication Reporting")]
	public class CommunicationReportingTemplateTest : TemplateTestCase
	{
		[TestDate(2008, 7, 14)]
		public void TestCommunicationLastEditDateFilter()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2009, 7, 14);
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				((DateRangeField)Report.FilterCollection["Communication Last Edit Date"]).ValueLow = new ZDateTime(2008, 5, 1);
				((DateRangeField)Report.FilterCollection["Communication Last Edit Date"]).ValueHigh = new ZDateTime(2008, 8, 1);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals(comm1.OQ_CommunicationID, excelInterface.WorkSheets[0][6, 2].ToString());
				Assert(!excelInterface.WorkSheets[0].ToString().Contains(comm2.OQ_CommunicationID));
			}
		}

		public void TestCommunicationReportHeadings()
		{
			var expectedHeadings = new List<(string, bool hidden)>()
			{
				("Communication ID", false),
				("Overall Disposition", false),
				("Date of Communication", false),
				("Client Name", false),
				("Subject", false),
				("Method", false),
				("Communication Status", false),
				("Client Size", false),
				("Client Vertical Market", true),
				("Client Contact", false),
				("Staff Coordinator Full Name", false),
				("Sales Team Code", false),
				("Sales Team Name", false),
				("Sales Relations Last Edit Date", false),
				("Has Sales Relation", false),
				("Internal Notes", false),
				("Follow Up Notes", false),
				("Purpose", false),
				("Communication Actual Date", true),
				("All Contact Attendees", true),
				("Client Code", true),
				("Location", true),
				("Other Attendees", true),
				("Purpose Desc", true),
				("Related Value Analysis", true),
				("Sales Category", true),
				("Communication Scheduled Date", true),
				("Staff Attendees", true),
				("Staff Coordinator Branch", true),
				("Staff Coordinator Initials", true),
				("Staff Coordinator Login", true),
				("Communication Last Edit Date", false),
				("Opportunity ID", true),
				("Opportunity Sales Type Code", true),
				("Opportunity Sales Type Description", true)
			};
			PrepareReportForRender();
			SelectAllOptionalTemplates();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			foreach (ColumnHeading heading in columnHeadings)
			{
				Assert($"Report contains unexpected heading '{heading.DisplayLabel}', '{heading.Hidden}'", expectedHeadings.Contains((heading.DisplayLabel, heading.Hidden)));
			}

			AssertEquals($"Expected {expectedHeadings.Count} headings, found {columnHeadings.Count} headings", expectedHeadings.Count, columnHeadings.Count);
		}

		public void TestCommunicationVerticalMarketFilter()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[12].Hidden = false;

			var assignedVerticalMarketType = "AERO";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_CMIndustryVertical = assignedVerticalMarketType;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MiscServ.OM_CMIndustryVertical = "FAKE";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org3.PK;

			Factory.Save();

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);
			orgFilterList.Add(org3);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				((CodeListMultipleChoice)Report.FilterCollection["Client Vertical Market"]).Value = string.Empty;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(sheetContent.Contains(comm1.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm3.OQ_CommunicationID));

				Report.ResetCachedExcelFileForTesting();

				((CodeListMultipleChoice)Report.FilterCollection["Client Vertical Market"]).Value = assignedVerticalMarketType;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(!sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(!sheetContent.Contains(comm3.OQ_CommunicationID));
				AssertEquals(assignedVerticalMarketType, excelInterface.WorkSheets[0][6, 20].ToString());
			}
		}

		public void TestClientSizeMultipleSelectionFilter()
		{
			var clientSizeList = new CodeDescriptionPairList();
			clientSizeList.AddPair("AAA", "AAADescription");
			clientSizeList.AddPair("BBB", "BBBDescription");
			clientSizeList.AddPair("CCC", "CCCDescription");

			OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientSizeList);

			PrepareReportForRender();
			FillReportWithDefaultValues();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[11].Hidden = false;

			var assignedClientSize = "CCC";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_CMClientSize = assignedClientSize;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MiscServ.OM_CMClientSize = "AAA";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org3.PK;
			Factory.Save();

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);
			orgFilterList.Add(org3);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var clientSizeFilter = ((OptionGroup)Report.FilterCollection["Client Size"]);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(sheetContent.Contains(comm1.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm3.OQ_CommunicationID));

				Report.ResetCachedExcelFileForTesting();

				clientSizeFilter.DescriptionCodePairList["AAADescription"].Value = true;
				clientSizeFilter.DescriptionCodePairList["CCCDescription"].Value = true;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(sheetContent.Contains(comm1.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(!sheetContent.Contains(comm3.OQ_CommunicationID));
				AssertEquals(assignedClientSize, excelInterface.WorkSheets[0][6, 9].ToString());
				AssertEquals("AAA", excelInterface.WorkSheets[0][7, 9].ToString());
			}
		}

		public void TestOpportunityIDFilter()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[32].Hidden = false;

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();

			var oppID1 = "O00001111";
			var oppID2 = "O00002222";
			var oppID3 = "O00003333";

			opp1.P8_OpportunityID = oppID1;
			opp2.P8_OpportunityID = oppID2;
			opp3.P8_OpportunityID = oppID3;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org3.PK;

			opp1.RelatedParentActivityPivotCollection.AddNewPivot(comm1);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm2);
			opp3.RelatedChildActivityPivotCollection.AddNewPivot(comm3);

			Factory.Save();

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);
			orgFilterList.Add(org3);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var opportunityIDFilter = ((MultipleSelectionLookup)Report.FilterCollection["Opportunity ID"]).BindToList;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();
				AssertEquals(oppID1, excelInterface.WorkSheets[0][6, 20].ToString());
				AssertEquals(oppID2, excelInterface.WorkSheets[0][7, 20].ToString());
				AssertEquals(oppID3, excelInterface.WorkSheets[0][8, 20].ToString());

				Report.ResetCachedExcelFileForTesting();

				opportunityIDFilter.Add(opp1);
				opportunityIDFilter.Add(opp3);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(!sheetContent.Contains(opp2.P8_OpportunityID));
				AssertEquals(oppID1, excelInterface.WorkSheets[0][6, 20].ToString());
				AssertEquals(oppID3, excelInterface.WorkSheets[0][7, 20].ToString());
			}
		}

		public void TestSortByCommunicationLastEditDateDESCMethodOrganization()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ZZZ";

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			comm1.OQ_TypeOfCall = "PHN";
			comm1.OQ_CommunicationID = "CM00001111";

			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			comm2.OQ_TypeOfCall = "MTG";
			comm2.OQ_CommunicationID = "CM00002222";

			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org2.PK;
			comm3.OQ_TypeOfCall = "EML";
			comm3.OQ_CommunicationID = "CM00003333";

			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2002/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm1.PK}'");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2020/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm2.PK}'");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2002/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm3.PK}'");

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var orgCodeAndCommRecordLastEditSort = Report.SortOrderCollection["Organization, Communication Last Edit Date"];
				var commRecordLastEditDESCMethodOrganizationSort = Report.SortOrderCollection["Communication Last Edit Date (Descending), Method, Organization"];
				Report.SortOrderCollection.SelectedOrder = orgCodeAndCommRecordLastEditSort;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals(comm1.OQ_CommunicationID, excelInterface.WorkSheets[0][6, 2].ToString());
				AssertEquals(comm3.OQ_CommunicationID, excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals(comm2.OQ_CommunicationID, excelInterface.WorkSheets[0][8, 2].ToString());

				Report.ResetCachedExcelFileForTesting();

				Report.SortOrderCollection.SelectedOrder = commRecordLastEditDESCMethodOrganizationSort;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals(comm2.OQ_CommunicationID, excelInterface.WorkSheets[0][6, 2].ToString());
				AssertEquals(comm3.OQ_CommunicationID, excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals(comm1.OQ_CommunicationID, excelInterface.WorkSheets[0][8, 2].ToString());
			}
		}

		public void TestOpportunitySalesTypeCodeFilter()
		{
			var description1 = "11111";
			var description2 = "22222";
			var type1 = "ONE";
			var type2 = "TWO";

			var list = new CodeDescriptionBoolCollection();
			list.Add(type1, (NoResString)description1, true);
			list.Add(type2, (NoResString)description2, true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			PrepareReportForRender();
			FillReportWithDefaultValues();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[33].Hidden = false;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org3.PK;

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var id1 = "O00001111";
			opp1.P8_OpportunityID = id1;
			opp1.P8_OpportunityType = type1;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var id2 = "O00002222";
			opp2.P8_OpportunityID = id2;
			opp2.P8_OpportunityType = type2;

			opp1.RelatedChildActivityPivotCollection.AddNewPivot(comm1);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm2);

			Factory.Save();

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);
			orgFilterList.Add(org3);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				((CodeListMultipleChoice)Report.FilterCollection["Opportunity Sales Type Code"]).Value = string.Empty;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(sheetContent.Contains(comm1.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(sheetContent.Contains(comm3.OQ_CommunicationID));

				Report.ResetCachedExcelFileForTesting();

				((CodeListMultipleChoice)Report.FilterCollection["Opportunity Sales Type Code"]).Value = type1;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				sheetContent = excelInterface.WorkSheets[0].ToString();
				Assert(sheetContent.Contains(comm1.OQ_CommunicationID));
				Assert(!sheetContent.Contains(comm2.OQ_CommunicationID));
				Assert(!sheetContent.Contains(comm3.OQ_CommunicationID));
			}
		}

		public void TestSortByOpportunityIDAndCommRecordLastEditDate()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ZZZ";

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			comm1.OQ_CommunicationID = "CM00001111";

			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			comm2.OQ_CommunicationID = "CM00002222";

			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org2.PK;
			comm3.OQ_CommunicationID = "CM00003333";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();

			opp1.P8_OpportunityID = "O00002222";
			opp2.P8_OpportunityID = "O00001111";

			opp1.RelatedChildActivityPivotCollection.AddNewPivot(comm1);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm2);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm3);

			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2002/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm1.PK}'");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2020/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm2.PK}'");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.OrgSalesCall SET OQ_SystemLastEditTimeUtc = '2002/7/14', OQ_SystemLastEditUser = 'E' WHERE OQ_PK = '{comm3.PK}'");

			var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
			orgFilterList.Add(org1);
			orgFilterList.Add(org2);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var orgCodeAndCommRecordLastEditSort = Report.SortOrderCollection["Organization, Communication Last Edit Date"];
				var oppIDAndCommRecordLastEditSort = Report.SortOrderCollection["Opportunity ID, Communication Last Edit Date"];
				Report.SortOrderCollection.SelectedOrder = orgCodeAndCommRecordLastEditSort;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals(comm1.OQ_CommunicationID, excelInterface.WorkSheets[0][6, 2].ToString());
				AssertEquals(comm3.OQ_CommunicationID, excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals(comm2.OQ_CommunicationID, excelInterface.WorkSheets[0][8, 2].ToString());

				Report.ResetCachedExcelFileForTesting();

				Report.SortOrderCollection.SelectedOrder = oppIDAndCommRecordLastEditSort;

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				AssertEquals(comm3.OQ_CommunicationID, excelInterface.WorkSheets[0][6, 2].ToString());
				AssertEquals(comm2.OQ_CommunicationID, excelInterface.WorkSheets[0][7, 2].ToString());
				AssertEquals(comm1.OQ_CommunicationID, excelInterface.WorkSheets[0][8, 2].ToString());
			}
		}

		public void TestOpportunityIDColumn()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[32].Hidden = false;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			comm1.OQ_CommunicationID = "CM00001111";
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			comm2.OQ_CommunicationID = "CM00002222";
			var comm3 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm3.OQ_OH = org2.PK;
			comm3.OQ_CommunicationID = "CM00003333";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var id1 = "O00001111";
			opp1.P8_OpportunityID = id1;
			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var id2 = "O00002222";
			opp2.P8_OpportunityID = id2;
			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var id3 = "O00003333";
			opp3.P8_OpportunityID = id3;

			opp1.RelatedChildActivityPivotCollection.AddNewPivot(comm1);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm2);
			opp3.RelatedChildActivityPivotCollection.AddNewPivot(comm2);

			opp1.P8_OH = org1.PK;
			opp2.P8_OH = org2.PK;
			opp3.P8_OH = org2.PK;

			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				var oppIDAndCommRecordLastEditSort = Report.SortOrderCollection["Opportunity ID, Communication Last Edit Date"];
				Report.SortOrderCollection.SelectedOrder = oppIDAndCommRecordLastEditSort;

				var orgFilterList = ((MultipleSelectionLookup)Report.FilterCollection["Organization"]).BindToList;
				orgFilterList.Add(org1);
				orgFilterList.Add(org2);

				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals("Opportunity ID", "", excelInterface.WorkSheets[0][6, 20].ToString());
				AssertEquals("Opportunity ID", id1, excelInterface.WorkSheets[0][7, 20].ToString());
				AssertEquals("Opportunity ID", id2, excelInterface.WorkSheets[0][8, 20].ToString());
				AssertEquals("Opportunity ID", id3, excelInterface.WorkSheets[0][9, 20].ToString());
			}
		}

		public void TestOpportunitySalesTypeCodeColumn()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var type1Code = "ONE";
			var type2Code = "TWO";

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[33].Hidden = false;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			comm1.OQ_CommunicationID = "CM00001111";
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			comm2.OQ_CommunicationID = "CM00002222";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityType = type1Code;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OpportunityType = type2Code;

			opp1.RelatedChildActivityPivotCollection.AddNewPivot(comm1);
			opp2.RelatedChildActivityPivotCollection.AddNewPivot(comm2);

			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals("Opportunity Sales Type Code", type1Code, excelInterface.WorkSheets[0][6, 20].ToString());
				AssertEquals("Opportunity Sales Type Code", type2Code, excelInterface.WorkSheets[0][7, 20].ToString());
			}
		}

		public void TestOpportunitySalesTypeDescriptionColumn()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var type1Description = "11111";
			var type2Description = "22222";
			var type1Code = "ONE";
			var type2Code = "TWO";

			var list = new CodeDescriptionBoolCollection();
			list.Add(type1Code, (NoResString)type1Description, true);
			list.Add(type2Code, (NoResString)type2Description, true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var columnHeadings = Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[0].ColumnHeadings;
			columnHeadings[34].Hidden = false;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var comm1 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm1.OQ_OH = org1.PK;
			comm1.OQ_CommunicationID = "CM00001111";
			var comm2 = Factory.NewWithValidTestData<OrgSalesCall>();
			comm2.OQ_OH = org2.PK;
			comm2.OQ_CommunicationID = "CM00002222";

			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_OpportunityType = type1Code;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_OpportunityType = type2Code;

			opp1.RelatedChildActivityPivotCollection.AddNewPivot(comm1);
			comm2.RelatedChildActivityPivotCollection.AddNewPivot(opp2);

			Factory.Save();

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				AssertEquals("Opportunity Sales Type Description", type1Description, excelInterface.WorkSheets[0][6, 20].ToString());
				AssertEquals("Opportunity Sales Type Description", type2Description, excelInterface.WorkSheets[0][7, 20].ToString());
			}
		}

		protected override void FillReportWithDefaultValues()
		{
			var collection = new CodeDescriptionBoolCollection();
			var meetingLocationResource = collection.AddNew();
			meetingLocationResource.Bool = true;
			meetingLocationResource.Code = "ROM";
			SystemDataRegistry.Instance.ResourceTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var opp = Factory.NewWithValidTestData<OrgOpportunity>();

			base.FillReportWithDefaultValues();
			Factory.New<SalesTeam>();
			var meetingLocation = Factory.New<GlbStaff>();
			meetingLocation.GS_IsResource = true;
			meetingLocation.GS_ResourceType = meetingLocationResource.Code;

			Factory.Save();
		}
	}

	public class CommunicationReportingReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Communication Reporting"; }
		}

		public override string Hint
		{
			get { return @"Communication Reporting shows a listing of all communications in the system."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new CommunicationReportingTemplateTest();
		}
	}
}
