using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TradeDetailClonerGUIManagerTest : TestCaseWithFactory
	{
		public void TestShowForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sourceOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			sourceOpp.P8_OH = org.PK;
			var targetOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			targetOpp.P8_OH = org.PK;

			var cloner = new TradeDetailCloner(sourceOpp, targetOpp);

			var manager = new TradeDetailClonerGUIManager();
			manager.ShowForm(sourceOpp, targetOpp);
			using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertEquals("", lastShownForm.Text);
				lastShownForm.Close();
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
		}
	}
}
