using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgAirCarrierModule))]
	sealed class OrgAirCarrierModuleTest : OrganisationModule_Test
	{
		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new ZWebTestHelper(Factory);
			Factory.Save();
			TestPage.SiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
		}
		ZWebTestHelper Helper;

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.OrgAirCarrierTracking; }
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			ArrayList defaults = new ArrayList(base.GetArrExpectedFilterBusinessObjectDefault());
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsShippingProvider, ZBool.True));
			defaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsAirLine, ZBool.True));
			return (FilterBusinessObjectDefault[])defaults.ToArray(typeof(FilterBusinessObjectDefault));
		}

		protected override void TestLoadCollectionInternal(ZFilterPage page)
		{
			page.SiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			base.TestLoadCollectionInternal(page);
		}
	}
}
