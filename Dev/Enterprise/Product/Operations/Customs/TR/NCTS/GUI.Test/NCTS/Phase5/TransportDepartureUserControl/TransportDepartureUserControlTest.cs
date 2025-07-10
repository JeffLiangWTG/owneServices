using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class TransportDepartureUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestTankerStatusDropEdit()
		{
			var tankerStatusDropEdit = control.TankerStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", tankerStatusDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader.TankerStatus), tankerStatusDropEdit.GetBindingMember());
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDepartureUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		TransportDepartureUserControl control;
	}
}
