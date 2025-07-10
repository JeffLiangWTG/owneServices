using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UnsubscribeContactsForm))]
	class UnsubscribeContactsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = new UnsubscribeContactsBusinessObjectForTest(Factory.NewWithValidTestData<GlbCompanyCampaign>());
			return new UnsubscribeContactsForm(bizO);
		}
	}
}
