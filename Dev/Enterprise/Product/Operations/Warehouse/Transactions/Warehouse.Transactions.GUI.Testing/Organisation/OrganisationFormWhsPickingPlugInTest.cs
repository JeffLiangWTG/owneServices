using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrganisationFormWhsPickingPlugInTest : WhsGuiTestCaseWithFactory
	{
		#region TestBusinessEntity

		public void TestBusinessEntity()
		{
			using (var plugin = new OrganisationFormWhsPickingPlugIn(null))
			{
				AssertNull("Plugin Business Entity", plugin.BusinessEntity);
			}

			var client = Helper.CreateClient();
			using (var plugin = new OrganisationFormWhsPickingPlugIn(client))
			{
				AssertEquals("PlugIn Business Entity should be based off the Client passed in.", client, ((WhsClientPickingParams)plugin.BusinessEntity).Client);
			}
		}

		#endregion

		#region TestLicenceCheckPoint

		public void TestLicenceCheckPoint()
		{
			using (var plugin = new OrganisationFormWhsPickingPlugIn(null))
			{
				AssertEquals("PlugIn Licence Checkpoint should be Core.", Env.Licence.Core, ((IPlugInInternals)plugin).LicenceCheckPoint);
			}
		}

		#endregion

		#region TestName

		public void TestName()
		{
			using (var plugin = new OrganisationFormWhsPickingPlugIn(null))
			{
				AssertEquals("PlugIn Name should be Picking.", "Picking", plugin.Name);
			}
		}

		#endregion

		#region TestUserControl

		public void TestUserControl()
		{
			using (var plugin = new OrganisationFormWhsPickingPlugIn(null))
			{
				AssertEquals("User Control should have correct Type.", typeof(WhsPickingUserControl), plugin.UserControl.GetType());
			}
		}

		#endregion
	}
}