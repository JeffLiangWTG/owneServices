using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ConsolCreationTemplateColumnStyleTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			using (var columnStyle = new ConsolCreationTemplateColumnStyle(new ConsolCreationTemplateColumnStyleInfo()))
			{
				AssertEquals(typeof(ConsolCreationTemplateUserControl), columnStyle.EditControl.GetType());
			}
		}
	}
}
