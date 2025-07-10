using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailClonerForm))]
	class TradeDetailClonerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var sourceOpp = Factory.New<OrgOpportunity>();
			var targetOpp = Factory.New<OrgOpportunity>();
			var cloner = new TradeDetailCloner(sourceOpp, targetOpp);
			return new TradeDetailClonerForm(cloner);
		}
	}
}
