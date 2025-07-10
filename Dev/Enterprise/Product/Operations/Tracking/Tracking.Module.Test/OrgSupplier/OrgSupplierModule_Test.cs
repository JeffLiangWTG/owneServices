using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgSupplierModule))]
	sealed class OrgSupplierModule_Test : OrganisationModuleTest
	{
		protected override WebModuleID TestID => WebModuleIDs.OrgSupplierTracking;

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var supplier = (OrgHeader)base.CreateNewElementForExcelExport();
			supplier.OH_IsConsignor = true;
			var link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = TestHelper.TestOrg.PK;

			return supplier;
		}
	}
}
