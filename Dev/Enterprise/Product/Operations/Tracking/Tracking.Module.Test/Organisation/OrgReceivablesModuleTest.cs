using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgReceivablesModule))]
	sealed class OrgReceivablesModuleTest : OrganisationModuleTest
	{
		protected override bool AllowActiveStatusFilterTest() => false;

		protected override ZWebTestHelper GetNewHelper() => new ZWebTestHelper(Factory);

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var org = (OrgHeader)base.CreateNewElementForExcelExport();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_IsConsignee = true;
			var link = org.SupplierLinks[0];
			link.OL_OH_Supplier = TestHelper.TestOrg.PK;

			return org;
		}

		protected override WebModuleID TestID => WebModuleIDs.OrgReceivablesTracking;

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			var defaults = new ArrayList(base.GetArrExpectedFilterBusinessObjectDefault());
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsReceivable, ZBool.True));

			return (FilterBusinessObjectDefault[])defaults.ToArray(typeof(FilterBusinessObjectDefault));
		}
	}
}
