using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitLoadListEventParentFinder : EventParentFinder
	{
		public WhsTransitLoadListEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			   : base(factory, manager, logger)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "event reference parameters")]
		const string StopLoadCode = "STOP LOAD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "event reference parameters")]
		const string CancelStopLoadCode = "CANCEL STOP LOAD";

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			factory.ReloadAll<WhsItemDispatchLoadList>();
			var result = Array.Empty<BusinessObject>();
			var valueObject = (IXmlEventValueObject)xmlEvent;
			var eventType = xmlEvent.EventType.GetValueOrDefault();
			var consolNumber = xmlEvent.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? ZString.Empty;

			if ((eventType == AutoEvents.ServiceSuspendedCode || eventType == AutoEvents.ServiceRequestedCode)
				&& (valueObject?.DataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.DTW || r.Code == RecipientRoleType.ATW) ?? false))
			{
				var unlocoCode = EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, xmlEvent.EventParameters, xmlEvent.EventReference).GetValueOrDefault();
				var masterBillNumber = valueObject.Context.MAWBNumber;

				if (masterBillNumber.IsEmpty)
				{
					masterBillNumber = xmlEvent.ContextCollection?.FirstOrDefault(c => c.Type.Type.Value == "MBOLNumber")?.Value ?? string.Empty;
				}

				result = TryGetLoadListsByXMLEvent(consolNumber, masterBillNumber, unlocoCode, true);
				if (result.Length > 0)
				{
					var universalFactory = new UniversalObjectFactory(factory);

					var typeCode = eventType == AutoEvents.ServiceSuspendedCode ? StopLoadCode : CancelStopLoadCode;

					foreach (WhsItemDispatchLoadList dll in result)
					{
						var isFlagChange = false;
						if (eventType == AutoEvents.ServiceSuspendedCode)
						{
							if (!dll.WDL_IsAwaitingForwardingChanges)
							{
								dll.WDL_IsAwaitingForwardingChanges = true;
								dll.WDL_CompleteTime = ZDateTimeOffset.Empty;
								LoadListController.StopLoadList(universalFactory, logger, dll, null, true);
								dll.WDL_IsReadyToStage = false;
								isFlagChange = true;
							}
						}
						else
						{
							if (dll.WDL_IsAwaitingForwardingChanges)
							{
								dll.WDL_IsAwaitingForwardingChanges = false;
								isFlagChange = true;
							}
						}

						if (isFlagChange)
						{
							var reference = WhsTransitLogHelper.GetEventReferenceString(
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, typeCode),
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Warehouse, dll.Warehouse.WW_WarehouseCode),
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, dll.WDL_JobID),
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, dll.WDL_ReferenceNumber));

							WhsTransitLogHelper.AddStmALog(universalFactory, logger, dll.PK.ToGuid(), dll.TableName, reference, Events.StatusUpdatedCode);
						}
						else
						{
							logger.Log(LogType.Information, Res.GetString("e2853a83-b250-4aa9-94fb-6ff5c467c99a", "Load List {0} already {1}.", dll.WDL_JobID, dll.WDL_IsAwaitingForwardingChanges ? (NoResString)"Stopped" : (NoResString)"Cancelled"));
						}
					}
				}
			}
			else if (eventType == AutoEvents.DepartureCode && (valueObject.DataContext?.RecipientRoleCollection?.Any(r => r.Code == RecipientRoleType.DTW) ?? false))
			{
				var mawbNumber = valueObject.Context.MAWBNumber;
				var originalUNLOCO = valueObject.Context.MBOLOriginUNLOCO;
				result = TryGetLoadListsByXMLEvent(consolNumber, mawbNumber, originalUNLOCO, false);
			}

			return result;
		}

		BusinessObject[] TryGetLoadListsByXMLEvent(ZString consolNumber, ZString masterBillNumber, ZString unlocoCode, bool ignoreTransportMode)
		{
			var result = Array.Empty<BusinessObject>();
			var unloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unlocoCode);
			if (unloco != null)
			{
				var matchedWarehouses = GetTransitWarehousesByUNLOCO(unloco);
				if (matchedWarehouses.Any())
				{
					if (matchedWarehouses.Length == 1)
					{
						logger.Log(LogType.Information, Res.GetString("59994328-5282-46e8-a26d-bb05bc6702b0", "Found warehouse '{0}' matching UNLOCO '{1}'.", matchedWarehouses.First().WW_WarehouseCode, unlocoCode));
					}
					else
					{
						logger.Log(LogType.Information, Res.GetString("9c8ff744-6ad3-4545-b2bc-70e5455ad6e8", "Found {0} warehouses matching UNLOCO '{1}': {2}.", matchedWarehouses.Length, unlocoCode, string.Join(", ", matchedWarehouses.Select(w => w.WW_WarehouseCode).OrderBy(c => c))));
					}

					result = TransitUniversalHelper.FindMatchingLoadLists(factory, consolNumber, masterBillNumber, matchedWarehouses.Select(w => w.PK), Array.Empty<ZGuid>(), logger, ignoreTransportMode).Cast<BusinessObject>().ToArray();

					if (!result.Any())
					{
						logger.Log(LogType.Error, Res.GetString("33d2825c-32f9-4ebc-9434-75eb3895662a", "No matching load lists found by consol number '{0}' or master bill number '{1}'.", consolNumber, masterBillNumber));
					}
				}
				else
				{
					logger.Log(LogType.Error, Res.GetString("b57bc080-38a5-40a0-8a9e-79ace28c7850", "No matching Transit Warehouse by UNLOCO '{1}'.", matchedWarehouses.Length, unlocoCode));
				}
			}
			return result;
		}

		WhsWarehouse[] GetTransitWarehousesByUNLOCO(RefUNLOCO originalUNLOCO)
		{
			var countryCode = originalUNLOCO.Country.Code;
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, originalUNLOCO.Code);
			addressSubQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_RN_NKCountryCode, countryCode);

			var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			warehouseQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);

			var warehouses = factory.Load<WhsWarehouse>(warehouseQuery);
			var matchedWarehouses = warehouses.Where(w => w.WarehouseAddress.OA_RL_NKRelatedPortCode == originalUNLOCO.Code);

			if (!matchedWarehouses.Any())
			{
				matchedWarehouses = warehouses.Where(w => w.WarehouseAddress.OA_RN_NKCountryCode == countryCode);
			}

			return matchedWarehouses.ToArray();
		}
	}
}
