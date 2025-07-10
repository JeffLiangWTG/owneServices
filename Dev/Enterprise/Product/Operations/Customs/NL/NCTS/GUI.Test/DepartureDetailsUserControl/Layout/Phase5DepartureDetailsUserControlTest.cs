using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

sealed class Phase5DepartureDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
	}

	public void TestCALCalculationMethodDropEdit()
	{
		var calCalculationMethodDropEdit = control.CalCalculationMethodDropEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDropEdit>("Type", calCalculationMethodDropEdit);
			AssertEquals("GetBindingMember", nameof(NctsHeader.CALCalculationMethod), calCalculationMethodDropEdit.GetBindingMember());
		});
	}

	public void TestDateLimitAndCalculationUserControl()
	{
		AssertType<DateLimitAndCalculationUserControl>("Type", control.DateLimitAndCalculationUserControl);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new Phase5DepartureDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	Phase5DepartureDetailsUserControl control;
}
