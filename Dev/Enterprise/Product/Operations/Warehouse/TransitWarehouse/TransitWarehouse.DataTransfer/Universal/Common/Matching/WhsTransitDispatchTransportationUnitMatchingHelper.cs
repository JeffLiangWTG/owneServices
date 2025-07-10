using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class WhsTransitDispatchTransportationUnitMatchingHelper
	{
		#region For Container

		public static WhsItemDispatchTransportationUnit GetExistingContainerDTU(UniversalObjectFactory factory, Container dataObject, IColumnIndexer loadList, List<WhsItemDispatchTransportationUnit> listOfContainersAlreadyMatched, IColumnIndexer warehouse)
		{
			if (string.IsNullOrEmpty(dataObject.ContainerNumber))
			{
				if (dataObject.ContainerType == null || !dataObject.ContainerType.Code.HasValue || string.IsNullOrEmpty(dataObject.ContainerType.Code.Value))
				{
					throw new DataObjectReadFailureException(Res.GetString("0edd192b-fc74-4d1e-aa85-f2e6b48916c2", "Container Type is mandatory for Containers. However, Container Type is missing for at least one Container (that has no Container Number)."));
				}

				var pivotQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadListDTUPivot));
				pivotQuery.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, loadList[WhsItemDispatchLoadListSchema.Constants.PK]);
				var pivots = factory.BOFactory.Load<WhsItemDispatchLoadListDTUPivot>(pivotQuery);
				var unMatchedPivots = pivots.Where(p => !listOfContainersAlreadyMatched.Contains(p.DispatchTransportationUnit));
				return unMatchedPivots.Where(p => p.DispatchTransportationUnit.HasContainerEquipmentDetails
				&& p.DispatchTransportationUnit.Container.ContainerType.RC_Code == dataObject.ContainerType.Code.Value).Select(p => p.DispatchTransportationUnit).FirstOrDefault();
			}

			var matchingDTU = MatchingDTUByContainerNumber(factory, dataObject, warehouse);
			return matchingDTU ?? MatchingContainerLinkedToDLL(factory, loadList, listOfContainersAlreadyMatched);
		}

		static WhsItemDispatchTransportationUnit MatchingDTUByContainerNumber(UniversalObjectFactory factory, Container dataObject, IColumnIndexer warehouse)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			headerQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, dataObject.ContainerNumber);
			headerQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime, SQLComparisonOperator.Equal, null);
			headerQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			headerQuery.OrderBy = WhsItemDispatchTransportationUnitSchema.WDH_SystemCreateTimeUtc.Name + " DESC";
			var matchingDTU = factory.BOFactory.LoadTop1<WhsItemDispatchTransportationUnit>(headerQuery);
			return matchingDTU;
		}

		static WhsItemDispatchTransportationUnit MatchingContainerLinkedToDLL(UniversalObjectFactory factory, IColumnIndexer loadList, List<WhsItemDispatchTransportationUnit> listOfContainersAlreadyMatched)
		{
			var pivotQuery = new ZQuery();
			pivotQuery.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, loadList.GetValue(WhsItemDispatchLoadListSchema.PK));
			pivotQuery.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, SQLComparisonOperator.NotEqual, listOfContainersAlreadyMatched.Select(d => d.PK).ToArray());
			var pivotsForDLL = factory.BOFactory.Load<WhsItemDispatchLoadListDTUPivot>(pivotQuery);
			return pivotsForDLL.Length == 1 && string.IsNullOrEmpty(pivotsForDLL.Single().DispatchTransportationUnit.WDH_VehicleReference)
				? pivotsForDLL.Single().DispatchTransportationUnit
				: null;
		}

		#endregion

		#region Run Sheet Number

		public static WhsItemDispatchTransportationUnit GetExistingDTUByRunSheetNumber(UniversalObjectFactory factory, Shipment dataObject, IColumnIndexer warehouse)
		{
			var runSheetDataSource = dataObject.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			var headerQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);

			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, runSheetDataSource?.Key);

			headerQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime, SQLComparisonOperator.Equal, null);
			headerQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			headerQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);
			return factory.BOFactory.LoadTop1<WhsItemDispatchTransportationUnit>(headerQuery);
		}

		#endregion

		#region Gate Booking

		public static WhsItemDispatchTransportationUnit GetExistingDTUForGateBooking(
			UniversalObjectFactory factory,
			Shipment dataObject,
			ZString vehicleReference,
			IColumnIndexer warehouse,
			IXmlImportLogger logger,
			bool isMatchGateMovementBookingUniversalLinkEnabled = true)
		{
			WhsItemDispatchTransportationUnit dtu = null;
			if (isMatchGateMovementBookingUniversalLinkEnabled)
			{
				dtu = GetDtuFromGateMovementBookingJobLinks(factory, dataObject, warehouse);
			}
			if (dtu == null)
			{
				dtu = GetDtuFromGateBookingJobLinks(factory, dataObject, warehouse);
			}
			if (dtu == null)
			{
				var transportOrgAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress)) ?? throw new DataObjectReadFailureException(Res.GetString("7fd1885c-8279-4cef-b512-84cb33996a96", "Transport Company details are missing from UXML."));
				var gateAddressMatcher = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(transportOrgAddress, logger, factory);
				var matchedTransportOrgAddress = gateAddressMatcher.GetMatched() as OrgAddress;
				var query = GetMatchingTransportationUnitFromVehicleReference(matchedTransportOrgAddress, vehicleReference, warehouse);
				dtu = factory.LoadTop1<WhsItemDispatchTransportationUnit>(query);
			}
			return dtu;
		}

		static WhsItemDispatchTransportationUnit GetDtuFromGateMovementBookingJobLinks(UniversalObjectFactory factory, Shipment dataObject, IColumnIndexer warehouse)
		{
			var firstSubShipmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking();
			if (firstSubShipmentFromGateBooking != null)
			{
				var source = firstSubShipmentFromGateBooking.GetMatchingDataSource(DataContextType.GateMovementBooking);
				var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateMovementBooking, null, WhsItemDispatchTransportationUnitSchema.Constants.Prefix);
				return GetLatestDtuFromJobLinks(factory, jobLinks, warehouse);
			}
			return null;
		}

		static WhsItemDispatchTransportationUnit GetDtuFromGateBookingJobLinks(UniversalObjectFactory factory, Shipment dataObject, IColumnIndexer warehouse)
		{
			var source = dataObject.GetMatchingDataSource(DataContextType.GateBooking);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateBooking, null, WhsItemDispatchTransportationUnitSchema.Constants.Prefix);
			return GetLatestDtuFromJobLinks(factory, jobLinks, warehouse);
		}

		static WhsItemDispatchTransportationUnit GetLatestDtuFromJobLinks(UniversalObjectFactory factory, IEnumerable<IColumnIndexer> jobLinks, IColumnIndexer warehouse)
		{
			var query = new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			query.OrderBy = WhsItemDispatchTransportationUnitSchema.Constants.WDH_SystemCreateTimeUtc + OrderByClause.Descending;
			query.MaximumRows = 1;

			return factory.LoadTop1<WhsItemDispatchTransportationUnit>(query);
		}

		static ZQuery GetMatchingTransportationUnitFromVehicleReference(OrgAddress matchedTransportOrgAddress, ZString vehicleReference, IColumnIndexer warehouse)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, vehicleReference);
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_GateOutTime, null);
			query.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			if (matchedTransportOrgAddress != null)
			{
				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, matchedTransportOrgAddress.PK);
				query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			}
			query.OrderBy = WhsItemDispatchTransportationUnitSchema.Constants.WDH_SystemCreateTimeUtc + OrderByClause.Ascending;
			return query;
		}

		#endregion
	}
}
