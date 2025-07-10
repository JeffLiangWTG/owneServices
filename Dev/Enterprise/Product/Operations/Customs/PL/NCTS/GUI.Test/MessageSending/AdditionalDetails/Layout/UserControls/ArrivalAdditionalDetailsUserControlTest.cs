using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class ArrivalAdditionalDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new ArrivalAdditionalDetailsUserControl())
		{
			AssertEquals(typeof(MessageSendingObject), control.DataSourceType);
		}
	}

	public void TestTirPageNumberDropEdit()
	{
		using (var control = new ArrivalAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var tirPageNumber = control.TirPageNumberDropEdit;
				AssertType<ZDropEdit>("Type", tirPageNumber);
				AssertEquals("BindingMember", nameof(MessageSendingObject.TirPageNumber), tirPageNumber.GetBindingMember());
			});
		}
	}

	public void TestTirUnloadingNumberDropEdit()
	{
		using (var control = new ArrivalAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				var tirUnloadingNumber = control.TirUnloadingNumberDropEdit;
				AssertType<ZDropEdit>("Type", tirUnloadingNumber);
				AssertEquals("BindingMember", nameof(MessageSendingObject.TirUnloadingNumber), tirUnloadingNumber.GetBindingMember());
			});
		}
	}
}
