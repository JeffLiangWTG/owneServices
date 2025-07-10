using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal sealed class RunSheetDashboardValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFilter()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			Assert(!viewer.DateRangeFilterInfo.HasErrors());
			viewer.DateRangeFilter = "BOOM";
			Assert(string.Format("Date range filter has been set to {0}; expected validation error on DateRangeFilter property", viewer.DateRangeFilter), viewer.DateRangeFilterInfo.HasErrors());
			viewer.DateRangeFilter = "Tomorrow";
			Assert(string.Format("Date range filter has been set to {0}; expected no validation errors on DateRangeFilter property", viewer.DateRangeFilter), !viewer.DateRangeFilterInfo.HasErrors());
			viewer.Branches.Load();
			viewer.Branch = viewer.Branches[0].PK;
			Assert(string.Format("Branch filter has been set to {0}; expected no validation errors on Branch property", viewer.Branches[0].PK), !viewer.BranchInfo.HasErrors());
		}

		public void TestDateRangeFrom()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			viewer.Validation.ValidateDateRangeFrom();
			Assert(!viewer.DateRangeFromInfo.HasErrors());
			viewer.DateRangeFilter = "Date range";
			viewer.Validation.ValidateDateRangeFrom();
			Assert(viewer.DateRangeFromInfo.HasErrors());
			viewer.DateRangeFrom = ZDateTime.Now;
			viewer.Validation.ValidateDateRangeFrom();
			Assert(!viewer.DateRangeFromInfo.HasErrors());
			viewer.DateRangeTo = ZDateTime.Now.AddDays(-1);
			viewer.Validation.ValidateDateRangeFrom();
			Assert(viewer.DateRangeFromInfo.HasErrors());
			viewer.DateRangeTo = ZDateTime.Now.AddDays(1);
			viewer.Validation.ValidateDateRangeFrom();
			Assert(!viewer.DateRangeFromInfo.HasErrors());
		}

		public void TestDateRangeTo()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			viewer.Validation.ValidateDateRangeTo();
			Assert(!viewer.DateRangeToInfo.HasErrors());
			viewer.DateRangeFilter = "Date range";
			viewer.Validation.ValidateDateRangeTo();
			Assert(viewer.DateRangeToInfo.HasErrors());
			viewer.DateRangeTo = ZDateTime.Now;
			viewer.Validation.ValidateDateRangeTo();
			Assert(!viewer.DateRangeToInfo.HasErrors());
			viewer.DateRangeFrom = ZDateTime.Now.AddDays(1);
			viewer.Validation.ValidateDateRangeTo();
			Assert(viewer.DateRangeToInfo.HasErrors());
			viewer.DateRangeFrom = ZDateTime.Now.AddDays(-1);
			viewer.Validation.ValidateDateRangeTo();
			Assert(!viewer.DateRangeToInfo.HasErrors());
		}

		[TestDate(2009, 4, 10)]
		public void TestMaxResults()
		{
			RunSheetDashboard viewer = new RunSheetDashboard(Factory);
			viewer.Branches.Load();
			GlbBranch branch = viewer.Branches[0];
			GlbStaff driver = Factory.New<GlbStaff>();
			driver.GS_FullName = "art the driver";
			driver.GS_Code = "RLS";
			driver.GS_GB_HomeBranch = branch.PK;
			for (int i = 1; i <= 50; i++)
			{
				CommonWorkSheet rs = Factory.New<CommonWorkSheet>();
				rs.EY_StartTime = ZDateTime.Today;
				rs.EY_EndTime = ZDateTime.Today.AddDays(1);
				rs.EY_DriversName = i.ToString();
				rs.EY_GS_NKTruckDriver = driver.GS_Code;
			}

			Factory.Save();
			viewer.Validation.ValidateDateRangeFilter();
			Assert("No warnings expected on DateRangeFilter", !viewer.DateRangeFilterInfo.HasWarnings());
			Assert("No errors expected on DateRangeFilter", !viewer.DateRangeFilterInfo.HasErrors());
			viewer.DateRangeFilter = "Date range";
			viewer.DateRangeTo = ZDateTime.Today;
			viewer.DateRangeFrom = ZDateTime.Today;
			viewer.Validation.ValidateDateRangeFilter();
			Assert("No warnings expected on DateRangeFilter", !viewer.DateRangeFilterInfo.HasWarnings());
			Assert("No errors expected on DateRangeFilter", !viewer.DateRangeFilterInfo.HasErrors());
			CommonWorkSheet extra = Factory.New<CommonWorkSheet>();
			extra.EY_StartTime = ZDateTime.Today;
			extra.EY_EndTime = ZDateTime.Today.AddDays(1);
			extra.EY_DriversName = "51";
			extra.EY_GS_NKTruckDriver = driver.GS_Code;
			Factory.Save();
			viewer.DateRangeFilter = "1";
			Assert("Errors expected on DateRangeFilter", viewer.DateRangeFilterInfo.HasErrors());
			viewer.DateRangeFilter = "Date range";
			viewer.DateRangeTo = ZDateTime.Today;
			viewer.DateRangeFrom = ZDateTime.Today;
			viewer.Validation.ValidateDateRangeFilter();
			Assert("No errors expected on DateRangeFilter", !viewer.DateRangeFilterInfo.HasErrors());
		}
	}
}
