using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ProfitShareRedistributionLogForm))]
	public class ProfitShareRedistributionLogFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var redistribution = Factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
			return new ProfitShareRedistributionLogForm(redistribution);
		}
	}
}
