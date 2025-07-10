using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgConsignorModule))]
	sealed class OrgConsignorModuleTest : OrganisationModuleTest
	{
		#region Overrides

		protected override ZWebTestHelper GetNewHelper() => new ZWebTestHelper(Factory);

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var consignor = (OrgHeader)base.CreateNewElementForExcelExport();
			consignor.OH_IsConsignor = true;
			var link = consignor.BuyerLinks.AddNew();
			link.OL_OH_Buyer = TestHelper.TestOrg.PK;
			link.OL_OH_Supplier = consignor.PK;

			return consignor;
		}

		protected override bool AllowActiveStatusFilterTest() => false;

		protected override WebModuleID TestID => WebModuleIDs.OrgConsignorTracking;

		protected override ZBool ExpectMatchConsignees => ZBool.False;

		#endregion
	}
}
