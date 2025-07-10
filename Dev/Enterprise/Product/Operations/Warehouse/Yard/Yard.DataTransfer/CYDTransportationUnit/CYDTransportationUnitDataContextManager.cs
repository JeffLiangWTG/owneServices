using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDTransportationUnitDataContextManager : ShipmentDataContextManager<CYDTransportationUnit>, IGateManagementFacilityDataContextManager
	{
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => new List<KeyValuePair<TypeWithDescription, IZType>>();

		protected IEnumerable<RecipientRoleType> SupportedRoleTypes => [RecipientRoleType.CYD];

		public override string DefaultOutputDirectory => null;

		public override bool ManagesShipments => true;

		public override DataContextType DataContextType => DataContextType.CYDTransportationUnit;

		public override ZString DataContextKey => ParentBO.YTU_TransportationUnitID;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(CYDTransportationUnitSchema.YTU_TransportationUnitID, matchingValues.Key);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new CYDTransportationUnitEventParentFinder(factory, this, logger);

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (universalShipment.GetMatchingDataSource(DataContextType.GateBooking) is not null)
			{
				return new CYDTransportationUnitForGateBookingDataObjectReader(universalShipment, logger, factory);
			}
			else if (universalShipment.GetMatchingDataSource(DataContextType.GateVehicleMovement) is not null)
			{
				return new CYDTransportationUnitForVehicleMovementDataObjectReader(universalShipment, logger, factory);
			}
			else
			{
				var errorMessage = Res.GetString("44f1d805-5cc6-4735-8bad-aa2583dd89bb", "No matching data source found from UXML.");
				logger.Log(LogType.Warning, errorMessage);
				return null;
			}
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CYDTransportationUnitDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code.HasValue && SupportedRoleTypes.Contains(o.Code.Value));
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			var eventTypeCode = eventAdded.EventType;
			switch (eventTypeCode)
			{
				case AutoEvents.CancelledCode:
					ResetTPUForCNCEvent(logger, eventAdded);
					break;
				default:
					break;
			}
		}

		public BusinessObject GetLinkedEntity(UniversalObjectFactory factory, IGteGateMovementBooking gateMovementBooking)
		{
			var linkQuery = new ZQuery(StmUniversalJobLinkSchema.UCL_SourceKey, gateMovementBooking.GBM_SourceReferenceNumber);
			var universalLink = factory.LoadTop1<StmUniversalJobLink>(linkQuery);
			if (universalLink == null)
			{
				return null;
			}

			if (gateMovementBooking.GBM_IsPickup)
			{
				var pickupQuery = new ZQuery(CYDPickupSchema.PK, universalLink.UCL_ParentID);
				return factory.LoadTop1<CYDPickup>(pickupQuery);
			}
			else
			{
				var deliveryQuery = new ZQuery(CYDDeliverySchema.PK, universalLink.UCL_ParentID);
				return factory.LoadTop1<CYDDelivery>(deliveryQuery);
			}
		}

		void ResetTPUForCNCEvent(IXmlSessionTracker logger, UniversalEvent xmlEvent)
		{
			var referenceEventCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.EventCode, xmlEvent.EventParameters, xmlEvent.EventReference);
			var isVehicleMovement = xmlEvent.GetMatchingDataSource(DataContextType.GateVehicleMovement)?.Key is not null;

			if (referenceEventCode.Equals(AutoEvents.GateInCode) && isVehicleMovement)
			{
				ResetTPUForCNCFromGateIn(logger, xmlEvent);
			}
		}

		void ResetTPUForCNCFromGateIn(IXmlSessionTracker logger, UniversalEvent xmlEvent)
		{
			if (ParentBO.YTU_GateInTime == ZDateTimeOffset.Empty)
			{
				throw new DataObjectReadFailureException("TPU has not been gated in.");
			}

			var factory = new BusinessObjectFactory();
			var pickupQuery = new ZDBOnlyQuery(typeof(CYDPickup));
			pickupQuery.AddToFilter(CYDPickupSchema.YPL_YTU_PickupTransportationUnit, SQLComparisonOperator.Equal, ParentBO.PK);
			pickupQuery.AddToFilter(CYDPickupSchema.YPL_GS_NKLoadUser, SQLComparisonOperator.NotEqual, "");
			var loadedPickups = factory.Load<CYDPickup>(pickupQuery);

			var deliveryQuery = new ZDBOnlyQuery(typeof(CYDDelivery));
			deliveryQuery.AddToFilter(CYDDeliverySchema.YDL_YTU_DeliveryTransportationUnit, SQLComparisonOperator.Equal, ParentBO.PK);
			deliveryQuery.AddToFilter(CYDDeliverySchema.YDL_GS_NKUnloadUser, SQLComparisonOperator.NotEqual, "");
			var unloadedDeliveries = factory.Load<CYDDelivery>(deliveryQuery);

			if (loadedPickups?.Length > 0 || unloadedDeliveries?.Length > 0)
			{
				throw new DataObjectReadFailureException("This record cannot be reversed as there are attached units that are either currently being handled, or have been processed.");
			}

			ParentBO.YTU_GateInTime = ZDateTimeOffset.Empty;
			ParentBO.YTU_WL_WaitingBayLocation = ZGuid.Empty;
		}
	}
}
