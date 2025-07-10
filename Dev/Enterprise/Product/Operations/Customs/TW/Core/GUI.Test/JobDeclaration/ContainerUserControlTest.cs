using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class ContainerUserControlTest : TestCaseWithFactory
	{
		public void TestGridId()
		{
			using (var control = new ContainerUserControl())
			{
				AssertEquals("GridLayoutb2j7ztwz5jEBb5tiCBrqlw==", control.CusContainersBoundGrid.InnerGrid.GridId);
			}
		}
	}
}
