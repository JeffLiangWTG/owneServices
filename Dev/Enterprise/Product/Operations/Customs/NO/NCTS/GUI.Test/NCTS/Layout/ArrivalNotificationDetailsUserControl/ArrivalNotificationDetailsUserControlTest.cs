using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalNotificationDetailsUserControl))]
sealed class ArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using var userControl = new ArrivalNotificationDetailsUserControl();
		_ = userControl.AssertContainsControl<ZArchitecture.ZTextBox>(nameof(userControl.GoodsRegistrationNumberTextBox),
				x => x.WithBindTo(nameof(NctsHeader.ArrivalMovementHeader) + "." + nameof(NctsArrivalMovementHeader.GoodsRegistrationNumber)));
	}
}
