using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsItemDispatchTransportationUnitDataObjectReaderForContainerTest : TransitUniversalTestCase
	{
		#region TestPopulateBizO_CreateNewHeader

		public void TestPopulateBizO_CreateNewHeader()
		{
			var businessObjectFactory = Factory.BOFactory;
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var warehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			var pivotQuery = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, header.PK);
			var pivot = Factory.Load<WhsItemDispatchLoadListDTUPivot>(pivotQuery);

			AssertEquals("Header must have point to the load list passed into the reader",
				loadList.GetValue(WhsItemDispatchLoadListSchema.PK), pivot.SingleOrDefault().DispatchLoadList.PK);
			AssertEquals("Header reference should be generated from number fountain.", "TD00000001", header.WDH_ReferenceNumber);
			AssertEquals("Warehouse must be set to the one that passed into the reader.", warehouse.GetValue(WhsWarehouseSchema.PK), header.WDH_WW_Warehouse);
		}

		public void TestPopulateBizO_CreateContainerRecord()
		{
			var businessObjectFactory = Factory.BOFactory;
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var warehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));
			var containerDO = GetDefaultContainer();
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			var packages = header.PackageJob.Packages;

			AssertContainsExactElementsInAnyOrder(new string[] { PkgUnit.Container }, packages.Select(p => p.KP_F3_NKPackType));
			var container = packages.Single().Container;
			AssertNotNull(container);
			AssertEquals(containerDO.ContainerType.Code, container.ContainerType.RC_Code);
			var uldPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, header.PackageExtension.KPN_KP_Package));
			AssertEquals(container.Package.PK, uldPackageState.Package.PK);
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, uldPackageState.WPS_Status);
			AssertEquals(true, uldPackageState.WPS_IsHandlingUnit);
			AssertEquals(container.Package.PK, header.PackageExtension.KPN_KP_Package);
			AssertEquals(header.PK, header.PackageExtension.KPN_ParentID);
			AssertEquals(header.TablePrefix, header.PackageExtension.KPN_ParentTableCode);
			AssertEquals(true, uldPackageState.WPS_IsHandlingUnit);
			AssertEquals(TransitWarehouseSecurityStatuses.Codes.NotRequired, uldPackageState.WPS_SecurityStatus);
			AssertEquals(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, uldPackageState.WPS_CustomsStatus);
		}

		#endregion

		#region estPopulateBizO_CreateContainerRecord_MatchingULD_NotInWarehouse

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithBookedStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("SEA", TransitWarehouseStatuses.Codes.Booked);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithDepartedStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("SEA", TransitWarehouseStatuses.Codes.Departed);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithFreightLoadedStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("SEA", TransitWarehouseStatuses.Codes.FreightLoaded);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithAdjustedOutStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("SEA", TransitWarehouseStatuses.Codes.AdjustedOut);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithFinalisedStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("SEA", TransitWarehouseStatuses.Codes.Finalized);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithBookedStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("AIR", TransitWarehouseStatuses.Codes.Booked);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithDepartedStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("AIR", TransitWarehouseStatuses.Codes.Departed);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithFreightLoadedStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("AIR", TransitWarehouseStatuses.Codes.FreightLoaded);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithAdjustedOutStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("AIR", TransitWarehouseStatuses.Codes.AdjustedOut);
		}

		public void TestPopulateBizO_CreateContainerRecord_MatchingULDWithFinalisedStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse("AIR", TransitWarehouseStatuses.Codes.Finalized);
		}

		void TestPopulateBizO_CreateContainerRecord_MatchingULDNotInWarehouse(string transportMode, string status)
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer(transportMode);
			var consolDO = Data.HeaderDataObject;

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package));
			rtuULDPackageState.WPS_Status = status;
			if (status != TransitWarehouseStatuses.Codes.Booked)
			{
				rtuULDPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
				if (status != TransitWarehouseStatuses.Codes.AdjustedOut)
				{
					var newDTU = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
					rtuULDPackageState.WPS_WDH_TransitDispatchHeader = newDTU.PK;
					var newDLL = helper.CreateDispatchLoadList("DLL1", warehouse.PK);
					rtuULDPackageState.WPS_WDL_LoadList = newDLL.PK;
					rtuULDPackageState.WPS_IsSecure = true;
					rtuULDPackageState.WPS_SecurityStatus = "SEC";
					rtuULDPackageState.WPS_UnloadedTime = ZDateTimeOffset.Now;
					rtuULDPackageState.WPS_UnloadedNotYetProcessedTime = rtuULDPackageState.WPS_UnloadedTime;
					rtuULDPackageState.WPS_LoadedTime = ZDateTimeOffset.Now;
					var newRTU = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
					newRTU.WRH_WW_Warehouse = warehouse.PK;
					rtuULDPackageState.WPS_WRH_TransitReceiveHeader = newRTU.PK;
				}
				else
				{
					rtuULDPackageState.WPS_AdjustedOut = "Adj";
				}
			}
			AssertNotNull("Precondition", rtuULDPackageState.Package.Container);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			AssertEquals(transportMode == "SEA" ? "CNT" : "ULD", dtu.WDH_UnitType);
			var uldPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotEquals(rtuULDPackageState.Package.Container.PK, uldPackageState.Package.Container.PK);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse

		public void TestPopulateBizO_CreateContainerRecord_ULDWithArrivedStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("SEA", TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithCTTStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("SEA", TransitWarehouseStatuses.Codes.Committed);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithPICStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("SEA", TransitWarehouseStatuses.Codes.Picked);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithSTAStatus_SEA()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("SEA", TransitWarehouseStatuses.Codes.Staged);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithArrivedStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("AIR", TransitWarehouseStatuses.Codes.Arrived);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithCTTStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("AIR", TransitWarehouseStatuses.Codes.Committed);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithPICStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("AIR", TransitWarehouseStatuses.Codes.Picked);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULDWithSTAStatus_AIR()
		{
			TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse("AIR", TransitWarehouseStatuses.Codes.Staged);
		}

		void TestPopulateBizO_CreateContainerRecord_ULD_MatchedToExistingContainerInWarehouse(string transportMode, string packageStatus)
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer(transportMode);
			var consolDO = Data.HeaderDataObject;

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package));
			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageState.Package.Container);
			UpdateULDPackageState(rtuULDPackageState, warehouse, packageStatus);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			AssertEquals(transportMode == "SEA" ? "CNT" : "ULD", dtu.WDH_UnitType);
			var uldPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals(rtuULDPackageState.Package.Container.PK, uldPackageState.Package.Container.PK);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_HasMatchedExistingContainerInAnotherWarehouse

		public void TestPopulateBizO_CreateContainerRecord_ULD_HasMatchedExistingContainerInAnotherWarehouse()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;

			var anotherWarehouse = Helper.CreateWarehouse("TW2", "A", 1, 1);
			anotherWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.SaveForTesting();

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(anotherWarehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package));
			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageState.Package.Container);
			UpdateULDPackageState(rtuULDPackageState, anotherWarehouse, TransitWarehouseStatuses.Codes.Arrived);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			AssertEquals("CNT", dtu.WDH_UnitType);
			var uldPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotEquals(rtuULDPackageState.Package.Container.PK, uldPackageState.Package.Container.PK);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_ResendingDispatchInstructionsMustNotCreateNewDTUExtension

		public void TestPopulateBizO_CreateContainerRecord_ULD_ResendingDispatchInstructionsMustNotCreateNewDTUExtension()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;
			var loadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Factory.SaveForTesting();

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageState.Package.Container);
			UpdateULDPackageState(rtuULDPackageState, warehouse);
			Factory.SaveForTesting();

			Logger.ClearLogs();
			var newFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory), warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			var dtuULDPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("RTU and DTU packageState must be same.", rtuULDPackageState.PK, dtuULDPackageState.PK);
			AssertNotEquals("RTU and DTU package extensions must be different.", dtu.PackageExtension.PK, rtu.PackageExtension.PK);
			AssertEquals("Extensions from RTU and DTU must point to the same package.", dtu.PackageExtension.Package.PK, rtu.PackageExtension.Package.PK);
			AssertNotContains($"Information - Multiple open containers found with reference C1 – could not match – created new TDU Ref {dtu.WDH_ReferenceNumber}", Logger.Logs);
			newFactory.SaveForTesting();

			// Resend dispatch instructions
			Logger.ClearLogs();
			var resendDispatchInstructionInNewFactory = new UniversalObjectFactory();
			var loadListAfterResendingDispatchInstruction = resendDispatchInstructionInNewFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var readerInNewFactory = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListAfterResendingDispatchInstruction), warehouse, containerDO, Logger, newFactory);
			var dtuInNewFactory = readerInNewFactory.ReadIntoBusinessObject();
			var dtuULDPackageStateAfterResending = resendDispatchInstructionInNewFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtuInNewFactory.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("RTU and DTU packageState must be same after resending dispatch instructions.", rtuULDPackageState.PK, dtuULDPackageStateAfterResending.PK);
			AssertEquals("No New Package extension must be created after resending dispatch instructions.", dtu.PackageExtension.PK, dtuInNewFactory.PackageExtension.PK);
			AssertNotContains($"Information - Multiple open containers found with reference C1 – could not match – created new TDU Ref {dtu.WDH_ReferenceNumber}", Logger.Logs);

			var packageExtensions = resendDispatchInstructionInNewFactory.Load<PkgPackageExtension>(new ZQuery());
			AssertEquals("Only package extensions remain in DB must be for RTU and DTU.", 2, packageExtensions.Length);
			var rtuPackagePK = packageExtensions.Where(p => p.KPN_ParentID == rtu.PK).Single().KPN_KP_Package;
			var dtuPackagePK = packageExtensions.Where(p => p.KPN_ParentID == dtuInNewFactory.PK).Single().KPN_KP_Package;
			AssertEquals("RTU and DTU package extensions must still point to the same package PK.", rtuPackagePK, dtuPackagePK);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_MultipleMatchingContainersInWarehouse

		public void TestPopulateBizO_CreateContainerRecord_ULD_MultipleMatchingContainersInWarehouse()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;
			var loadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Factory.SaveForTesting();

			var latestRTU = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var latestRTUULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, latestRTU.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", latestRTUULDPackageState.Package.Container);
			UpdateULDPackageState(latestRTUULDPackageState, warehouse);
			latestRTU.WRH_VehicleReference = "V1"; // rename it so won't match again
			Factory.SaveForTesting();

			var anotherRTU = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var anotherRTUULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, anotherRTU.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", anotherRTUULDPackageState.Package.Container);
			UpdateULDPackageState(anotherRTUULDPackageState, warehouse);
			latestRTU.WRH_VehicleReference = "C1"; // rename it back to match multiple RTUs
			Factory.SaveForTesting();

			var rtuForDifferentContainer = Helper.CreateReceiveTransportationUnitWithContainerType("Different", warehouse.PK, warehouse.DefaultLocation.PK, "Different");
			AssertEquals("Precondition", anotherRTU.WRH_VehicleReference, latestRTU.WRH_VehicleReference);
			latestRTU.WRH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-4); // latest matching RTU
			anotherRTU.WRH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-5);
			rtuForDifferentContainer.WRH_SystemCreateTimeUtc = ZDateTime.Now;
			var differentRTUULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtuForDifferentContainer.PackageExtension.KPN_KP_Package)).Single();
			differentRTUULDPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			differentRTUULDPackageState.WPS_UnloadedTime = ZDateTimeOffset.Now;
			differentRTUULDPackageState.WPS_UnloadedNotYetProcessedTime = differentRTUULDPackageState.WPS_UnloadedTime;
			var newRTU = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			newRTU.WRH_WW_Warehouse = warehouse.PK;
			differentRTUULDPackageState.WPS_WRH_TransitReceiveHeader = newRTU.PK;
			differentRTUULDPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.SaveForTesting();

			// Send dispatch instructions
			Logger.ClearLogs();
			var newFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory), warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			var dtuULDPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotEquals("DTU's ULD Package State must be the same as latest RTU's package State.", latestRTUULDPackageState.PK, dtuULDPackageState.PK);
			AssertContains($"Information - Multiple open containers found with reference C1 – could not match – created new TDU Ref {dtu.WDH_ReferenceNumber}", Logger.Logs);
			newFactory.SaveForTesting();

			// Resend dispatch instructions
			Logger.ClearLogs();
			var resendDispatchInstructionInNewFactory = new UniversalObjectFactory();
			var loadListAfterResending = resendDispatchInstructionInNewFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var readerInNewFactory = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListAfterResending), warehouse, containerDO, Logger, resendDispatchInstructionInNewFactory);
			var dtuInNewFactory = readerInNewFactory.ReadIntoBusinessObject();
			AssertEquals("Precondition", dtu.PK, dtuInNewFactory.PK);
			AssertNoExceptionThrown(() => resendDispatchInstructionInNewFactory.SaveForTesting());
			AssertContains($"Information - Multiple open containers found with reference C1 – could not match – created new TDU Ref {dtuInNewFactory.WDH_ReferenceNumber}", Logger.Logs);
			var dtuULDPackageStateAfterResending = resendDispatchInstructionInNewFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtuInNewFactory.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("DTU's ULD Package State must be the same as latest RTU's package State after resending dispatch instructions.", dtuULDPackageState.PK, dtuULDPackageStateAfterResending.PK);
			AssertEquals("No New Package extension must be created.", dtu.PackageExtension.PK, dtuInNewFactory.PackageExtension.PK);
			AssertEquals("Package Status must be Booked.", TransitWarehouseStatuses.Codes.Booked, dtuULDPackageStateAfterResending.WPS_Status);
			AssertEquals("Last Location of Package State must be empty.", ZGuid.Empty, dtuULDPackageStateAfterResending.WPS_WL_LastLocation);
			AssertEquals("Unloaded time must be empty.", ZDateTimeOffset.Empty, dtuULDPackageStateAfterResending.WPS_UnloadedTime);
			AssertEquals("Package state must be for Handling unit.", true, dtuULDPackageStateAfterResending.WPS_IsHandlingUnit);
		}

		[TestDate(2022, 11, 24)]
		public void TestPopulateBizO_CreateContainerRecord_ULD_MultipleMatchingContainersInWarehouse_OneWithAnExtensionForAnotherDTU()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;
			var loadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Factory.SaveForTesting();

			var rtuWithoutDTUPackageExtension = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageStateWithoutDTUPackageExtension = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtuWithoutDTUPackageExtension.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageStateWithoutDTUPackageExtension.Package.Container);
			UpdateULDPackageState(rtuULDPackageStateWithoutDTUPackageExtension, warehouse);
			rtuWithoutDTUPackageExtension.WRH_VehicleReference = "V1"; // rename it so won't match again
			Factory.SaveForTesting();

			var anotherRTUWithMatchingContainerNumberButWithExtension = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var anotherRTUULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, anotherRTUWithMatchingContainerNumberButWithExtension.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", anotherRTUULDPackageState.Package.Container);
			UpdateULDPackageState(anotherRTUULDPackageState, warehouse);
			rtuWithoutDTUPackageExtension.WRH_VehicleReference = "C1"; // rename it back to match multiple RTUs
			Factory.SaveForTesting();

			AssertEquals("Precondition", anotherRTUWithMatchingContainerNumberButWithExtension.WRH_VehicleReference, rtuWithoutDTUPackageExtension.WRH_VehicleReference);
			rtuWithoutDTUPackageExtension.WRH_SystemCreateTimeUtc = ZDateTime.Now;
			anotherRTUWithMatchingContainerNumberButWithExtension.WRH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1); // Make it the latest one.
			Helper.PackingHelper.CreatePackageExtension(Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>(), anotherRTUWithMatchingContainerNumberButWithExtension.PackageExtension.Package); // RTU is assigned to another DTU via package extension
			Factory.SaveForTesting();

			// Send dispatch instructions
			var newFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory), warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			var uldPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("DTU package state must be the same as RTU ULD Package state which does not have a DTU package extension.", uldPackageState.PK, rtuULDPackageStateWithoutDTUPackageExtension.PK);
			AssertEquals("DTU must point to the same container package as RTU without a DTU package extension.", rtuULDPackageStateWithoutDTUPackageExtension.Package.PK, dtu.PackageExtension.Package.PK);
			newFactory.SaveForTesting();

			// Resend dispatch instrcutions
			var resendDispatchInstructionInNewFactory = new UniversalObjectFactory();
			var loadListAfterResendingDispatchInstruction = resendDispatchInstructionInNewFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var readerInNewFactory = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListAfterResendingDispatchInstruction), warehouse, containerDO, Logger, newFactory);
			var dtuInResendingFactory = readerInNewFactory.ReadIntoBusinessObject();
			AssertEquals("Precondition", dtu.PK, dtuInResendingFactory.PK);
			var dtuULDPackageStateAfterResendingInstructions = resendDispatchInstructionInNewFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtuInResendingFactory.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("RTU package state and DTUs package state must be the same after resending dispatch instructions.", rtuULDPackageStateWithoutDTUPackageExtension.PK, dtuULDPackageStateAfterResendingInstructions.PK);
			AssertEquals("No New Package extension must be created.", dtu.PackageExtension.PK, dtuInResendingFactory.PackageExtension.PK);
			AssertEquals("Arrived Package Status must remain as it is after resending.", TransitWarehouseStatuses.Codes.Arrived, dtuULDPackageStateAfterResendingInstructions.WPS_Status);
			AssertEquals("Last Location of Package State must not be changed.", warehouse.DefaultLocation.PK, dtuULDPackageStateAfterResendingInstructions.WPS_WL_LastLocation);
			AssertEquals("Unloaded time must be same as before.", rtuULDPackageStateWithoutDTUPackageExtension.WPS_UnloadedTime, dtuULDPackageStateAfterResendingInstructions.WPS_UnloadedTime);
			AssertEquals("Package state must still be for Handling unit.", true, dtuULDPackageStateAfterResendingInstructions.WPS_IsHandlingUnit);
		}

		public void TestPopulateBizO_CreateContainerRecord_ULD_MatchingRTUIsDifferentUnitType()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;
			var loadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Factory.SaveForTesting();

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var latestRTUULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotNull("Precondition - RTU container must be created.", latestRTUULDPackageState.Package.Container);
			UpdateULDPackageState(latestRTUULDPackageState, warehouse);
			rtu.WRH_UnitType = TransportUnitTypes.ULD;
			Factory.SaveForTesting();

			// Send dispatch instructions
			var newFactory = new UniversalObjectFactory();
			var loadListInNewFactory = newFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory), warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			newFactory.SaveForTesting();
			var dtuULDPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertEquals("DTU Unit type must be Container.", TransportUnitTypes.Container, dtu.WDH_UnitType);
			AssertNull("DTU must not match to RTU with matching container in another Unit Type hence package state does not point to a RTU.", dtuULDPackageState.ReceiveTransportationUnit);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_MatchingULDHasAnotherDTU

		public void TestPopulateBizO_CreateContainerRecord_ULD_MatchingULDHasAnotherDTU()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package));
			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageState.Package.Container);
			UpdateULDPackageState(rtuULDPackageState, warehouse);
			var dtuPackageExtension = Helper.PackingHelper.CreatePackageExtension(Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>(), rtu.PackageExtension.Package); // RTU is assigned to another DTU via package extension
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtuBO = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition - Container number is still the same.", "C1", dtuBO.ContainerNumber);
			var uldPackageStateForDTUBO = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtuBO.PackageExtension.KPN_KP_Package)).Single();
			AssertNotEquals("Since matching RTU ULD has an extension for another DTU, matching RTUs package State is not assigned to the DTU.", rtuULDPackageState.PK, uldPackageStateForDTUBO.PK);
			AssertNotEquals("New DTUs package extension points to a different container package.", dtuPackageExtension.Package.Container.PK, dtuBO.PackageExtension.Package.Container.PK);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_MatchingULDIsATruck

		public void TestPopulateBizO_CreateContainerRecord_ULD_MatchingULDIsATruck()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();

			var rtu = Helper.CreateReceiveTransportationUnit("C1", warehouse.PK, warehouse.DefaultLocation.PK);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtuBO = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition - Container number is still the same.", "C1", dtuBO.ContainerNumber);
			AssertNotEquals("DTU package extension must not point to RTU's package since RTU is a Truck.", rtu.PackageExtension?.Package?.PK, dtuBO.PackageExtension.Package.PK);
		}

		#endregion

		#region TestPopulateBizO_ROATransportMode

		public void TestPopulateBizO_ROATransportMode()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer("ROA");

			var newFactory = new UniversalObjectFactory();
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			AssertEquals("WDH_UnitType of Vehicle is always VEH.", "VEH", dtu.WDH_UnitType);
			AssertNotNull("Package Extension must be created for Vehicles.", dtu.PackageExtension);
			var uldPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package));
			AssertNull("No Package State must be created for Vehicles.", uldPackageState);
		}

		#endregion

		#region TestPopulateBizo_TestImportTransportCompany

		public void TestPopulateBizo_TestImportTransportCompany()
		{
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var warehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));

			var org = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ArrivalCFSLocalTransportAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1" };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "CS1000000");
			shipment.DataContext = dataContext;
			Logger.TopLevelDataObject = shipment;

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, header.PK));
			var transportDocAddress = addresses.Single(doc => doc.E2_AddressType == DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			CombineAssertions("Transport company in DTU should be same as Shipment's ArrivalCFSLocalTransportAddress", () =>
			{
				AssertEquals(org.CompanyName, transportDocAddress.CompanyName);
				AssertEquals(org.Address1, transportDocAddress.Address1);
				AssertEquals(org.Address2, transportDocAddress.Address2);
				AssertEquals(org.Postcode, transportDocAddress.Postcode);
			});
		}

		#endregion

		#region TestPopulateBizO_LinkMatchingHeader

		public void TestPopulateBizO_LinkMatchingHeader()
		{
			var businessObjectFactory = Factory.BOFactory;
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var differentWarehouse = Data.WarehouseINTHEMSYD;
			var alreadyLoadedCompletedHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "2", "C1", true);
			var matchingHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "1", "C1");
			var headerForAnotherContainer = CreateDispatchTransportationUnit(referenceHelper, warehouse, "3", "C2", false);
			var headerForDifferentWarehouse = CreateDispatchTransportationUnit(referenceHelper, differentWarehouse, "4", "C2");

			Factory.SaveForTesting();

			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Existing header must be matched.", matchingHeader.PK, header.PK);
			AssertEquals("Matching header must not be load completed.", true, header.WDH_FinalisedTime.IsEmpty);
			AssertEquals("Header should be related to two dispatch load lists", 2, header.DispatchLoadLists.Count);
			AssertNotNull("Header should be related to new ispatch load list", header.DispatchLoadLists.Where(t => t.PK == loadList.GetValue(WhsItemDispatchLoadListSchema.PK)));
			AssertEquals("Old warehouse must remain.", warehouse.PK, header.WDH_WW_Warehouse);
			Factory.SaveForTesting();

			var newWarehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));
			var readerForNewWarehouse = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, newWarehouse, GetDefaultContainer(), Logger, Factory);
			var newHeader = readerForNewWarehouse.ReadIntoBusinessObject();
			AssertNotEquals("Should create a new header since the warehouse is different.", matchingHeader.PK, newHeader.PK);

			var additionalReferences = Factory.BOFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, matchingHeader.PK)).ToArray();
			AssertEquals("No Additional references must be created.", 0, additionalReferences.Length);
		}

		public void TestPopulateBizO_DoesNotLinkLoadCompletedHeader()
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var loadCompleteHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "1", "C1", true);

			Factory.SaveForTesting();

			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNotEquals("Existing header did not match because it is Load Complete, new one is created instead.", loadCompleteHeader.PK, header.PK);
		}
		
		public void TestPopulateBizO_LinkLoadNotCompletedHeader()
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var loadNotCompleteHeader = CreateDispatchTransportationUnit(referenceHelper, warehouse, "1", "C1", false);

			Factory.SaveForTesting();

			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Existing header matches because it is not Load Complete.", loadNotCompleteHeader.PK, header.PK);
		}

		WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(WhsTransitAdditionalReferencesHelper referenceHelper, IWhsWarehouse warehouse,
			string referenceNumber, string containerNumber, bool isLoadComplete = false)
		{
			var itemDispatchLoadList = Factory.BOFactory.New<WhsItemDispatchLoadList>();
			itemDispatchLoadList.WDL_JobID = referenceNumber;
			itemDispatchLoadList.WDL_ReferenceNumber = referenceNumber;
			itemDispatchLoadList.WDL_WW_Warehouse = warehouse.PK;
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var completeTime = isLoadComplete ? dateTimeOffset : ZDateTimeOffset.Empty;
			header.WDH_GateInTime = completeTime;
			header.WDH_LoadCompleteTime = completeTime;
			header.WDH_GateOutTime = completeTime;
			header.WDH_FinalisedTime = completeTime;
			header.WDH_WW_Warehouse = warehouse.PK;
			header.WDH_ReferenceNumber = referenceNumber;
			header.WDH_VehicleReference = containerNumber;

			var pivot = Factory.NewWithValidTestData<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDH_TransitDispatchTransportationUnit = header.PK;
			pivot.WLD_WDL_TransitDispatchLoadList = itemDispatchLoadList.PK;

			return header;
		}

		#endregion

		#region TestPopulateBizo_SelectLatestDTUWhenMultipleMatched

		public void TestPopulateBizo_SelectLatestDTUWhenMultipleMatched()
		{
			var businessObjectFactory = Factory.BOFactory;
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var warehouse = Data.Warehouse;
			var dtu1 = CreateDispatchTransportationUnit(referenceHelper, warehouse, "RC01", "Container1");
			var dtu2 = CreateDispatchTransportationUnit(referenceHelper, warehouse, "RC02", "Container1");
			var dtu3 = CreateDispatchTransportationUnit(referenceHelper, warehouse, "RC03", "Container2");
			var dtu4 = CreateDispatchTransportationUnit(referenceHelper, warehouse, "RC04", "Container2");

			dtu1.WDH_SystemCreateTimeUtc = ZDateTime.Today;
			dtu2.WDH_SystemCreateTimeUtc = dtu1.WDH_SystemCreateTimeUtc.AddDays(1);
			dtu3.WDH_SystemCreateTimeUtc = dtu1.WDH_SystemCreateTimeUtc.AddDays(1);
			dtu4.WDH_SystemCreateTimeUtc = dtu1.WDH_SystemCreateTimeUtc.AddDays(2);

			Factory.SaveForTesting();

			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			var container = new Container { ContainerNumber = "Container1", ContainerType = new ContainerType { Code = "20GP" } };
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, container, Logger, Factory);
			var matchedDTU = reader.ReadIntoBusinessObject();
			AssertEquals("The Latest DTU \"dtu2\" Should Be Matched.", dtu2.PK, matchedDTU.PK);

			dtu2.WDH_SystemCreateTimeUtc = dtu1.WDH_SystemCreateTimeUtc.AddDays(-1);
			Factory.SaveForTesting();
			reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, container, Logger, Factory);
			matchedDTU = reader.ReadIntoBusinessObject();
			AssertEquals("The Latest DTU \"dtu1\" Should Be Matched.", dtu1.PK, matchedDTU.PK);

			container.ContainerNumber = "Container2";
			reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, container, Logger, Factory);
			matchedDTU = reader.ReadIntoBusinessObject();
			AssertEquals("The Latest DTU \"dtu4\" Should Be Matched.", dtu4.PK, matchedDTU.PK);
		}

		#endregion

		#region TestPackingGroupAndWarehouseFromConsolNotNull

		public void TestPackingGroupAndWarehouseFromConsolNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Load list must not be null.", () => new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(null, new Mock<IColumnIndexer>().Object, new Container(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory));
			AssertExceptionThrown<ArgumentNullException>("Warehouse must not be null.", () => new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(new Mock<IColumnIndexer>().Object, null, new Container(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory));
		}

		#endregion

		#region TestPopulateBizO_ContainerWithoutTypeOrNumber_ThrowsException

		public void TestPopulateBizO_ContainerWithoutTypeOrNumber_ThrowsException()
		{
			var container = new Container { ContainerNumber = string.Empty, ContainerType = null };
			var businessObjectFactory = Factory.BOFactory;
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var warehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));

			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, container, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Container Type is mandatory for Containers. However, Container Type is missing for at least one Container (that has no Container Number).", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBizO_ContainerWithInvalidType_ThrowsException

		public void TestPopulateBizO_ContainerWithInvalidType_ThrowsException()
		{
			var containerWithoutNumber = new Container { ContainerNumber = string.Empty, ContainerType = new ContainerType { Code = "XXX" } };
			var containerWithNumber = new Container { ContainerNumber = "CNT1", ContainerType = new ContainerType { Code = "XXX" } };
			var businessObjectFactory = Factory.BOFactory;
			var loadList = Factory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var warehouse = DataObjectReader.GetColumnIndexerFromRow(Factory.RowFactory.New(WhsWarehouseSchema.Constants.TableName));

			var readerForContainerWithoutNumber = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerWithoutNumber, Logger, Factory);
			var readerForContainerWithNumber = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerWithNumber, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Container does not have a supported Container Type.", () => readerForContainerWithoutNumber.ReadIntoBusinessObject());
			AssertExceptionThrown<DataObjectReadFailureException>("Container CNT1 does not have a supported Container Type.", () => readerForContainerWithNumber.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateBizo_BillToParty

		public void TestPopulateBizo_BillToParty_ArrivalWarehouse()
		{
			var warehouse = Data.Warehouse;

			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ReceivingForwarderAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1" };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "CS1000000");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext = dataContext;
			Logger.TopLevelDataObject = shipment;

			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var newAddressPK = Data.Orgs.INTHEMSYD.MainAddress.PK;
			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ReceivingForwarderAddress));
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = reader.ReadIntoBusinessObject();
			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be overridden", newAddressPK, billToPartyAddress2.E2_OA_Address);
		}

		public void TestPopulateBizo_BillToParty_DepartureWarehouse()
		{
			var warehouse = Data.Warehouse;

			var org = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.SendingForwarderAddress));
			org.Port = new UNLOCO() { Code = "AUSYD" };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { VoyageFlightNo = "V1" };
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "CS1000000");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });
			shipment.DataContext = dataContext;
			Logger.TopLevelDataObject = shipment;

			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, GetDefaultContainer(), Logger, Factory);
			var header = reader.ReadIntoBusinessObject();

			var billToPartyAddress = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, billToPartyAddress.E2_OA_Address);
			Factory.SaveForTesting();

			var newAddressPK = Data.Orgs.INTHEMSYD.MainAddress.PK;
			var overrideOrg = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.SendingForwarderAddress));
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { overrideOrg });

			header = reader.ReadIntoBusinessObject();
			var billToPartyAddress2 = header.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
			AssertNotNull("Bill To Party in DTU in Departure Warehouse should be same as Consol's SendingForwarderAddress", billToPartyAddress2);
			AssertEquals("Bill To Party in DTU in Departure Warehouse should be overridden", newAddressPK, billToPartyAddress2.E2_OA_Address);
		}

		#endregion

		#region TestPopulateBizO_CreateContainerRecord_ULD_RTUContainerIsInactive

		public void TestPopulateBizO_CreateContainerRecord_ULD_RTUContainerIsInactive()
		{
			var warehouse = Data.Warehouse;
			var containerDO = GetDefaultContainer();
			var consolDO = Data.HeaderDataObject;

			var rtu = new WhsTransitReceiveTransportationUnitDataObjectReader(warehouse, containerDO, consolDO, null, Logger, Factory, GlbBranch.CurrentBranch.OrgProxy).ReadIntoBusinessObject();
			var rtuULDPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package));
			rtu.PackageExtension.KPN_IsActive = false;
			Factory.SaveForTesting();

			AssertNotNull("Precondition - RTU container must be created.", rtuULDPackageState.Package.Container);
			AssertEquals("Precondition - RTU container must be deactive.", rtu.PackageExtension.KPN_IsActive, false);

			var newFactory = new UniversalObjectFactory();
			var loadList = newFactory.RowFactory.NewRowWithPK(WhsItemDispatchLoadListSchema.Instance);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(loadList, warehouse, containerDO, Logger, newFactory);
			var dtu = reader.ReadIntoBusinessObject();
			var uldPackageState = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			AssertNotEquals(rtuULDPackageState.Package.Container.PK, uldPackageState.Package.Container.PK);
		}

		#endregion

		#region TestPopulateBizO_ContainerTypeUpdated

		public void TestPopulateBizO_ContainerTypeUpdated()
		{
			var warehouse = Data.Warehouse;
			var containerDO =  Data.CreateContainer("C1", 0, "20GP", "SEA");
			var consolDO = Data.HeaderDataObject;
			var loadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Factory.SaveForTesting();

			var loadListInNewFactory = Factory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var reader = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListInNewFactory), warehouse, containerDO, Logger, Factory);
			var dtu = reader.ReadIntoBusinessObject();
			helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), "WDH", "GB1", "GateMovementBooking");

			var dtuULDPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).Single();
			Factory.SaveForTesting();

			// Resend dispatch instructions
			Logger.ClearLogs();
			containerDO.ContainerType = new ContainerType { Code = "40GP" };
			var newFactory = new UniversalObjectFactory();
			var loadListAfterResendingDispatchInstruction = newFactory.RowFactory.LoadFromPK(WhsItemDispatchLoadListSchema.Constants.TableName, loadList.PK);
			var readerInNewFactory = new WhsItemDispatchTransportationUnitDataObjectReaderForContainer(DataObjectReader.GetColumnIndexerFromRow(loadListAfterResendingDispatchInstruction), warehouse, containerDO, Logger, newFactory);
			var dtuInNewFactory = readerInNewFactory.ReadIntoBusinessObject();

			var dtuEventLog = dtuInNewFactory.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.ContainerTypeUpdatedCode).SingleOrDefault();
			AssertNotNull("Should create Container Type updated log for dtu", dtuEventLog);
			AssertEquals("DTU Container Type updated event must be logged.", "|TYP=ContainerType|NEW=40GP|OLD=20GP", dtuEventLog.SL_Reference);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		#endregion

		#region Helper

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion

		Container GetDefaultContainer(string deliveryMode = "SEA")
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, deliveryMode));
			return Data.CreateContainer("C1", 0, refContainer.RC_Code, deliveryMode);
		}

		void UpdateULDPackageState(WhsItemPackageState packageState, WhsWarehouse warehouse, string packageStatus = TransitWarehouseStatuses.Codes.Arrived)
		{
			packageState.WPS_Status = packageStatus;
			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState.WPS_UnloadedTime = ZDateTimeOffset.Now;
			packageState.WPS_UnloadedNotYetProcessedTime = packageState.WPS_UnloadedTime;
			var newRTU = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			newRTU.WRH_WW_Warehouse = warehouse.PK;
			packageState.WPS_WRH_TransitReceiveHeader = newRTU.PK;
			packageState.WPS_IsHandlingUnit = true;
		}
	}
}
