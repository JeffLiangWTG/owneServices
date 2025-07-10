using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(BulkCommunicationForm))]
	public class BulkCommunicationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			BulkCommunication bulkComm = new BulkCommunication(Factory, campaign);
			return new BulkCommunicationForm(bulkComm);
		}
	}
}
