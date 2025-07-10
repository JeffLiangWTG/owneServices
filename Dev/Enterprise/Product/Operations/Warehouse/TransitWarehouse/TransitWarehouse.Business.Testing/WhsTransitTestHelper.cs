using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransportLeg = Enterprise.UniversalDataBuss.DataObjects.Universal.TransportLeg;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public static class ZDateTimeOffsetExtensions
	{
		public static ZDateTimeOffset WithoutSeconds(this ZDateTimeOffset dateTimeOffset)
		{
			return new ZDateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, 0, 0, dateTimeOffset.Offset);
		}
	}

	public class WhsTransitTestHelper : WhsTestHelperFunctionsEnv
	{
		public WhsTransitTestHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region CreateAdditionalReference

		public CusEntryNumber CreateAdditionalReference(BusinessObject bizo, string entryNum, string entryType = CusEntryNumber.EntryType.ActualArrivalStatus, string category = CusEntryNumber.Categories.AdditionalReferenceNumber, string status = "", string country = "")
		{
			return CreateAdditionalReference(bizo.PK, bizo.TableName, entryNum, entryType, category, status, country);
		}

		public CusEntryNumber CreateCustomsAdditionalReference<T>(T bizO, string entryType, string entryNum, string category = CusEntryNumber.Categories.CustomsPermitClearanceNumber, string status = "", string country = "") where T : BusinessObject
		{
			return CreateAdditionalReference(bizO.PK, bizO.TableName, entryNum, entryType, category, status, country);
		}

		public CusEntryNumber CreateCustomsNumber<T>(T bizO, string entryNum, string sourceType = "") where T : BusinessObject
		{
			var cen = CreateCustomsAdditionalReference(bizO, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, entryNum);
			if (!sourceType.IsNullOrEmpty())
			{
				cen.PopulateAddOnValue("SourceType", "STR", sourceType);
			}
			return cen;
		}

		public CusEntryNumber CreateCustomsReleaseNumber<T>(T bizO, string entryNum, string sourceType = "") where T : BusinessObject
		{
			var crn = CreateCustomsAdditionalReference(bizO, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, entryNum);
			if (!sourceType.IsNullOrEmpty())
			{
				crn.PopulateAddOnValue("SourceType", "STR", sourceType);
			}
			return crn;
		}

		public CusEntryNumber CreateAdditionalReference(ZGuid parentID, string parentTable, string entryNum, string entryType, string category = CusEntryNumber.Categories.AdditionalReferenceNumber, string status = "", string country = "")
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_ParentID = parentID;
			cusEntryNumber.CE_ParentTable = parentTable;
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_Category = category;
			cusEntryNumber.CE_EntryStatus = status;
			cusEntryNumber.CE_RN_NKCountryCode = country;

			return cusEntryNumber;
		}

		#endregion

		#region CreateGoverningReference

		public CusEntryNumber CreateGoverningReference(ZGuid parentPK, string parentTableCode, string entryType, string entryNum = "123", string entryStatus = "")
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_ParentID = parentPK;
			cusEntryNumber.CE_ParentTable = parentTableCode;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			return cusEntryNumber;
		}

		#endregion

		#region CreatePortAuthorityReference

		public CusEntryNumber CreatePortAuthorityReference(ZGuid parentPK, string parentTableCode, string entryType = "PAN", string entryNum = "123", string status = "")
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_Category = TransitWarehouseReferenceCategories.Codes.PortReference;
			cusEntryNumber.CE_ParentID = parentPK;
			cusEntryNumber.CE_ParentTable = parentTableCode;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryStatus = status;
			cusEntryNumber.CE_EntryNum = entryNum;
			return cusEntryNumber;
		}

		#endregion

		#region CreateChargeCode

		public AccChargeCode CreateChargeCode(ZString code, ZString description, ZString chargeGroup, ZString chargeSubGroup, string chargeType = Constants.ChargeType.Revenue)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();

			result.AC_Code = code;
			result.AC_Desc = description;
			result.AC_ChargeGroup = chargeGroup;
			result.AC_ChargeSubGroup = chargeSubGroup;
			result.AC_ChargeType = chargeType;
			result.AC_IsActive = true;
			result.SetGLAccountDataForTesting();
			SetTemporaryDepartmentValueOnChargeCode(result);

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			result.AC_AT_GSTRate = rate.PK;

			return result;
		}

		void SetTemporaryDepartmentValueOnChargeCode(AccChargeCode accChargeCode)
		{
			accChargeCode.AC_DepartmentFilterList = "ALL";
		}

		#endregion

		#region CreateClientRate

		public ClientRate CreateClientRate(OrgHeader client)
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;
			return clientRate;
		}

		#endregion

		#region CreateCosting

		public Costing CreateCosting(OrgHeader supplier)
		{
			var result = Factory.New<Costing>();

			if (supplier != null)
			{
				result.TH_OH = supplier.PK;
				supplier.OH_IsCreditor = true;
			}

			return result;
		}

		#endregion

		#region CreateDispatchDLLDTUPivot

		public WhsItemDispatchLoadListDTUPivot CreateDispatchDLLDTUPivot(ZGuid dllpk, ZGuid dtuPK)
		{
			var dispatchLoadListDTUPivot = Factory.NewWithValidTestData<WhsItemDispatchLoadListDTUPivot>();
			dispatchLoadListDTUPivot.WLD_WDL_TransitDispatchLoadList = dllpk;
			dispatchLoadListDTUPivot.WLD_WDH_TransitDispatchTransportationUnit = dtuPK;
			return dispatchLoadListDTUPivot;
		}

		#endregion

		#region CreateDispatchLoadList

		public WhsItemDispatchLoadList CreateDispatchLoadList(ZString reference, ZGuid warehousePK, WhsLocation stagingLocation = null, bool isReadyToStage = false, OrgHeader creditor = null)
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_WW_Warehouse = warehousePK;
			dispatchLoadList.WDL_JobID = reference;
			dispatchLoadList.WDL_ReferenceNumber = reference;
			dispatchLoadList.WDL_WL_StagingLocation = stagingLocation?.PK ?? Guid.Empty;
			dispatchLoadList.WDL_IsReadyToStage = isReadyToStage;

			if (creditor != null)
			{
				CreateJobDocAddressFromAddress(dispatchLoadList, DocAddressTypes.Codes.Creditor, creditor.MainAddress);
			}

			return dispatchLoadList;
		}

		#endregion

		#region CreateDispatchConsignment

		public WhsItemDispatchConsignment CreateDispatchConsignment(ZString consignmentID, ZGuid warehousePK, OrgHeader bookedByParty = null, OrgHeader consignor = null, OrgHeader consignee = null, OrgHeader deliveryAddress = null, OrgHeader transportCompany = null, OrgHeader departureCTO = null)
		{
			var dispatchConsignment = CreateDispatchConsignment(consignmentID, warehousePK, "");

			if (bookedByParty != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookedByParty.MainAddress);
			}
			if (consignor != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
			}

			if (consignee != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consignee.MainAddress);
			}

			if (deliveryAddress != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, deliveryAddress.MainAddress);
			}

			if (transportCompany != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			}

			if (departureCTO != null)
			{
				CreateJobDocAddressFromAddress(dispatchConsignment, DocAddressTypes.Codes.DepartureCTOAddress, departureCTO.MainAddress);
			}

			return dispatchConsignment;
		}

		public void LoadPackage(WhsItemDispatchTransportationUnit dtu, WhsItemPackageState package)
		{
			package.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			package.WPS_WDH_TransitDispatchHeader = dtu.PK;
			package.WPS_IsSecure = true;
			package.WPS_SecurityStatus = "SEC";
		}

		#endregion

		#region CreateDispatchConsignment

		public WhsItemDispatchConsignment CreateDispatchConsignment(string consignmentID, ZGuid warehousePK, ZString serviceLevel = default(ZString), string jobID = "", ZGuid? parentPK = null, string parentCode = "", string transportMode = "", string direction = "", bool isAuthorizedForDispatch = true)
		{
			var dispatchConsignment = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dispatchConsignment.WDC_ConsignmentID = consignmentID;
			dispatchConsignment.WDC_WW_Warehouse = warehousePK;
			dispatchConsignment.WDC_RS_NKServiceLevel = serviceLevel;
			dispatchConsignment.WDC_JobID = string.IsNullOrEmpty(jobID) ? consignmentID : jobID;
			dispatchConsignment.WDC_TransportMode = transportMode;
			dispatchConsignment.WDC_Direction = direction;
			dispatchConsignment.WDC_IsAuthorizedForDispatch = isAuthorizedForDispatch;
			if (parentPK.HasValue)
			{
				dispatchConsignment.WDC_ParentID = parentPK.Value;
				dispatchConsignment.WDC_ParentTableCode = parentCode;
			}
			return dispatchConsignment;
		}

		#endregion

		#region CreateDispatchTransportationUnit

		public WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(ZString reference, ZGuid warehousePK, string vehicleRef = "V1", string dtuUnitType = TransportUnitTypes.Vehicle, string packageType = PkgUnit.Unit)
		{
			var (dtu, _) = CreateDispatchTransportationUnitWithPackage(reference, warehousePK, vehicleRef, dtuUnitType, packageType);

			return dtu;
		}

		public (WhsItemDispatchTransportationUnit, PkgPackage) CreateDispatchTransportationUnitWithPackage(ZString reference, ZGuid warehousePK, string vehicleRef = "V1", string dtuUnitType = TransportUnitTypes.Vehicle, string packageType = PkgUnit.Unit)
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			dtu.WDH_ReferenceNumber = reference;
			dtu.WDH_WW_Warehouse = warehousePK;
			dtu.WDH_VehicleReference = vehicleRef;
			dtu.WDH_UnitType = dtuUnitType;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dtu);
			var package = PackingHelper.CreatePackage(packageJob, vehicleRef, 1, packageType);

			if (!package.PackageExtensions.Any(p => p.KPN_ParentTableCode == WhsItemDispatchTransportationUnitSchema.Constants.Prefix))
			{
				PackingHelper.CreatePackageExtension(dtu, package);
			}

			return (dtu, package);
		}

		public void FinishLoading(WhsItemDispatchTransportationUnit dtu)
		{
			var now = DateTimeOffset.Now;
			FinishLoading(dtu, now.AddDays(-2), now, now.AddDays(1));
		}

		public void FinishLoading(WhsItemDispatchTransportationUnit dtu, ZDateTimeOffset gateIn, ZDateTimeOffset loadComplete, ZDateTimeOffset gateOut)
		{
			foreach (var packageState in dtu.PackageStates)
			{
				packageState.WPS_Status = gateOut.IsValid ? TransitWarehouseStatuses.Codes.Departed : TransitWarehouseStatuses.Codes.FreightLoaded;
			}

			dtu.WDH_GateInTime = gateIn.WithoutSeconds();
			dtu.WDH_LoadCompleteTime = loadComplete.WithoutSeconds();
			dtu.WDH_GateOutTime = gateOut.WithoutSeconds();

			CompleteLoadListsIfLoaded(dtu, dtu.WDH_LoadCompleteTime);
		}

		void CompleteLoadListsIfLoaded(WhsItemDispatchTransportationUnit dtu, ZDateTimeOffset completeTime)
		{
			foreach (var loadList in dtu.DispatchLoadLists)
			{
				if (loadList.WDL_CompleteTime.IsEmpty &&
					loadList.DispatchTransportationUnits.Any() &&
					loadList.DispatchTransportationUnits.All(d => d.WDH_LoadCompleteTime.IsValid) &&
					loadList.PackageStates.Any() &&
					loadList.PackageStates.All(p => p.WPS_LoadedTime.IsValid))
				{
					loadList.WDL_CompleteTime = completeTime;
					loadList.WDL_IsReadyToStage = false;
				}
			}
		}

		public void CompleteLoadList(WhsItemDispatchLoadList loadList, ZDateTimeOffset completeTime)
		{
			loadList.WDL_CompleteTime = completeTime;
			loadList.WDL_IsReadyToStage = false;
		}

		public void FinaliseDTU(WhsItemDispatchTransportationUnit dtu)
		{
			foreach (var packageState in dtu.PackageStates)
			{
				packageState.WPS_Status = TransitWarehouseStatuses.Codes.Finalized;
			}
			dtu.WDH_FinalisedTime = dtu.WDH_GateOutTime.AddDays(1).WithoutSeconds();
		}

		public WhsItemDispatchTransportationUnit CreateDispatchTransportationUnitWithContainerType(ZString reference, ZGuid warehousePK, string containerID = "CNT1", string containerType = "20GP")
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			var unitType = TransportUnitTypes.ConvertTypeToTransportUnitType(refContainer);

			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			dtu.WDH_ReferenceNumber = reference;
			dtu.WDH_WW_Warehouse = warehousePK;
			dtu.WDH_VehicleReference = containerID;
			dtu.WDH_UnitType = unitType;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dtu);
			PackingHelper.CreatePackage(packageJob, containerID, 1, PkgUnit.Container, containerType);

			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = dtu.PackageExtension.KPN_KP_Package;
			packageState.WPS_WW_Warehouse = warehousePK;
			packageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			packageState.WPS_IsHandlingUnit = true;
			packageState.WPS_UnitType = ConvertUnitType(unitType, packageState.WPS_IsHandlingUnit);
			packageState.WPS_CustomsStatus = TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired;

			return dtu;
		}

		public string ConvertUnitType(string containerUnitType, bool isHandlingUnit)
		{
			switch (containerUnitType)
			{
				case TransportUnitTypes.Vehicle:
					return isHandlingUnit ? PackageStateUnitType.Codes.HandlingUnit : PackageStateUnitType.Codes.Package;
				case TransportUnitTypes.Container:
					return PackageStateUnitType.Codes.SeaContainer;
				case TransportUnitTypes.ULD:
					return PackageStateUnitType.Codes.AirULDContainer;
				default:
					return string.Empty;
			}
		}

		public WhsItemDispatchTransportationUnit CreateDispatchTransportationUnitWithCreateTime(ZString reference, ZGuid warehousePK, string vehicleRef, ZDateTime createTime)
		{
			var dtu = CreateDispatchTransportationUnit(reference, warehousePK, vehicleRef);
			dtu.WDH_SystemCreateTimeUtc = createTime;

			return dtu;
		}

		#endregion

		#region CreateReceiveConsignment

		public WhsItemReceiveConsignment CreateReceiveConsignment(ZString consignmentID, ZGuid warehousePK, OrgHeader bookedByParty = null, OrgHeader consignor = null, OrgHeader consignee = null, string serviceLevel = "", OrgHeader arrivalCTO = null, OrgHeader pickup = null, string unloco = "")
		{
			var receiveConsignment = CreateReceiveConsignment(consignmentID, serviceLevel, warehousePK, consignmentID, unloco: unloco);

			if (bookedByParty != null)
			{
				CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookedByParty.MainAddress);
			}
			if (consignor != null)
			{
				CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
			}

			if (consignee != null)
			{
				CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consignee.MainAddress);
			}

			if (arrivalCTO != null)
			{
				CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.ArrivalCTOAddress, arrivalCTO.MainAddress);
			}

			if (pickup != null)
			{
				CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.ConsignorPickupDeliveryAddress, pickup.MainAddress);
			}

			return receiveConsignment;
		}

		public WhsItemReceiveConsignment CreateReceiveConsignment(string consignmentID, ZString serviceLevel, ZGuid warehousePK, string jobID = "", ZGuid? parentPK = null, string parentCode = "", string direction = "", string unloco = "")
		{
			var receiveConsignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_ConsignmentID = consignmentID;
			receiveConsignment.WRC_JobID = !string.IsNullOrEmpty(jobID) ? jobID : consignmentID;
			receiveConsignment.WRC_RS_NKServiceLevel = serviceLevel;
			receiveConsignment.WRC_WW_IntendedWarehouse = warehousePK;
			receiveConsignment.WRC_Direction = direction;
			receiveConsignment.WRC_CustomsStatus = TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired;
			receiveConsignment.WRC_RL_NKDestination = unloco;
			if (parentPK.HasValue)
			{
				receiveConsignment.WRC_ParentID = parentPK.Value;
				receiveConsignment.WRC_ParentTableCode = parentCode;
			}
			return receiveConsignment;
		}

		public JobDocAddress CreateJobDocAddressFromAddress(BusinessObject bizo, string type, OrgAddress address)
		{
			var jda = CreateJobDocAddress(bizo, type);
			jda.E2_OA_Address = address.PK;
			return jda;
		}

		public void CreateJobDocAddressWithOverride(BusinessObject bizo, string type, string companyName = null)
		{
			var jda = CreateJobDocAddress(bizo, type);
			jda.E2_AddressOverride = true;
			jda.E2_CompanyName = companyName;
		}

		public bool HasJobDocAddressOfType(BusinessObject bizo, string type)
		{
			var query = new ZQuery();
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, type);
			query.AddToFilter(JobDocAddressSchema.E2_ParentID, bizo.PK);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, bizo.TablePrefix);
			return bizo.Factory.Load<JobDocAddress>(query).Any();
		}

		public JobDocAddress CreateJobDocAddress(BusinessObject bizo, string type, ZGuid addressPK = new ZGuid())
		{
			var jda = Factory.New<JobDocAddress>();
			jda.E2_AddressType = type;
			jda.E2_ParentID = bizo.PK;
			jda.E2_ParentTableCode = bizo.TablePrefix;
			jda.E2_OA_Address = addressPK;

			return jda;
		}

		#endregion

		#region CreateReceiveASNRTUPivot

		public WhsItemReceiveASNRTUPivot CreateReceiveASNRTUPivot(ZGuid rtuPK, ZGuid asnPK)
		{
			var receiveASNRTUPivot = Factory.New<WhsItemReceiveASNRTUPivot>();
			receiveASNRTUPivot.WAR_WRH_TransitReceiveTransportationUnit = rtuPK;
			receiveASNRTUPivot.WAR_WRP_TransitReceiveASN = asnPK;
			return receiveASNRTUPivot;
		}

		#endregion

		#region CreateReceiveASN

		public WhsItemReceiveASN CreateReceiveASN(ZString asnID, ZGuid warehousePK, OrgHeader bookedByParty, OrgHeader transportCompany)
		{
			var receiveASN = CreateReceiveASN(asnID, warehousePK);

			CreateJobDocAddressFromAddress(receiveASN, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookedByParty.MainAddress);
			CreateJobDocAddressFromAddress(receiveASN, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			return receiveASN;
		}

		public WhsItemReceiveASN CreateReceiveASN(ZString asnID, ZGuid warehousePK)
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			receiveASN.WRP_ReferenceNumber = asnID;
			receiveASN.WRP_WW_IntendedWarehouse = warehousePK;
			return receiveASN;
		}

		#endregion

		#region CreateReceiveTransportationUnit

		public WhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(ZString reference, ZGuid warehousePK, ZGuid locationPK, string vehicleRef = "V1", ZDateTimeOffset? gateIn = null, ZDateTimeOffset? unLoadCompleteTime = null, string rtuUnitType = TransportUnitTypes.Vehicle)
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_ReferenceNumber = reference;
			rtu.WRH_WL_StagingLocation = locationPK;
			rtu.WRH_WW_Warehouse = warehousePK;
			rtu.WRH_VehicleReference = vehicleRef;
			rtu.WRH_UnloadCompleteTime = unLoadCompleteTime ?? ZDateTimeOffset.Empty;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.WRH_GateInTime = gateIn ?? ZDateTimeOffset.Empty;
			rtu.WRH_UnitType = rtuUnitType;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rtu);
			var package = PackingHelper.CreatePackage(packageJob, vehicleRef, 1, PkgUnit.Unit);

			if (!package.PackageExtensions.Any(p => p.KPN_ParentTableCode == WhsItemReceiveTransportationUnitSchema.Constants.Prefix))
			{
				PackingHelper.CreatePackageExtension(rtu, package);
			}

			return rtu;
		}

		public WhsItemReceiveTransportationUnit CreateReceiveTransportationUnitWithContainerType(ZString reference, ZGuid warehousePK, ZGuid locationPK, string containerID = "CNT1", string containerType = "20GP", string containerMode = "FCL", string vehicleRef = null)
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			var unitType = TransportUnitTypes.ConvertTypeToTransportUnitType(refContainer);

			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_ReferenceNumber = reference;
			rtu.WRH_VehicleReference = vehicleRef ?? reference;
			rtu.WRH_WL_StagingLocation = locationPK;
			rtu.WRH_WW_Warehouse = warehousePK;
			rtu.WRH_UnitType = unitType;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rtu);
			var package = PackingHelper.CreatePackage(packageJob, containerID, 1, PkgUnit.Container, containerType);
			package.Container.K0_ContainerMode = containerMode;

			if (!package.PackageExtensions.Any(p => p.KPN_ParentTableCode == WhsItemReceiveTransportationUnitSchema.Constants.Prefix))
			{
				PackingHelper.CreatePackageExtension(rtu, package);
			}

			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WW_Warehouse = warehousePK;
			packageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			packageState.WPS_IsHandlingUnit = true;
			packageState.WPS_CustomsStatus = TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired;
			packageState.WPS_UnitType = ConvertUnitType(unitType, packageState.WPS_IsHandlingUnit);

			return rtu;
		}

		public void FinishUnloading(WhsItemReceiveTransportationUnit rtu, ZDateTimeOffset gateIn, ZDateTimeOffset loadComplete, ZDateTimeOffset gateOut)
		{
			rtu.WRH_GateInTime = gateIn.WithoutSeconds();
			rtu.WRH_UnloadCompleteTime = loadComplete.WithoutSeconds();
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.WRH_GateOutTime = gateOut.WithoutSeconds();
		}

		#endregion

		#region CreateRateEntry

		public RateEntry CreateRateEntry(ClientRate clientRate, string rateCategory, WhsWarehouse warehouse)
		{
			var entry = CreateRateEntry(clientRate, rateCategory);
			entry.TI_RateStartDate = ZDateTime.Now.AddDays(-5).Date;
			entry.TI_RateEndDate = ZDate.Empty;
			entry.TI_WW_Warehouse = warehouse?.PK ?? ZGuid.Empty;
			return entry;
		}

		public RateEntry CreateRateEntry(ClientRate clientRate, string category)
		{
			var warehouseEntry = clientRate.AddRateEntry(category, "ALL", "", "", "", "");
			warehouseEntry.RateLines.RemoveAndDeleteAll();

			return warehouseEntry;
		}

		#endregion

		#region CreateRateLine

		public RateLine CreateRateLine(RateEntry rateEntry, AccChargeCode chargeCode, ZString unit, ZDecimal price)
		{
			return CreateRateLine(rateEntry, chargeCode, unit, price, UnitCalculator.Code);
		}

		public RateLine CreateRateLine(RateEntry rateEntry, AccChargeCode chargeCode, ZString unit, ZDecimal price, ZString calculator, bool isPalletized = false)
		{
			RateLine rateLine = rateEntry.AddRateLine(chargeCode, calculator, unit);
			rateLine.Calculator.Decimal1 = price;
			rateLine.TL_IsOnPallets = isPalletized;
			return rateLine;
		}

		#endregion

		#region CreatePackageHandlingUnit

		public PkgHandlingUnit CreatePackageHandlingUnit()
		{
			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "TWH";
			return handlingUnit;
		}

		#endregion

		#region CreatePortReference

		public PortReference CreatePortReference(string code, string description, string countryCode, string reference, ZString? statusCode)
		{
			var referenceType = new PortReferenceType() { Code = code, Description = description };
			var country = new Country { Code = countryCode };

			if (statusCode.HasValue)
			{
				var status = new PortReferenceStatus { Code = statusCode };
				return new PortReference { Type = referenceType, Country = country, Reference = reference, Status = status };
			}

			return new PortReference { Type = referenceType, Country = country, Reference = reference };
		}

		#endregion

		#region CreateAdditionalReference

		public AdditionalReference CreateAdditionalReference(string type, string description, string reference, string contextInformation, ZDateTime issueDate)
		{
			return new AdditionalReference
			{
				Type = new EntryType { Code = type, Description = description },
				ReferenceNumber = reference,
				ContextInformation = contextInformation,
				IssueDate = issueDate
			};
		}

		#endregion

		#region CreateCustomsReference

		public EntryNumber CreateEntryReference(string type, string description, string reference, string lineReference, bool isSystem, string entryStatus, ZDateTime issueDate, ZDateTime expiryDate, string countryCode)
		{
			return new EntryNumber
			{
				Type = new EntryType { Code = type, Description = description },
				Number = reference,
				EntryLineReference = lineReference,
				EntryIsSystemGenerated = isSystem,
				EntryStatus = new EntryStatus { Code = entryStatus },
				IssueDate = issueDate,
				ExpiryDate = expiryDate,
				CountryOfIssue = new Country { Code = countryCode }
			};
		}

		#endregion

		#region CreateTransitReferenceMapping

		public TransitReferenceMapping CreateTransitReferenceMapping(string sourceCategory, string sourceType, string targetCategory, string targetType)
		{
			return new TransitReferenceMapping()
			{
				SourceCategory = sourceCategory,
				SourceType = sourceType,
				TargetCategory = targetCategory,
				TargetType = targetType
			};
		}

		#endregion

		#region PackPackageIntoHandlingUnit

		public PkgPackageHandlingUnitDivot PackPackageIntoHandlingUnit(WhsItemPackageState handlingPackage, WhsItemPackageState childPackage, ZDateTimeOffset packedTime, ZString packedUser, WhsItemPackageState topHandlingUnit = null)
		{
			var divot = Factory.New<PkgPackageHandlingUnitDivot>();
			divot.KPD_KP_HandlingUnit = handlingPackage.Package.PK;
			divot.KPD_KP_Package = childPackage.Package.PK;
			divot.KPD_PackedTime = packedTime;
			divot.KPD_GS_NKPackedUser = packedUser;

			if (handlingPackage.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
			{
				childPackage.Package.KP_KP_ParentPackage = handlingPackage.WPS_KP_Package;
			}

			if (topHandlingUnit != null)
			{
				childPackage.Package.KP_KP_TopHandlingUnitPackage = topHandlingUnit.WPS_KP_Package;
			}

			return divot;
		}

		public PkgPackageHandlingUnitDivot PackPackageIntoHandlingUnit(WhsItemPackageState handlingPackage, WhsItemPackageState childPackage, ZDateTimeOffset packedTime, ZString packedUser, ZDateTimeOffset unpackedTime)
		{
			var divot = PackPackageIntoHandlingUnit(handlingPackage, childPackage, packedTime, packedUser);
			divot.KPD_UnpackedTime = unpackedTime;
			divot.KPD_GS_NKUnpackedUser = packedUser;
			childPackage.Package.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			childPackage.Package.KP_KP_ParentPackage = ZGuid.Empty;

			return divot;
		}

		#endregion

		#region UnpackPackageFromHandlingUnit

		public void UnpackPackageFromHandlingUnit(PkgPackageHandlingUnitDivot openDivot, ZDateTimeOffset unpackedTime, ZString unpackedUser)
		{
			openDivot.KPD_UnpackedTime = unpackedTime;
			openDivot.KPD_GS_NKUnpackedUser = unpackedUser;
			openDivot.Package.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			openDivot.Package.KP_KP_ParentPackage = ZGuid.Empty;
		}

		#endregion

		#region CreateHandlingUnitPackage

		public WhsItemPackageState CreateHandlingUnitPackage(string packageId, PkgHandlingUnit handlingUnit, WhsItemReceiveTransportationUnit receiveUnit, string status = "ARV", string handlingUnitPackageType = "PLT", string unitType = PackageStateUnitType.Codes.HandlingUnit, WhsItemReceiveConsignment rcn = null, WhsItemReceiveASN asn = null, WhsItemDispatchConsignment dcn = null, WhsItemDispatchLoadList dll = null, WhsItemDispatchTransportationUnit dtu = null, string entryNum = "")
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var package = PackingHelper.CreatePackage(packageJob, packageId, 1, handlingUnitPackageType);
			var handlingUnitPackageState = CreatePackageState(package, status, receiveUnit: receiveUnit, dispatchConsignment: dcn, receiveConsignment: rcn, unitType: unitType, receiveASN: asn, dispatchLoadList: dll, dispatchUnit: dtu);
			handlingUnitPackageState.WPS_IsHandlingUnit = true;

			if (!string.IsNullOrEmpty(entryNum))
			{
				var ent = Factory.New<CusEntryNumber>();
				ent.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
				ent.CE_EntryNum = entryNum;
				ent.CE_ParentID = handlingUnitPackageState.PK;
				ent.CE_ParentTable = WhsItemPackageStateSchema.Constants.TableName;
				ent.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			}

			return handlingUnitPackageState;
		}
		#endregion

		#region CreateOverpackPackage

		public WhsItemPackageState CreateOverpackPackage(string packageId, IPackingParent packingParent, WhsItemReceiveTransportationUnit receiveUnit, string status = "ARV", string overpackUnitPackageType = "PLT", string unitType = PackageStateUnitType.Codes.Overpack, WhsItemReceiveConsignment rcn = null, WhsItemReceiveASN asn = null, WhsItemDispatchConsignment dcn = null, WhsItemDispatchLoadList dll = null, WhsItemDispatchTransportationUnit dtu = null, string entryNum = "")
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);
			var package = PackingHelper.CreatePackage(packageJob, packageId, 1, overpackUnitPackageType);
			var overpacPackageState = CreatePackageState(package, status, receiveUnit: receiveUnit, dispatchConsignment: dcn, receiveConsignment: rcn, unitType: unitType, receiveASN: asn, dispatchLoadList: dll, dispatchUnit: dtu);
			overpacPackageState.WPS_IsHandlingUnit = true;

			if (!string.IsNullOrEmpty(entryNum))
			{
				var ent = Factory.New<CusEntryNumber>();
				ent.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
				ent.CE_EntryNum = entryNum;
				ent.CE_ParentID = overpacPackageState.PK;
				ent.CE_ParentTable = WhsItemPackageStateSchema.Constants.TableName;
				ent.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			}

			return overpacPackageState;
		}

		#endregion

		#region CreateAndAttachArrivedInners

		public void CreateAndAttachArrivedInners(WhsItemPackageState parentPackageState, ZInt innerQuantity, ZString packageType)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentPackageState.ReceiveConsignment);
			var package = PackingHelper.CreatePackage(packageJob, innerQuantity, packageType);
			package.KP_KP_ParentPackage = parentPackageState.WPS_KP_Package;
		}

		#endregion

		#region CreatePackage

		public PkgPackage CreatePackage(PkgPackageJob packageJob, ZString packageID, ZInt qty, ZString packageType)
		{
			return PackingHelper.CreatePackage(packageJob, packageID, qty, packageType);
		}

		public void SetupPackageForOutturn(PkgPackage package, int bookedQty, int damagedQty, decimal height, decimal length, decimal volume, decimal weight, decimal width)
		{
			package.KP_Height = height;
			package.KP_Length = length;
			package.KP_Volume = volume;
			package.KP_Weight = weight;
			package.KP_Width = width;
			package.KP_IsDamaged = damagedQty > 0;
			package.BookedDimensions.KPB_PackageQty = bookedQty;
		}

		#endregion

		#region CreatePackageBookedDetail

		public PkgPackageBookedDetail CreatePackageBookedDetail(PkgPackage package, decimal volume = 0.0m, string volumeUQ = "M3", decimal weight = 0.0m, string weightUQ = "KG")
		{
			var packageBookedDetails = Factory.New<PkgPackageBookedDetail>();
			packageBookedDetails.KPB_Volume = volume;
			packageBookedDetails.KPB_VolumeUQ = volumeUQ;
			packageBookedDetails.KPB_Weight = weight;
			packageBookedDetails.KPB_WeightUQ = weightUQ;
			packageBookedDetails.KPB_KP_Package = package.PK;

			return packageBookedDetails;
		}

		#endregion

		#region CreatePackageState

		public WhsItemPackageState CreatePackageState(WhsItemReceiveConsignment receiveConsignment, int qty, string packageType, string packageId, ZString status, WhsItemReceiveTransportationUnit receiveUnit = null, WhsItemDispatchConsignment dispatchConsignment = null, WhsItemDispatchTransportationUnit dispatchUnit = null,
			WhsItemDispatchLoadList dispatchLoadList = null, WhsItemReceiveASN receiveASN = null, WhsLocation location = null, string entryNum = "", decimal volume = 0.0m, string volumeUQ = "M3", decimal weight = 0.0m, string weightUQ = "KG", string adjustedOut = null, bool isDamaged = false, bool isPillaged = false, string externalReference = null, string unitType = PackageStateUnitType.Codes.Package, bool isHighRisk = false)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			var package = (PkgPackage)PackingHelper.CreatePackage(packageJob.PK, packageId, qty, packageType, volume, volumeUQ, weight, weightUQ);
			package.KP_IsDamaged = isDamaged;
			package.KP_IsPillaged = isPillaged;
			package.KP_ExternalReference = externalReference;

			var packageState = CreatePackageState(package, status, receiveConsignment, receiveUnit, dispatchConsignment, dispatchUnit, dispatchLoadList, receiveASN, adjustedOut, location, unitType, isHighRisk);

			if (status == TransitWarehouseStatuses.Codes.Booked)
			{
				CreatePackageBookedDetail(package, volume, volumeUQ, weight, weightUQ);
			}

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

		public WhsItemPackageState CreatePackageState(WhsItemReceiveTransportationUnit receiveUnit, int qty, string packageType, string packageId, ZString status, WhsItemDispatchConsignment dispatchConsignment = null, WhsItemDispatchTransportationUnit dispatchUnit = null, WhsItemDispatchLoadList dispatchLoadList = null, WhsItemReceiveASN receiveASN = null, decimal volume = 0.0m, string volumeUQ = "M3", decimal weight = 0.0m, string weightUQ = "KG", string adjustedOut = null)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(receiveUnit);
			var package = (PkgPackage)PackingHelper.CreatePackage(packageJob.PK, packageId, qty, packageType, volume, volumeUQ, weight, weightUQ);
			var packageState = CreatePackageState(package, status, receiveConsignment: null, receiveUnit, dispatchConsignment, dispatchUnit, dispatchLoadList, receiveASN, adjustedOut);
			return packageState;
		}

		public WhsItemPackageState CreatePackageState(PkgPackage package, ZString status, WhsItemReceiveConsignment receiveConsignment = null, WhsItemReceiveTransportationUnit receiveUnit = null, WhsItemDispatchConsignment dispatchConsignment = null, WhsItemDispatchTransportationUnit dispatchUnit = null, WhsItemDispatchLoadList dispatchLoadList = null, WhsItemReceiveASN receiveASN = null, string adjustedOut = null, WhsLocation location = null, string unitType = PackageStateUnitType.Codes.Package, bool isHighRisk = false)
		{
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK.ToGuid();
			packageState.WPS_Status = status;
			packageState.WPS_UnitType = unitType;
			packageState.WPS_IsHighRisk = isHighRisk;
			packageState.WPS_CustomsStatus = TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired;

			if (receiveConsignment != null)
			{
				packageState.WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK.ToGuid();
				packageState.WPS_WW_Warehouse = receiveConsignment.WRC_WW_IntendedWarehouse;
			}

			if (receiveUnit != null)
			{
				packageState.WPS_WRH_TransitReceiveHeader = receiveUnit.PK;
				if (status != TransitWarehouseStatuses.Codes.Booked)
				{
					packageState.WPS_WL_LastLocation = location != null ? location.PK : receiveUnit.WRH_WL_StagingLocation;
					packageState.WPS_UnloadedTime = ZDateTimeOffset.Now;
				}

				if (packageState.WPS_WW_Warehouse.IsEmpty)
				{
					packageState.WPS_WW_Warehouse = receiveUnit.WRH_WW_Warehouse;
				}
			}

			if (dispatchConsignment != null)
			{
				packageState.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;

				if (packageState.WPS_WW_Warehouse.IsEmpty)
				{
					packageState.WPS_WW_Warehouse = dispatchConsignment.WDC_WW_Warehouse;
				}
			}

			if (dispatchUnit != null)
			{
				packageState.WPS_WDH_TransitDispatchHeader = dispatchUnit.PK;
				packageState.WPS_IsSecure = true;
				packageState.WPS_SecurityStatus = "SEC";

				if (packageState.WPS_WW_Warehouse.IsEmpty)
				{
					packageState.WPS_WW_Warehouse = dispatchUnit.WDH_WW_Warehouse;
				}
			}

			if (dispatchLoadList != null)
			{
				packageState.WPS_WDL_LoadList = dispatchLoadList.PK;

				if (packageState.WPS_WW_Warehouse.IsEmpty)
				{
					packageState.WPS_WW_Warehouse = dispatchLoadList.WDL_WW_Warehouse;
				}
			}

			if (receiveASN != null)
			{
				packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
				if (packageState.WPS_WW_Warehouse.IsEmpty)
				{
					packageState.WPS_WW_Warehouse = receiveASN.WRP_WW_IntendedWarehouse;
				}
			}

			if (status == TransitWarehouseStatuses.Codes.AdjustedOut)
			{
				packageState.WPS_AdjustedOut = adjustedOut ?? "ADJ";
			}

			return packageState;
		}

		#endregion

		#region CreateCycleCountLocation

		public WhsItemCycleCountLocation CreateCycleCountLocation(WhsLocation location, string status, ZDateTimeOffset startTime, ZDateTimeOffset processingTime, ZDateTimeOffset endTime, string assignedTo = "", WhsItemCycleCountLocation rejectedCycleCount = null, byte priority = 20)
		{
			var cycleCount = Factory.New<WhsItemCycleCountLocation>();
			cycleCount.WIC_WL_Location = location.PK;
			cycleCount.WIC_Status = status;
			cycleCount.WIC_StartTime = startTime;
			cycleCount.WIC_ProcessingTime = processingTime;
			cycleCount.WIC_EndTime = endTime;
			cycleCount.WIC_GS_NKAssignedTo = assignedTo;
			cycleCount.WIC_WIC_RejectedCycleCount = rejectedCycleCount == null ? ZGuid.Empty : rejectedCycleCount.PK;
			cycleCount.WIC_Priority = priority;

			return cycleCount;
		}

		#endregion

		#region CreateCycleCountLocationVariance

		public WhsItemCycleCountLocationVariance CreateCycleCountLocationVariance(WhsItemCycleCountLocation cycleCountLocation, WhsItemPackageState packageState = null, byte varianceQty = 0, string status = CycleCountVarianceStatuses.Codes.Open, string packageNotInWhsID = "", WhsLocation expectedLocation = null, WhsItemPackageState expectedHandlingUnit = null, WhsItemTransferLine transferLine = null, WhsItemReceiveConsignment rcn = null)
		{
			var variance = Factory.New<WhsItemCycleCountLocationVariance>();
			variance.WIV_WIC_CycleCountLocation = cycleCountLocation.PK;
			variance.WIV_WPS_PackageState = packageState?.PK ?? ZGuid.Empty;
			variance.WIV_VarianceQty = varianceQty;
			variance.WIV_Status = status;
			variance.WIV_PackageNotInWhsID = packageNotInWhsID;
			variance.WIV_WPS_ExpectedHandlingUnit = expectedHandlingUnit?.PK ?? ZGuid.Empty;
			variance.WIV_WL_ExpectedStockLocation = expectedLocation?.PK ?? ZGuid.Empty;
			variance.WIV_WTF_TransferLine = transferLine?.PK ?? ZGuid.Empty;
			variance.WIV_WRC_ReceiveConsignment = rcn?.PK ?? ZGuid.Empty;

			return variance;
		}

		#endregion

		#region CreateWarehouse

		public override WhsWarehouse CreateWarehouse(ZString name, bool shouldPreGenerateDDL = false)
		{
			var whs = base.CreateWarehouse(name, shouldPreGenerateDDL);
			whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			return whs;
		}

		public WhsWarehouse CreateTransitWarehouseInCurrentBranch(string code = "WH1")
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = CreateWarehouse(code, address, GlbBranch.CurrentBranch);
			whs.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			return whs;
		}

		#endregion

		#region CreateRow

		public WhsLocation CreateLocation(WhsWarehouse whs, string code = "Dock")
		{
			var row = CreateRowAndGenerateLocations(whs, code, 1, 1);
			return row.Locations.First(l => l.ToLocationString() == code);
		}

		#endregion

		#region CreateTransportRouting

		public ITransport CreateTransportRouting(WhsItemReceiveConsignment consignment, string voyage, string discPort, string loadPort)
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(WhsItemReceiveConsignment);
			transport.JW_ParentGUID = consignment.PK;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		public ITransport CreateTransportRouting(WhsItemReceiveASN asn, string voyage, string discPort, string loadPort)
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(WhsItemReceiveASN);
			transport.JW_ParentGUID = asn.PK;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		public ITransport CreateTransportRouting(WhsItemDispatchLoadList dll, string voyage, string discPort, string loadPort)
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(WhsItemDispatchLoadList);
			transport.JW_ParentGUID = dll.PK;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_VoyageFlight = voyage;

			return transport;
		}

		#endregion

		#region CreateTransport

		public Transport CreateTransport(WhsItemReceiveConsignment consignment, string discPort, string loadPort)
		{
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(WhsItemReceiveConsignment);
			transport.JW_ParentGUID = consignment.PK;
			transport.JW_ParentType = WhsItemReceiveConsignmentSchema.Constants.Prefix;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_RL_NKLoadPort = loadPort;

			return transport;
		}

		#endregion

		#region GetEnterpriseAndServerCodeNumberCustomisation

		public BillOfLadingNumberCustomisation GetEnterpriseAndServerCodeNumberCustomisation()
		{
			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			Array.ForEach(customisation.UnFilteredElements.ToArray<BillOfLadingNumberCustomisationElement>(), x => x.Include = false);
			IncludeCustomisationElement(customisation, BillOfLadingNumberCustomisationElement.Keys.EnterpriseCode, 1, true, "");
			IncludeCustomisationElement(customisation, BillOfLadingNumberCustomisationElement.Keys.ServerCode, 2, true, "");
			IncludeCustomisationElement(customisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 3, false, "8");
			return customisation;
		}

		void IncludeCustomisationElement(BillOfLadingNumberCustomisation customisation, ZString key, ZByte order, ZBool fountain, ZString detail)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Fountain = fountain;
			element.Detail = detail;
		}

		#endregion

		#region PackingHelper

		public PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#region Events

		public ProcessTask GetEventTrigger<T>(T entity, string eventType, string eventCode, string description)
		{
			var trigger = ((IWorkflowProvider)entity).WorkflowItems.AddNew();
			trigger.P9_Type = eventType;
			trigger.TriggerConditions.TriggerEventCode = eventCode;
			trigger.P9_Description = description;

			return trigger;
		}

		public ProcessTaskNotification GetEventNotification(ProcessTask eventTrigger, string triggerType, string triggerParty)
		{
			var notification = eventTrigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_TriggerParty = triggerParty;

			return notification;
		}

		#endregion

		#region CreatePackageScreening

		public PkgPackageScreening CreatePackageScreening(PkgPackage package, string methodCode, bool passed = false)
		{
			var packageScreening = package.Screenings.AddNew();
			packageScreening.KPS_GS_NKScreenedBy = "STF";
			packageScreening.KPS_Passed = passed;
			packageScreening.KPS_Method = methodCode;
			packageScreening.KPS_Time = DateTimeOffset.Now;

			return packageScreening;
		}

		#endregion

		#region CreateTransitTransfer

		public WhsItemTransferHeader CreateTransitTransfer(string transferType, string refNumber, ZGuid warehousePK, bool isFinalised)
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_ReferenceNumber = refNumber;
			transferHeader.WTH_TransferType = transferType;
			transferHeader.WTH_WW_Warehouse = warehousePK;
			transferHeader.WTH_IsFinalised = isFinalised;
			return transferHeader;
		}

		#endregion

		#region CreateTransitTransferLine

		public WhsItemTransferLine CreateTransitTransferLine(WhsItemTransferHeader transferHeader,
			WhsItemPackageState packageState, WhsLocation from, WhsLocation to, DateTime? pickTime = null, string pickUser = "", DateTime? putTime = null, string putUser = "")
		{
			var line = transferHeader.Lines.AddNew();
			line.WTF_WPS_PackageState = packageState.PK;
			line.WTF_WL_From = from.PK;
			line.WTF_WL_To = to.PK;
			line.WTF_WTH_TransitTransferHeader = transferHeader.PK;
			if (pickTime.HasValue)
			{
				line.WTF_PickTime = pickTime.Value;
				line.WTF_GS_NKPickUser = pickUser;
			}

			if (putTime.HasValue)
			{
				line.WTF_PutTime = putTime.Value;
				line.WTF_GS_NKPutUser = putUser;
			}
			return line;
		}

		#endregion

		#region SetPremiseIDFortWarehouseAddress

		public static void SetPremiseIDForWarehouse(WhsWarehouse warehouse, string premiseId, string countryCode = null)
		{
			var orgCusCode = warehouse.WarehouseAddress.CustomsCodes.AddNew();
			orgCusCode.OK_OA_PremisesAddress = warehouse.WarehouseAddress.PK;
			orgCusCode.OK_OH = warehouse.WarehouseAddress.Header.PK;
			orgCusCode.OK_CustomsRegNo = premiseId;
			orgCusCode.OK_CodeType = CargoWise.Definitions.Customs.CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID;
			orgCusCode.OK_RN_NKCodeCountry = countryCode ?? GlbBranch.CurrentBranch.BaseCountry.Code;
		}

		#endregion

		#region AddOrgCode

		public OrgCusCode AddOrgCode(OrgAddress address, string type, string code)
		{
			var orgCusCode = address.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = type;
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			orgCusCode.OK_CustomsRegNo = code;

			return orgCusCode;
		}

		#endregion

		#region CreateOrgHeaderAndSetupEDICommunications

		public OrgHeader CreateOrgHeaderAndSetupEDICommunications(string ek_Module)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = ek_Module;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		#endregion

		#region DisableTopLevelHUFKForTest

		public void DisableTopLevelHUFKForTest(DbConnection testConnection)
		{
			// Disable Trigger to allow saving a Divot with no top handling unit
			testConnection.ExecuteNonQuery("DISABLE TRIGGER TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit ON PkgPackageHandlingUnitDivot");
		}

		public void DisableCheckOverpackInnersRCNDCNForTest(DbConnection testConnection)
		{
			// Disable Trigger to allow saving a Divot with no top handling unit
			testConnection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN ON WhsItemPackageState");
		}

		#endregion

		#region Assertions

		public PkgPackage AssertAndReturnPackage(IEnumerable<PkgPackage> packages, ZGuid packageJobParentPk, int expected, string packageId, string externalReference, string marksAndNumbers, decimal weight, string weightUQ, decimal length, decimal width, decimal height, string dimUQ, int packageQty, string packType, ZGuid? parentPackagePK = null)
		{
			AssertEquals(expected, packages.Count());

			var package = string.IsNullOrEmpty(packageId) ? packages.SingleOrDefault(p => p.KP_MarksAndNumbers == marksAndNumbers) : packages.Single(p => p.KP_PackageID == packageId);
			AssertEquals(packageJobParentPk, package.PackageJob?.KJ_ParentID);
			AssertEquals(packageQty, package.KP_PackageQty);
			AssertEquals(marksAndNumbers, package.KP_MarksAndNumbers);
			AssertEquals(weight, package.KP_Weight);
			AssertEquals(weightUQ, package.KP_WeightUQ);
			AssertEquals(length, package.KP_Length);
			AssertEquals(width, package.KP_Width);
			AssertEquals(height, package.KP_Height);
			AssertEquals(dimUQ, package.KP_DimensionUQ);
			AssertEquals(packType, package.KP_F3_NKPackType);
			AssertEquals(externalReference, package.KP_ExternalReference);
			if (parentPackagePK.HasValue)
			{
				AssertEquals(parentPackagePK, package.KP_KP_ParentPackage);
			}

			return package;
		}

		public void AssertAdditionalReferences(IEnumerable<CusEntryNumber> cusEntryNumbers, string entryType, string value, string name)
		{
			var additionalReference = cusEntryNumbers.Single(e => e.CE_EntryType == entryType);
			AssertEquals($"Should have imported {name} correctly for {entryType}.", value, additionalReference.CE_EntryNum);
			AssertEquals("Should populate as an additional reference.", CusEntryNumber.Categories.AdditionalReferenceNumber, additionalReference.CE_Category);
		}

		public void AssertAdditionalReference(CusEntryNumber cusEntryNumber, string entryType, string value, string countryCode = "", string status = "", string category = CusEntryNumber.Categories.CustomsPermitClearanceNumber)
		{
			AssertEquals($"Should have imported correct {entryType}.", entryType, cusEntryNumber.CE_EntryType);
			AssertEquals("Should populate as a customs additional reference.", category, cusEntryNumber.CE_Category);
			AssertEquals("Should populate customs reference value.", value, cusEntryNumber.CE_EntryNum);
			AssertEquals($"Should populate country code as {countryCode}.", countryCode, cusEntryNumber.CE_RN_NKCountryCode);
			AssertEquals($"Should populate status as {status}.", status, cusEntryNumber.CE_EntryStatus);
			AssertEquals($"System create type should not empty for {entryType}", false, cusEntryNumber.CE_SystemCreateTimeUtc.IsEmpty);
			AssertEquals($"System create user should not empty for {entryType}", false, cusEntryNumber.CE_SystemCreateUser.IsEmpty);
			AssertEquals($"System last edit time should not be empty for {entryType}.", false, cusEntryNumber.CE_SystemLastEditTimeUtc.IsEmpty);
			AssertEquals($"System last edit user should not be empty for {entryType}.", false, cusEntryNumber.CE_SystemLastEditUser.IsEmpty);
		}

		public void AssertPortReference(PortReference reference, string typeCode, string referenceValue, string status, string country)
		{
			AssertEquals(typeCode, reference.Type.Code);
			AssertEquals(referenceValue, reference.Reference.Value);
			AssertEquals(status, reference.Status.Code);
			AssertEquals(country, reference.Country.Code);
		}

		public void AssertAdditionalReference(AdditionalReference reference, string type, string referenceValue, string contextInformation, ZDateTime issueDate)
		{
			AssertEquals(type, reference.Type.Code);
			AssertEquals(referenceValue, reference.ReferenceNumber);
			AssertEquals(contextInformation, reference.ContextInformation);
			AssertEquals(issueDate, reference.IssueDate.Value);
		}

		public void AssertEntryReference(EntryNumber number, string type, string reference, string lineReferece, string entryStatus, bool isSystem, string countryCode, ZDateTime issueDate, ZDateTime expiryDate)
		{
			AssertEquals(type, number.Type.Code);
			AssertEquals(reference, number.Number.Value);
			AssertEquals(lineReferece, number.EntryLineReference);
			AssertEquals(entryStatus, number.EntryStatus.Code);
			AssertEquals(isSystem, number.EntryIsSystemGenerated);
			AssertEquals(countryCode, number.CountryOfIssue.Code);
			AssertEquals(issueDate, number.IssueDate.Value);
			AssertEquals(expiryDate, number.ExpiryDate.Value);
		}

		#endregion

		#region CreateNotes & AssertNoteContents

		public DataObjectList<Note> CreateNotes(string description, string noteText)
		{
			return new DataObjectList<Note>
			{
				new Note
				{
					Description = description,
					NoteText = noteText,
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "AAA", Description = "Note Context Description" },
					IsCustomDescription = false
				}
			};
		}

		public Note CreateNote(string description, string noteText, string visibility, string visibilityDesc)
		{
			return new Note
			{
				Description = description,
				NoteText = noteText,
				Visibility = new CodeDescriptionPair() { Code = visibility, Description = visibilityDesc },
				NoteContext = new NoteContext() { Code = "AAA", Description = "Note Context Description" },
				IsCustomDescription = false
			};
		}

		public void AssertNoteContents(StmNote noteBO, string description, string noteText, string noteType = "PUB", bool isCustomDescription = true)
		{
			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", description, noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteText", noteText, noteBO.ST_NoteText);
				AssertEquals("noteBO.ST_NoteType", noteType, noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_NoteContext", "AAA", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_IsCustomDescription", isCustomDescription, noteBO.ST_IsCustomDescription);
			});
		}

		#endregion

		#region CreateAdditionalService & AssertAdditionalService

		public AdditionalService CreateAdditionalService(string code, ZDateTime? bookedTime = null, ZDateTime? completedTime = null, OrganizationAddress contractor = null, OrganizationAddress location = null, ZDateTime? duration = null, ZString? reference = null, ZString? serviceNote = null, int count = 1, string serviceID = "")
		{
			return new AdditionalService()
			{
				ServiceCode = new CodeDescriptionPair { Code = code, Description = $"Test {code}" },
				Booked = bookedTime,
				Completed = completedTime,
				Contractor = contractor,
				Duration = duration,
				Location = location,
				References = reference,
				ServiceCount = count,
				ServiceNote = serviceNote,
				ServiceId = serviceID,
			};
		}

		public void AssertAdditionalService(JobService jobServiceBO, string code, ZDateTime? bookedTime = null, ZDateTime? completedTime = null, ZGuid? contractorPK = null, ZGuid? locationPK = null, ZDateTime? duration = null, ZString? reference = null, ZString? serviceNote = null, ZDecimal? count = null)
		{
			CombineAssertions(delegate
			{
				AssertEquals("jobServiceBO.ES_ServiceCode", code, jobServiceBO.ES_ServiceCode);
				AssertEquals("jobServiceBO.ES_Booked", bookedTime ?? ZDateTime.Empty, jobServiceBO.ES_Booked);
				AssertEquals("jobServiceBO.ES_Completed", completedTime ?? ZDateTime.Empty, jobServiceBO.ES_Completed);
				AssertEquals("jobServiceBO.ES_OH_Contractor", contractorPK ?? ZGuid.Empty, jobServiceBO.ES_OH_Contractor);
				AssertEquals("jobServiceBO.ES_Duration", duration ?? ZDateTime.Empty, jobServiceBO.ES_Duration);
				AssertEquals("jobServiceBO.ES_OA_Location", locationPK ?? ZGuid.Empty, jobServiceBO.ES_OA_Location);
				AssertEquals("jobServiceBO.ES_References", reference ?? ZString.Empty, jobServiceBO.ES_References);
				AssertEquals("jobServiceBO.ES_ServiceCount", count ?? 1, jobServiceBO.ES_ServiceCount);
				AssertEquals("jobServiceBO.ES_ServiceNote", serviceNote ?? ZString.Empty, jobServiceBO.ES_ServiceNote);
			});
		}

		public void AssertAddOnValue(GenCustomAddOnValue addOnValue, ZGuid parentPK, ZString tablePrefix, ZString type, ZString data, ZString name)
		{
			CombineAssertions(delegate
			{
				AssertEquals("jobServiceBO.XV_ParentID", parentPK, addOnValue.XV_ParentID);
				AssertEquals("jobServiceBO.XV_ParentTableCode", tablePrefix, addOnValue.XV_ParentTableCode);
				AssertEquals("jobServiceBO.XV_Type", type, addOnValue.XV_Type);
				AssertEquals("jobServiceBO.XV_Data", data, addOnValue.XV_Data);
				AssertEquals("jobServiceBO.XV_Name", name, addOnValue.XV_Name);
			});
		}

		#endregion

		#region SetShipmentAdditionalReference

		public void SetShipmentAdditionalReference(UniversalShipment shipment, params (string code, string number, string countryCode)[] types)
		{
			var list = new DataObjectList<AdditionalReference>();
			foreach (var type in types)
			{
				list.Add(new AdditionalReference()
				{
					Type = new EntryType
					{
						Code = type.code
					},
					ReferenceNumber = type.number,
					CountryOfIssue = new Country { Code = type.countryCode }
				});
			}
			shipment.SetAdditionalReferenceCollection(() => list);
		}

		#endregion

		#region CreatePackingLine

		public PackingLine CreatePackingLine(string referenceNumber, string packingLineID, string packType, decimal weight = 0m, decimal length = 0m, decimal width = 0m, decimal height = 0m, string weightUQ = "KG", string dimUQ = "CM", int packQty = 1, string marksAndNumbers = "", bool isHighRisk = false)
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = referenceNumber,
				PackingLineID = packingLineID,
				PackType = new PackageType() { Code = packType, Description = packType },
				Weight = weight,
				Length = length,
				Width = width,
				Height = height,
				WeightUnit = new UnitOfWeight() { Code = weightUQ },
				LengthUnit = new UnitOfLength() { Code = dimUQ },
				MarksAndNos = marksAndNumbers,
				PackQty = packQty,
				IsHighRisk = isHighRisk
			};
		}

		#endregion

		#region AutoRateJob

		public Job CreateRatingJob(IJobInvoicingPlugIn jobToRate, string jobToRatePrefix)
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = jobToRatePrefix;
			job.JH_ParentID = jobToRate.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.PlugInData = jobToRate;

			return job;
		}

		public AutoRateInfoCollection AutoRateJob(IBusiness masterBizO, Job jobHeader, IAutoRating[] jobsToRate, CostSell costOrSell = CostSell.Revenue)
		{
			var context = new RatingContext();
			var interactor = new LoggerDecorator(context.Logger);
			var costAdapterIDs = ((IRatingSupporter)masterBizO).AdaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts).Select(x => x.OperationalJobCode).ToArray();
			var sellAdapterIDs = ((IRatingSupporter)masterBizO).AdaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts).Select(x => x.OperationalJobCode).ToArray();

			var adaptersProvider = new Mock<IRatingAdaptersProvider>();
			var strategy = new AutoRateInvoicingStrategy(masterBizO, jobHeader);
			var autoRatesCollection = new AutoRatingRunner(masterBizO, context).RetrieveAllCharges(jobsToRate, adaptersProvider.Object, costOrSell);
			strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Cost, costAdapterIDs);
			strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Revenue, sellAdapterIDs);

			return autoRatesCollection;
		}

		#endregion

		#region CreateJobCharge

		public JobCharge CreateJobCharge(IJobHeaderParent parent, string chargeCode, string chargeCodeGroup = ChargeCodeGroupList.Codes.TRWReceive)
		{
			var charge = CreateChargeCode(chargeCode, chargeCode, chargeCodeGroup, "");
			return CreateJobCharge(parent, charge, 0m);
		}

		public JobCharge CreateJobCharge(IJobHeaderParent parent, AccChargeCode chargeCode, ZDecimal sellAmount, ZGuid? sellAccount = null)
		{
			var job = new Job.Loader(parent).Load();
			var charge = job.Charges.AddNew();
			if (chargeCode != null)
			{
				charge.JR_AC = chargeCode.PK;
			}

			if (!sellAmount.IsEmpty)
			{
				charge.JR_OSSellAmt = sellAmount;
				charge.JR_LocalSellAmt = sellAmount;
				charge.JR_OH_SellAccount = sellAccount ?? job.LocalCharges.PK;
			}

			return charge;
		}

		#endregion

		#region Shipments

		public void AddTestDataToShipment(
			UniversalShipment shipment,
			UniversalTransportLeg inboundLeg = null,
			UniversalTransportLeg outboundLeg = null,
			string portOfOrigin = null,
			string portOfDestination = null,
			string portOfLoading = null,
			string portOfDischarge = null,
			ZDateTime? estimatedPickup = null,
			ZDateTime? pickupRequiredFrom = null,
			ZDateTime? estimatedDelivery = null,
			ZDateTime? deliveryRequiredBy = null
		)
		{
			shipment.SetTransportLegCollection(() => new DataObjectList<UniversalTransportLeg>());
			if (inboundLeg != null)
			{
				shipment.TransportLegCollection.Add(inboundLeg);
			}
			if (outboundLeg != null)
			{
				shipment.TransportLegCollection.Add(outboundLeg);
			}

			shipment.PortOfOrigin = portOfOrigin != null ? new UNLOCO { Code = portOfOrigin } : null;
			shipment.PortOfDestination = portOfDestination != null ? new UNLOCO { Code = portOfDestination } : null;
			shipment.PortOfLoading = portOfLoading != null ? new UNLOCO { Code = portOfLoading } : null;
			shipment.PortOfDischarge = portOfDischarge != null ? new UNLOCO { Code = portOfDischarge } : null;

			shipment.LocalProcessing = new LocalProcessing();
			shipment.LocalProcessing.EstimatedPickup = estimatedPickup;
			shipment.LocalProcessing.PickupRequiredFrom = pickupRequiredFrom;
			shipment.LocalProcessing.EstimatedDelivery = estimatedDelivery;
			shipment.LocalProcessing.DeliveryRequiredBy = deliveryRequiredBy;
		}

		#endregion

		#region TransportLegs

		// If the portOfDischarge is your home port, it's an 'inbound leg'.
		// If the portOfLoading is your home port, then it's an 'outbound leg'.
		public UniversalTransportLeg CreateTransportLeg(
			string portOfDischarge = null,
			string portOfLoading = null,
			ZDateTime? estimatedArrivalDate = null,
			ZDateTime? cfsReceivalDate = null,
			ZDateTime? ctoCutOffDate = null
		)
		{
			return new UniversalTransportLeg
			{
				PortOfDischarge = portOfDischarge != null ? new UNLOCO { Code = portOfDischarge } : null,
				PortOfLoading = portOfLoading != null ? new UNLOCO { Code = portOfLoading } : null,
				EstimatedArrival = estimatedArrivalDate,
				LCLReceivalCommences = cfsReceivalDate,
				FCLCutOff = ctoCutOffDate
			};
		}
		#endregion

		#region CreateUnloadTask

		public WhsItemUnloadTask CreateUnloadTask(ZGuid rtuPK, ZGuid warehousePK, ZGuid rcnPK, string scannedID = "")
		{
			var task = Factory.New<WhsItemUnloadTask>();
			task.WUT_WRC_ActiveReceiveConsignment = rcnPK;
			task.WUT_WRH_ActiveReceiveHeader = rtuPK;
			task.WUT_WW_Warehouse = warehousePK;
			task.WUT_LastScannedPackageID = scannedID;
			return task;
		}

		#endregion

		#region CreateJobService

		public JobService CreateJobService(ZGuid parentId, string parentTableCode, string serviceCode = "FUM", ZDateTime? completedTime = null)
		{
			var jobService = Factory.New<WhsJobService>();
			jobService.ES_ServiceCode = serviceCode;
			jobService.ES_Booked = ZDateTime.Now;
			jobService.ES_Completed = completedTime ?? ZDateTime.Empty;
			jobService.ES_ParentID = parentId;
			jobService.ES_ParentTableCode = parentTableCode;
			return jobService;
		}

		#endregion

		#region CreateJobServiceLink

		public JobServiceLink CreateJobServiceLink(ZGuid serviceId, BusinessObject parent, int qty = 1, bool isCompleted = false)
		{
			var jobServiceLink = Factory.New<JobServiceLink>();
			jobServiceLink.ESL_ES_JobService = serviceId;
			jobServiceLink.ESL_ParentID = parent.PK;
			jobServiceLink.ESL_ParentTableCode = parent.TablePrefix;
			jobServiceLink.ESL_Quantity = qty;
			jobServiceLink.ESL_CompletedTime = isCompleted ? new ZDateTimeOffset(DateTimeOffset.Now) : ZDateTimeOffset.Empty;

			return jobServiceLink;
		}

		#endregion

		#region CreateStmUniversalJobLink

		public StmUniversalJobLink CreateStmUniversalJobLink(Guid parentPK, string parentTablePrefix, string sourceKey, string sourceType)
		{
			var universalJobLink = Factory.New<StmUniversalJobLink>();
			universalJobLink.UCL_CompanyCode = "EDI";
			universalJobLink.UCL_EnterpriseCode = "EDI";
			universalJobLink.UCL_ServerCode = "AAA";
			universalJobLink.UCL_ParentID = parentPK;
			universalJobLink.UCL_ParentTableCode = parentTablePrefix;
			universalJobLink.UCL_SourceType = sourceType;
			universalJobLink.UCL_SourceKey = sourceKey;

			return universalJobLink;
		}

		#endregion

		#region CreateConsignmentOrderReference

		public WhsItemConsignmentOrderReference CreateWhsItemConsignmentOrderReference(string orderReference, BusinessObject parent)
		{
			var consignmentOrderReference = Factory.New<WhsItemConsignmentOrderReference>();
			consignmentOrderReference.WOR_OrderReference = orderReference;
			consignmentOrderReference.WOR_ParentTableCode = parent.TablePrefix;
			consignmentOrderReference.WOR_ParentID = parent.PK;

			return consignmentOrderReference;
		}

		#endregion

		#region CreateStmALog

		public StmALog CreateStmALog(IStmALogParent parent, string eventCode, string reference)
		{
			var log = parent.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = eventCode;
				log.SL_Reference = reference;
				log.SL_EventTime = DateTime.Now;
			}

			return log;
		}

		#endregion
	}
}
