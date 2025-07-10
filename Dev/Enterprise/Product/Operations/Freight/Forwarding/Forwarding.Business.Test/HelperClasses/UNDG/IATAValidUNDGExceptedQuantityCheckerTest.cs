using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class IATAValidUNDGExceptedQuantityCheckerTest : TestCaseWithFactory
	{
		public void TestDoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1234";
			substance.DG_ExceptedQuantityCode = "E1";
			substance.DG_CargoMaxAmtUQ = "L";

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var leg = consol.Transports.AddNew();
			leg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGVolume = 0.5;
			undgDataItem.DI_UnitOfVolume = "L";

			var isValueExceeded = ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(undgDataItem, UNDGPackType.SingleUNDGPack);
			AssertEquals("Dangerous Goods does not exceed value", isValueExceeded, false);

			undgDataItem.DI_DGVolume = 3;

			isValueExceeded = ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(undgDataItem, UNDGPackType.SingleUNDGPack);
			AssertEquals("Dangerous Goods does exceed value", isValueExceeded, true);
		}
	}
}
