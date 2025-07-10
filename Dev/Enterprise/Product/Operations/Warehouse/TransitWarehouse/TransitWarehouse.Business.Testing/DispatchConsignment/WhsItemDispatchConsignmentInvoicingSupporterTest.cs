using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemConsignmentInvoicingSupporter<WhsItemDispatchConsignment>))]
	public class WhsItemDispatchConsignmentInvoicingSupporterTest : TransitJobInvoicingSupporterTest<WhsItemDispatchConsignment>
	{
		protected override IJobInvoicingSupporter GetInvoicingSupporter(WhsItemDispatchConsignment consignment) => new WhsItemConsignmentInvoicingSupporter<WhsItemDispatchConsignment>(consignment);

		void createTestConsignment(int index = 1, decimal volume = 0.0M, string volumeUQ = "M3", decimal weight = 0.0M, string weightUQ = "KG", bool hasHU = false)
		{
			BusinessObject.WDC_JobID = $"DCN{index}";
			BusinessObject.WDC_ConsignmentID = $"DCN{index}";
			BusinessObject.WDC_WW_Warehouse = Warehouse.PK;
			BusinessObject.WDC_RL_NKDestination = "AUSYD";
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment($"RCN{index}", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit($"RTU{index}", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList($"DLL{index}", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit($"DTU{index}", Warehouse.PK);
			if (hasHU)
			{
				var handlingUnit = Helper.CreatePackageHandlingUnit();
				var handlingUnitPackage = Helper.CreateHandlingUnitPackage($"HU{index}", handlingUnit, rtu);
				var childPackageState = Helper.CreatePackageState(rcn, 1, "PKG", $"PC{index}", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, BusinessObject, dtu, dll, volume: volume, volumeUQ: volumeUQ, weight: weight, weightUQ: weightUQ);
				childPackageState.Package.KP_KP_TopHandlingUnitPackage = handlingUnitPackage.WPS_KP_Package;
				childPackageState.WPS_WL_LastLocation = stageLocation.PK;
				childPackageState.WPS_WL_ReceiveLocation = stageLocation.PK;

				Helper.DisableTopLevelHUFKForTest(TestConnection);
				Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC");
			}
			else
			{
				var packageState = Helper.CreatePackageState(rcn, 1, "PKG", $"P{index}", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, BusinessObject, dtu, dll, volume: volume, volumeUQ: volumeUQ, weight: weight, weightUQ: weightUQ);
				packageState.WPS_WL_LastLocation = stageLocation.PK;
				packageState.WPS_WL_ReceiveLocation = stageLocation.PK;
			}

			Factory.Save();
		}

		#region TestHouseBillNumber

		public void TestHouseBillNumber()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);

			createTestConsignment();

			AssertEquals("DCN1", invoicingSupporter.HouseBillNumber);

			BusinessObject.WDC_HouseBillNumber = "HSB Test";

			AssertEquals("HSB Test", invoicingSupporter.HouseBillNumber);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertNull(invoicingSupporter.Destination);

			createTestConsignment();

			AssertEquals("AUSYD", invoicingSupporter.Destination.Code);
		}

		#endregion

		#region TestActualVolumeAndUnit

		public void TestActualVolumeAndUnit()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0m, invoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, invoicingSupporter.ActualVolumeUnit);

			createTestConsignment(1, 1m, Constants.Volume.CubicMetres);
			createTestConsignment(2, 1000m, Constants.Volume.CubicDecimetres);
			createTestConsignment(3, 1m, Constants.Volume.CubicMetres, hasHU: true);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(3m, invoicingSupporter.ActualVolume);
			AssertEquals(Constants.Volume.CubicMetres, invoicingSupporter.ActualVolumeUnit);
		}

		#endregion

		#region TestActualWeightAndUnit

		public void TestActualWeightAndUnit()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0m, invoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, invoicingSupporter.ActualWeightUnit);

			createTestConsignment(1, weight: 1m, weightUQ: Constants.Weight.Kilograms);
			createTestConsignment(2, weight: 1000m, weightUQ: Constants.Weight.Grams);
			createTestConsignment(3, weight: 1m, weightUQ: Constants.Weight.Kilograms, hasHU: true);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(3m, invoicingSupporter.ActualWeight);
			AssertEquals(Constants.Weight.Kilograms, invoicingSupporter.ActualWeightUnit);
		}

		#endregion

		#region TestOuterPackTotal

		public override void TestOuterPackTotal()
		{
			var invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(0, invoicingSupporter.OuterPackTotal);

			createTestConsignment(1);
			createTestConsignment(2);
			createTestConsignment(3, hasHU: true);

			invoicingSupporter = GetInvoicingSupporter(BusinessObject);
			AssertEquals(3, invoicingSupporter.OuterPackTotal);
		}

		#endregion

		#region Implementation

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.TransitDispatch;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.WhsItemDispatchConsignmentAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.WhsItemDispatchConsignmentJobInvoicing;

		protected override bool ExpectedIncludeInConsolCosting => true;

		#endregion
	}
}
