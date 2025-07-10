using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public sealed class DtbBookingDataContextManager : ShipmentDataContextManager<DtbBooking>,
		IShipmentDataContextManager
	{
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>(2);
			if (!ParentBO.KM_TransportReference.IsEmpty)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingJobID, ParentBO.KM_JobID);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportReference, ParentBO.KM_TransportReference);
			}
			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DtbBookingEventParentFinder(factory, this, logger);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbBookingDataObjectWriter(null, writeManager, true);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbBookingDataObjectReader_DataTargetIsTransportBooking(universalShipment, logger, factory);
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			switch (eventAdded.EventType.Value)
			{
				case Enterprise.ZArchitecture.Business.AutoEvents.ServiceCommencedCode:
					SetTransportReference(eventAdded.EventReference);
					ParentBO.KM_Status = TransportStatuses.Codes.ServiceCommenced;
					ParentBO.UpdateStatus();
					break;

				case AutoEvents.ServiceCancelledCode:
					ParentBO.KM_Status = TransportStatuses.Codes.Available;
					ParentBO.UpdateStatus();
					break;

				default:
					break;
			}

			eventAdded.OnCO2eRejectionEvent(ParentBO);
		}

		void SetTransportReference(ZString? eventReference)
		{
			if (eventReference.HasValue)
			{
				ParentBO.KM_TransportReference = StmALog.GetFreeTextFromReference(eventReference.Value).Left(ParentBO.KM_TransportReferenceInfo.MaxLength);
			}
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.KM_JobID; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZDBOnlyQuery(typeof(DtbBooking));
			query.AddToFilter(DtbBookingSchema.KM_JobID, matchingValues.Key);

			var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			consolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, GetConsolidationJobTypes());
			query.AddSubQuery(consolidationSubQuery, JoinCondition.And);

			return query;
		}

		string[] GetConsolidationJobTypes()
		{
			return new[] { TransportConsolidationJobTypes.Codes.Booking };
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBooking; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}
	}
}
