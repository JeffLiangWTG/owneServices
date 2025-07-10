using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal.Common.Matching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using TransitStatus = Enterprise.Warehouse.Transit.Business.TransitWarehouseStatuses.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsignmentEventParentFinder : WhsTransitConsignmentEventParentFinder
	{
		public WhsTransitReceiveConsignmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] FindAndUpdateLogParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			BusinessObject[] result;
			var eventTypeCode = xmlEvent.EventType.GetValueOrDefault();

			switch (eventTypeCode)
			{
				case AutoEvents.ScannedCode:
					result = FindAndUpdateSSCParents(xmlEvent);
					break;
				case AutoEvents.CustomsEntryStatusCode:
					result = FindOrUpdateCESParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.ClearanceCompletedCode:
					result = FindOrUpdateSCMParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.HeldCode:
					result = FindOrUpdateSHLParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.CustomsNumberEnteredCode:
					result = FindOrUpdateCENParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.CustomsReleaseNumberEnteredCode:
					result = FindOrUpdateCRNParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.MessageAcceptedCode:
					result = FindOrUpdateMAAParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.MessageRejectedCode:
					result = FindOrUpdateMRJParents(xmlEvent);
					break;
				case AutoEvents.MessageReceivedCode:
					result = FindOrUpdateMRRParents(xmlEvent);
					break;
				default:
					// we must link all other events to the matching RCNs if they are targetting TransitReceive
					result = parentsMatchedByDataTarget;
					break;
			}

			// Consider splitting into separate classes for receive + dispatch if this is ever implemented
			return result;
		}

		#region Implementation

		#region VolCam TransitReceive

		BusinessObject[] FindAndUpdateSSCParents(IXmlEventValueObject xmlEvent)
		{
			var barcode = VolCamScanData.GetBarcode(xmlEvent);
			var warehouseCode = VolCamScanData.GetWarehouseCode(xmlEvent);

			if (!barcode.IsEmpty && !warehouseCode.IsEmpty)
			{
				var transitPackage = GetTransitPackage(barcode, warehouseCode);
				if (transitPackage != null)
				{
					return new[] { transitPackage };
				}
			}

			return Array.Empty<BusinessObject>();
		}

		PkgPackage GetTransitPackage(ZString barcode, ZString warehouseCode)
		{
			var packageWhereSql = @"
WPS_PK IN 
(
	SELECT TOP 2
		WPS_PK 
	FROM
		dbo.WhsItemPackageState 
		JOIN dbo.PkgPackage ON KP_PK = WPS_KP_Package 
		JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
		JOIN dbo.WhsWarehouse ON WW_PK = WPS_WW_Warehouse
	WHERE 
		KPH_PackageID = @Barcode
		AND WPS_Status IN (@ArrivedNotProcessed, @ArrivedStatus, @CommittedStatus, @PickedStatus, @PutawayStatus, @StagedStatus)
		AND WW_WarehouseCode = @WarehouseCode 
		AND WW_WarehouseType = @TransitWarehouseCode
	ORDER BY CASE WHEN WPS_Status = @ArrivedStatus THEN 1 ELSE 0 END DESC
)"; // Part of a SQL expression.
			PkgPackage result = null;

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@Barcode", barcode, PkgPackageHeaderSchema.KPH_PackageID },
				{ "@ArrivedNotProcessed", TransitStatus.ArrivedNotProcessed, WhsItemPackageStateSchema.WPS_Status },
				{ "@ArrivedStatus", TransitStatus.Arrived, WhsItemPackageStateSchema.WPS_Status },
				{ "@CommittedStatus", TransitStatus.Committed, WhsItemPackageStateSchema.WPS_Status },
				{ "@PickedStatus", TransitStatus.Picked, WhsItemPackageStateSchema.WPS_Status },
				{ "@PutawayStatus", TransitStatus.Putaway, WhsItemPackageStateSchema.WPS_Status },
				{ "@StagedStatus", TransitStatus.Staged, WhsItemPackageStateSchema.WPS_Status },
				{ "@WarehouseCode", warehouseCode, WhsWarehouseSchema.WW_WarehouseCode },
				{ "@TransitWarehouseCode", WarehouseTypes.Codes.Transit, WhsWarehouseSchema.WW_WarehouseType },
			};

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddFilterAndZSQLParameterCollection(packageWhereSql, sqlParams);

			var packages = factory.Load<WhsItemPackageState>(query);
			if (packages.Length == 2 && packages[0].WPS_Status == packages[1].WPS_Status)
			{
				logger.Log(LogType.Error, ResString.GetMultilingualString("f00751d0-46ea-4010-beba-5ea321b1de9b", "More than one package was returned with the same status for barcode {0}.", barcode));
			}
			else
			{
				var packageState = packages.SingleOrDefault(x => x.WPS_Status == TransitStatus.Arrived || x.WPS_Status == TransitStatus.ArrivedNotProcessed || x.WPS_Status == TransitStatus.Committed || x.WPS_Status == TransitStatus.Picked || x.WPS_Status == TransitStatus.Putaway || x.WPS_Status == TransitStatus.Staged);
				result = packageState?.Package;
			}

			return result;
		}

		#endregion

		#region Customs Entry Status Event

		BusinessObject[] FindOrUpdateCESParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var receiveConsignment = (WhsItemReceiveConsignment)parentsMatchedByDataTarget.FirstOrDefault() ?? GetReceiveConsignmentByHouseBill(xmlEvent);

			if (receiveConsignment != null)
			{
				var processor = new CESEventProcessor(xmlEvent, logger, factory);
				processor.Process(receiveConsignment);

				UpdateCustomsStatus(new WhsItemReceiveConsignment[] { receiveConsignment }, Array.Empty<WhsItemPackageState>());

				return new[] { receiveConsignment };
			}

			return Array.Empty<BusinessObject>();
		}

		WhsItemReceiveConsignment GetReceiveConsignmentByHouseBill(IXmlEventValueObject eventDataObject)
		{
			var housebill = eventDataObject.Context.HBOLNumber.GetValueOrDefault();
			WhsItemReceiveConsignment receiveConsignment = null;

			if (!housebill.IsEmpty)
			{
				var consignments = factory.Load<WhsItemReceiveConsignment>(GetReceiveConsignmentByHouseBillQuery(housebill));
				if (consignments.Length == 1)
				{
					receiveConsignment = consignments.SingleOrDefault();
					logger.Log(LogType.Information, ResString.GetMultilingualString("2e706eb8-2831-48b3-9f8e-9bb9fac003d8", "Found receive consignment with consignment id '{0}' matching house bill '{1}'.", receiveConsignment.WRC_JobID, housebill));
				}
				else if (consignments.Length > 1)
				{
					receiveConsignment = consignments.OrderByDescending(rcn => rcn.WRC_SystemCreateTimeUtc).FirstOrDefault();
					logger.Log(LogType.Warning, ResString.GetMultilingualString("29e10b94-f210-49b2-ba1b-3b082948e3ee", "The house bill '{0}' has matched to more than one receive consignment, the latest one with consignment id '{1}' is selected by default.", housebill, receiveConsignment.WRC_JobID));
				}
			}

			return receiveConsignment;
		}

		ZQuery GetReceiveConsignmentByHouseBillQuery(string housebill)
		{
			var rcnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveConsignment));
			rcnQuery.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, housebill);
			rcnQuery.AddToFilter(JoinCondition.Or, WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber, housebill);

			return rcnQuery;
		}

		#endregion

		#region Port Events

		BusinessObject[] FindOrUpdateMAAParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var processor = new MAAEventProcessor(xmlEvent, logger);
			var bizos = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);

			if (xmlEvent.EventParameters?.MessageType?.ToString() == TransitWarehouseMessageTypes.CRESAMessageType)
			{
				foreach (var bizo in bizos)
				{
					if (bizo is WhsItemReceiveConsignment)
					{
						(bizo as IStmNoteParent).UpdateCRESAMessageStatusNote(AutoEvents.MessageAcceptedCode);
					}
				}
			}

			return bizos;
		}

		BusinessObject[] FindOrUpdateSCMParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Any() || ShouldProcessPortReferenceEvent(xmlEvent, AutoEvents.ClearanceCompletedCode))
			{
				var processor = new SCMEventProcessor(xmlEvent, logger);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);

				foreach (var bizo in matchingBusinessObjects)
				{
					if (bizo is WhsItemReceiveConsignment)
					{
						(bizo as IStmNoteParent).UpdateCRESAMessageStatusNote(AutoEvents.ClearanceCompletedCode);
					}
				}
			}

			return matchingBusinessObjects;
		}

		BusinessObject[] FindOrUpdateSHLParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Any() || ShouldProcessPortReferenceEvent(xmlEvent, AutoEvents.HeldCode))
			{
				var processor = new SHLEventProcessor(xmlEvent, logger);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);
			}

			return matchingBusinessObjects;
		}

		#endregion

		#region Customs Events

		BusinessObject[] FindOrUpdateCENParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Any() || ShouldProcessCustomsNumberEvent(xmlEvent))
			{
				var processor = new CENEventProcessor(xmlEvent, logger, universalObjectFactory);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);
			}

			return matchingBusinessObjects;
		}

		BusinessObject[] FindOrUpdateCRNParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Any() || ShouldProcessCustomsNumberEvent(xmlEvent))
			{
				var processor = new CRNEventProcessor(xmlEvent, logger, universalObjectFactory);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);
			}

			return matchingBusinessObjects;
		}

		#endregion

		#region CIN Events

		BusinessObject[] FindOrUpdateMRJParents(UniversalEvent xmlEvent)
		{
			var result = new List<BusinessObject>();
			var messageType = xmlEvent.EventParameters?.MessageType?.ToString();
			WhsItemReceiveConsignment matchedRCN = null;

			switch (messageType)
			{
				case CIN750NotificationConstants.CIN750MessageType:
					matchedRCN = GetReceiveConsignmentByJobID(xmlEvent, xmlEvent.EventParameters?.ReferenceNumber);
					if (matchedRCN != null)
					{
						var messageID = xmlEvent.EventParameters.RequestNumber?.ToString();
						var failedReason = xmlEvent.EventParameters.Reason?.ToString();
						matchedRCN.UpdateCIN750MessageStatusNote(messageID, failedReason);

						var documentData = AttachRCNAndDocumentData(xmlEvent, result, matchedRCN);
						var processor = new MRJEventProcessor(xmlEvent, logger, factory);
						processor.Process(matchedRCN, documentData);
					}
					break;
				case TransitWarehouseMessageTypes.CRESAMessageType:
					matchedRCN = GetReceiveConsignmentByJobID(xmlEvent, xmlEvent.DataContext?.DataTargetCollection?.First()?.Key);
					if (matchedRCN != null)
					{
						matchedRCN.UpdateCRESAMessageStatusNote(AutoEvents.MessageRejectedCode, xmlEvent.EventParameters.Reason?.ToString());
						result.Add(matchedRCN);
					}
					break;
				default:
					break;
			}

			return result.ToArray();
		}

		BusinessObject[] FindOrUpdateMRRParents(UniversalEvent xmlEvent)
		{
			var result = new List<BusinessObject>();

			var matchedRCN = GetReceiveConsignmentByJobID(xmlEvent, xmlEvent.EventParameters?.ReferenceNumber);
			if (matchedRCN != null && xmlEvent.EventParameters?.MessageType?.ToString() == CIN750NotificationConstants.CIN750MessageType)
			{
				var messageID = xmlEvent.EventParameters.RequestNumber?.ToString();
				matchedRCN.UpdateCIN750MessageStatusNote(messageID);

				AttachRCNAndDocumentData(xmlEvent, result, matchedRCN);
			}
			return result.ToArray();
		}

		static VisualizerDocumentData AttachRCNAndDocumentData(UniversalEvent xmlEvent, List<BusinessObject> bizos, WhsItemReceiveConsignment matchedRCN)
		{
			var supporter = matchedRCN.GetSupporter();
			var documentEventParent = supporter?.GetEventParent(xmlEvent);

			if (documentEventParent is VisualizerDocumentData data)
			{
				bizos.Add(data);
				return data;
			}
			return null;
		}

		WhsItemReceiveConsignment GetReceiveConsignmentByJobID(UniversalEvent xmlEvent, ZString? jobID)
		{
			if (xmlEvent.GetMatchingDataTarget(DataContextType.TransitReceive) != null)
			{
				if (jobID.HasValue)
				{
					var rcnQuery = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_JobID, jobID.Value);
					return factory.Load<WhsItemReceiveConsignment>(rcnQuery).FirstOrDefault();
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		BusinessObject[] ProcessGoverningEvent(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent, GoverningEventProcessor processor)
		{
			var bizos = new List<BusinessObject>();
			var matchingReceiveConsignments = Array.Empty<WhsItemReceiveConsignment>();
			var matchingReceiveConsignmentsFromBill = Array.Empty<WhsItemReceiveConsignment>();
			var matchingPackageStates = Array.Empty<WhsItemPackageState>();

			if (parentsMatchedByDataTarget.Length > 0)
			{
				matchingReceiveConsignments = parentsMatchedByDataTarget.OfType<WhsItemReceiveConsignment>().ToArray();
			}
			else
			{
				matchingReceiveConsignments = GetReceiveConsignmentByShipmentNumber(xmlEvent);
				matchingReceiveConsignmentsFromBill = GetReceiveConsignmentByBillNumber(xmlEvent);
				matchingPackageStates = GetPackageStatesByShipmentNumber(xmlEvent);
			}

			var allMatchingReceiveConsignments = matchingReceiveConsignments.Concat(matchingReceiveConsignmentsFromBill).ToArray();

			ProcessReceiveConsignmentsForGoverningEvents(matchingReceiveConsignments, processor.Process);
			ProcessPackageStatesForGoverningEvents(matchingPackageStates, processor.Process);
			ProcessReceiveConsignmentsForGoverningEvents(matchingReceiveConsignmentsFromBill, processor.Process);
			UpdateCustomsStatus(allMatchingReceiveConsignments, matchingPackageStates);

			bizos.AddRange(allMatchingReceiveConsignments);
			bizos.AddRange(matchingPackageStates.Select(ps => ps.Package));

			return bizos.ToArray();
		}

		void ProcessReceiveConsignmentsForGoverningEvents(WhsItemReceiveConsignment[] receiveConsignments, Action<WhsItemReceiveConsignment> process)
		{
			if (receiveConsignments.Length > 0)
			{
				foreach (var receiveConsignment in receiveConsignments)
				{
					process(receiveConsignment);
				}
			}
		}

		protected void UpdateCustomsStatus(WhsItemReceiveConsignment[] receiveConsignments, WhsItemPackageState[] packageStates)
		{
			if (receiveConsignments.Length > 0 || packageStates.Length > 0)
			{
				var warehouse = receiveConsignments.FirstOrDefault()?.Warehouse ?? packageStates.FirstOrDefault(ps => ps.Warehouse != null).Warehouse;
				var packageStatesRCNs = packageStates.Select(ps => ps.ReceiveConsignment).ToList();

				if (warehouse != null)
				{
					var warehouseColumnIndexer = WhsTransitUniversalEventHelper.GetBizoColumnIndexers(universalObjectFactory, [warehouse]).FirstOrDefault();
					var packageStatePKs = receiveConsignments.Concat(packageStatesRCNs).SelectMany(rcn => rcn.PackageStates).Select(p => p.PK.ToGuid()).Distinct().ToList();

					if (warehouseColumnIndexer != null && packageStatePKs.Count > 0)
					{
						factory.Saved += (BusinessObjectFactory factory, bool savedSuccessfully) =>
						{
							if (savedSuccessfully)
							{
								TransitUniversalHelper.ExecuteUpdatePackageStateAndRCNCustomStatusProcedure(warehouse, packageStatePKs);
							}
						};
					}

					// updating rows where the BizO is already loaded in the same factory causes issues
					// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
					packageStates.ToList().ForEach(ps => ps.HasChanges = true);
					packageStatesRCNs.ForEach(ps => ps.HasChanges = true);
					receiveConsignments.ToList().ForEach(rcn => rcn.HasChanges = true);
				}
			}
		}

		bool ShouldProcessCustomsNumberEvent(UniversalEvent eventDataObject)
		{
			var forwardingShipmentDataSource = eventDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			var shipmentNumber = forwardingShipmentDataSource?.Key;

			var customEntryDataSource = eventDataObject.GetMatchingDataSource(DataContextType.WarehouseCustomsEntry);
			var customEntryNumber = customEntryDataSource?.Key;

			var hasDataSourceKey = shipmentNumber.GetValueOrDefault() != "";
			var hasCustomEntryNumber = customEntryNumber.GetValueOrDefault() != "";
			var hasLocation = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault() != "";
			var hasCustomsReferenceNumber = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault() != "";

			var result = (hasDataSourceKey && hasLocation && hasCustomsReferenceNumber) || hasCustomEntryNumber;
			if (!result)
			{
				logger.Log(LogType.Warning, ResString.GetMultilingualString("3fff2b3f-c314-41e9-b9ad-a2f588524289",
@"Universal event received could not be used for matching because of missing data. Correct them and try again.
{0}{1}{2}{3}",
forwardingShipmentDataSource != null ? "" : (NoResString)"Expected DataSource to be 'ForwardingShipment'.\r\n",
hasLocation ? "" : (NoResString)"Location is not found.\r\n",
hasDataSourceKey ? "" : (NoResString)"Data source key is not found.\r\n",
hasCustomsReferenceNumber ? "" : (NoResString)"Customs reference number is not found."));
			}

			return result;
		}

		WhsItemReceiveConsignment[] GetReceiveConsignmentByShipmentNumber(UniversalEvent eventDataObject)
		{
			var sourceKey = eventDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key;
			var receiveConsignments = Array.Empty<WhsItemReceiveConsignment>();

			var location = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();
			if (sourceKey.GetValueOrDefault() != "" && location != "")
			{
				receiveConsignments = factory.Load<WhsItemReceiveConsignment>(GetConsignmentsByShipmentNumberQuery(sourceKey, location));
				if (receiveConsignments.Length > 0)
				{
					var concatenatedRCNReferences = string.Join(", ", receiveConsignments.OrderBy(rcn => rcn.WRC_ConsignmentID).Select(rcn => rcn.WRC_ConsignmentID));
					logger.Log(LogType.Information, ResString.GetMultilingualString("81c65541-93d7-44eb-9950-28cf948271ac", "Found Receive Consignments '{0}' matching shipment number '{1}'.", concatenatedRCNReferences, sourceKey));
					logger.Log(LogType.Information, ResString.GetMultilingualString("b936da3b-4d50-4201-aaab-90798449cf60", "Populating matching Receive Consignments.", sourceKey));
				}
			}

			return receiveConsignments;
		}

		WhsItemReceiveConsignment[] GetReceiveConsignmentByBillNumber(UniversalEvent eventDataObject)
		{
			var masterAirWayBill = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "MAWBNumber")?.Value ?? string.Empty;
			var houseAirWayBill = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "HAWBNumber")?.Value ?? string.Empty;
			var masterBillOfLading = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "MBOLNumber")?.Value ?? string.Empty;
			var houseBillOfLading = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "HBOLNumber")?.Value ?? string.Empty;
			var messageType = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "EntryNumberType")?.Value ?? string.Empty;
			var masterLocation = ZString.Empty;
			var houseLocation = ZString.Empty;
			var country = ZString.Empty;

			if (messageType == "IMP")
			{
				masterLocation = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "MBOLDestinationUNLOCO")?.Value ?? string.Empty;
				houseLocation = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "HBOLDestinationUNLOCO")?.Value ?? string.Empty;
			}
			else if (messageType == "EXP")
			{
				masterLocation = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "MBOLOriginUNLOCO")?.Value ?? string.Empty;
				houseLocation = eventDataObject.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "HBOLOriginUNLOCO")?.Value ?? string.Empty;
			}
			if (masterLocation == "" && houseLocation == "")
			{
				country = eventDataObject.DataContext?.CountryCodeToImportInto ?? string.Empty;
			}

			var receiveConsignments = Array.Empty<WhsItemReceiveConsignment>();

			if ((!masterAirWayBill.IsEmpty || !houseAirWayBill.IsEmpty || !masterBillOfLading.IsEmpty || !houseBillOfLading.IsEmpty) && (!masterLocation.IsEmpty || !houseLocation.IsEmpty || !country.IsEmpty))
			{
				var masterBill = masterAirWayBill == "" ? masterBillOfLading : masterAirWayBill;
				var houseBill = houseAirWayBill == "" ? houseBillOfLading : houseAirWayBill;
				var billNumber = houseBill;

				if (!houseBill.IsEmpty)
				{
					receiveConsignments = factory.Load<WhsItemReceiveConsignment>(GetConsignmentsByBillNumberQuery(houseLocation, country, houseBillNumber: houseBill));
				}

				if (receiveConsignments.Length == 0 && !masterBill.IsEmpty)
				{
					receiveConsignments = factory.Load<WhsItemReceiveConsignment>(GetConsignmentsByBillNumberQuery(masterLocation, country, masterBillNumber: masterBill));
					billNumber = masterBill;
				}
				if (receiveConsignments.Length > 0)
				{
					var concatenatedRCNReferences = string.Join(", ", receiveConsignments.OrderBy(rcn => rcn.WRC_ConsignmentID).Select(rcn => rcn.WRC_ConsignmentID));
					logger.Log(LogType.Information, ResString.GetMultilingualString("2e3c6d90-f90f-4c98-bba0-5ca8a8a764a3", "Found Receive Consignments '{0}' matching bill number '{1}'.", concatenatedRCNReferences, billNumber));
					logger.Log(LogType.Information, ResString.GetMultilingualString("5c057e29-9100-4495-b2f4-375879fb9281", "Populating matching Receive Consignments."));
				}
			}

			return receiveConsignments;
		}

		#endregion

		#endregion
	}
}
