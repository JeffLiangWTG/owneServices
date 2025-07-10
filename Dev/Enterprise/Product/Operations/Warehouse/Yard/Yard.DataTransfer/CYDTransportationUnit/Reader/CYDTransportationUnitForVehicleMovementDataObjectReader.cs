using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDTransportationUnitForVehicleMovementDataObjectReader : CYDTransportationUnitDataObjectReader
	{
		public CYDTransportationUnitForVehicleMovementDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			GateValidationServiceHelper.ValidateWaitingBayLocation(dataObject);
			GateValidationServiceHelper.ValidateContainerCollections(dataObject);
			GateValidationServiceHelper.ValidateBookingConfirmationReference(dataObject);
			GateValidationServiceHelper.ValidateContainerInfo(dataObject);

			GateInTime = CYDTransportationUnitMatchingHelper.GetGateInTime(dataObject);
			WaitingBayLocation = CYDTransportationUnitMatchingHelper.GetWaitingBayLocation(dataObject, logger, Yard);
			(DeliveryContainers, PickupContainers) = CYDTransportationUnitMatchingHelper.GetPickupAndDeliveryContainers(dataObject);
		}

		ZDateTimeOffset GateInTime { get; }
		ZGuid? WaitingBayLocation { get; }
		DataObjectList<Container> DeliveryContainers { get; }
		DataObjectList<Container> PickupContainers { get; }

		protected override CYDTransportationUnit GetMatchingTransportationUnitFromJobLinks()
		{
			var source = dataObject.GetMatchingDataSource(DataContextType.GateVehicleMovement);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateVehicleMovement, MatchedTransportOrgAddress.Header, CYDTransportationUnitSchema.Constants.Prefix);
			return GetLatestTransportationUnitRowsFromJobLinks(jobLinks);
		}

		protected override CYDTransportationUnit GetMatchingTransportationUnitUsingQuery()
		{
			var tpuQuery = new ZDBOnlyQuery(typeof(CYDTransportationUnit));
			tpuQuery.AddToFilter(CYDTransportationUnitSchema.YTU_TransportationReference, Vehicle.Registration.Number);
			tpuQuery.AddToFilter(CYDTransportationUnitSchema.YTU_EstimatedGateInTime, SQLComparisonOperator.NotEqual, null);
			if (GateInTime.IsValid)
			{
				var gateInTimeQuery = new ZDBOnlyQuery(typeof(CYDTransportationUnit))
					.AddToFilter(CYDTransportationUnitSchema.YTU_GateInTime, SQLComparisonOperator.GreaterThanOrEqualTo, GateInTime.AddMinutes(-2))
					.AddToFilter(CYDTransportationUnitSchema.YTU_GateInTime, SQLComparisonOperator.LessThanOrEqualTo, GateInTime.AddMinutes(2))
					.AddToFilter(JoinCondition.Or, CYDTransportationUnitSchema.YTU_GateInTime, SQLComparisonOperator.IsBlank, null);
				tpuQuery.AddToFilter(gateInTimeQuery, JoinCondition.And);
			}

			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, MatchedTransportOrgAddress.PK);

			tpuQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			tpuQuery.OrderBy = CYDTransportationUnitSchema.Constants.YTU_EstimatedGateInTime + OrderByClause.Ascending;

			var transportationUnits = factory.Load<CYDTransportationUnit>(tpuQuery);

			return transportationUnits.Length > 1 ? CYDTransportationUnitMatchingHelper.ChooseTransportationUnitBasedOnMatchedContainers(transportationUnits, DeliveryContainers, PickupContainers) : transportationUnits.FirstOrDefault();
		}

		protected override void PopulateBusinessObject(CYDTransportationUnit transportationUnit)
		{
			var transportationUnitRow = GetColumnIndexer(transportationUnit);

			if (IsNewBO)
			{
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_TransportationReference, Vehicle.Registration.Number);
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_WW_Yard, Yard.PK);
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_TransportationUnitID, new TransportationUnitJobNumberStrategy(factory.BOFactory).GetJobNumber());
			}
			else
			{
				UpdateDeliveryContainersOfTransportationUnit(transportationUnit, DeliveryContainers);
				UpdatePickupContainersOfTransportationUnit(transportationUnit, PickupContainers);
			}

			SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_GateInTime, GateInTime);
			if (WaitingBayLocation.HasValue && !WaitingBayLocation.Value.IsEmpty)
			{
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_WL_WaitingBayLocation, new ZGuid(WaitingBayLocation));
			}
			PopulateDeliveriesAndPickups(transportationUnit);
			PopulateJobDocAddressAndLink(transportationUnit, TransportOrgAddress, DataContextType.GateVehicleMovement);
		}

		void UpdateDeliveryContainersOfTransportationUnit(CYDTransportationUnit transportationUnit, DataObjectList<Container> deliveryContainers)
		{
			if (deliveryContainers.Count > 0)
			{
				var bookedYardUnits = transportationUnit.ReceiveYardUnits.Select(y => y.YUS_UnitID).ToArray();
				var gateInYardUnits = deliveryContainers.Select(c => c.ContainerNumber).Where(c => !string.IsNullOrEmpty(c)).Cast<ZString>().ToArray();
				var gateInButNotBookedYardUnits = gateInYardUnits.Except(bookedYardUnits).ToArray();
				foreach (var unit in gateInButNotBookedYardUnits)
				{
					// TODO: improvement need to be done to get the correct yard unit
					var yardUnit = factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, unit));
					var otherTransportationUnit = yardUnit.ReceiveTransportationUnit;
					if (otherTransportationUnit != null && (otherTransportationUnit.YTU_GateInTime.IsDefault || otherTransportationUnit.YTU_GateInTime == transportationUnit.YTU_GateInTime))
					{
						yardUnit.Delivery.YDL_YTU_DeliveryTransportationUnit = transportationUnit.PK;
						yardUnit.YUS_YTU_ReceiveTransportationUnit = transportationUnit.PK;
						transportationUnit.ReceiveYardUnits.Add(yardUnit);
						DeleteTransportationUnitIfEmpty(otherTransportationUnit);
					}
				}

				var bookedButNotGateInYardUnits = bookedYardUnits.Except(gateInYardUnits).ToArray();
				if (bookedButNotGateInYardUnits.Length > 0)
				{
					var clonedTransportationUnit = (CYDTransportationUnit)transportationUnit.Clone();
					logger.Log(LogType.Information, Res.GetString("90eceef2-d40e-40ee-9e70-e5d8b0656b55", "Split Transportation Unit {0} for remaining deliveries.", transportationUnit.YTU_TransportationUnitID));
					foreach (var unit in bookedButNotGateInYardUnits)
					{
						// TODO: Improvement need to be done to get the correct yard unit
						var yardUnit = factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, unit));
						yardUnit.Delivery.YDL_YTU_DeliveryTransportationUnit = clonedTransportationUnit.PK;
						yardUnit.YUS_YTU_ReceiveTransportationUnit = clonedTransportationUnit.PK;
						PopulateJobDocAddress(clonedTransportationUnit, TransportOrgAddress);
					}
				}
			}
		}

		void UpdatePickupContainersOfTransportationUnit(CYDTransportationUnit transportationUnit, DataObjectList<Container> pickupContainers)
		{
			if (pickupContainers.Count > 0)
			{
				var bookedYardUnits = transportationUnit.ReleaseYardUnits.Select(y => y.YUS_UnitID).ToArray();
				var gateOutYardUnits = pickupContainers.Select(c => c.ContainerNumber).Where(c => !string.IsNullOrEmpty(c)).Cast<ZString>().ToArray();
				var gateOutButNotBookedYardUnits = gateOutYardUnits.Except(bookedYardUnits).ToArray();
				foreach (var unit in gateOutButNotBookedYardUnits)
				{
					// TODO: improvement need to be done to get the correct yard unit
					var yardUnit = factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, unit));
					var otherTransportationUnit = yardUnit.DispatchTransportationUnit;
					if (otherTransportationUnit != null && (otherTransportationUnit.YTU_GateInTime.IsDefault || otherTransportationUnit.YTU_GateInTime == transportationUnit.YTU_GateInTime))
					{
						yardUnit.Pickup.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;
						yardUnit.YUS_YTU_DispatchTransportationUnit = transportationUnit.PK;
						transportationUnit.ReleaseYardUnits.Add(yardUnit);
						DeleteTransportationUnitIfEmpty(otherTransportationUnit);
					}
				}

				var bookedButNotGateOutYardUnits = bookedYardUnits.Except(gateOutYardUnits).ToArray();
				if (bookedButNotGateOutYardUnits.Length > 0)
				{
					var clonedTransportationUnit = (CYDTransportationUnit)transportationUnit.Clone();
					logger.Log(LogType.Information, Res.GetString("b9c21600-b682-4a1f-be87-cf693cb19db6", "Split Transportation Unit {0} for remaining pickups.", transportationUnit.YTU_TransportationUnitID));
					foreach (var unit in bookedButNotGateOutYardUnits)
					{
						// TODO: improvement need to be done to get the correct yard unit
						var yardUnit = factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, unit));
						yardUnit.Pickup.YPL_YTU_PickupTransportationUnit = clonedTransportationUnit.PK;
						yardUnit.YUS_YTU_DispatchTransportationUnit = clonedTransportationUnit.PK;
						PopulateJobDocAddress(clonedTransportationUnit, TransportOrgAddress);
					}
				}
			}
		}

		void DeleteTransportationUnitIfEmpty(CYDTransportationUnit transportationUnit)
		{
			if (transportationUnit.ReceiveYardUnits.Count == 0 && transportationUnit.ReleaseYardUnits.Count == 0)
			{
				logger.Log(LogType.Information, Res.GetString("31ccc6fe-6443-4937-8fd6-8f0d6b079bb8", "Deleting empty Transportation Unit {0}.", transportationUnit.YTU_TransportationUnitID));
				factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, transportationUnit.PK)).DeleteAll();
				transportationUnit.Delete();
			}
		}
	}
}
