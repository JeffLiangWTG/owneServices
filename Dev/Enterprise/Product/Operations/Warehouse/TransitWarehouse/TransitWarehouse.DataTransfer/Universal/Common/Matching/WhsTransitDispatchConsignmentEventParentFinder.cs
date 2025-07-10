using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal.Common.Matching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsignmentEventParentFinder : WhsTransitConsignmentEventParentFinder
	{
		public WhsTransitDispatchConsignmentEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		#region GetLogParentsForEventUsingContext

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			return FindAndUpdateLogParents(System.Array.Empty<BusinessObject>(), xmlEvent);
		}

		#endregion

		#region GetChildrenIfSpecifiedInContext

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			return FindAndUpdateLogParents(logParents, eventData);
		}

		#endregion

		protected override BusinessObject[] FindAndUpdateLogParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			BusinessObject[] result;
			var eventTypeCode = xmlEvent.EventType.GetValueOrDefault();

			switch (eventTypeCode)
			{
				case AutoEvents.MessageRejectedCode:
					result = FindOrUpdateMRJParents(xmlEvent);
					break;
				case AutoEvents.MessageReceivedCode:
					result = FindOrUpdateMRRParents(xmlEvent);
					break;
				case AutoEvents.MessageAcceptedCode:
					result = FindOrUpdateMAAParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.ClearanceCompletedCode:
					result = FindOrUpdateSCMParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				case AutoEvents.HeldCode:
					result = FindOrUpdateSHLParents(parentsMatchedByDataTarget, xmlEvent);
					break;
				default:
					result = parentsMatchedByDataTarget;
					break;
			}

			return result;
		}

		#region CIN Events

		BusinessObject[] FindOrUpdateMRJParents(UniversalEvent xmlEvent)
		{
			var result = new List<BusinessObject>();
			var messageType = xmlEvent.EventParameters?.MessageType?.ToString();

			switch (messageType)
			{
				case CIN750NotificationConstants.CIN750MessageType:
					var matchedDCNForCIN750 = GetDispatchConsignmentByCINReference(xmlEvent);
					if (matchedDCNForCIN750 != null)
					{
						var messageID = xmlEvent.EventParameters.RequestNumber?.ToString();
						var failedReason = xmlEvent.EventParameters.Reason?.ToString();
						matchedDCNForCIN750.UpdateCIN750MessageStatusNote(messageID, failedReason);

						var documentData = AttachDCNAndDocumentData(xmlEvent, result, matchedDCNForCIN750);
						var processor = new MRJEventProcessor(xmlEvent, logger, factory);
						processor.Process(matchedDCNForCIN750, documentData);
					}
					break;
				case TransitWarehouseMessageTypes.CRESAMessageType:
					var matchedDCNForCRESA = GetDispatchConsignmentByJobID(xmlEvent, xmlEvent.DataContext?.DataTargetCollection?.First()?.Key);
					if (matchedDCNForCRESA != null)
					{
						matchedDCNForCRESA.UpdateCRESAMessageStatusNote(AutoEvents.MessageRejectedCode, xmlEvent.EventParameters.Reason?.ToString());
						result.Add(matchedDCNForCRESA);
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

			var matchedDCN = GetDispatchConsignmentByCINReference(xmlEvent);
			if (matchedDCN != null && xmlEvent.EventParameters?.MessageType?.ToString() == CIN750NotificationConstants.CIN750MessageType)
			{
				var messageID = xmlEvent.EventParameters.RequestNumber?.ToString();
				matchedDCN.UpdateCIN750MessageStatusNote(messageID);

				AttachDCNAndDocumentData(xmlEvent, result, matchedDCN);
			}
			return result.ToArray();
		}

		static VisualizerDocumentData AttachDCNAndDocumentData(UniversalEvent xmlEvent, List<BusinessObject> bizos, WhsItemDispatchConsignment matchedDCN)
		{
			var supporter = matchedDCN.GetSupporter();
			var documentEventParent = supporter?.GetEventParent(xmlEvent);

			if (documentEventParent is VisualizerDocumentData data)
			{
				bizos.Add(data);
				return data;
			}
			return null;
		}

		WhsItemDispatchConsignment GetDispatchConsignmentByCINReference(UniversalEvent xmlEvent)
		{
			if (xmlEvent.GetMatchingDataTarget(DataContextType.TransitDispatch) != null)
			{
				var dcnID = xmlEvent.EventParameters.ReferenceNumber;
				var dcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_JobID, dcnID);
				return factory.Load<WhsItemDispatchConsignment>(dcnQuery).FirstOrDefault();
			}
			return null;
		}

		WhsItemDispatchConsignment GetDispatchConsignmentByJobID(UniversalEvent xmlEvent, ZString? jobID)
		{
			if (xmlEvent.GetMatchingDataTarget(DataContextType.TransitDispatch) != null)
			{
				if (jobID.HasValue)
				{
					var dcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_JobID, jobID.Value);
					return factory.Load<WhsItemDispatchConsignment>(dcnQuery).FirstOrDefault();
				}
			}

			return null;
		}

		#endregion

		#region Port Events

		BusinessObject[] FindOrUpdateMAAParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Length > 0 || ShouldProcessPortReferenceEvent(xmlEvent, AutoEvents.MessageAcceptedCode, false))
			{
				var processor = new MAAEventProcessor(xmlEvent, logger);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);
			}

			return matchingBusinessObjects;
		}

		BusinessObject[] FindOrUpdateSCMParents(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent)
		{
			var matchingBusinessObjects = Array.Empty<BusinessObject>();

			if (parentsMatchedByDataTarget.Length > 0 || ShouldProcessPortReferenceEvent(xmlEvent, AutoEvents.ClearanceCompletedCode, false))
			{
				var processor = new SCMEventProcessor(xmlEvent, logger);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);

				foreach (var bizo in matchingBusinessObjects)
				{
					if (bizo is WhsItemDispatchConsignment)
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

			if (parentsMatchedByDataTarget.Length > 0 || ShouldProcessPortReferenceEvent(xmlEvent, AutoEvents.HeldCode, false))
			{
				var processor = new SHLEventProcessor(xmlEvent, logger);
				matchingBusinessObjects = ProcessGoverningEvent(parentsMatchedByDataTarget, xmlEvent, processor);
			}

			return matchingBusinessObjects;
		}

		BusinessObject[] ProcessGoverningEvent(BusinessObject[] parentsMatchedByDataTarget, UniversalEvent xmlEvent, GoverningEventProcessor processor)
		{
			var bizos = new List<BusinessObject>();
			var matchingDispatchConsignments = Array.Empty<WhsItemDispatchConsignment>();

			if (parentsMatchedByDataTarget.Length > 0)
			{
				matchingDispatchConsignments = parentsMatchedByDataTarget.OfType<WhsItemDispatchConsignment>().ToArray();
			}
			else
			{
				matchingDispatchConsignments = GetDispatchConsignmentByShipmentNumber(xmlEvent);
			}

			matchingDispatchConsignments = matchingDispatchConsignments.Where(dcn => dcn.WDC_Direction == TransitWarehouseConsignmentDirections.Codes.Export).ToArray();
			ProcessDispatchConsignmentsForGoverningEvents(matchingDispatchConsignments, processor.Process);
			UpdateCustomsStatus(matchingDispatchConsignments);

			bizos.AddRange(matchingDispatchConsignments);
			return bizos.ToArray();
		}

		protected void UpdateCustomsStatus(WhsItemDispatchConsignment[] dispatchConsignments)
		{
			if (dispatchConsignments.Length > 0)
			{
				var warehouse = dispatchConsignments.FirstOrDefault()?.Warehouse;

				if (warehouse != null)
				{
					var warehouseColumnIndexer = WhsTransitUniversalEventHelper.GetBizoColumnIndexers(universalObjectFactory, [warehouse]).FirstOrDefault();
					var packageStates = dispatchConsignments.SelectMany(dcn => dcn.PackageStates).Distinct().ToList();
					var packageStatePKs = packageStates.Select(p => p.PK.ToGuid()).ToList();

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
				}
			}
		}

		WhsItemDispatchConsignment[] GetDispatchConsignmentByShipmentNumber(UniversalEvent eventDataObject)
		{
			var sourceKey = eventDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key;
			var dispatchConsignments = Array.Empty<WhsItemDispatchConsignment>();

			var location = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, eventDataObject.EventParameters, eventDataObject.EventReference).GetValueOrDefault();
			if (sourceKey.GetValueOrDefault() != "" && location != "")
			{
				dispatchConsignments = factory.Load<WhsItemDispatchConsignment>(GetConsignmentsByShipmentNumberQuery(sourceKey, location, false));
				if (dispatchConsignments.Length > 0)
				{
					var concatenatedDCNReferences = string.Join(", ", dispatchConsignments.OrderBy(dcn => dcn.WDC_ConsignmentID).Select(dcn => dcn.WDC_ConsignmentID));
					logger.Log(LogType.Information, ResString.GetMultilingualString("86c77084-c6aa-4a43-9192-5a8a3de0cac4", "Found Dispatch Consignments '{0}' matching shipment number '{1}'.", concatenatedDCNReferences, sourceKey));
					logger.Log(LogType.Information, ResString.GetMultilingualString("f0667d9e-6813-4132-9fb2-c5fd7187c832", "Populating matching Dispatch Consignments.", sourceKey));
				}
			}

			return dispatchConsignments;
		}

		void ProcessDispatchConsignmentsForGoverningEvents(WhsItemDispatchConsignment[] matchingDispatchConsignments, Action<WhsItemDispatchConsignment> process)
		{
			if (matchingDispatchConsignments.Length > 0)
			{
				foreach (var dispatchConsignment in matchingDispatchConsignments)
				{
					process(dispatchConsignment);
				}
			}
		}

		#endregion
	}
}
