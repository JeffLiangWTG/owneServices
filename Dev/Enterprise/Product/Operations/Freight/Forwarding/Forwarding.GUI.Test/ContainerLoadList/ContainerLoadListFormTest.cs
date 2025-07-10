using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ContainerLoadListForm))]
	public class ContainerLoadListFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var bO = factory.NewWithValidTestData<CYContainerLoadList>();

			factory.Save();

			var result = new ContainerLoadListForm(bO);
			result.ControllerID = ControllerIDs.ContainerLoadList;
			return result;
		}
	}
}
