using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class ForwardingToTWULDIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region TestImportShipment_ReceiveAndDispatch_AIRTransportMode

		public void TestImportShipment_ReceiveAndDispatch_AIRTransportMode()
		{
			ImportShipment_ReceiveAndDispatch_ForTransportModes(Core.Constants.TransportModes.Air, TransportUnitTypes.ULD);
		}

		public void TestImportShipment_ReceiveAndDispatch_SEATransportMode()
		{
			ImportShipment_ReceiveAndDispatch_ForTransportModes(Core.Constants.TransportModes.Sea, TransportUnitTypes.Container);
		}

		public void TestImportShipment_ReceiveAndDispatch_ROATransportMode()
		{
			ImportShipment_ReceiveAndDispatch_ForTransportModes(TransportModes.Road, TransportUnitTypes.Vehicle);
		}

		void ImportShipment_ReceiveAndDispatch_ForTransportModes(string transportMode, string expectedUnitType)
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var loadingDevice = CreateContainer(consol, "loadingDevice", transportMode);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, loadingDevice);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			CombineAssertions($"Package extension, {(transportMode != Core.Constants.TransportModes.Road ? "package state," : string.Empty)} package and container records must be created correctly for RTU and DTU.",
			() =>
			{
				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				var rtuForULD = newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
				AssertEquals($"Expected unit type must be {expectedUnitType}.", expectedUnitType, rtuForULD.WRH_UnitType);
				AssertNotNull("RTU must have a Package Extension.", rtuForULD.PackageExtension);
				AssertNotNull("RTU's package extention must have a Package.", rtuForULD.PackageExtension.Package);
				AssertNotNull("RTU's package extention's package must have a Container.", rtuForULD.PackageExtension.Package.Container);
				AssertEquals($"Expected pack type must be CNT.", PkgUnit.Container, rtuForULD.PackageExtension.Package.KP_F3_NKPackType);

				if (transportMode != Core.Constants.TransportModes.Road)
				{
					var packageStateForRTUPackageExtension = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtuForULD.PackageExtension.Package.PK));
					AssertNotNull($"Since transport Mode is {transportMode} RTU's Package must have a Package State.", packageStateForRTUPackageExtension);
				}

				var dtuForULD = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Single();
				AssertEquals($"Expected unit type must be {expectedUnitType}.", expectedUnitType, rtuForULD.WRH_UnitType);
				AssertNotNull("DTU must have a Package Extension.", dtuForULD.PackageExtension);
				AssertNotNull("DTU's package extention must have a Package.", dtuForULD.PackageExtension.Package);
				AssertNotNull("DTU's package extention's package must have a Container.", dtuForULD.PackageExtension.Package.Container);
				AssertEquals($"Expected pack type must be CNT.", PkgUnit.Container, rtuForULD.PackageExtension.Package.KP_F3_NKPackType);

				if (transportMode != Core.Constants.TransportModes.Road)
				{
					var packageStateForDTUPackageExtension = newBizOFactoryAfterRunningLogWalker.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtuForULD.PackageExtension.Package.PK));
					AssertNotNull($"Since transport Mode is {transportMode} DTU's Package must have a Package State.", packageStateForDTUPackageExtension);
				}
			});
		}

		#endregion

		#region TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes

		public void TestImportShipment_ReceiveAndDispatch_BreakBulk()
		{
			TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes(ContainerModes.BreakBulk);
		}

		public void TestImportShipment_ReceiveAndDispatch_Bulk()
		{
			TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes(ContainerModes.Bulk);
		}

		public void TestImportShipment_ReceiveAndDispatch_Liquid()
		{
			TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes(ContainerModes.Liquid);
		}

		public void TestImportShipment_ReceiveAndDispatch_RollOnRollOff()
		{
			TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes(ContainerModes.RollOnRollOff);
		}

		void TestImportShipment_ReceiveAndDispatch_UnSupportedContainerModes(string containerMode)
		{
			var testData = CreateTestDataForCombined("NZCHC");
			// workflow template
			//   BookingRequested event send Combined Instructions
			//   BookingConfirmed event send Dispatch Instructions
			CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(JobInvoicingConsumerTypes.Consol.Code, true);

			var consol = CreateConsol("MSB1", testData.vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			var loadingDevice = CreateContainer(consol, "loadingDevice", TransportModes.Sea);
			loadingDevice.JC_ContainerMode = containerMode;
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box, loadingDevice);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			CombineAssertions($"Package extension and package records must be created correctly for RTU and DTU.",
			() =>
			{
				var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };
				AssertEquals($"No RTU's must be created since we don't support Container Mode - {containerMode}.", 0, newBizOFactoryAfterRunningLogWalker.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Length);
				AssertEquals($"No DTU's must be created since we don't support Container Mode - {containerMode}.", 0, newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Length);
			});
		}

		#endregion
	}
}
