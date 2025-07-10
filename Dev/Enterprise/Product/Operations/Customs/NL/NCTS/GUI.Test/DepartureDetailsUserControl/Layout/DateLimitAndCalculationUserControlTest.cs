using CargoWise.Windows.UI;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

sealed class DateLimitAndCalculationUserControlTest : TestCase
{
	public void TestDateLimitDateEdit()
	{
		var dateLimitDateEdit = control.DateLimitDateEdit;
		CombineAssertions(() =>
		{
			AssertType<ZDateEdit>("Type", dateLimitDateEdit);
			AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_ExportDate), dateLimitDateEdit.GetBindingMember());
		});
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

	protected override void SetUp()
	{
		base.SetUp();
		control = new DateLimitAndCalculationUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	DateLimitAndCalculationUserControl control;
}
