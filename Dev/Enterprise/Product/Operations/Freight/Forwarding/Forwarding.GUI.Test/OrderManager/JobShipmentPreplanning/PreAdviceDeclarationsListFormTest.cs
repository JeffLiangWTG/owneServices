using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(PreAdviceDeclarationsListForm))]
	public class PreAdviceDeclarationsListFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			return new PreAdviceDeclarationsListForm(new QueryUserFindboxEventArgs(DummyModuleIDs.Dummy, preAdvice.Lookups.Declarations));
		}
	}
}
