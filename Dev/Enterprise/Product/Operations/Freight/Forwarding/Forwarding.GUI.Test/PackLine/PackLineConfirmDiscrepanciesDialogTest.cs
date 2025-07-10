using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class PackLineConfirmDiscrepanciesDialogTest : TestCaseWithFactory
	{
		public void TestAcceptDiscrepancyReadOnly()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();
			var addr1 = Factory.New<OrgAddress>();
			var addr2 = Factory.New<OrgAddress>();

			shipment.JS_OA_ExportReceivingDepot = addr1.PK;
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = addr1.PK;
			packLine2.JL_OA_LastKnownTransitWarehouseAddress = addr2.PK;
			packLine3.JL_OA_LastKnownTransitWarehouseAddress = addr1.PK;

			packLine1.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine3.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;

			AssertAcceptDiscrepancyPopup(packLine1, Moq.Times.Never());
			AssertAcceptDiscrepancyPopup(packLine2, Moq.Times.Once());
			AssertAcceptDiscrepancyPopup(packLine3, Moq.Times.Once());
		}

		public void TestAcceptDiscrepancyReadOnly_Empty()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();

			AssertEquals("pre: last known transit warehouse empty", null, packLine1.LastKnownTransitWarehouseAddress);
			AssertEquals("pre: origin transit warehouse empty", null, packLine1.JL_Calc_OriginTransitWarehouse);

			AssertAcceptDiscrepancyPopup(packLine1, Moq.Times.Once());
		}

		void AssertAcceptDiscrepancyPopup(PackLine packLine, Times moqTimes)
		{
			var mock = new Mock<PackLineConfirmDiscrepanciesDialogProvider> { CallBase = true };
			mock.Protected()
				.Setup<PackLineConfirmDiscrepancyAction>("ShowDialogAndGetResult", ItExpr.IsAny<PackLine>())
				.Returns(PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm);

			using (var form = new PackLineConfirmDiscrepanciesDialog(packLine))
			{
				mock.Object.ConditionallyShowDialogAndGetResult(packLine);
				mock.Protected().Verify("ShowDialogAndGetResult", moqTimes, ItExpr.IsAny<PackLine>());
				Assert(true); // to stop nunit complaining
			}
		}
	}
}
