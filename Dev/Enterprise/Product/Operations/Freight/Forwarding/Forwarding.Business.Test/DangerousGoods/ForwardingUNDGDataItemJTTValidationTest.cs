using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGDataItemJTTValidationTest : TestCaseWithFactory
	{
		#region Pack Line

		public void TestCheckDI_DGVolumeForJTTSubstances_InnerPack_E1() =>
			CheckWeightOrVolumeForPackLineSubstances("E1", "inner", "Volume", 0.01, 0.04);

		public void TestCheckDI_DGVolumeForJTTSubstances_InnerPack_E2() =>
			CheckWeightOrVolumeForPackLineSubstances("E2", "inner", "Volume", 0.01, 0.04);

		public void TestCheckDI_DGVolumeForJTTSubstances_InnerPack_E3() =>
			CheckWeightOrVolumeForPackLineSubstances("E3", "inner", "Volume", 0.01, 0.04);

		public void TestCheckDI_DGVolumeForJTTSubstances_InnerPack_E4() =>
			CheckWeightOrVolumeForPackLineSubstances("E4", "inner", "Volume", 0.0005, 0.005);

		public void TestCheckDI_DGVolumeForJTTSubstances_InnerPack_E5() =>
			CheckWeightOrVolumeForPackLineSubstances("E5", "inner", "Volume", 0.0005, 0.005);

		public void TestCheckDI_DGWeightForJTTSubstances_InnerPack_E1() =>
			CheckWeightOrVolumeForPackLineSubstances("E1", "inner", "Weight", 0.01, 0.04);

		public void TestCheckDI_DGWeightForJTTSubstances_InnerPack_E2() =>
			CheckWeightOrVolumeForPackLineSubstances("E2", "inner", "Weight", 0.01, 0.04);

		public void TestCheckDI_DGWeightForJTTSubstances_InnerPack_E3() =>
			CheckWeightOrVolumeForPackLineSubstances("E3", "inner", "Weight", 0.01, 0.04);

		public void TestCheckDI_DGWeightForJTTSubstances_InnerPack_E4() =>
			CheckWeightOrVolumeForPackLineSubstances("E4", "inner", "Weight", 0.0005, 0.005);

		public void TestCheckDI_DGWeightForJTTSubstances_InnerPack_E5() =>
			CheckWeightOrVolumeForPackLineSubstances("E5", "inner", "Weight", 0.0005, 0.005);

		public void TestCheckDI_DGVolumeForJTTSubstances_OuterPack_E1() =>
			CheckWeightOrVolumeForPackLineSubstances("E1", "outer", "Volume", 0.5, 2);

		public void TestCheckDI_DGVolumeForJTTSubstances_OuterPack_E2() =>
			CheckWeightOrVolumeForPackLineSubstances("E2", "outer", "Volume", 0.3, 0.7);

		public void TestCheckDI_DGVolumeForJTTSubstances_OuterPack_E3() =>
			CheckWeightOrVolumeForPackLineSubstances("E3", "outer", "Volume", 0.1, 4);

		public void TestCheckDI_DGVolumeForJTTSubstances_OuterPack_E4() =>
			CheckWeightOrVolumeForPackLineSubstances("E4", "outer", "Volume", 0.3, 0.6);

		public void TestCheckDI_DGVolumeForJTTSubstances_OuterPack_E5() =>
			CheckWeightOrVolumeForPackLineSubstances("E5", "outer", "Volume", 0.2, 0.4);

		public void TestCheckDI_DGWeightForJTTSubstances_OuterPack_E1() =>
			CheckWeightOrVolumeForPackLineSubstances("E1", "outer", "Weight", 0.5, 2);

		public void TestCheckDI_DGWeightForJTTSubstances_OuterPack_E2() =>
			CheckWeightOrVolumeForPackLineSubstances("E2", "outer", "Weight", 0.3, 0.7);

		public void TestCheckDI_DGWeightForJTTSubstances_OuterPack_E3() =>
			CheckWeightOrVolumeForPackLineSubstances("E3", "outer", "Weight", 0.1, 0.4);

		public void TestCheckDI_DGWeightForJTTSubstances_OuterPack_E4() =>
			CheckWeightOrVolumeForPackLineSubstances("E4", "outer", "Weight", 0.3, 0.6);

		public void TestCheckDI_DGWeightForJTTSubstances_OuterPack_E5() =>
			CheckWeightOrVolumeForPackLineSubstances("E5", "outer", "Weight", 0.2, 0.4);

		void CheckWeightOrVolumeForPackLineSubstances(string exceptedQuantity, string innerOrOuter, string weightOrVolume, double amountBefore, double amountAfter)
		{
			const string exceedMaximumMessage =
				"This value exceeds the maximum quantity per pack so cannot be transported in Excepted Quantities, per JT/T 617.";

			var substance = CreateJTTSubstanceWithExceptedCode("1234", exceptedQuantity);

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var leg = consol.Transports.AddNew();
			leg.JW_TransportMode = Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var packline = shipment.OuterPackLines.AddNew();
			if (innerOrOuter == "inner")
			{
				packline.UNDGs.AddNew();
			}

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			var exceptedMessage = "Excepted " + weightOrVolume + " exceeded for " + exceptedQuantity;

			switch (weightOrVolume)
			{
				case "Volume":
					undgDataItem.DI_DGVolume = amountBefore;
					undgDataItem.DI_UnitOfVolume = "L";
					undgDataItem.Validation.ValidateDI_DGVolume();

					AssertNoWarning(exceptedMessage, undgDataItem.DI_DGVolumeInfo, exceedMaximumMessage);

					undgDataItem.DI_DGVolume = amountAfter;

					AssertHasWarning(exceptedMessage, undgDataItem.DI_DGVolumeInfo, exceedMaximumMessage);
					break;

				case "Weight":
					undgDataItem.DI_DGWeight = amountBefore;
					undgDataItem.DI_UnitOfWeight = "KG";
					undgDataItem.Validation.ValidateDI_DGWeight();

					AssertNoWarning(exceptedMessage, undgDataItem.DI_DGWeightInfo, exceedMaximumMessage);

					undgDataItem.DI_DGWeight = amountAfter;

					AssertHasWarning(exceptedMessage, undgDataItem.DI_DGWeightInfo, exceedMaximumMessage);
					break;
			}
		}

		UNDGSubstanceJTT CreateJTTSubstanceWithExceptedCode(string unno, string exceptedQuantity)
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = unno;
			substance.JTT_ExceptedQuantityCode = exceptedQuantity;

			return substance;
		}

		#endregion

		#region Company Permit

		public void TestCheckDI_DGForJTTSubstances_PickupCompanyHasPermit() => CheckCompanyHasPermit("Pickup");

		public void TestCheckDI_DGForJTTSubstances_DeliveryCompanyHasPermit() => CheckCompanyHasPermit("Delivery");

		void CheckCompanyHasPermit(string pickupOrDelivery)
		{
			var company = Factory.NewWithValidTestData<OrgHeader>();
			company.OH_Code = "PIK";
			var companyAddress = company.Addresses.AddNew();
			companyAddress.OA_Address1 = "123 Somewhere";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			switch (pickupOrDelivery)
			{
				case "Pickup":
					shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = companyAddress.PK;
					break;

				case "Delivery":
					shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = companyAddress.PK;
					break;
			}

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "CNNBO";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_TransportMode = Constants.TransportModes.Road;

			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";

			Factory.Save();

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			var permitMessage = "A valid road transport permit is not attached to the organization profile for the selected " +
								pickupOrDelivery + " Transport Company.";

			AssertHasWarning("No permit edocs attached - should show warning", undgDataItem.DI_DGInfo, permitMessage);

			company.DocManagerInfo().AddFileOrDocument(new byte[] { 5, 6, 7 }, "permit.pdf", Constants.RefDocTypes.Permit);

			Factory.Save();

			undgDataItem.Validation.ValidateDI_DG();

			var authorisationMessage = "Ensure the selected " + pickupOrDelivery +
									" Transport Company has the required authorizations to transport the specified goods.";

			AssertHasWarning("Permit document attached - warning for authorization", undgDataItem.DI_DGInfo, authorisationMessage);
		}

		#endregion

		#region China Road Leg

		const string ChinaRoadLegMessage =
			"JT/T 617 Substances are only allowed for shipments with domestic or international movements traveling by Road to/from Chinese territories.";

		public void TestCheckDI_DGForJTTSubstances_ShipmentDoesNotHaveChinaRoadLeg()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var leg = shipment.Transports.AddNew();
			leg.JW_TransportMode = Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			AssertHasError("Shipment has Sea leg that is not China based - expect error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);
		}

		public void TestCheckDI_DGForJTTSubstances_ShipmentHasChinaRoadLeg()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var leg = shipment.Transports.AddNew();
			leg.JW_TransportMode = Constants.TransportModes.Road;
			leg.JW_RL_NKLoadPort = "CNNBO";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			AssertNoError("Shipment has Road leg that is loading in China, no error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);

			leg.JW_RL_NKLoadPort = "USLAX";
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError("Load changed to US - expect error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);
		}

		public void TestCheckDI_DGForJTTSubstances_ConsolDoesNotHaveChinaRoadLeg()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var leg = consol.Transports.AddNew();
			leg.JW_TransportMode = Constants.TransportModes.Sea;
			leg.JW_RL_NKLoadPort = "AUSYD";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			AssertHasError("Consol has Sea leg that is not China based - expect error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);
		}

		public void TestCheckDI_DGForJTTSubstances_ConsolHasChinaRoadLeg()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var leg = shipment.Transports.AddNew();
			leg.JW_TransportMode = Constants.TransportModes.Road;
			leg.JW_RL_NKLoadPort = "CNNBO";
			leg.JW_RL_NKDiscPort = "NZAKL";

			var undgDataItem = shipment.OuterPackLines.AddNew().UNDGs.AddNew();
			undgDataItem.DI_DG = substance.PK;

			AssertNoError("Consol has Road leg that is loading in China, no error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);

			leg.JW_RL_NKLoadPort = "USLAX";
			undgDataItem.Validation.ValidateDI_DG();

			AssertHasError("Load changed to US - expect error",
				undgDataItem.DI_DGInfo,
				ChinaRoadLegMessage);
		}

		#endregion
	}
}
