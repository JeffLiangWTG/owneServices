using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal.Common.Matching
{
	public abstract class WhsTransitConsignmentEventParentFinder : EventParentFinder
	{
		public WhsTransitConsignmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
			universalObjectFactory = new UniversalObjectFactory(factory);
		}

		protected readonly UniversalObjectFactory universalObjectFactory;

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			return FindAndUpdateLogParents(logParents, eventData);
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return FindAndUpdateLogParents(Array.Empty<BusinessObject>(), xmlEvent);
		}

		protected abstract BusinessObject[] FindAndUpdateLogParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent);

		#region Queries

		protected ZQuery GetPackageStateByShipmentNumberQuery(string shipmentNumber, string warehouseLocation)
		{
			var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, shipmentNumber);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, warehouseLocation);

			var warehouseQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsItemPackageStateSchema.WPS_WW_Warehouse);
			warehouseQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

			var packageStateQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, false);
			packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WL_LastLocation, SQLComparisonOperator.NotEqual, null);
			packageStateQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);
			packageStateQuery.AddSubQuery(warehouseQuery, JoinCondition.And);

			return packageStateQuery;
		}

		protected ZQuery GetConsignmentsByShipmentNumberQuery(string shipmentNumber, string warehouseLocation, bool isRCN = true)
		{
			var consignmentQuery = isRCN ? new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment)) : new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));

			var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, shipmentNumber);
			consignmentQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, warehouseLocation);

			var warehouseQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), isRCN ? WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse : WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse);
			warehouseQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

			consignmentQuery.AddSubQuery(warehouseQuery, JoinCondition.And);

			return consignmentQuery;
		}

		protected ZQuery GetConsignmentsByBillNumberQuery(string location, string country, string masterBillNumber = "", string houseBillNumber = "", bool isRCN = true)
		{
			var consignmentQuery = isRCN ? new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment)) : new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));
			var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID, isRCN ? WhsItemReceiveConsignmentSchema.PK : WhsItemDispatchConsignmentSchema.PK);

			if (!string.IsNullOrEmpty(masterBillNumber))
			{
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.MasterBill);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, masterBillNumber);
				consignmentQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);
			}

			if (!string.IsNullOrEmpty(houseBillNumber))
			{
				consignmentQuery.AddToFilter(isRCN ? WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber : WhsItemDispatchConsignmentSchema.WDC_HouseBillNumber, houseBillNumber);
			}

			var warehouseQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), isRCN ? WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse : WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse);
			if (!string.IsNullOrEmpty(location))
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, location);
				warehouseQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			else
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, country);
				warehouseQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}

			consignmentQuery.AddSubQuery(warehouseQuery, JoinCondition.And);

			return consignmentQuery;
		}

		#endregion

		#region ShouldProcessPortReferenceEvent

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching to xml data")]
		protected bool ShouldProcessPortReferenceEvent(UniversalEvent eventDataObject, string eventCode, bool isRCN = true)
		{
			var forwardingShipmentDataSource = eventDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			var shipmentNumber = forwardingShipmentDataSource?.Key;
			var customsDepartmentText = "Customs";
			var cfsText = CargoWise.EventReference.Constants.Facilities.Code.Depot;
			var importHeldText = "Port Notification Import Status";
			var exportHeldText = "Port Notification Export Status";

			var hasDataSourceKey = shipmentNumber.GetValueOrDefault() != "";
			var hasLocation = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault() != "";
			var hasCustomsReferenceNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault() != "";
			var hasValidImportHeldMessageType = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault().EqualsIgnoringCase(importHeldText);
			var hasValidExportHeldMessageType = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault().EqualsIgnoringCase(exportHeldText);
			var isDepartmentCustoms = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault().EqualsIgnoringCase(customsDepartmentText);
			var isFacilityCFS = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault().EqualsIgnoringCase(cfsText);

			var result = hasDataSourceKey && hasLocation && hasCustomsReferenceNumber
						&& (eventCode == AutoEvents.HeldCode ? (isDepartmentCustoms || (isFacilityCFS && ((isRCN && hasValidImportHeldMessageType) || hasValidExportHeldMessageType))) : isDepartmentCustoms);
			var expectedMessageType = isRCN ? ResString.GetMultilingualString("61a32ed4-90b8-4dec-ae94-692f7a666d03", "Expected message type to be 'Port Notification Import Status' or 'Port Notification Export Status'.") : ResString.GetMultilingualString("64b49c11-1a0c-40d3-a71e-0a617e2bc02c", "Expected message type to be 'Port Notification Export Status'.");
			var expectedDepartment = ResString.GetMultilingualString("976833bf-ca85-40b4-80fb-4c0c307b8139", "Expected department to be 'Customs' or Facility to be 'CFS'.");
			if (!result)
			{
				var heldEventError = isDepartmentCustoms ? "" : (isFacilityCFS ? ((isRCN && hasValidImportHeldMessageType) || hasValidExportHeldMessageType ? "" : expectedMessageType) : expectedDepartment);

				logger.Log(LogType.Warning, ResString.GetMultilingualString("05e5c4d8-1be1-48ea-a4b3-a7c5052bcdab",
@"Universal event received could not be used for matching because of missing data. Correct them and try again.
{0}{1}{2}{3}{4}",
forwardingShipmentDataSource != null ? "" : "Expected DataSource to be 'ForwardingShipment'.\r\n",
hasLocation ? "" : "Location is not found.\r\n",
hasDataSourceKey ? "" : "Data source key is not found.\r\n",
hasCustomsReferenceNumber ? "" : "Customs reference number is not found.\r\n",
eventCode == AutoEvents.HeldCode ? heldEventError : (isDepartmentCustoms ? "" : "Expected department to be 'Customs'.")));
			}

			return result;
		}

		#endregion

		#region Implementation

		protected void ProcessPackageStatesForGoverningEvents(WhsItemPackageState[] matchingPackageStates, Action<PkgPackage> process)
		{
			if (matchingPackageStates.Length > 0)
			{
				foreach (var packageState in matchingPackageStates)
				{
					process(packageState.Package);
				}
			}
		}

		protected WhsItemPackageState[] GetPackageStatesByShipmentNumber(UniversalEvent eventDataObject)
		{
			var sourceKey = eventDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key;
			var packageStates = Array.Empty<WhsItemPackageState>();

			var location = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();
			if (sourceKey.GetValueOrDefault() != "" && location != "")
			{
				packageStates = factory.Load<WhsItemPackageState>(GetPackageStateByShipmentNumberQuery(sourceKey, location));
				if (packageStates.Length > 0)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("ee98f12d-237b-4a36-9f30-1b25641cf38f", "Found Package(s) matching shipment number '{0}'.", sourceKey));
					logger.Log(LogType.Information, ResString.GetMultilingualString("3924c7fe-3a4b-42c4-a4fe-bd18c3047ece", "Populating matching Packages."));
				}
			}

			return packageStates;
		}

		#endregion
	}
}
