using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class PlugInTest : TestCaseWithFactory
	{
		public void TestPlugInsContainsClaimsAndQueries()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedTab = form.ReceivablesTabPage;
				bool contains = false;
				for (int i = 0; i < form.ReceivablesControl.ARTabControl.PlugIns.Instances.Length; i++)
				{
					string name = form.ReceivablesControl.ARTabControl.PlugIns.Instances[i].Name;
					if (name == "Receivables Claims and Queries")
					{
						contains = true;
						break;
					}
				}
				Assertion.Assert("PlugIns should include Claims and Queries", contains);
			}
		}
	}
}
