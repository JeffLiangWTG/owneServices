using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	static class CYDTransportationUnitMatchingHelper
	{
		public static Vehicle GetVehicle(Shipment dataObject)
		{
			var preCarriageVehicle = dataObject.PreCarriageShipmentCollection?.FirstOrDefault()?.VehicleRun?.Vehicle;
			var vehicleRunVehicle = dataObject.VehicleRun?.Vehicle;

			var vehicle = preCarriageVehicle ?? vehicleRunVehicle;
			return vehicle;
		}

		public static OrgAddress GetMatchedTransportOrgAddress(Shipment dataObject, OrganizationAddress transportOrgAddress, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var gateAddressMatcher = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(transportOrgAddress, logger, factory);
			return gateAddressMatcher.GetMatched() as OrgAddress;
		}

		public static CYDTransportationUnit ChooseTransportationUnitBasedOnMatchedContainers(CYDTransportationUnit[] transportationUnits, DataObjectList<Container> deliveryContainers, DataObjectList<Container> pickupContainers)
		{
			var deliveryContainerNumbers = deliveryContainers.Select(c => c.ContainerNumber).Where(c => !string.IsNullOrEmpty(c)).Cast<ZString>().ToArray();
			var pickupContainerNumbers = pickupContainers.Select(c => c.ContainerNumber).Where(c => !string.IsNullOrEmpty(c)).Cast<ZString>().ToArray();

			var matchingOrder = transportationUnits
				.Select(tpu => new
				{
					TransportationUnit = tpu,
					MatchCount = deliveryContainerNumbers.Count(container => tpu.ReceiveYardUnits.Any(y => y.YUS_UnitID == container)) +
						pickupContainerNumbers.Count(container => tpu.ReleaseYardUnits.Any(y => y.YUS_UnitID == container))
				})
				.OrderByDescending(tpu => tpu.MatchCount);

			return matchingOrder.FirstOrDefault()?.TransportationUnit;
		}

		public static (DataObjectList<Container> deliveryContainers, DataObjectList<Container> pickupContainers) GetPickupAndDeliveryContainers(Shipment dataObject)
		{
			DataObjectList<Container> deliveryContainers = [];
			DataObjectList<Container> pickupContainers = [];
			foreach (var subShipment in dataObject.SubShipmentCollection.Where(s => s.ContainerCollection != null && s.ContainerCollection.Any()))
			{
				var direction = subShipment.TransportBookingDirection.Code.GetValueOrDefault();
				if (direction == TransportBookingDirections.Codes.Delivery)
				{
					deliveryContainers.AddRange(subShipment.ContainerCollection);
				}
				else if (direction == TransportBookingDirections.Codes.Pickup)
				{
					pickupContainers.AddRange(subShipment.ContainerCollection);
				}
			}

			return (deliveryContainers, pickupContainers);
		}

		public static ZDateTimeOffset GetGateInTime(Shipment dataObject)
		{
			var vehicleEntryTime = dataObject.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start);
			if (vehicleEntryTime != null && vehicleEntryTime.Value.HasValue)
			{
				var gateInTime = vehicleEntryTime.Value.Value.ToZDateTimeOffset();
				var gateInTimeWithoutSeconds = new ZDateTimeOffset(
					gateInTime.Year,
					gateInTime.Month,
					gateInTime.Day,
					gateInTime.Hour,
					gateInTime.Minute,
					0,
					0,
					gateInTime.Offset);
				return gateInTimeWithoutSeconds;
			}
			return ZDateTimeOffset.Empty;
		}

		public static ZGuid? GetWaitingBayLocation(Shipment dataObject, IXmlImportLogger logger, WhsWarehouse yard)
		{
			var location = yard.FindLocation(dataObject.WarehouseLocation.Value);
			if (location != null)
			{
				return location.PK;
			}

			logger.Log(LogType.Warning, Res.GetString("6a199c5e-df70-41eb-84cb-c5de09667f4f", "There is no location found for yard '{0}'.", yard.WW_WarehouseCode));
			return null;
		}

		public static bool IsBooking(Shipment dataObject)
		{
			return dataObject.GetMatchingDataSource(DataContextType.GateMovementBooking) is not null
				&& dataObject.GetMatchingDataSource(DataContextType.GateMovement) is null;
		}

		public static CYDReleaseAdviceLine GetReleaseAdviceLine(string bookingReference, string containerTypeCode, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(CYDReleaseAdviceLine));

			var subQuery = new ZDBOnlySubQuery(typeof(CYDReleaseAdvice), CYDReleaseAdviceLineSchema.YEL_YRE_ReleaseAdvice);
			subQuery.AddToFilter(CYDReleaseAdviceSchema.YRE_ReleaseNumber, bookingReference);
			var today = ZDateTime.Today.Date;
			subQuery.AddToFilter(CYDReleaseAdviceSchema.YRE_FromDate, SQLComparisonOperator.LessThanOrEqualTo, today);
			subQuery.AddToFilter(CYDReleaseAdviceSchema.YRE_ToDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var subQuery2 = new ZDBOnlySubQuery(typeof(CYDUnitLineItem), CYDReleaseAdviceLineSchema.YEL_YLI_UnitLineItem);
			var subQuery3 = new ZDBOnlySubQuery(typeof(RefContainer), CYDUnitLineItemSchema.YLI_RC_ContainerType);
			subQuery3.AddToFilter(RefContainerSchema.RC_Code, containerTypeCode);
			subQuery2.AddSubQuery(subQuery3, JoinCondition.And);

			query.AddSubQuery(subQuery2, JoinCondition.And);

			var releaseAdviceLines = factory.Load<CYDReleaseAdviceLine>(query);
			var availableReleaseAdviceLine = releaseAdviceLines.FirstOrDefault(r => r.IsAvailableToPickup);

			return availableReleaseAdviceLine ?? throw new DataObjectReadFailureException(
				Res.GetString(
					"9168f008-d37f-4e29-ae6c-e0fd51b631a9",
					"Release Advice Line ({0} / {1}) is not found or has 0 balance quantity.",
					bookingReference,
					containerTypeCode));
		}

		public static CYDYardUnitState FindYardUnitForDelivery(CYDTransportationUnit transportationUnit, Container container, string bookingReference)
		{
			var yardUnit = transportationUnit.ReceiveYardUnits.FirstOrDefault(y => string.Equals(y.YUS_UnitID, container.ContainerNumber)) ??
				(string.IsNullOrEmpty(bookingReference)
					? YardMatchingHelper.GetYardUnitForDropOff(transportationUnit.Factory, container.ContainerNumber, transportationUnit.Yard)
					: FindYardUnitFromAcceptanceNumber(transportationUnit, container.ContainerNumber, bookingReference));

			return yardUnit;
		}

		public static CYDYardUnitState CreateYardUnitForDelivery(CYDTransportationUnit transportationUnit, Container container, string bookingReference, IXmlImportLogger logger)
		{
			logger.Log(LogType.Information, Res.GetString("9c8013fb-31ee-4f25-8399-47019feea2f6", "The container '{0}' of Acceptance Number '{1}' is not found, a new Yard Unit is created.", container.ContainerNumber, bookingReference));
			var yardUnit = transportationUnit.Factory.New<CYDYardUnitState>();
			yardUnit.YUS_UnitID = container.ContainerNumber!.Value;
			yardUnit.YUS_WW_CurrentYard = transportationUnit.YTU_WW_Yard;

			return yardUnit;
		}

		public static CYDYardUnitState FindYardUnitForPickup(Container container, string bookingReference, UniversalObjectFactory factory)
		{
			if (string.IsNullOrEmpty(container.ContainerNumber))
			{
				return null;
			}

			var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			query.AddToFilter(CYDYardUnitStateSchema.YUS_UnitID, container.ContainerNumber);
			query.OrderBy = CYDYardUnitStateSchema.Constants.YUS_SystemCreateTimeUtc + OrderByClause.Descending;

			var subQuery = new ZDBOnlySubQuery(typeof(CYDReleaseAdviceLine), CYDYardUnitStateSchema.YUS_YEL_ReleaseLine);
			var subQuery2 = new ZDBOnlySubQuery(typeof(CYDReleaseAdvice), CYDReleaseAdviceLineSchema.YEL_YRE_ReleaseAdvice);
			subQuery2.AddToFilter(CYDReleaseAdviceSchema.YRE_ReleaseNumber, bookingReference);
			subQuery2.AddToFilter(CYDReleaseAdviceSchema.YRE_ToDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today.Date);

			subQuery.AddSubQuery(subQuery2, JoinCondition.And);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return factory.Load<CYDYardUnitState>(query).FirstOrDefault()
				?? throw new YardUnitNotFoundException(container.ContainerNumber, bookingReference);
		}

		static CYDYardUnitState FindYardUnitFromAcceptanceNumber(CYDTransportationUnit transportationUnit, string containerNumber, string acceptanceNumber)
		{
			if (!string.IsNullOrEmpty(acceptanceNumber))
			{
				var factory = transportationUnit.Factory;
				var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
				query.AddToFilter(CYDYardUnitStateSchema.YUS_UnitID, containerNumber);
				query.OrderBy = CYDYardUnitStateSchema.Constants.YUS_SystemCreateTimeUtc + " desc";

				var receiveLineSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDYardUnitStateSchema.YUS_YRL_ReceiveLine);
				var receiveSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdvice), CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice);
				receiveSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_AcceptanceNumber, acceptanceNumber);
				receiveSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_WW_Yard, transportationUnit.Yard.PK);

				var today = transportationUnit.Yard.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow).Date;
				receiveSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_FromDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				receiveSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_ToDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				receiveLineSubQuery.AddSubQuery(receiveSubQuery, JoinCondition.And);
				query.AddSubQuery(receiveLineSubQuery, JoinCondition.And);

				return factory.Load<CYDYardUnitState>(query).FirstOrDefault();
			}

			return null;
		}
	}
}
