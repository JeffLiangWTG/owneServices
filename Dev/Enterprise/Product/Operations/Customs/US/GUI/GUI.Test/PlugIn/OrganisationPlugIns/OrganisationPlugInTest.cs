using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI
{
	sealed class OrganisationPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestUserControl()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			using (OrganisationPlugIn plugIn = new OrganisationPlugIn(organisation))
			{
				AssertEquals(typeof(OrganisationPlugInUserControl), plugIn.UserControl.GetType());
				AssertEquals(typeof(OrgHeaderWrapper), plugIn.BusinessEntity.GetType());
				AssertEquals(typeof(OrganisationPlugInMenu), plugIn.TopLevelMenu.GetType());
				AssertEquals("Customs Messaging", plugIn.Name);
				Assert(!Env.Licence.ImportBroker.IsLoggedIn);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			return new OrganisationPlugIn(organisation);
		}
	}
}
