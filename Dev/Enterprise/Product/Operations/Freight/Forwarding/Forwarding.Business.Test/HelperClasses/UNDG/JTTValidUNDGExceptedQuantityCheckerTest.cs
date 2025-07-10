using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Freight.Forwarding.Business.ExceptedQuantityUtilities;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class JTTValidUNDGExceptedQuantityCheckerTest : TestCaseWithFactory
	{
		public void TestDoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";
			substance.JTT_ExceptedQuantityCode = "E1";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var leg = consol.Transports.AddNew();
			leg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew() as ForwardingUNDGDataItem;
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGVolume = 0.5;
			undgDataItem.DI_UnitOfVolume = "L";

			var isValueExceeded = JTTValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(
				undgDataItem, UNDGPackType.SingleUNDGPack, ExceptedQuantityMeasurementType.Volume);
			AssertEquals("Dangerous Goods does not exceed value", isValueExceeded, false);

			undgDataItem.DI_DGVolume = 3;

			isValueExceeded = JTTValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(
				undgDataItem, UNDGPackType.SingleUNDGPack, ExceptedQuantityMeasurementType.Volume);
			AssertEquals("Dangerous Goods does exceed value", isValueExceeded, true);
		}

		public void TestExceptedQuantityExceededForMostRestrictiveCode()
		{
			var substance1 = Factory.New<UNDGSubstanceJTT>();
			substance1.JTT_UNNO = "1234";
			substance1.JTT_ExceptedQuantityCode = "E1";

			var substance2 = Factory.New<UNDGSubstanceJTT>();
			substance2.JTT_UNNO = "2354";
			substance2.JTT_ExceptedQuantityCode = "E5";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var leg = consol.Transports.AddNew();
			leg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem1 = packline.UNDGs.AddNew() as ForwardingUNDGDataItem;
			undgDataItem1.DI_DG = substance1.PK;
			undgDataItem1.DI_DGWeight = 0.5;
			undgDataItem1.DI_UnitOfWeight = "KG";

			var undgDataItem2 = packline.UNDGs.AddNew() as ForwardingUNDGDataItem;
			undgDataItem2.DI_DG = substance2.PK;

			var isValueExceeded = JTTValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(
				undgDataItem1, UNDGPackType.MultiUNDGPack, ExceptedQuantityMeasurementType.Weight);
			AssertEquals("Dangerous Goods does exceed value due to undgDataItem2 being E5 code", isValueExceeded, true);
		}
	}
}
