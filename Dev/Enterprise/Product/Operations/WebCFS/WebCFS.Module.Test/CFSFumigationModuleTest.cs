using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Module
{
	[TestedType(typeof(CFSFumigationModule))]
	sealed class CFSFumigationModuleTest : ZFilterGridModuleTest
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

		protected override Type GetElementTypeForExcelExport() => typeof(CFSContainer);

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var container = (CFSContainer)base.CreateNewElementForExcelExport();
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			container.JC_JX = sailing.PK;
			var service = container.Services.AddNew();
			service.ES_ServiceCode = "FUM";

			return container;
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.CFSFumigation; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(vw_List_FumigationSchema.LFV_FumigationBooked.Name, ListSortDirection.Ascending) };

		#endregion

		public override void TestSelectionColumnIsZHyperLinkColumn()
		{
			Assert("Not applicable here", true);
		}
	}
}
