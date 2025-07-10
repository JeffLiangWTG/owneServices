using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.WebCFS.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Module
{
	[TestedType(typeof(CFSSailingsModule))]
	sealed class CFSSailingsModuleTest : ZFilterGridModuleTest
	{
		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		#endregion

		#region Setup

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var sailing = (Sailing)base.CreateNewElementForExcelExport();
			sailing.JX_IsPublished = true;
			sailing.Origin.Voyage.JV_IsChartered = false;
			sailing.Origin.Voyage.JV_IsActive = true;
			sailing.Origin.Voyage.JV_AirSeaRoad = "SEA";
			sailing.Origin.JA_E_DEP = ZDateTime.Now.AddDays(10);

			return sailing;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.CFSSailings; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return System.Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(JobSailingSchema.JX_DepotAvailabilityDate.Name, ListSortDirection.Ascending) };

		#endregion

		public override void TestSelectionColumnIsZHyperLinkColumn()
		{
			Assert("Not applicable here", true);
		}
	}
}
