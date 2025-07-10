using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class OrganisationsUserControlTest : TestCaseWithFactory
	{
		public void TestOrganisationsUserControl_AutoScroll()
		{
			using (var control = new OrganisationsUserControl())
			{
				AssertEquals("OrganisationsUserControl should set AutoScroll = true", true, control.DynamicOrganisationsPanel.AutoScroll);
			}
		}
	}
}
