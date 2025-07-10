using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Warehouse.Transit.Shared.Testing
{
	public abstract class ITransitWarehouseParentTestCaseCore<T> : TestCaseWithFactory
	{
		protected WhsItemReceiveConsignment CreateReceiveConsignment(string rcnID, ZGuid warehousePK)
		{
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_JobID = rcnID;
			receiveConsignment.WRC_ConsignmentID = rcnID;
			receiveConsignment.WRC_WW_IntendedWarehouse = warehousePK;
			return receiveConsignment;
		}

		protected WhsItemDispatchConsignment CreateDispatchConsignment(string dcnID, ZGuid warehousePK)
		{
			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			dispatchConsignment.WDC_JobID = dcnID;
			dispatchConsignment.WDC_ConsignmentID = dcnID;
			dispatchConsignment.WDC_WW_Warehouse = warehousePK;
			return dispatchConsignment;
		}

		protected WhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(string reference, ZGuid warehousePK, ZGuid stagingLocationPK)
		{
			var receiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveTransportationUnit.WRH_ReferenceNumber = reference;
			receiveTransportationUnit.WRH_WW_Warehouse = warehousePK;
			receiveTransportationUnit.WRH_WL_StagingLocation = stagingLocationPK;
			return receiveTransportationUnit;
		}

		protected WhsItemDispatchLoadList CreateDispatchLoadList(string dllID, ZGuid warehousePK)
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_JobID = dllID;
			dispatchLoadList.WDL_ReferenceNumber = dllID;
			dispatchLoadList.WDL_WW_Warehouse = warehousePK;
			return dispatchLoadList;
		}

		protected WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(string dtuID, ZGuid warehousePK)
		{
			var dispatchTransportationUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			dispatchTransportationUnit.WDH_ReferenceNumber = dtuID;
			dispatchTransportationUnit.WDH_WW_Warehouse = warehousePK;
			return dispatchTransportationUnit;
		}

		protected WhsItemPackageState CreatePackageState(WhsItemReceiveConsignment rcn, WhsItemDispatchConsignment dcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchLoadList dll, WhsItemDispatchTransportationUnit dtu, string packageId, string status, string entryNum = "")
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			var packageState = Factory.New<WhsItemPackageState>();
			var package = PackingHelper.CreatePackage(packageJob, 1, "BOX", packageId);
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WW_Warehouse = rcn.WRC_WW_IntendedWarehouse;
			packageState.WPS_Status = status;
			packageState.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packageState.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WDH_TransitDispatchHeader = dtu != null ? dtu.PK : ZGuid.Empty;
			packageState.WPS_WDL_LoadList = dll != null ? dll.PK : ZGuid.Empty;
			packageState.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";

			if (!string.IsNullOrEmpty(entryNum))
			{
				var ent = Factory.New<CusEntryNumber>();
				ent.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
				ent.CE_EntryNum = entryNum;
				ent.CE_ParentID = packageState.PK;
				ent.CE_ParentTable = WhsItemPackageStateSchema.Constants.TableName;
				ent.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			}

			return packageState;
		}

		#region Implementation

		protected abstract T GetNewParent(string jobNumber);

		protected abstract string GetParentTableCode();

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = GetNewTestHelperFunctions();
				}
				return helper;
			}
		}

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		#region PackingHelper

		public PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}

	[TestsSubclassesOf(typeof(ITransitWarehouseParent), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class ITransitWarehouseParentForShipmentTestCase<T> : ITransitWarehouseParentTestCaseCore<T> where T : ITransitWarehouseParent
	{
		#region TestGetRelatedReceiveJobsAndItsPackagesForTransitWarehouseParent

		public virtual void TestGetRelatedReceiveJobsAndItsPackagesForTransitWarehouseParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var parent = GetNewParent("JS123");

			var rcn = CreateReceiveConsignment("RC123", warehouse.PK);
			rcn.WRC_ParentID = parent.PK;
			rcn.WRC_ParentTableCode = GetParentTableCode();
			var dcn = CreateDispatchConsignment("DC123", warehouse.PK);
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageState = CreatePackageState(rcn, dcn, rtu, null, null, "P1", TransitWarehouseStatuses.Codes.Putaway);
			var package = packageState.Package;

			var otherRCN = CreateReceiveConsignment("RC456", warehouse.PK);
			var otherDCN = CreateDispatchConsignment("DC789", warehouse.PK);
			var otherRTU = CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var otherPackageState = CreatePackageState(otherRCN, otherDCN, otherRTU, null, null, "P2", TransitWarehouseStatuses.Codes.Putaway);
			var otherPackage = otherPackageState.Package;

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;

			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the receiveConsignment.", rcn.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the receive transportation unit.", rtu.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the rtu package.", package.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherReceiveConsignment.", otherRCN, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherRTU.", otherRTU.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherRTU package.", otherPackage.PK, relatedObjectPKs);
		}

		#endregion

		#region TestGetRelatedDispatchJobsAndItsPackagesForTransitWarehouseParent

		public virtual void TestGetRelatedDispatchJobsAndItsPackagesForTransitWarehouseParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var parent = GetNewParent("JS123");

			var rcn = CreateReceiveConsignment("RC123", warehouse.PK);
			var dcn = CreateDispatchConsignment("DC123", warehouse.PK);
			dcn.WDC_ParentID = parent.PK;
			dcn.WDC_ParentTableCode = GetParentTableCode();
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var packageState = CreatePackageState(rcn, dcn, rtu, dll, dtu, "P1", TransitWarehouseStatuses.Codes.FreightLoaded);
			var package = packageState.Package;

			var otherRCN = CreateReceiveConsignment("RC456", warehouse.PK);
			var otherDCN = CreateDispatchConsignment("DC789", warehouse.PK);
			var otherRTU = CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var otherDLL = CreateDispatchLoadList("DLL2", warehouse.PK);
			var otherDTU = CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var otherPackageState = CreatePackageState(otherRCN, otherDCN, otherRTU, otherDLL, otherDTU, "P2", TransitWarehouseStatuses.Codes.FreightLoaded);
			var otherPackage = otherPackageState.Package;

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;

			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the dispatchConsignment.", dcn.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the dispatch transportation unit.", dtu.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the dtu package.", package.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherDCN.", otherDCN.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherDTU.", otherDTU.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain otherdTU package.", otherPackage.PK, relatedObjectPKs);
		}

		#endregion

		#region TestGetBlindPackagesAndItsParentsForTransitWarehouseParent

		public virtual void TestGetBlindPackagesAndItsParentsForTransitWarehouseParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = GetNewParent("JS123");
			var rcn = CreateReceiveConsignment("RC123", warehouse.PK);
			var dcn = CreateDispatchConsignment("DC123", warehouse.PK);
			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var blindPackageState = CreatePackageState(rcn, dcn, rtu, dll, dtu, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, "JS123");
			var blindPackage = blindPackageState.Package;

			var siblingPackageState = CreatePackageState(rcn, dcn, rtu, null, null, "P2", TransitWarehouseStatuses.Codes.Arrived);
			var siblingPackage = siblingPackageState.Package;

			var rcn_noRef = CreateReceiveConsignment("RC456", warehouse.PK);
			var dcn_noRef = CreateDispatchConsignment("DC456", warehouse.PK);
			var rtu_noRef = CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var dll_noRef = CreateDispatchLoadList("DLL2", warehouse.PK);
			var dtu_noRef = CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var blindPackageStateWithoutRef = CreatePackageState(rcn_noRef, dcn_noRef, rtu_noRef, dll_noRef, dtu_noRef, "P3", TransitWarehouseStatuses.Codes.FreightLoaded);
			var blindPackageWithoutRef = blindPackageStateWithoutRef.Package;

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the blind package with parent reference.", blindPackage.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the receive Consignment.", rcn.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the dispatch Consignment.", dcn.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the receive transportation unit.", rtu.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the dispatch transportation unit.", dtu.PK, relatedObjectPKs);

			AssertCollectionNotContains("Should NOT contain blind package's sibling package from the same parent.", siblingPackage.PK, relatedObjectPKs);

			AssertCollectionNotContains("Should NOT contain blind package without parent reference.", blindPackageWithoutRef.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain blind package's RCN without parent reference.", rcn_noRef.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain blind package's DCN without parent reference.", dcn_noRef.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain blind package's RTU without parent reference.", rtu_noRef.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain blind package's DTU without parent reference.", dtu_noRef.PK, relatedObjectPKs);
		}

		#endregion
	}

	[TestsSubclassesOf(typeof(IForwardingConsol), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class ITransitWarehouseParentForConsolTestCase<T> : ITransitWarehouseParentTestCaseCore<T> where T : IForwardingConsol
	{
		public virtual void TestGetRelatedReceiveTransportationUnitsForTransitWarehouseParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = (IForwardingConsol)GetNewParent("CON123");
			var asn = CreateASN("ASN1", warehouse.PK);
			asn.WRP_ParentID = parent.PK;
			asn.WRP_ParentTableCode = GetParentTableCode();

			var rtu1 = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var pivot1 = CreateASNRTUPivot(asn, rtu1);

			var rtu2 = CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var pivot2 = CreateASNRTUPivot(asn, rtu2);

			var rtu3 = CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK);

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the receive transprtation unit that link to asn.", rtu1.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the receive transprtation unit that link to asn.", rtu2.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain the receive transprtation unit that NOT link to asn.", rtu3.PK, relatedObjectPKs);
		}

		public virtual void TestGetRelatedReceiveTransportationUnitsForTransitWarehouseParent_ViaContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = (IForwardingConsol)GetNewParent("CON123");
			var container = CreateContainer();
			container.JC_JK = parent.PK;

			var asn = CreateASN("ASN1", warehouse.PK);
			asn.WRP_ParentID = ((BusinessObject)container).PK;
			asn.WRP_ParentTableCode = JobContainerSchema.Constants.Prefix;

			var rtu = CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var pivot1 = CreateASNRTUPivot(asn, rtu);

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the receive transprtation unit that link to asn.", rtu.PK, relatedObjectPKs);
		}

		public virtual void TestGetRelatedDispatchTransportationUnitsForTransitWarehouseParent()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = (IForwardingConsol)GetNewParent("CON123");
			var dll = CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_ParentID = parent.PK;
			dll.WDL_ParentTableCode = GetParentTableCode();

			var dtu1 = CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var pivot1 = CreateDLLDTUPivot(dll, dtu1);

			var dtu2 = CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var pivot2 = CreateDLLDTUPivot(dll, dtu2);

			var dtu3 = CreateDispatchTransportationUnit("DTU3", warehouse.PK);

			var docManagerInfo = ((IDocManagerSupport)parent).DocManagerInfo;
			Factory.Save();

			var relatedObjectPKs = docManagerInfo.RelatedObjects.Select(b => b.PK);
			AssertCollectionContains("Should contain the dispatch transprtation unit that link to dll.", dtu1.PK, relatedObjectPKs);
			AssertCollectionContains("Should contain the dispatch transprtation unit that link to dll.", dtu2.PK, relatedObjectPKs);
			AssertCollectionNotContains("Should NOT contain the dispatch transprtation unit that NOT link to dll.", dtu3.PK, relatedObjectPKs);
		}

		protected WhsItemReceiveASN CreateASN(string asnID, ZGuid warehousePK)
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			asn.WRP_ReferenceNumber = asnID;
			asn.WRP_WW_IntendedWarehouse = warehousePK;

			return asn;
		}

		protected WhsItemReceiveASNRTUPivot CreateASNRTUPivot(WhsItemReceiveASN asn, WhsItemReceiveTransportationUnit rtu)
		{
			var pivot = Factory.New<WhsItemReceiveASNRTUPivot>();
			pivot.WAR_WRP_TransitReceiveASN = asn.PK;
			pivot.WAR_WRH_TransitReceiveTransportationUnit = rtu.PK;

			return pivot;
		}

		protected WhsItemDispatchLoadListDTUPivot CreateDLLDTUPivot(WhsItemDispatchLoadList dll, WhsItemDispatchTransportationUnit dtu)
		{
			var pivot = Factory.New<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDL_TransitDispatchLoadList = dll.PK;
			pivot.WLD_WDH_TransitDispatchTransportationUnit = dtu.PK;

			return pivot;
		}

		protected IForwardingContainer CreateContainer()
		{
			var container = (IForwardingContainer)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingContainer>());
			return container;
		}
	}
}
