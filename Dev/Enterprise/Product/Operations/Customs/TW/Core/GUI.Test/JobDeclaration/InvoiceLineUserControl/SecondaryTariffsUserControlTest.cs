using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class SecondaryTariffsUserControlTest : TestCaseWithFactory
	{
		public void TestReadOnlyColumns()
		{
			using (var userControl = new SecondaryTariffsUserControl())
			{
				AssertEquals(true, userControl.SecondaryTariffsGrid.GetColumnStyle("JLT_TypeDescription").IsReadOnly);
				AssertEquals(true, userControl.SecondaryTariffsGrid.GetColumnStyle("JLT_TariffDesc").IsReadOnly);
			}
		}
	}
}
