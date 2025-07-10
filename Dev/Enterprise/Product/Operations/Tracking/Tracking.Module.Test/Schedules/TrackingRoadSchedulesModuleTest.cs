using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(TrackingRoadSchedulesModule))]
	sealed class TrackingRoadSchedulesModuleTest : ZFilterStripGridModuleTestCase
	{
		public void TestGetNewFilterStripBusinessObject()
		{
			Assert(TestPage.SiteUser.IsLoggedIn);
			Assert(((JobSailingFilterBusinessObject)FilterStripGridModule.CreateNewFilterBusinessObject()).UserIsLoggedIn);
			TestPage.SiteUser.Logout();
			Assert(!((JobSailingFilterBusinessObject)FilterStripGridModule.CreateNewFilterBusinessObject()).UserIsLoggedIn);
		}

		protected override bool AllowActiveStatusFilterTest() => false;

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (var i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<JobSailing>();
				testObject.JX_ReservedMasterBill = "Include" + i.ToString();
				result.Add(testObject);
			}

			return result;
		}

		protected override List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter()
		{
			var result = new List<BusinessObject>();
			for (int i = 1; i < 10; i++)
			{
				var testObject = Factory.NewWithValidTestData<JobSailing>();
				testObject.JX_ReservedMasterBill = "Other" + i.ToString();
				result.Add(testObject);
			}

			return result;
		}

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var schedule = (JobSailing)base.CreateNewElementForExcelExport();
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = "ROA";
			var origin = voyage.Origins.AddNew();
			schedule.JX_JA = origin.PK;
			schedule.JX_IsPublished = true;

			return schedule;
		}

		protected override void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
			filter.AddToFilter(JobSailingSchema.JX_ReservedMasterBill, SQLComparisonOperator.StartsWith, "Include");
		}

		#endregion

		protected override WebModuleID TestID => WebModuleIDs.TrackingRoadSchedules;

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobSailingSchema.JX_DepotAvailabilityDate.Name, ListSortDirection.Ascending) };

		protected override DataGridColumn[] ExpectedRequiredGridColumns => new[] { (FilterGridModule as TrackingRoadSchedulesModule).AllColumns[0] };

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				var result = new List<DataGridColumn>();
				var defaults = base.ExpectedDefaultGridColumns;
				for (var i = 1; i < 9; i++)
				{
					result.Add(defaults[i]);
				}

				return result.ToArray();
			}
		}

		protected override ColumnDetailsForTest[] ExpectedColumnDetails => new[]
		{
			new ColumnDetailsForTest("Truck Ref.", 0, typeof(ZHyperLinkColumn)),
			new ColumnDetailsForTest("Load Port", 1, typeof(ZCodeFindBoxColumn)),
			new ColumnDetailsForTest("Discharge Port", 2, typeof(ZCodeFindBoxColumn)),
			new ColumnDetailsForTest("CFS Cut Off", 3, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("ETD", 4, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("ETA", 5, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("CFS Avail.", 6, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("Doc. Cutoff", 7, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("Carrier", 8, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("Chartered", 9, typeof(ZCheckBoxColumn)),
			new ColumnDetailsForTest("CFS Receival Start", 10, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("CFS Storage Start", 11, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("Rsrvd. Master", 12, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("Departure Berth", 13, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("Arrival Berth", 14, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("Departure Ref.", 15, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("Arrival Ref.", 16, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("ATD", 17, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("ATA", 18, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("T/ship", 19, typeof(ZCheckBoxColumn)),
			new ColumnDetailsForTest("Type", 20, typeof(ZTextEditColumn)),
			new ColumnDetailsForTest("CTO Cut Off", 21, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("CTO Receival Start", 22, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("CTO Avail.", 23, typeof(ZDateTimeColumn)),
			new ColumnDetailsForTest("CTO Storage Start", 24, typeof(ZDateTimeColumn))
		};

		public override bool GridHasHyperLinkColumn => false;

		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingRoadSchedules;

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");

			return result;
		}
	}
}
