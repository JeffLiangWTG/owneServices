using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class ServiceDataContextManager : EventDataContextManager<JobService>
	{
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory,
			IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		public override DataContextType DataContextType => DataContextType.Service;

		public override ZString DataContextKey => ParentBO.ES_ServiceId;

		public override string DefaultOutputDirectory => null;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>
			{
				{ UniversalEvent.ContextTypes.ServiceId, ParentBO.ES_ServiceId },
				{ UniversalEvent.ContextTypes.ServiceType, ParentBO.ES_ServiceCode },
				{ UniversalEvent.ContextTypes.ServiceCount, ParentBO.ES_ServiceCount },
				{ UniversalEvent.ContextTypes.ServiceNotes, ParentBO.ES_ServiceNote },
				{ UniversalEvent.ContextTypes.ServiceRate, ParentBO.ES_ServiceRate },
				{ UniversalEvent.ContextTypes.ServiceRateCurrency, ParentBO.ES_RX_NKServiceRateCurrency },
				{ UniversalEvent.ContextTypes.ServiceMeasurementBasis, ParentBO.ES_MeasurementBasis },
				{ UniversalEvent.ContextTypes.ServiceSubLocation, ParentBO.ES_SubLocation },
				{ UniversalEvent.ContextTypes.ServiceDuration, ParentBO.ES_Duration },
				{ UniversalEvent.ContextTypes.ServiceReference, ParentBO.ES_References },
				{ UniversalEvent.ContextTypes.ServiceContractor, ParentBO.Contractor?.OH_Code ?? ZString.Empty },
				{ UniversalEvent.ContextTypes.ServiceLocation, ParentBO.Location?.Header?.OH_Code ?? ZString.Empty },
				{ UniversalEvent.ContextTypes.ServiceLocationAddress, ParentBO.Location?.OA_Code ?? ZString.Empty }
			};

			if (ParentBO.ES_ExternalServiceId.IsEmpty)
			{
				var consignment = ParentBO.RequestForServiceParent as IConsignment;
				if (consignment != null)
				{
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ShipmentNumber, consignment.ShipmentNumber);
				}
			}
			else
			{
				contextValues.Add(UniversalEvent.ContextTypes.ExternalServiceId, ParentBO.ES_ExternalServiceId);
			}

			return contextValues;
		}

		#region GetEventParentFinder

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ServiceEventParentFinder(factory, this, logger);
		}

		#endregion

		protected override bool IsNotFromSameSystemAndModule(IDataContextDataObject dataContext, ISimpleLogger logger) => true;

		#region OnUniversalEventAddedCore

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);

			var eventValueObject = (IXmlEventValueObject)eventAdded;
			if ((eventValueObject?.Context?.ServiceId).GetValueOrDefault() == ZString.Empty)
			{
				return;
			}

			switch (eventValueObject.EventType)
			{
				case AutoEvents.ServiceRequestedCode:
				case AutoEvents.ServiceCompletedCode:
					ParentBO.SetIfNotNull(JobServiceSchema.ES_ExternalServiceId, eventValueObject.Context.ServiceId);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_ServiceCode, eventValueObject.Context.ServiceType);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_ServiceCount, eventValueObject.Context.ServiceCount);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_ServiceNote, eventValueObject.Context.ServiceNotes);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_ServiceRate, eventValueObject.Context.ServiceRate);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_MeasurementBasis, eventValueObject.Context.ServiceMeasurementBasis);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_SubLocation, eventValueObject.Context.ServiceSubLocation);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_Duration, eventValueObject.Context.ServiceDuration);
					ParentBO.SetIfNotNull(JobServiceSchema.ES_References, eventValueObject.Context.ServiceReference);

					switch (eventAdded.EventType.GetValueOrDefault())
					{
						case AutoEvents.ServiceRequestedCode:
							ParentBO.SetIfNotNull(JobServiceSchema.ES_Booked, eventAdded.EventTime?.ToZDateTime());
							break;
						case AutoEvents.ServiceCompletedCode:
							ParentBO.SetIfNotNull(JobServiceSchema.ES_Completed, eventAdded.EventTime?.ToZDateTime());
							break;
					}

					if (eventValueObject.Context.ServiceContractor.HasValue)
					{
						var serviceContractor = eventValueObject.Context.ServiceContractor.Value.IsEmpty
							? null
							: ParentBO.Factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, eventValueObject.Context.ServiceContractor.Value);
						ParentBO.SetIfNotNull(JobServiceSchema.ES_OH_Contractor, serviceContractor?.PK ?? Guid.Empty);
					}

					if (eventValueObject.Context.ServiceLocation.HasValue && eventValueObject.Context.ServiceLocationAddress.HasValue)
					{
						var serviceLocationAddress = eventValueObject.Context.ServiceLocation.Value.IsEmpty && eventValueObject.Context.ServiceLocationAddress.Value.IsEmpty
							? null
							: ParentBO.Factory.LoadTop1<OrgAddress>(GetOrgAddressQuery(eventValueObject.Context.ServiceLocation.Value, eventValueObject.Context.ServiceLocationAddress.Value));
						ParentBO.SetIfNotNull(JobServiceSchema.ES_OA_Location, serviceLocationAddress?.PK ?? Guid.Empty);
					}

					ParentBO.SetIfNotNull(JobServiceSchema.ES_RX_NKServiceRateCurrency, eventValueObject.Context.ServiceRateCurrency);
					ParentBO.UpdateExternalServiceId(eventValueObject.Context);

					break;
			}
		}

		static ZQuery GetOrgAddressQuery(ZString orgHeaderCode, ZString orgAddressCode)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, orgHeaderCode);

			var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Code, orgAddressCode);
			orgAddressQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			return orgAddressQuery;
		}

		#endregion
	}
}
