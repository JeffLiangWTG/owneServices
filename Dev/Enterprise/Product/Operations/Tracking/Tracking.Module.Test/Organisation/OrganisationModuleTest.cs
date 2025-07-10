using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrganisationModule))]
	class OrganisationModuleTest : OrganisationModule_Test
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Save();
		}

		#endregion Setup

		#region Overrides

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var org = (OrgHeader)base.CreateNewElementForExcelExport();
			var link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = SiteUser.LoggedInOrganisation.PK;

			return org;
		}

		protected override ZWebTestHelper GetNewHelper() => new TestHelper(Factory);

		protected override bool AllowActiveStatusFilterTest() => false;

		protected override WebModuleID TestID => WebModuleIDs.OrganisationTracking;

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			var defaults = new ArrayList(base.GetArrExpectedFilterBusinessObjectDefault());
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_RelatedConsign, ((OrgContactWebUser)TestPage.SiteUser).LoggedInOrganisation.PK));
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsConsignee, ExpectMatchConsignees));
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsConsignor, ExpectMatchConsignors));

			return (FilterBusinessObjectDefault[])defaults.ToArray(typeof(FilterBusinessObjectDefault));
		}

		protected virtual ZBool ExpectMatchConsignees => ZBool.True;

		protected virtual ZBool ExpectMatchConsignors => ZBool.True;

		#endregion

		protected override void TestLoadCollectionInternal(ZFilterPage page)
		{
			page.SiteUser.Login(TestHelper.TestOrg.OH_Code, TestHelper.TestContact.OC_Email, TestHelper.TestContact.PasswordForTesting);
			base.TestLoadCollectionInternal(page);
		}
	}
}
