using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(PreAdviceConsolsListForm))]
	public class PreAdviceConsolsListFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			return new PreAdviceConsolsListForm(new QueryUserFindboxEventArgs(DummyModuleIDs.Dummy, preAdvice.Lookups.Consolidations));
		}
	}
}
