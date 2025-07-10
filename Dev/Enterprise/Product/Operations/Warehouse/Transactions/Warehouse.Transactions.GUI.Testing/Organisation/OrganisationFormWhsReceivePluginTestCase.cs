using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class OrganisationFormWhsReceivePluginTestCase : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				AssertEquals(Org, plugIn.OrgForTest);
			}
		}

		public void TestName()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				AssertEquals("Receive", plugIn.Name);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				AssertEquals(Env.Licence.Core, plugIn.LicenceCheckPointForTest);
			}
		}

		public void TestGetNewUserControl()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				using (var userControl = plugIn.GetNewUserControlForTest())
				{
					AssertEquals(typeof(WhsReceiveUserControl), userControl.GetType());
				}
			}
		}

		public void TestGetBusinessEntityForPlugIn()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				var clientParams = plugIn.GetBusinessEntityForPlugInForTest();
				AssertNotNull(clientParams);
				AssertEquals(Org, clientParams.Client);
			}
		}

		public void TestHasUserControl()
		{
			using (plugIn = new OrganisationFormWhsReceivePlugin(Org))
			{
				AssertEquals(true, plugIn.HasUserControlForTest);
			}
		}

		#region Implementation

		OrgHeader Org => org ?? (org = Factory.New<OrgHeader>());
		OrgHeader org;
		OrganisationFormWhsReceivePlugin plugIn;

		#endregion
	}
}
