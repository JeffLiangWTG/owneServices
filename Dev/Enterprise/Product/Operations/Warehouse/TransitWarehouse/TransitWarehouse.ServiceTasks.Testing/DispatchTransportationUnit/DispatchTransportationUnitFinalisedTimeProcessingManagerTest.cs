using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transit.ServiceTasks.Testing
{
	class DispatchTransportationUnitFinalisedTimeProcessingManagerTest : WhsTransitTestCaseWithFactory
	{
		#region TestSetFinalisedTime

		[TestDate(2020, 9, 20, 11, 0, 0)]
		public void TestSetFinalisedTime()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

			var dispatchTransportationUnit1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1, dispatchLoadList: loadList);
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1, dispatchLoadList: loadList);

			var dispatchTransportationUnit2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var packageState3 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit2, dispatchLoadList: loadList);
			var packageState4 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit2, dispatchLoadList: loadList);

			var dispatchTransportationUnit3 = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);
			var packageState5 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit3, dispatchLoadList: loadList);

			packageState1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState3.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState4.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState5.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			dispatchTransportationUnit1.WDH_VehicleReference = "VEH1";
			dispatchTransportationUnit2.WDH_VehicleReference = "VEH2";
			dispatchTransportationUnit3.WDH_VehicleReference = "VEH3";
			dispatchTransportationUnit1.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0, TimeSpan.FromHours(8));
			dispatchTransportationUnit2.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0, TimeSpan.FromHours(11));
			dispatchTransportationUnit3.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0, TimeSpan.FromHours(8));
			dispatchTransportationUnit1.WDH_GateOutTime = dispatchTransportationUnit1.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0, TimeSpan.FromHours(8));
			dispatchTransportationUnit2.WDH_GateOutTime = dispatchTransportationUnit2.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0, TimeSpan.FromHours(11));
			dispatchTransportationUnit3.WDH_GateOutTime = dispatchTransportationUnit3.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0, TimeSpan.FromHours(8));
			dispatchTransportationUnit3.Finalise(new ZDateTimeOffset(2020, 9, 18, 20, 0, 0, TimeSpan.FromHours(8)), "Pre-Finalised");

			Factory.Save();

			var manager = new DispatchTransportationUnitFinalisedTimeProcessingManager(Logger);
			manager.SetFinalizedIfRequired();

			var log = Logger.ToString().Trim();

			AssertEquals(new ZDateTimeOffset(2020, 9, 19, 10, 0, 0, TimeSpan.FromHours(8)), dispatchTransportationUnit1.WDH_FinalisedTime);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState1.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState2.WPS_Status);

			AssertEquals(new ZDateTimeOffset(2020, 9, 19, 10, 0, 0, TimeSpan.FromHours(11)), dispatchTransportationUnit2.WDH_FinalisedTime);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState3.WPS_Status);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState4.WPS_Status);

			AssertEquals(new ZDateTimeOffset(2020, 9, 18, 20, 0, 0, TimeSpan.FromHours(8)), dispatchTransportationUnit3.WDH_FinalisedTime);

			dispatchTransportationUnit3.Finalise(new ZDateTimeOffset(2020, 9, 18, 20, 0, 0, TimeSpan.FromHours(8)), "Pre-Finalised");
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState5.WPS_Status);

			Assert(log.Contains(@"Information|Finalize Dispatch Transportation Units.
Found 2 Dispatch Transport Units to be Finalized:"));
			Assert(log.Contains("DTU DTU1 (VEH1) gated out at 18-Sep-20 10:00."));
			Assert(log.Contains("DTU DTU2 (VEH2) gated out at 18-Sep-20 10:00."));
		}

		[TestDate(2020, 9, 18, 12, 0, 0)]
		public void TestSetFinalisedTime_FinaliseStatusMessage_AutoFinaliseTimeIsOneHour()
		{
			TestSetFinalisedTime_FinaliseStatusMessageCore(1);
		}

		[TestDate(2020, 9, 19, 11, 0, 0)]
		public void TestSetFinalisedTime_FinaliseStatusMessage_AutoFinaliseTimeIsMoreThanOneHour()
		{
			TestSetFinalisedTime_FinaliseStatusMessageCore(24);
		}

		void TestSetFinalisedTime_FinaliseStatusMessageCore(int period)
		{
			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, period))
			{
				var warehouse = Helper.CreateTRWWarehouse();
				var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
				var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

				var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
				var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
				var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
				var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

				var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
				var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dtu, dispatchLoadList: loadList);
				packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

				dtu.WDH_VehicleReference = "VEH1";
				dtu.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0, TimeSpan.FromHours(8));
				dtu.WDH_GateOutTime = dtu.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0, TimeSpan.FromHours(8));

				Factory.Save();

				var manager = new DispatchTransportationUnitFinalisedTimeProcessingManager(Logger);
				manager.SetFinalizedIfRequired();

				var log = Logger.ToString().Trim();

				AssertEquals(false, dtu.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState.WPS_Status);

				AssertLog(dtu, $"DTU1|RES=Auto Finalised after {period} {(period > 1 ? "hours" : "hour")}|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState.Package, $"P1|FAC=CFS|TYP=Finalised|WHS=WHS", Events.ItemDocumentJobFinalised.Code);
			}
		}

		[TestDate(2020, 9, 19, 11, 0, 0)]
		public void TestSetFinalisedTime_FinaliseStatusMessage_UsingBranchRegistry()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, 8))
			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 24))
			{
				var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
				var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

				var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
				var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
				var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
				var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

				var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
				var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dtu, dispatchLoadList: loadList);
				packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

				dtu.WDH_VehicleReference = "VEH1";
				dtu.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0);
				dtu.WDH_GateOutTime = dtu.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 15, 0, 0);

				Factory.Save();

				var manager = new DispatchTransportationUnitFinalisedTimeProcessingManager(Logger);
				manager.SetFinalizedIfRequired();

				var log = Logger.ToString().Trim();

				AssertEquals(false, dtu.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState.WPS_Status);

				AssertLog(dtu, $"DTU1|RES=Auto Finalised after 8 hours|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState.Package, $"P1|FAC=CFS|TYP=Finalised|WHS=WHS", Events.ItemDocumentJobFinalised.Code);
			}
		}

		[TestDate(2020, 9, 19, 11, 0, 0)]
		public void TestSetFinalisedTime_FinaliseStatusMessage_MultipleWarehouse()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse("TR2");
			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, 8))
			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 24))
			{
				var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
				var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

				var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
				var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
				var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
				var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

				var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
				var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dtu, dispatchLoadList: loadList);
				packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

				dtu.WDH_VehicleReference = "VEH1";
				dtu.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0);
				dtu.WDH_GateOutTime = dtu.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 15, 0, 0);

				var row2 = Helper.CreateRowAndGenerateLocations(warehouse2, "Dock2", 2, 2);
				var location2 = row2.Locations.First(l => l.ToLocationString() == "Dock2-1-1");

				var receiveConsignment2 = Helper.CreateReceiveConsignment("RCN2", warehouse2.PK);
				var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse2.PK, location2.PK);
				var dispatchConsignment2 = Helper.CreateDispatchConsignment("DCN2", warehouse2.PK);
				var loadList2 = Helper.CreateDispatchLoadList("DL2", warehouse2.PK);

				var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse2.PK);
				var packageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit2, dispatchConsignment: dispatchConsignment2, dispatchUnit: dtu2, dispatchLoadList: loadList2);
				packageState2.WPS_WL_LastLocation = warehouse2.DefaultLocation.PK;

				dtu2.WDH_VehicleReference = "VEH2";
				dtu2.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0);
				dtu2.WDH_GateOutTime = dtu2.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0);

				Factory.Save();

				var manager = new DispatchTransportationUnitFinalisedTimeProcessingManager(Logger);
				manager.SetFinalizedIfRequired();

				var log = Logger.ToString().Trim();

				AssertEquals(false, dtu.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState.WPS_Status);
				AssertEquals(false, dtu2.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState2.WPS_Status);

				AssertLog(dtu, $"DTU1|RES=Auto Finalised after 8 hours|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState.Package, $"P1|FAC=CFS|TYP=Finalised|WHS=WHS", Events.ItemDocumentJobFinalised.Code);
				AssertLog(dtu2, $"DTU2|RES=Auto Finalised after 24 hours|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState2.Package, $"P2|FAC=CFS|TYP=Finalised|WHS=TR2", Events.ItemDocumentJobFinalised.Code);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[TestDate(2020, 9, 19, 11, 0, 0)]
		public void TestSetFinalisedTime_FinaliseStatusMessage_MultipleWarehouse_NoErrorInLogging()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse("TR2");
			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition", Guid.Empty, branchPK);

			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, 8))
			using (WarehouseDataRegistry.Instance.DepartedPackageAutoFinalizationDelay.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 24))
			{
				var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
				var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
				var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
				var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

				var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
				var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchUnit: dtu, dispatchLoadList: loadList);
				packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

				dtu.WDH_VehicleReference = "VEH1";
				dtu.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0);
				dtu.WDH_GateOutTime = dtu.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 15, 0, 0);

				var receiveConsignment2 = Helper.CreateReceiveConsignment("RCN2", warehouse2.PK);
				var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse2.PK, warehouse.DefaultInboundDockDoorLocation.PK);
				var dispatchConsignment2 = Helper.CreateDispatchConsignment("DCN2", warehouse2.PK);
				var loadList2 = Helper.CreateDispatchLoadList("DL2", warehouse2.PK);

				var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse2.PK);
				var packageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Departed, receiveUnit: receiveTransportationUnit2, dispatchConsignment: dispatchConsignment2, dispatchUnit: dtu2, dispatchLoadList: loadList2);
				packageState2.WPS_WL_LastLocation = warehouse2.DefaultLocation.PK;

				dtu2.WDH_VehicleReference = "VEH2";
				dtu2.WDH_GateInTime = new ZDateTimeOffset(2020, 9, 18, 9, 0, 0);
				dtu2.WDH_GateOutTime = dtu2.WDH_LoadCompleteTime = new ZDateTimeOffset(2020, 9, 18, 10, 0, 0);

				Factory.Save();

				var originUserContext = Env.CurrentUserContext;

				try
				{
					ErrorReporter.Clear();
					using (Env.Instance.TemporaryServiceTaskContext("DFT", canRunInAnyBranch: true))
					{
						var manager = new DispatchTransportationUnitFinalisedTimeProcessingManager(Logger);
						manager.SetFinalizedIfRequired();

						var log = Logger.ToString().Trim();
					}
				}
				finally
				{
					Env.SetUserContext(originUserContext);
					AssertNullOrEmpty("Should not report \"Direct access to Env.CurrentBranch is not allowed\" message.", ErrorReporter.LastMessageReported);
				}

				AssertEquals(false, dtu.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState.WPS_Status);
				AssertEquals(false, dtu2.WDH_FinalisedTime.IsEmpty);
				AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState2.WPS_Status);

				AssertLog(dtu, $"DTU1|RES=Auto Finalised after 8 hours|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState.Package, $"P1|FAC=CFS|TYP=Finalised|WHS=WHS", Events.ItemDocumentJobFinalised.Code);
				AssertLog(dtu2, $"DTU2|RES=Auto Finalised after 24 hours|TYP=Finalised", Events.StatusUpdated.Code);
				AssertLog(packageState2.Package, $"P2|FAC=CFS|TYP=Finalised|WHS=TR2", Events.ItemDocumentJobFinalised.Code);
			}
		}

		void AssertLog(EnterpriseBusinessObject parent, string expectedLogMessage, string status)
		{
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, status);
			var logs = parent.Logs.Find(logQuery);
			AssertEquals("Should create finalise log", 1, logs.Length);
			AssertEquals(expectedLogMessage, logs.SingleOrDefault().SL_Reference);
		}

		#endregion

		#region Logger

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion
	}
}
