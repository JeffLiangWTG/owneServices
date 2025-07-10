using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class NctsMessagingMenuTest : TestCaseWithFactory
	{
		public void TestMenuSendDepartureMessageStatus()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				menu.NctsHeader = header;

				var sendDeparture = menu.MenuItems.FindByText("Send Departure Message");

				CombineAssertions("Send Departure Message MenuItem", () =>
				{
					AssertEquals("BM_CustomsStatus is Empty", true, sendDeparture.Visible);

					menu.NctsHeader.MovementHeader.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.CTR;
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("BM_CustomsStatus is CTR", false, sendDeparture.Visible);

					menu.NctsHeader.MovementHeader.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("BM_CustomsStatus is DRJ", true, sendDeparture.Visible);
				});
			}
		}
	}
}
