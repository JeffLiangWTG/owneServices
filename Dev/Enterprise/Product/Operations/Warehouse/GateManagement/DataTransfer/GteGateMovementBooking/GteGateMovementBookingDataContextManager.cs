using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.GateManagementConstants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteGateMovementBookingDataContextManager : EventDataContextManager<GteGateMovementBooking>
	{
		public override DataContextType DataContextType => DataContextType.GateMovementBooking;

		public override ZString DataContextKey => ParentBO.GBM_MovementBookingNumber;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(GteGateMovementBookingSchema.GBM_MovementBookingNumber, matchingValues.Key);
			query.AddToFilter(GteGateMovementBookingSchema.GBM_CancelledReason, SQLComparisonOperator.Equal, ZString.Empty);
			query.OrderBy = GteGateMovementBookingSchema.GBM_SlotStartTime.Name;

			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var direction = ParentBO.GBM_IsPickup ? TransportBookingDirections.Codes.Pickup : TransportBookingDirections.Codes.Delivery;

				result.AddIfNotEmpty(UniversalEvent.ContextTypes.VBSNotificationID, ParentBO.GBM_SourceReferenceNumber);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.Direction, (ZString)direction);
			}

			return result;
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent xml)
		{
			base.OnUniversalEventAddedCore(logger, xml);

			var finder = GetEventParentFinder(ParentBO.Factory, logger);
			var matchedGBM = finder.GetLogParentsForEvent(xml).Cast<GteGateMovementBooking>().FirstOrDefault();
			if (matchedGBM != null)
			{
				var eventValueObject = (IXmlEventValueObject)xml;
				var eventType = eventValueObject.EventType;
				switch (eventType)
				{
					case EventCodes.BookingCanceled:
						CancelGateMovementBookings(xml, matchedGBM);
						break;
					case EventCodes.BookingConfirmed:
						CreateLinkAndPopulateFacilityJobID(xml, matchedGBM, logger);
						break;
					case EventCodes.ContainerTypeUpdated:
						UpdateContainerType(xml, matchedGBM);
						break;
				}
			}
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
			=> new GteGateMovementBookingEventParentFinder(factory, this, logger);

		void CancelGateMovementBookings(IXmlEventValueObject xml, GteGateMovementBooking gateMovementBooking)
		{
			if (!gateMovementBooking.GBM_CancelledReason.IsEmpty)
			{
				throw new Exception(Res.GetString("b3c082c7-17de-5eac-4699-6a779b8b988a", "Cannot cancel Gate Movement Booking '{0}' because it is already canceled.", gateMovementBooking.GBM_SourceReferenceNumber));
			}

			if (BookingIsGatedIn(gateMovementBooking))
			{
				throw new Exception(Res.GetString("302da00e-1172-be8e-430e-ed096c9af0b6", "Cannot cancel Gate Movement Booking '{0}' because it is already gated-in.", gateMovementBooking.GBM_SourceReferenceNumber));
			}

			gateMovementBooking.GBM_CancelledReason = Res.GetString("d28dc266-9b26-a990-47c1-1fbc3df1eba3", "Canceled by Data Import");
		}

		void CreateLinkAndPopulateFacilityJobID(IXmlEventValueObject xmlEvent, GteGateMovementBooking gateMovementBooking, IXmlSessionTracker logger)
		{
			var dataSource = xmlEvent.DataContext.DataSourceCollection.FirstOrDefault();
			var jobNumber = dataSource.Key;
			var linkCreator = new UniversalJobLinkCreator(gateMovementBooking.Factory, gateMovementBooking, null, xmlEvent.DataContext, logger, true);

			if (dataSource.Type.Value == nameof(DataContextType.TransitReceiveHeader))
			{
				var job = (BusinessObject)gateMovementBooking.Factory.LoadTop1<IWhsItemReceiveTransportationUnit>(new ZQuery(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber, jobNumber));
				linkCreator.TryCreateJobLink(DataContextType.TransitReceiveHeader);

				if (job != null)
				{
					gateMovementBooking.GBM_FacilityJobId = job.PK;
					gateMovementBooking.GBM_FacilityTableCode = WhsItemReceiveTransportationUnitSchema.Constants.Prefix;
				}
			}
			else if (dataSource.Type.Value == nameof(DataContextType.TransitDispatchHeader))
			{
				var job = (BusinessObject)gateMovementBooking.Factory.LoadTop1<IWhsItemDispatchTransportationUnit>(new ZQuery(WhsItemDispatchTransportationUnitSchema.WDH_ReferenceNumber, jobNumber));
				linkCreator.TryCreateJobLink(DataContextType.TransitDispatchHeader);

				if (job != null)
				{
					gateMovementBooking.GBM_FacilityJobId = job.PK;
					gateMovementBooking.GBM_FacilityTableCode = WhsItemDispatchTransportationUnitSchema.Constants.Prefix;
				}
			}
			else if (dataSource.Type.Value == nameof(DataContextType.CYDPickup))
			{
				var job = (BusinessObject)gateMovementBooking.Factory.LoadTop1<ICYDPickup>(new ZQuery(CYDPickupSchema.YPL_PickupID, jobNumber));
				linkCreator.TryCreateJobLink(DataContextType.CYDPickup);

				if (job != null)
				{
					gateMovementBooking.GBM_FacilityJobId = job.PK;
					gateMovementBooking.GBM_FacilityTableCode = CYDPickupSchema.Constants.Prefix;
				}
			}
			else if (dataSource.Type.Value == nameof(DataContextType.CYDDelivery))
			{
				var job = (BusinessObject)gateMovementBooking.Factory.LoadTop1<ICYDDelivery>(new ZQuery(CYDDeliverySchema.YDL_DeliveryID, jobNumber));
				linkCreator.TryCreateJobLink(DataContextType.CYDDelivery);

				if (job != null)
				{
					gateMovementBooking.GBM_FacilityJobId = job.PK;
					gateMovementBooking.GBM_FacilityTableCode = CYDDeliverySchema.Constants.Prefix;
				}
			}
		}

		void UpdateContainerType(IXmlEventValueObject xml, GteGateMovementBooking gateMovementBooking)
		{
			if (!gateMovementBooking.GBM_CancelledReason.IsEmpty)
			{
				return;
			}
		
			var referenceParameters = StmALog.GetParametersFromReference(xml.EventReference);
			referenceParameters.TryGetValue(EventReferenceParameters.Codes.New, out var newContainerCode);

			var refContainer = gateMovementBooking.Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, newContainerCode)).FirstOrDefault();
			if (refContainer == null)
			{
				return;
			}

			gateMovementBooking.GBM_RC_UnitType = refContainer.PK;
			var gateMovement = gateMovementBooking.GateMovements.FirstOrDefault(gateMovement => gateMovement.GGM_CancelledReason == string.Empty);
			if (gateMovement != null)
			{
				gateMovement.GGM_RC_UnitType = refContainer.PK;
			}
		}

		bool BookingIsGatedIn(GteGateMovementBooking gateMovementBooking)
		{
			if (gateMovementBooking != null)
			{
				var ggm = gateMovementBooking.Factory.LoadTop1<GteGateMovement>(new ZQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, gateMovementBooking.PK).AddToFilter(GteGateMovementSchema.GGM_CancelledReason, ZString.Empty));

				return ggm != null;
			}

			return false;
		}
	}
}
