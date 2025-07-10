using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class OrganisationDetailsPlugInTest : ZPlugInGenericTest
	{
		public void TestPlugInMembers()
		{
			using (OrganisationDetailsPlugIn plugIn = new OrganisationDetailsPlugIn(Organisation))
			{
				AssertEquals("Name", "USA", plugIn.Name);
				AssertEquals(typeof(OrgHeaderWrapper), plugIn.BusinessEntity.GetType());
				AssertEquals(typeof(OrganisationDetailPlugInUserControl), plugIn.UserControl.GetType());
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new OrganisationDetailsPlugIn(Organisation);

		OrgHeader organisation;
		OrgHeader Organisation
		{
			get
			{
				if (organisation == null)
				{
					organisation = Factory.New<OrgHeader>();
					organisation.FillWithValidTestData();
				}

				return organisation;
			}
		}
	}
}
