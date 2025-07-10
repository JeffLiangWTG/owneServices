using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class Phase5ArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new Phase5ArrivalNotificationDetailsUserControl();
		AssertEquals(typeof(NctsHeader), control.DataSourceType);
	}

	public void TestRepresentativeTraderGuidFindBox()
	{
		using var control = new Phase5ArrivalNotificationDetailsUserControl();
		CombineAssertions(() =>
		{
			var representativeTraderGuidFindBox = control.RepresentativeTraderGuidFindBox;
			AssertType<ZGuidFindBox>("Type", representativeTraderGuidFindBox);
			AssertEquals("BindingMember", nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.RepresentativeTrader), representativeTraderGuidFindBox.GetBindingMember());
		});
	}
}

