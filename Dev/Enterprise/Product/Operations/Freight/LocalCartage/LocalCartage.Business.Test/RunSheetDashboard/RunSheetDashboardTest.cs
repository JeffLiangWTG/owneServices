using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(RunSheetDashboard))]
	sealed class RunSheetDashboardTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2009, 4, 10)]
		public void TestRunSheets()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = ZDateTime.Today;
			runSheet1.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet1.EY_DriversName = "Bob";
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_StartTime = ZDateTime.Today;
			runSheet2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet2.EY_DriversName = "Ted";
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			AssertEquals(0, viewer.RunSheets.Count);
			ZQuery query = new ZQuery(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			query.AddToFilter(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today.AddDays(1));
			viewer.RunSheets.AdditionalFilter = query;
			AssertEquals(2, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1, runSheet2 }, viewer.RunSheets);
		}

		[TestDate(2009, 9, 3)]
		public void TestWeekday()
		{
			//	Yesterday / Today
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = ZDateTime.Today.AddDays(-1);
			runSheet1.EY_EndTime = ZDateTime.Today;
			runSheet1.EY_DriversName = "Bob";
			//	Today / Tomorrow
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_StartTime = ZDateTime.Today;
			runSheet2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet2.EY_DriversName = "Bob";
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet3.EY_StartTime = ZDateTime.Today;
			runSheet3.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet3.EY_DriversName = "Ted";
			//	Tomorrow / Day after tomorrow
			CommonWorkSheet runSheet4 = Factory.New<CommonWorkSheet>();
			runSheet4.EY_StartTime = ZDateTime.Today.AddDays(1);
			runSheet4.EY_EndTime = ZDateTime.Today.AddDays(2);
			runSheet4.EY_DriversName = "Ted";
			//	Future dates
			CommonWorkSheet runSheet5 = Factory.New<CommonWorkSheet>();
			runSheet5.EY_StartTime = ZDateTime.Today.AddDays(6);
			runSheet5.EY_EndTime = ZDateTime.Today.AddDays(7);
			runSheet5.EY_DriversName = "Ted";
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			AssertEquals(0, viewer.RunSheets.Count);
			viewer.DateRangeFilter = "Yesterday";
			AssertEquals(1, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1 }, viewer.RunSheets);
			viewer.DateRangeFilter = "Today";
			AssertEquals(2, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet2, runSheet3 }, viewer.RunSheets);
			viewer.DateRangeFilter = "Tomorrow";
			AssertEquals(1, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet4 }, viewer.RunSheets);
			viewer.DateRangeFilter = "Tuesday";
			AssertEquals(0, viewer.RunSheets.Count);
			viewer.DateRangeFilter = "Wednesday";
			AssertEquals(1, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet5 }, viewer.RunSheets);
		}

		[TestDate(2009, 9, 3)]
		public void TestWeekdays()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			CodeDescriptionPairList list = viewer.Weekdays;
			AssertEquals(9, list.Count);
			AssertEquals("-1", list[0].Description);
			AssertEquals("0", list[1].Description);
			AssertEquals("1", list[2].Description);
			AssertEquals("2", list[3].Description);
			AssertEquals("3", list[4].Description);
			AssertEquals("4", list[5].Description);
			AssertEquals("5", list[6].Description);
			AssertEquals("6", list[7].Description);
			AssertEquals("Date range", list[8].Description);
			AssertEquals("Yesterday", list[0].Code);
			AssertEquals("Today", list[1].Code);
			AssertEquals("Tomorrow", list[2].Code);
			AssertEquals("Saturday", list[3].Code);
			AssertEquals("Sunday", list[4].Code);
			AssertEquals("Monday", list[5].Code);
			AssertEquals("Tuesday", list[6].Code);
			AssertEquals("Wednesday", list[7].Code);
			AssertEquals("Date range", list[8].Code);
		}

		[TestDate(2010, 4, 15)]
		public void TestBranch()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "KNZ";
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch1.GB_Code = "B1";
			branch1.GB_OH_OrgProxy = orgHeader.PK;
			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company.PK;
			branch2.GB_Code = "B2";
			GlbBranch branch3 = Factory.New<GlbBranch>();
			branch3.GB_GC = company.PK;
			branch3.GB_Code = "B3";
			GlbStaff driver1Branch1 = Factory.New<GlbStaff>();
			driver1Branch1.GS_FullName = "Bart the branch 1 driver";
			driver1Branch1.GS_Code = "D1";
			driver1Branch1.GS_GB_HomeBranch = branch1.PK;
			driver1Branch1.GS_LoginName = "D1B1";
			Assert(string.Format("Deiver set incorrectly"), driver1Branch1.GS_GB_HomeBranch == branch1.PK);
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_GS_NKTruckDriver = driver1Branch1.GS_Code;
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			Assert(string.Format("Run sheet 1 driver is [{0}], expected [{1}]", runSheet1.EY_DriversName, driver1Branch1.GS_FullName), runSheet1.EY_DriversName == driver1Branch1.GS_FullName);
			RefEquipment truck1Branch1 = Factory.New<RefEquipment>();
			truck1Branch1.RQ_IsVehicle = true;
			truck1Branch1.RQ_OH_Owner = orgHeader.PK;
			truck1Branch1.RQ_Registration = "B1T1";
			truck1Branch1.RQ_ShortCode = "1.1";
			runSheet1.EY_RQ_Truck = truck1Branch1.PK;
			Assert(string.Format("Run sheet 1 truck rego is [{0}], expected [{1}]", runSheet1.EY_TruckRegistration, truck1Branch1.RQ_Registration), runSheet1.EY_TruckRegistration == truck1Branch1.RQ_Registration);
			RefEquipment truck2Branch1 = Factory.New<RefEquipment>();
			truck2Branch1.RQ_IsVehicle = true;
			truck2Branch1.RQ_OH_Owner = orgHeader.PK;
			truck2Branch1.RQ_Registration = "B1T2";
			truck2Branch1.RQ_ShortCode = "1.2";
			Assert("Null date range filter expected", string.IsNullOrEmpty(viewer.DateRangeFilter));
			Assert("Null branch filter expected", viewer.Branch.IsEmpty);
			Assert("Runsheet dashboard has no filters; 0 run sheets expected", viewer.RunSheets.Count == 0);
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_RQ_Truck = truck2Branch1.PK;
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			Factory.Save();
			viewer.Branch = branch1.PK;
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 2 run sheets", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 2);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1, runSheet2 }, viewer.RunSheets);
			viewer.Branch = branch2.PK;
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 0 run sheets", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 0);
			GlbStaff driver1Branch2 = Factory.New<GlbStaff>();
			driver1Branch2.GS_FullName = "Sam the branch 2 driver";
			driver1Branch2.GS_Code = "D2";
			driver1Branch2.GS_GB_HomeBranch = branch2.PK;
			driver1Branch2.GS_LoginName = "D2B2";
			CommonWorkSheet runSheet4 = Factory.New<CommonWorkSheet>();
			runSheet4.EY_GS_NKTruckDriver = driver1Branch2.GS_Code;
			Factory.Save();
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 1 run sheet", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 1);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet4 }, viewer.RunSheets);
			viewer.Branch = ZGuid.Empty;
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 0 run sheets", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 0);
		}

		public void TestErrorMessageForDifferentLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			{
				var viewer = new RunSheetDashboard(Factory);
				viewer.DateRangeFilter = "Yesterday";
				AssertEquals("Keine übereinstimmenden Rollkarten für Gestern gefunden", viewer.ErrorMessage);
			}
		}

		public void TestDateRangeFilterMultilingualStringForDifferentLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			{
				var viewer = new RunSheetDashboard(Factory);
				AssertEquals("", viewer.DateRangeFilterMultilingualString);
				viewer.DateRangeFilter = "ABC";
				AssertEquals("ABC", viewer.DateRangeFilterMultilingualString);
				viewer.DateRangeFilter = "Yesterday";
				AssertEquals("Gestern", viewer.DateRangeFilterMultilingualString);
			}

			var viewerInEnglish = new RunSheetDashboard(Factory);
			AssertEquals("", viewerInEnglish.DateRangeFilterMultilingualString);
			viewerInEnglish.DateRangeFilter = "ABC";
			AssertEquals("ABC", viewerInEnglish.DateRangeFilterMultilingualString);
			viewerInEnglish.DateRangeFilter = "Yesterday";
			AssertEquals("Yesterday", viewerInEnglish.DateRangeFilterMultilingualString);
		}

		[TestDate(2010, 4, 15)]
		public void TestRunSheetsBranchAndDateRange()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_Code = "KNZ";
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch1.GB_Code = "B1";
			branch1.GB_OH_OrgProxy = orgHeader.PK;
			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company.PK;
			branch2.GB_Code = "B2";
			GlbBranch branch3 = Factory.New<GlbBranch>();
			branch3.GB_GC = company.PK;
			branch3.GB_Code = "B3";
			GlbStaff driver1Branch1 = Factory.New<GlbStaff>();
			driver1Branch1.GS_FullName = "Bart the branch 1 driver";
			driver1Branch1.GS_Code = "D1";
			driver1Branch1.GS_GB_HomeBranch = branch1.PK;
			driver1Branch1.GS_LoginName = "D1B1";
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_GS_NKTruckDriver = driver1Branch1.GS_Code;
			runSheet1.EY_StartTime = ZDateTime.Today.AddDays(-1);
			runSheet1.EY_EndTime = ZDateTime.Today;
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			RefEquipment truck1Branch1 = Factory.New<RefEquipment>();
			truck1Branch1.RQ_IsVehicle = true;
			truck1Branch1.RQ_OH_Owner = orgHeader.PK;
			truck1Branch1.RQ_Registration = "B1T1";
			truck1Branch1.RQ_ShortCode = "1.1";
			runSheet1.EY_RQ_Truck = truck1Branch1.PK;
			RefEquipment truck2Branch1 = Factory.New<RefEquipment>();
			truck2Branch1.RQ_IsVehicle = true;
			truck2Branch1.RQ_OH_Owner = orgHeader.PK;
			truck2Branch1.RQ_Registration = "B1T2";
			truck2Branch1.RQ_ShortCode = "1.2";
			Assert("Null date range filter expected", string.IsNullOrEmpty(viewer.DateRangeFilter));
			Assert("Null branch filter expected", viewer.Branch.IsEmpty);
			Assert("Runsheet dashboard has no filters; 0 run sheets expected", viewer.RunSheets.Count == 0);
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_RQ_Truck = truck2Branch1.PK;
			runSheet2.EY_StartTime = ZDateTime.Today.AddDays(-10);
			runSheet2.EY_EndTime = ZDateTime.Today.AddDays(-9);
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			Factory.Save();
			viewer.Branch = branch1.PK;
			viewer.DateRangeFilter = "Yesterday";
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 1 run sheet", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 1);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1 }, viewer.RunSheets);
			viewer.Branch = branch1.PK;
			viewer.DateRangeFilter = string.Empty;
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 2 run sheets", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 2);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1, runSheet2 }, viewer.RunSheets);
			viewer.DateRangeFilter = "Date range";
			viewer.DateRangeFrom = ZDateTime.Today.AddDays(-12);
			viewer.DateRangeTo = ZDateTime.Today.AddDays(-3);
			Factory.Save();
			Assert(string.Format("Runsheet dashboard filter problem {0}; expected 1 run sheet", viewer.RunSheets.AdditionalFilter.GetAsWhereClause(true)), viewer.RunSheets.Count == 1);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet2 }, viewer.RunSheets);
		}

		public void TestIsDateRange()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			Assert(!viewer.IsDateRange);
			viewer.DateRangeFilter = "1";
			Assert(!viewer.IsDateRange);
			viewer.DateRangeFilter = "Date range";
			Assert(viewer.IsDateRange);
		}

		[TestDate(2009, 4, 10)]
		public void TestRunSheets_DateRange()
		{
			CommonWorkSheet runSheet1 = Factory.New<CommonWorkSheet>();
			runSheet1.EY_StartTime = ZDateTime.Today.AddDays(-1);
			runSheet1.EY_EndTime = ZDateTime.Today;
			runSheet1.EY_DriversName = "Bob";
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_StartTime = ZDateTime.Today;
			runSheet2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheet2.EY_DriversName = "Ted";
			CommonWorkSheet runSheet3 = Factory.New<CommonWorkSheet>();
			runSheet3.EY_StartTime = ZDateTime.Today.AddDays(1);
			runSheet3.EY_EndTime = ZDateTime.Today.AddDays(2);
			runSheet3.EY_DriversName = "Zub";
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			AssertEquals(0, viewer.RunSheets.Count);
			viewer.DateRangeFilter = "Date range";
			viewer.DateRangeFrom = ZDateTime.Today;
			viewer.DateRangeTo = ZDateTime.Today.AddDays(1);
			AssertEquals(2, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet2, runSheet3 }, viewer.RunSheets);
			viewer.DateRangeFrom = ZDateTime.Today.AddDays(-1);
			viewer.DateRangeTo = ZDateTime.Today;
			AssertEquals(2, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheet1, runSheet2 }, viewer.RunSheets);
		}

		[TestDate(2016, 9, 13)]
		public void TestRunSheets_SingleDay()
		{
			var runSheetB1 = Factory.New<CommonWorkSheet>();
			runSheetB1.EY_StartTime = ZDateTime.Today.AddHours(8);
			runSheetB1.EY_EndTime = ZDateTime.Today.AddHours(18);
			runSheetB1.EY_DriversName = "Bob1";
			var runSheetB2 = Factory.New<CommonWorkSheet>();
			runSheetB2.EY_StartTime = ZDateTime.Today;
			runSheetB2.EY_EndTime = ZDateTime.Today.AddDays(1);
			runSheetB2.EY_DriversName = "Bob2";
			var runSheetT1 = Factory.New<CommonWorkSheet>();
			runSheetT1.EY_StartTime = ZDateTime.Today.AddDays(1).AddHours(8);
			runSheetT1.EY_EndTime = ZDateTime.Today.AddDays(1).AddHours(18);
			runSheetT1.EY_DriversName = "Ted1";
			var runSheetT2 = Factory.New<CommonWorkSheet>();
			runSheetT2.EY_StartTime = ZDateTime.Today.AddDays(1);
			runSheetT2.EY_EndTime = ZDateTime.Today.AddDays(1).AddHours(1);
			runSheetT2.EY_DriversName = "Ted2";
			var runSheetN = Factory.New<CommonWorkSheet>();
			runSheetN.EY_StartTime = ZDateTime.Today.AddDays(-1).AddHours(18);
			runSheetN.EY_EndTime = ZDateTime.Today.AddHours(8);
			runSheetN.EY_DriversName = "Nik";
			var runSheetS = Factory.New<CommonWorkSheet>();
			runSheetS.EY_StartTime = ZDateTime.Today.AddDays(2);
			runSheetS.EY_EndTime = ZDateTime.Today.AddDays(3);
			runSheetS.EY_DriversName = "Sam";
			var viewer = new RunSheetDashboard(Factory);
			AssertEquals(0, viewer.RunSheets.Count);
			viewer.DateRangeFilter = "Today";
			AssertEquals(3, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheetB1, runSheetB2, runSheetN }, viewer.RunSheets);
			viewer.DateRangeFilter = "Tomorrow";
			AssertEquals(2, viewer.RunSheets.Count);
			AssertContainsExactElementsInAnyOrder(new CommonWorkSheet[] { runSheetT1, runSheetT2 }, viewer.RunSheets);
		}

		[TestDate(2014, 4, 10)]
		public void TestCopyTransientProperties()
		{
			var dashBoard = new RunSheetDashboard(Factory);
			dashBoard.DateRangeFilter = "Date range";
			dashBoard.DateRangeFrom = ZDateTime.Today.AddDays(-5);
			dashBoard.DateRangeTo = ZDateTime.Today.AddDays(-3);
			dashBoard.Branch = ZGuid.NewZGuid();
			var newDashBoard = new RunSheetDashboard(Factory);
			newDashBoard.FilterChanged += delegate
			{
				throw new InvalidOperationException("Filter Change should have been suspended.");
			};
			dashBoard.CopyTransientProperties(newDashBoard);
			AssertEquals("Date range", newDashBoard.DateRangeFilter);
			AssertEquals(ZDateTime.Today.AddDays(-5), newDashBoard.DateRangeFrom);
			AssertEquals(ZDateTime.Today.AddDays(-3), newDashBoard.DateRangeTo);
			AssertEquals(dashBoard.Branch, newDashBoard.Branch);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RunSheetDashboard(Factory);
		}
	}
}
