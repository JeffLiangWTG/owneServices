using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDTransportationUnitForGateBookingDataObjectReader : CYDTransportationUnitDataObjectReader
	{
		public CYDTransportationUnitForGateBookingDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			EstimatedGateInTime = GetEstimatedGateInTime();
		}

		ZDateTimeOffset EstimatedGateInTime { get; }

		protected override void PopulateBusinessObject(CYDTransportationUnit transportationUnit)
		{
			var transportationUnitRow = GetColumnIndexer(transportationUnit);

			if (IsNewBO)
			{
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_TransportationReference, Vehicle.Registration.Number);
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_WW_Yard, Yard.PK);
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_TransportationUnitID, new TransportationUnitJobNumberStrategy(factory.BOFactory).GetJobNumber());
				SetValue(transportationUnitRow, CYDTransportationUnitSchema.YTU_EstimatedGateInTime, EstimatedGateInTime);

				PopulateJobDocAddressAndLink(transportationUnit, TransportOrgAddress, DataContextType.GateBooking);
			}

			PopulateDeliveriesAndPickups(transportationUnit);
			AddBookingConfirmedLog(transportationUnit);
		}

		ZDateTimeOffset GetEstimatedGateInTime()
		{
			var minSlotStartTime = dataObject.SubShipmentCollection
				?.SelectMany(subShipment => subShipment.DateCollection?.Where(d => d.Type == DateType.Start).Select(d => d.Value) ?? Enumerable.Empty<UXmlDateTime?>())
				.MinOrDefault(slotTime =>
				{
					if (slotTime is not null)
					{
						return slotTime.Value.ZDate is ZDateTime ?
							Yard.GetWarehouseBranchLocalDateTimeOffset(slotTime.Value) :
							Yard.GetWarehouseBranchDateTimeOffset(slotTime.Value.ToZDateTimeOffset().ToUtcZDateTime());
					}

					return (ZDateTimeOffset?)null;
				});

			if (minSlotStartTime == null)
			{
				var errorMessage = Res.GetString("83fb83ec-e6a7-4692-a53d-6cabe8f36cbe", "Booking slot date time are missing from UXML.");
				throw new DataObjectReadFailureException(errorMessage);
			}

			return (ZDateTimeOffset)minSlotStartTime;
		}

		protected override CYDTransportationUnit GetMatchingTransportationUnitUsingQuery()
		{
			var query = new ZDBOnlyQuery(typeof(CYDTransportationUnit));
			query.AddToFilter(CYDTransportationUnitSchema.YTU_TransportationReference, Vehicle.Registration.Number);
			query.AddToFilter(CYDTransportationUnitSchema.YTU_GateInTime, null);
			query.AddToFilter(CYDTransportationUnitSchema.YTU_WW_Yard, Yard.PK);
			query.AddToFilter(CYDTransportationUnitSchema.YTU_EstimatedGateInTime, SQLComparisonOperator.GreaterThanOrEqualTo, EstimatedGateInTime.AddMinutes(-2));
			query.AddToFilter(CYDTransportationUnitSchema.YTU_EstimatedGateInTime, SQLComparisonOperator.LessThanOrEqualTo, EstimatedGateInTime.AddMinutes(2));

			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, MatchedTransportOrgAddress.PK);

			query.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			query.OrderBy = CYDTransportationUnitSchema.Constants.YTU_SystemCreateTimeUtc + OrderByClause.Ascending;

			var transportationUnit = factory.LoadTop1<CYDTransportationUnit>(query);
			return transportationUnit;
		}

		protected override CYDTransportationUnit GetMatchingTransportationUnitFromJobLinks()
		{
			var source = dataObject.GetMatchingDataSource(DataContextType.GateBooking);
			var jobLinks = UniversalJobLinkHelper.GetMatchingJobLinks(factory, source?.Key, DataContextType.GateBooking, MatchedTransportOrgAddress.Header, CYDTransportationUnitSchema.Constants.Prefix);
			return GetLatestTransportationUnitRowsFromJobLinks(jobLinks);
		}

		public void AddBookingConfirmedLog(CYDTransportationUnit transportationUnit)
		{
			var warehouseCode = Yard?.WW_WarehouseCode ?? ZString.Empty;
			var warehouseAddressPK = Yard?.WW_OA_WarehouseAddress;
			var address = warehouseAddressPK.HasValue ? GetColumnIndexerFromRow(factory.RowFactory.LoadFromPK(OrgAddressSchema.Constants.TableName, warehouseAddressPK.Value)) : null;
			var addressCode = address != null ? address.GetValue(OrgAddressSchema.OA_City) : ZString.Empty;

			var referenceforTPULevel = FormattableString.Invariant($"{transportationUnit.YTU_TransportationUnitID}|WHS={warehouseCode}|LOC={addressCode}");
			ContainerYardUniversalHelper.AddStmALog(factory, logger, transportationUnit.PK.ToGuid(), transportationUnit.TableName, referenceforTPULevel, Events.BookingConfirmedCode);

			TriggerLineLevelEvents(transportationUnit, warehouseCode, addressCode);
		}

		public void TriggerLineLevelEvents(CYDTransportationUnit transportationUnit, ZString warehouseCode, ZString addressCode)
		{
			foreach (var delivery in transportationUnit.Deliveries)
			{
				if (delivery != null)
				{
					var referenceforDeliveryLevel = FormattableString.Invariant($"{delivery.ReceiveAdviceLine?.ReceiveAdvice?.YRA_AcceptanceNumber}|WHS={warehouseCode}|LOC={addressCode}");
					ContainerYardUniversalHelper.AddStmALog(factory, logger, delivery.PK.ToGuid(), delivery.TableName, referenceforDeliveryLevel, Events.BookingConfirmedCode);
				}
			}

			foreach (var pickup in transportationUnit.Pickups)
			{
				if (pickup != null)
				{
					var referenceforPickupLevel = FormattableString.Invariant($"{pickup.ReleaseAdviceLine?.ReleaseAdvice?.YRE_ReleaseNumber}|WHS={warehouseCode}|LOC={addressCode}");
					ContainerYardUniversalHelper.AddStmALog(factory, logger, pickup.PK.ToGuid(), pickup.TableName, referenceforPickupLevel, Events.BookingConfirmedCode);
				}
			}
		}
	}
}
