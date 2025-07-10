using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution.ExportToExcel;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Excel;
using Moq;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ProfitShareRedistributionExcelExporterTest : TestCaseWithFactory
	{
		public void TestExportToExcel()
		{
			var notificationsMock = new Mock<IExcelExporterNotifications>();
			var notifications = notificationsMock.Object;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_IsCreditor = true;

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_IsCreditor = true;

			var shipment = CreateShipment("SHP0001", "AUSYD", "NZAKL", "AIR", 400, 6700, "KG", 2.8, "M3", pickupAgent, deliveryAgent, new[] { consol });

			var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
			profitShareRedistribution.PSR_TotalProfitShare = 1300;
			profitShareRedistribution.PSR_RX_NKCurrency = "AUD";

			var consolProfitShare = profitShareRedistribution.ConsolProfitShares.AddNew();
			consolProfitShare.CPS_JK = consol.PK;
			consolProfitShare.CPS_TotalConsolProfitShare = 1500;
			consolProfitShare.CPS_RedistributedConsolProfitShare = 1300;
			consolProfitShare.CPS_RX_NKCurrency = "AUD";

			var shipmentProfitShare = consolProfitShare.ShipmentProfitShares.AddNew();
			shipmentProfitShare.PSS_JS = shipment.PK;
			shipmentProfitShare.PSS_PickupAgentShare = 700;
			shipmentProfitShare.PSS_DeliveryAgentShare = 600;
			Factory.Save();

			var redistribution = Factory.Load<ForwardingProfitShareRedistribution>(profitShareRedistribution.PK);

			var exporter = new ProfitShareRedistributionExcelExporter(redistribution, notifications);
			exporter.ExportToExcelAndOpen();

			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				try
				{
					excelInterface.LoadExcelFile(exporter.LatestExportedFileNameForTest);
					using (var consolWorkSheet = excelInterface.WorkSheets[0])
					using (var shipmentWorkSheet = excelInterface.WorkSheets[1])
					{
						AssertConsolSheetColumnHeaders(consolWorkSheet);
						AssertConsolSheetRecordValues(consolWorkSheet, consol, 1500, 1300, 1);

						AssertShipmentSheetColumnHeaders(shipmentWorkSheet);
						AssertShipmentSheetRecordValues(shipmentWorkSheet, shipment, 700, 600, 1);
					}
				}
				finally
				{
					if (!string.IsNullOrEmpty(exporter.LatestExportedFileNameForTest) && File.Exists(exporter.LatestExportedFileNameForTest))
					{
						File.Delete(exporter.LatestExportedFileNameForTest);
					}
				}
			}
		}

		ForwardingShipment CreateShipment(
			string shipmentNum,
			string origin = null,
			string destination = null,
			string transportMode = "AIR",
			ZDecimal? chargeable = null,
			ZDecimal? weight = null,
			string weightUnit = "KG",
			ZDecimal? volume = null,
			string volumeUnit = "M3",
			OrgHeader pickupAgent = null,
			OrgHeader deliveryAgent = null,
			ForwardingConsol[] forwardingConsols = null)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment(shipmentNum, origin, destination, transportMode: transportMode);

			if (chargeable.HasValue)
			{
				shipment.JS_ActualChargeable = chargeable.Value;
			}

			if (weight.HasValue)
			{
				shipment.JS_ActualWeight = weight.Value;
				shipment.JS_UnitOfWeight = weightUnit;
			}

			if (volume.HasValue)
			{
				shipment.JS_ActualVolume = volume.Value;
				shipment.JS_UnitOfVolume = volumeUnit;
			}

			if (pickupAgent != null)
			{
				pickupAgent.OH_IsCreditor = true;
				shipment.PickupAgentPK = pickupAgent.PK;
			}

			if (deliveryAgent != null)
			{
				deliveryAgent.OH_IsCreditor = true;
				shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			}

			if (forwardingConsols != null)
			{
				shipment.Consols.AddRange(forwardingConsols);
			}

			return shipment;
		}

		void AssertShipmentSheetColumnHeaders(IExcelWorkSheet workSheet)
		{
			var dummyShipment = Factory.New<ForwardingShipment>();

			AssertEquals(14, workSheet.ColumnCount);
			AssertEquals(dummyShipment.JS_UniqueConsignRefInfo.HumanReadableName, workSheet[0, 0]);
			AssertEquals(dummyShipment.JS_RL_NKOriginInfo.HumanReadableName, workSheet[0, 1]);
			AssertEquals(dummyShipment.JS_RL_NKDestinationInfo.HumanReadableName, workSheet[0, 2]);
			AssertEquals(dummyShipment.JS_HouseBillInfo.HumanReadableName, workSheet[0, 3]);
			AssertEquals(dummyShipment.JS_OuterPacksInfo.HumanReadableName, workSheet[0, 4]);
			AssertEquals(dummyShipment.JS_ActualWeightInfo.HumanReadableName, workSheet[0, 5]);
			AssertEquals(dummyShipment.JS_UnitOfWeightInfo.HumanReadableName, workSheet[0, 6]);
			AssertEquals(dummyShipment.JS_ActualVolumeInfo.HumanReadableName, workSheet[0, 7]);
			AssertEquals(dummyShipment.JS_UnitOfVolumeInfo.HumanReadableName, workSheet[0, 8]);
			AssertEquals(dummyShipment.JS_E_DEPInfo.HumanReadableName, workSheet[0, 9]);
			AssertEquals(dummyShipment.JS_E_ARVInfo.HumanReadableName, workSheet[0, 10]);
			AssertEquals(dummyShipment.JS_JK_ConsolIDInfo.HumanReadableName, workSheet[0, 11]);
			AssertEquals("Pickup Agent Profit Amount", workSheet[0, 12]);
			AssertEquals("Delivery Agent Profit Amount", workSheet[0, 13]);
		}

		void AssertShipmentSheetRecordValues(IExcelWorkSheet workSheet, ForwardingShipment shipment, decimal pickupAgentShare, decimal deliveryAgentShare, int record)
		{
			AssertEquals(14, workSheet.ColumnCount);
			AssertEquals(shipment.JS_UniqueConsignRef.ToString(), workSheet[record, 0]);
			AssertEquals(shipment.JS_RL_NKOrigin.ToString(), workSheet[record, 1]);
			AssertEquals(shipment.JS_RL_NKDestination.ToString(), workSheet[record, 2]);
			AssertEquals(shipment.JS_HouseBill.ToString(), workSheet[record, 3]);
			AssertEquals(shipment.JS_OuterPacks.ToString(), workSheet[record, 4]);
			AssertEquals(shipment.JS_ActualWeight.ToString(), workSheet[record, 5]);
			AssertEquals(shipment.JS_UnitOfWeight.ToString(), workSheet[record, 6]);
			AssertEquals(shipment.JS_ActualVolume.ToString(), workSheet[record, 7]);
			AssertEquals(shipment.JS_UnitOfVolume.ToString(), workSheet[record, 8]);
			AssertEquals(shipment.JS_E_DEP.ToString(), workSheet[record, 9]);
			AssertEquals(shipment.JS_E_ARV.ToString(), workSheet[record, 10]);
			AssertEquals(shipment.JS_JK_ConsolID.ToString(), workSheet[record, 11]);
			AssertEquals(pickupAgentShare.ToString(), workSheet[record, 12]);
			AssertEquals(deliveryAgentShare.ToString(), workSheet[record, 13]);
		}

		void AssertConsolSheetColumnHeaders(IExcelWorkSheet workSheet)
		{
			var dummyConsol = Factory.New<ForwardingConsol>();

			AssertEquals(24, workSheet.ColumnCount);

			AssertEquals(dummyConsol.JK_UniqueConsignRefInfo.HumanReadableName, workSheet[0, 0]);
			AssertEquals(dummyConsol.JK_TransportModeInfo.HumanReadableName, workSheet[0, 1]);
			AssertEquals(dummyConsol.JK_ConsolModeInfo.HumanReadableName, workSheet[0, 2]);
			AssertEquals(dummyConsol.JK_MasterBillNumInfo.HumanReadableName, workSheet[0, 3]);
			AssertEquals(dummyConsol.JK_RL_NKLoadPortInfo.HumanReadableName, workSheet[0, 4]);
			AssertEquals(dummyConsol.JK_RL_NKDischargePortInfo.HumanReadableName, workSheet[0, 5]);
			AssertEquals(dummyConsol.JK_JX_JA_E_DEPInfo.HumanReadableName, workSheet[0, 6]);
			AssertEquals(dummyConsol.JK_JX_JB_E_ARVInfo.HumanReadableName, workSheet[0, 7]);
			AssertEquals(dummyConsol.JK_JX_JV_VoyageFlightInfo.HumanReadableName, workSheet[0, 8]);
			AssertEquals(dummyConsol.JK_ConsolChargeableRateInfo.HumanReadableName, workSheet[0, 9]);
			AssertEquals(dummyConsol.JK_TotalShipmentWeightUnitInfo.HumanReadableName, workSheet[0, 10]);
			AssertEquals(dummyConsol.JK_TotalShipmentVolumeUnitInfo.HumanReadableName, workSheet[0, 11]);
			AssertEquals(dummyConsol.JK_TotalShipmentChargeableInfo.HumanReadableName, workSheet[0, 12]);
			AssertEquals(dummyConsol.JK_Calc_TotalShipmentChargeableUnitInfo.HumanReadableName, workSheet[0, 13]);
			AssertEquals(dummyConsol.JK_CorrectedConsolWeightInfo.HumanReadableName, workSheet[0, 14]);
			AssertEquals(dummyConsol.JK_CorrectedConsolWeightUnitInfo.HumanReadableName, workSheet[0, 15]);
			AssertEquals(dummyConsol.JK_CorrectedConsolVolumeInfo.HumanReadableName, workSheet[0, 16]);
			AssertEquals(dummyConsol.JK_CorrectedConsolVolumeUnitInfo.HumanReadableName, workSheet[0, 17]);
			AssertEquals(dummyConsol.JK_SendingForwarderHandlingTypeInfo.HumanReadableName, workSheet[0, 18]);
			AssertEquals(dummyConsol.JK_ReceivingForwarderHandlingTypeInfo.HumanReadableName, workSheet[0, 19]);
			AssertEquals(dummyConsol.JK_Calc_SendingAgentCodeInfo.HumanReadableName, workSheet[0, 20]);
			AssertEquals(dummyConsol.JK_Calc_ReceivingAgentCodeInfo.HumanReadableName, workSheet[0, 21]);
			AssertEquals("Total Profit Share Amount", workSheet[0, 22]);
			AssertEquals("Redistributed Profit Share Amount", workSheet[0, 23]);
		}

		void AssertConsolSheetRecordValues(IExcelWorkSheet workSheet, ForwardingConsol consol, decimal totalProfitShareAmount, decimal profitShareAmount, int record)
		{
			AssertEquals(24, workSheet.ColumnCount);

			AssertEquals(consol.JK_UniqueConsignRef.ToString(), workSheet[record, 0]);
			AssertEquals(consol.JK_TransportMode.ToString(), workSheet[record, 1]);
			AssertEquals(consol.JK_ConsolMode.ToString(), workSheet[record, 2]);
			AssertEquals(consol.JK_MasterBillNum.ToString(), workSheet[record, 3]);
			AssertEquals(consol.JK_RL_NKLoadPort.ToString(), workSheet[record, 4]);
			AssertEquals(consol.JK_RL_NKDischargePort.ToString(), workSheet[record, 5]);
			AssertEquals(consol.JK_JX_JA_E_DEP.ToString(), workSheet[record, 6]);
			AssertEquals(consol.JK_JX_JB_E_ARV.ToString(), workSheet[record, 7]);
			AssertEquals(consol.JK_JX_JV_VoyageFlight.ToString(), workSheet[record, 8]);
			AssertEquals(consol.JK_ConsolChargeableRate.ToString(), workSheet[record, 9]);
			AssertEquals(consol.JK_TotalShipmentWeightUnit.ToString(), workSheet[record, 10]);
			AssertEquals(consol.JK_TotalShipmentVolumeUnit.ToString(), workSheet[record, 11]);
			AssertEquals(consol.JK_TotalShipmentChargeable.ToString(), workSheet[record, 12]);
			AssertEquals(consol.JK_Calc_TotalShipmentChargeableUnit.ToString(), workSheet[record, 13]);
			AssertEquals(consol.JK_CorrectedConsolWeight.ToString(), workSheet[record, 14]);
			AssertEquals(consol.JK_CorrectedConsolWeightUnit.ToString(), workSheet[record, 15]);
			AssertEquals(consol.JK_CorrectedConsolVolume.ToString(), workSheet[record, 16]);
			AssertEquals(consol.JK_CorrectedConsolVolumeUnit.ToString(), workSheet[record, 17]);
			AssertEquals(consol.JK_SendingForwarderHandlingType.ToString(), workSheet[record, 18]);
			AssertEquals(consol.JK_ReceivingForwarderHandlingType.ToString(), workSheet[record, 19]);
			AssertEquals(consol.JK_Calc_SendingAgentCode.ToString(), workSheet[record, 20]);
			AssertEquals(consol.JK_Calc_ReceivingAgentCode.ToString(), workSheet[record, 21]);
			AssertEquals(totalProfitShareAmount.ToString(), workSheet[record, 22]);
			AssertEquals(profitShareAmount.ToString(), workSheet[record, 23]);
		}
	}
}
