using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ContainerLoadPlanForm))]
	public class ContainerLoadPlanFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<CFSContainerLoadList>();

			factory.Save();

			var result = new ContainerLoadPlanForm(bO);
			result.ControllerID = ControllerIDs.ContainerLoadPlan;
			return result;
		}
	}
}
