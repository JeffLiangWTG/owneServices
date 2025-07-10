using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalMovementForm))]
sealed class Phase5ArrivalMovementFormTest : EU.NCTS.GUI.Testing.Phase5ArrivalMovementFormAbstractTest<NctsHeader>
{
	public void TestMessagesUserControlType()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

		using (var form = new Phase5ArrivalMovementForm(nctsHeader))
		{
			var tabPage = form.FindSingle<ZTabPage>("MessagesTabPage");
			tabPage.Show();
			var messagesTabDynamicUserControl = tabPage.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
			AssertEquals(typeof(MessagesTabUserControl), messagesTabDynamicUserControl.UserControlType);
		}
	}
}
