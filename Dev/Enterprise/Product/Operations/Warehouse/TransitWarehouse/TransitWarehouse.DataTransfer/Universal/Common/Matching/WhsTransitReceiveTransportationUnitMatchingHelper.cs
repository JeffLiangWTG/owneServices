using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class WhsTransitReceiveTransportationUnitMatchingHelper
	{
		#region For SeaCargo Outturn

		public static WhsItemReceiveTransportationUnit GetExistingRTUForSeaCargo(UniversalObjectFactory factory, IXmlImportLogger logger, ZString containerNumber, IColumnIndexer warehouse, ZString vesselLloyds, ZString voyageFlightNo, ZString premiseID, bool isRejectedByHigherPriority)
		{
			var warehouseBO = factory.BOFactory.Load<WhsWarehouse>(warehouse.GetValue(WhsWarehouseSchema.PK));

			if (vesselLloyds.IsEmpty || voyageFlightNo.IsEmpty || premiseID.IsEmpty)
			{
				throw new DataObjectReadFailureException(Res.GetString("e0e900b3-2921-46cc-bd96-a0019ddb8ed6",
	@"UXML received could not be used to match to an RTU because of below errors. Correct them and try again.
{0}{1}{2}",
	!vesselLloyds.IsEmpty ? "" : "Vessel Lloyds is not found.\r\n",
	!premiseID.IsEmpty ? "" : "Premise ID is not found.\r\n",
	!voyageFlightNo.IsEmpty ? "" : "Voyage Flight Number is not found."));
			}

			var warehousePremiseID = TransitWarehouseHelper.GetPremiseIDFortWarehouse(warehouseBO);
			if (!warehousePremiseID.Equals(premiseID, StringComparison.OrdinalIgnoreCase))
			{
				throw new DataObjectReadFailureException(Res.GetString("7f5c5cec-13b2-418f-a2a6-82c5dd50ee42",
					"The premise ID '{0}' did not match the warehouse '{1}' premise ID (CCP) '{2}'.", premiseID, warehouseBO.WW_WarehouseNameMultilingual, warehousePremiseID));
			}

			return FindExistingRTU(factory, logger, containerNumber, vesselLloyds, voyageFlightNo, warehouse, isRejectedByHigherPriority);
		}

		static WhsItemReceiveTransportationUnit FindExistingRTU(UniversalObjectFactory factory, IXmlImportLogger logger, ZString containerNumber, ZString vesselLloyds, ZString voyageFlightNo, IColumnIndexer warehouse, bool isRejectedByHigherPriority)
		{
			var matchingRTUs = FindMatchingRTUs(factory, warehouse, containerNumber, vesselLloyds, voyageFlightNo, isRejectedByHigherPriority);

			if (matchingRTUs.Any())
			{
				foreach (var rtu in matchingRTUs)
				{
					factory.RowFactory.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, rtu.GetValue(WhsItemReceiveTransportationUnitSchema.PK)));
				}

				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"d5636ff5-487d-4f45-82dd-60a4c2789e6e",
					"RTU '{0}' was matched for the Consignment - It was created within the last 30 days and has the provided Vessel Lloyds/IMO '{1}' and Voyage Flight Number '{2}' and Container Number '{3}'.",
					matchingRTUs[0].GetValue(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber),
					vesselLloyds,
					voyageFlightNo,
					containerNumber));

				var rtuToReturn = matchingRTUs[0]; // grab first element as they are ordered by latest Create Time
				return factory.Load<WhsItemReceiveTransportationUnit>(rtuToReturn.GetValue(WhsItemReceiveTransportationUnitSchema.PK)); // need to return a BizO to the architecture
			}
			else
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"a864baac-c5bc-4000-8b8e-186be65673b0",
					"RTU matching for the Consignment failed - No RTU created within the last 30 days could be found with the provided Vessel Lloyds/IMO '{0}', Voyage Flight Number '{1}' and Container Number '{2}'.",
					vesselLloyds,
					voyageFlightNo,
					containerNumber));

				return null;
			}
		}

		static IColumnIndexer[] FindMatchingRTUs(UniversalObjectFactory factory, IColumnIndexer warehouse, ZString containerNumber, ZString vesselLloyds, ZString voyageFlightN, bool isRejectedByHigherPriority)
		{
			var rtuQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			rtuQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			rtuQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, containerNumber);
			rtuQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddDays(-30));

			if (!isRejectedByHigherPriority)
			{
				var voyageNumberReferenceQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), WhsItemReceiveTransportationUnitSchema.PK, CusEntryNumSchema.CE_ParentID);
				voyageNumberReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
				voyageNumberReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, voyageFlightN);

				var vesselLloydsReferenceQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), WhsItemReceiveTransportationUnitSchema.PK, CusEntryNumSchema.CE_ParentID);
				vesselLloydsReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
				vesselLloydsReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, vesselLloyds);

				rtuQuery.AddSubQuery(voyageNumberReferenceQuery, JoinCondition.And);
				rtuQuery.AddSubQuery(vesselLloydsReferenceQuery, JoinCondition.And);
			}

			var rtusFromQuery = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveTransportationUnitSchema.Constants.TableName, rtuQuery), DataObjectReader.GetColumnIndexerFromRow);
			var matchingRTUs = rtusFromQuery;
			return matchingRTUs;
		}

		#endregion

		#region For Consol

		public static WhsItemReceiveTransportationUnit GetExistingRTUForConsol(UniversalObjectFactory factory, IXmlImportLogger logger, ZString containerNumber, IColumnIndexer warehouse, ZString masterBill, ZString consolNumber)
		{
			if (string.IsNullOrEmpty(masterBill) && string.IsNullOrEmpty(consolNumber))
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString("0213c817-d08b-41e7-9af1-222a0d3ece88", "RTU matching failed - A Master Bill or Consol Number was not provided for the Consolidation of Consignments."));
				return null;
			}

			var headerContainerNumberQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			headerContainerNumberQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, containerNumber);
			headerContainerNumberQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			headerContainerNumberQuery.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddDays(-30));

			var headersFromQuery = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveTransportationUnitSchema.Constants.TableName, headerContainerNumberQuery), DataObjectReader.GetColumnIndexerFromRow);
			var matchingHeaders = headersFromQuery;

			if (matchingHeaders.Any())
			{
				foreach (var header in matchingHeaders)
				{
					factory.RowFactory.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, header.GetValue(WhsItemReceiveTransportationUnitSchema.PK)));
				}

				if (!masterBill.IsEmpty && !consolNumber.IsEmpty)
				{
					matchingHeaders = GetRTUsMatchingReferencesWithFallback(factory, logger, warehouse, headersFromQuery,  masterBill, consolNumber, containerNumber);
				}
				else if (!consolNumber.IsEmpty)
				{
					matchingHeaders = GetRTUsMatchingConsolNumberWithNoMasterBill(factory, logger, warehouse, headersFromQuery, masterBill, consolNumber, containerNumber);
				}
				else if (!masterBill.IsEmpty)
				{
					matchingHeaders = GetRTUsMatchingMasterBillWithNoConsolNumber(factory, logger, warehouse, headersFromQuery, masterBill, consolNumber, containerNumber);
				}
			}

			return GetLatestRTU(factory, logger, matchingHeaders, warehouse, containerNumber, masterBill, consolNumber);
		}

		static WhsItemReceiveTransportationUnit GetLatestRTU(UniversalObjectFactory factory, IXmlImportLogger logger, IColumnIndexer[] matchingHeaders, IColumnIndexer warehouse, ZString containerNumber, ZString masterBill, ZString consolNumber)
		{
			if (matchingHeaders.Length >= 1)
			{
				var header = matchingHeaders[0]; // grab first element as they are ordered by latest Create Time
				return factory.Load<WhsItemReceiveTransportationUnit>(header.GetValue(WhsItemReceiveTransportationUnitSchema.PK)); // need to return a BizO to the architecture
			}

			logger.Log(LogType.Information, ResString.GetMultilingualString(
				"2bfa76b9-5271-4930-b90f-c42bd23ab3f2",
				"RTU matching failed - No RTU created within the last 30 days could be found with the provided Container Number {0}, Warehouse {1}, Master Bill {2}, and Consol Number {3}.",
				containerNumber,
				warehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
				masterBill,
				consolNumber));
			return null;
		}

		#region GetHeadersMatchingMasterBillOrdered

		static IColumnIndexer[] GetRTUsMatchingReferencesWithFallback(UniversalObjectFactory factory, IXmlImportLogger logger, IColumnIndexer warehouse, IColumnIndexer[] rtus, ZString masterBill, ZString consolNumber, ZString containerNumber)
		{
			var matchingHeaders = GetRTUsMatchingSpecifiedReferencesOrdered(factory, rtus, masterBill, consolNumber, isMasterBillRequired: true, isConsolNumberRequired: true);

			if (matchingHeaders.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"b3e9ee9e-aaf1-4ba7-a314-a9f8321cd484",
					"RTU {0} was matched - It was created within the last 30 days and has the provided Container Number {1}, Warehouse {2}, Master Bill {3}, and Consol Number {4}.",
					matchingHeaders[0].GetValue(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber),
					containerNumber,
					warehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					masterBill,
					consolNumber));
			}
			else
			{
				matchingHeaders = GetRTUsMatchingConsolNumberWithNoMasterBill(factory, logger, warehouse, rtus, masterBill, consolNumber, containerNumber);
				if (!matchingHeaders.Any())
				{
					matchingHeaders = GetRTUsMatchingMasterBillWithNoConsolNumber(factory, logger, warehouse, rtus, masterBill, consolNumber, containerNumber);
				}
			}

			return matchingHeaders;
		}

		static IColumnIndexer[] GetRTUsMatchingConsolNumberWithNoMasterBill(UniversalObjectFactory factory, IXmlImportLogger logger, IColumnIndexer warehouse, IColumnIndexer[] rtus, ZString masterBill, ZString consolNumber, ZString containerNumber)
		{
			var matchingRTUs = GetRTUsMatchingSpecifiedReferencesOrdered(factory, rtus, masterBill, consolNumber, isMasterBillRequired: false, isConsolNumberRequired: true);
			if (matchingRTUs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"430ff465-e2b7-4f46-83b1-aed6d3b5e9b4",
					"RTU {0} was matched - It was created within the last 30 days and has the provided Container Number {1}, Warehouse {2}, and Consol Number {3}.",
					matchingRTUs[0].GetValue(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber),
					containerNumber,
					warehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					consolNumber));
			}

			return matchingRTUs;
		}

		static IColumnIndexer[] GetRTUsMatchingMasterBillWithNoConsolNumber(UniversalObjectFactory factory, IXmlImportLogger logger, IColumnIndexer warehouse, IColumnIndexer[] rtus, ZString masterBill, ZString consolNumber, ZString containerNumber)
		{
			var matchingRTUs = GetRTUsMatchingSpecifiedReferencesOrdered(factory, rtus, masterBill, consolNumber, isMasterBillRequired: true, isConsolNumberRequired: false);
			if (matchingRTUs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"43c95c0a-ec76-4f4b-b4b2-348906126bb6",
					"RTU {0} was matched - It was created within the last 30 days and has the provided Container Number {1}, Warehouse {2}, and Master Bill {3}.",
					matchingRTUs[0].GetValue(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber),
					containerNumber,
					warehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					masterBill));
			}

			return matchingRTUs;
		}

		static IColumnIndexer[] GetRTUsMatchingSpecifiedReferencesOrdered(UniversalObjectFactory factory, IColumnIndexer[] rtus, ZString? masterBill, ZString? consolNumber, bool isMasterBillRequired, bool isConsolNumberRequired)
		{
			var matchingHeaders = (
				from rtu in rtus
				let additionalReferences = GetAdditionalReferencesOnJob(factory, rtu)
				let masterbillOnRTU = additionalReferences.FirstOrDefault(IsMasterBill)
				let consolNumberOnRTU = additionalReferences.FirstOrDefault(IsConsolNumber)
				where MatchReferences(masterBill, consolNumber, masterbillOnRTU, consolNumberOnRTU, isMasterBillRequired, isConsolNumberRequired)
				orderby rtu.GetValue(WhsItemReceiveTransportationUnitSchema.WRH_SystemCreateTimeUtc) descending
				select rtu
			).ToArray();

			return matchingHeaders;
		}

		static IColumnIndexer[] GetAdditionalReferencesOnJob(UniversalObjectFactory factory, IColumnIndexer additionalReferenceParent)
		{
			var additionalReferenceQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, additionalReferenceParent.GetValue(WhsItemReceiveTransportationUnitSchema.PK));

			var additionalReferences = factory.RowFactory.Load(CusEntryNumSchema.Constants.TableName, additionalReferenceQuery);
			return Array.ConvertAll(additionalReferences, DataObjectReader.GetColumnIndexerFromRow);
		}

		static bool IsMasterBill(IColumnIndexer additionalReference)
		{
			return IsAdditionalReferenceOfType(additionalReference, AdditionalReferenceTypes.Codes.MasterBill);
		}

		static bool IsConsolNumber(IColumnIndexer additionalReference)
		{
			return IsAdditionalReferenceOfType(additionalReference, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
		}

		static bool IsAdditionalReferenceOfType(IColumnIndexer additionalReference, string additionalReferenceType)
		{
			return additionalReference.GetValue(CusEntryNumSchema.CE_EntryType).EqualsIgnoringCase(additionalReferenceType);
		}

		static bool MatchReferences(ZString? masterBill, ZString? consolNumber, IColumnIndexer masterBillOnRTU, IColumnIndexer consolNumberOnRTU, bool isMasterBillRequired, bool isConsolNumberRequired)
		{
			var hasMatchingMasterBill = HasMatchingReference(masterBill, masterBillOnRTU, isMasterBillRequired);
			var hasMatchingConsolNumber = HasMatchingReference(consolNumber, consolNumberOnRTU, isConsolNumberRequired);

			return hasMatchingMasterBill && hasMatchingConsolNumber;
		}

		static bool HasMatchingReference(ZString? reference, IColumnIndexer referenceOnRTU, bool isReferenceRequired)
		{
			var actualValue = referenceOnRTU?.GetValue(CusEntryNumSchema.CE_EntryNum);
			var isActualValueSpecified = !string.IsNullOrEmpty(actualValue);

			return (isActualValueSpecified && reference == actualValue)
				|| (!isActualValueSpecified && !isReferenceRequired);
		}

		#endregion

		#endregion

		#region For Gate Booking

		public static WhsItemReceiveTransportationUnit GetExistingRTUForGateBooking(
			UniversalObjectFactory factory,
			UniversalShipment dataObject,
			IColumnIndexer warehouse,
			ZString? vehicleReference,
			OrgAddress matchedTransportOrgAddress,
			bool isMatchGateMovementBookingUniversalLinkEnabled = true)
		{
			WhsItemReceiveTransportationUnit rtu = null;
			if (isMatchGateMovementBookingUniversalLinkEnabled)
			{
				rtu = GetRtuFromGateMovementBookingJobLinks(factory, dataObject, warehouse);
			}
			if (rtu == null)
			{
				rtu = GetRtuFromGateBookingJobLinks(factory, dataObject, warehouse);
			}
			if (rtu == null)
			{
				var query = GetQueryForExistingBusinessObject(warehouse, vehicleReference, matchedTransportOrgAddress);
				rtu = factory.LoadTop1<WhsItemReceiveTransportationUnit>(query);
			}
			return rtu;
		}

		static WhsItemReceiveTransportationUnit GetRtuFromGateBookingJobLinks(UniversalObjectFactory factory, UniversalShipment dataObject, IColumnIndexer warehouse)
		{
			var source = dataObject.GetMatchingDataSource(DataContextType.GateBooking);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateBooking, null, WhsItemReceiveTransportationUnitSchema.Constants.Prefix);
			return GetLatestRtuFromJobLinks(factory, jobLinks, warehouse);
		}

		static WhsItemReceiveTransportationUnit GetRtuFromGateMovementBookingJobLinks(UniversalObjectFactory factory, UniversalShipment dataObject, IColumnIndexer warehouse)
		{
			var firstSubShipmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking();
			if (firstSubShipmentFromGateBooking != null)
			{
				var source = firstSubShipmentFromGateBooking.GetMatchingDataSource(DataContextType.GateMovementBooking);
				var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateMovementBooking, null, WhsItemReceiveTransportationUnitSchema.Constants.Prefix);
				return GetLatestRtuFromJobLinks(factory, jobLinks, warehouse);
			}
			return null;
		}

		static WhsItemReceiveTransportationUnit GetLatestRtuFromJobLinks(UniversalObjectFactory factory, IEnumerable<IColumnIndexer> jobLinks, IColumnIndexer warehouse)
		{
			var query = new ZQuery(WhsItemReceiveTransportationUnitSchema.PK, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			query.OrderBy = WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemCreateTimeUtc + OrderByClause.Descending;
			query.MaximumRows = 1;

			return factory.LoadTop1<WhsItemReceiveTransportationUnit>(query);
		}

		static ZQuery GetQueryForExistingBusinessObject(IColumnIndexer warehouse, ZString? vehicleReference, OrgAddress matchedTransportOrgAddress)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, vehicleReference);
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_GateInTime, null);
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			if (matchedTransportOrgAddress != null)
			{
				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, matchedTransportOrgAddress.PK);
				query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			}
			query.OrderBy = WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemCreateTimeUtc + OrderByClause.Ascending;
			return query;
		}

		#endregion
	}
}
