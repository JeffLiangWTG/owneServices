using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(ContainersForm))]
	sealed class ContainersFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ContainersForm result = new ContainersForm(Factory.New<CommonContainer>());
			result.ControllerID = ControllerIDs.Containers;
			return result;
		}

		public void TestFormCaption()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "CONT1111111";

			using (var form = new ContainersForm(container))
			{
				AssertEquals("Container CONT1111111", form.FormCaption);
				container.JC_ContainerNum = "";
				AssertEquals("Container ", form.FormCaption);
			}
		}
	}
}
