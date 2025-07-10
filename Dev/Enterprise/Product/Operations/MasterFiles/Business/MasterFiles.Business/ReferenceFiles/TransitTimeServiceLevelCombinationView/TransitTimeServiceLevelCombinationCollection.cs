using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.TransitTimeServiceLevelCombination)]
	public class TransitTimeServiceLevelCombinationCollection : ActiveBusinessObjectCollection<TransitTimeServiceLevelCombinationView>, IFilterModuleExtraNotificationProvider
	{
		public TransitTimeServiceLevelCombinationCollection(BusinessObjectFactory factory, OrgHeader originZoneOwner, OrgHeader destinationZoneOwner, JobDocAddress pickupAddress, JobDocAddress deliveryAddress, ZString? mode) : base(factory)
		{
			this.originZoneOwner = originZoneOwner;
			this.destinationZoneOwner = destinationZoneOwner;
			this.pickupAddress = pickupAddress;
			this.deliveryAddress = deliveryAddress;
			this.mode = mode;

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Service Level", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Origin Zone", "Property", ZGuid.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Destination Zone", "Property", ZGuid.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Origin Zone Owner/Carrier", "Property", originZoneOwner?.PK ?? ZGuid.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Destination Zone Owner/Carrier", "Property", destinationZoneOwner?.PK ?? ZGuid.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Mode", "Property", mode ?? ZString.Empty));

			SetAdditionalFilter();
		}

		readonly OrgHeader originZoneOwner;
		readonly OrgHeader destinationZoneOwner;
		readonly JobDocAddress pickupAddress;
		readonly JobDocAddress deliveryAddress;
		readonly ZString? mode;

		void SetAdditionalFilter()
		{
			if (originZoneOwner != null)
			{
				AdditionalFilter.AddToFilter(AddTransitHoursSubQuery(GetRelatedOrgFilter(originZoneOwner.PK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_OriginDomesticZone)));
			}

			if (destinationZoneOwner != null)
			{
				AdditionalFilter.AddToFilter(AddTransitHoursSubQuery(GetRelatedOrgFilter(destinationZoneOwner.PK, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_DestinationDomesticZone)));
			}

			AddModeFilterForTransportMode(mode);
		}

		void AddModeFilterForTransportMode(ZString? mode)
		{
			if (mode.HasValue)
			{
				var ratingConstantsHelper = ObjectFactory.Get<IRatingConstantsHelper>();
				var applicableModes = ratingConstantsHelper.GetRateModes(mode.Value);
				var mainQuery = new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_Mode, SQLComparisonOperator.StartsWith, applicableModes);
				AdditionalFilter.AddToFilter(AddTransitHoursSubQuery(mainQuery));
			}
		}

		ZQuery GetRelatedOrgFilter(ZGuid orgPK, SchemaGuidColumn domesticColumn)
		{
			var transportProviderSubQuery = new ZDBOnlySubQuery(typeof(IRateTransportProvider), RateTransportProviderSchema.PK);
			transportProviderSubQuery.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, orgPK);

			var domesticQuery = new ZDBOnlySubQuery(typeof(IRateTransportZone), RateTransportZonesSchema.PK);
			domesticQuery.AddSubQuery(RateTransportZonesSchema.TZ_TP, transportProviderSubQuery, JoinCondition.And);

			var mainQuery = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			mainQuery.AddSubQuery(domesticColumn, domesticQuery, JoinCondition.Or);
			return mainQuery;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var transitTimeServiceLevelCombinationView = (TransitTimeServiceLevelCombinationView)selectedBusinessObject;

			if (transitTimeServiceLevelCombinationView.TSC_RTT.IsEmpty && transitTimeServiceLevelCombinationView.TSC_TransitHours > 0)
			{
				return;
			}

			if (originZoneOwner != null && transitTimeServiceLevelCombinationView.OriginZone?.TransportZoneOwnerPK != originZoneOwner.PK)
			{
				errors.Add(Res.GetString("23439eae-f272-493f-a86e-31f2f8a63e6d", "A Service Level selected from here must have an Origin Owner/Carrier ({0}).", originZoneOwner.OH_Code));
			}

			if (destinationZoneOwner != null && transitTimeServiceLevelCombinationView.DestinationZone?.TransportZoneOwnerPK != destinationZoneOwner.PK)
			{
				errors.Add(Res.GetString("dc091017-b0d6-4370-be5e-62a3dc0e54b8", "A Service Level selected from here must have a Destination Owner/Carrier ({0}).", destinationZoneOwner.OH_Code));
			}
		}

		bool IsThereATransitTimeBetweenTheOriginAndDestinationZonesWithThisServiceLevel(ZGuid? serviceLevelPK)
		{
			if (originZoneOwner == null || destinationZoneOwner == null)
			{
				return false;
			}

			foreach (var item in this)
			{
				if (item.TSC_RS == serviceLevelPK && item.OriginZone?.TransportZoneOwnerPK == originZoneOwner.PK && item.DestinationZone?.TransportZoneOwnerPK == destinationZoneOwner.PK)
				{
					return true;
				}
			}

			return false;
		}

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			var currentItem = businessObject as TransitTimeServiceLevelCombinationView;
			var isCurrentItemAGenericServiceLevelRecord = currentItem != null && (currentItem.OriginZone == null || currentItem.DestinationZone == null);
			if (isCurrentItemAGenericServiceLevelRecord)
			{
				var serviceLevelPK = currentItem.TSC_RS;
				if (IsThereATransitTimeBetweenTheOriginAndDestinationZonesWithThisServiceLevel(serviceLevelPK))
				{
					var message = Res.GetString("ffec78b4-84b8-4e67-b468-4e3f9d0124d6", "This generic service level cannot be selected since a transit time between the origin and destination zones exists.");
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, message);
				}
			}

			return null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var transitTimeQuery = new ZDBOnlyQuery(typeof(TransitTimeServiceLevelCombinationView));
			AddZoneQuery(pickupAddress, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_OriginDomesticZone, transitTimeQuery);
			AddZoneQuery(deliveryAddress, TransitTimeServiceLevelCombinationViewSchema.TSC_TZ_DestinationDomesticZone, transitTimeQuery);

			var additionalFilter = base.CreateRelationshipFilter();
			additionalFilter.AddToFilter(transitTimeQuery);
			return additionalFilter;
		}

		static void AddZoneQuery(JobDocAddress address, SchemaGuidColumn domesticColumn, ZDBOnlyQuery transitTimeQuery)
		{
			var zoneQuery = CreateRateTransportZoneItemQuery(address);
			if (zoneQuery != null)
			{
				transitTimeQuery.AddSubQuery(domesticColumn, zoneQuery, JoinCondition.And);
				AddTransitHoursSubQuery(transitTimeQuery);
			}
		}

		static ZDBOnlySubQuery CreateRateTransportZoneItemQuery(JobDocAddress address)
		{
			if (address == null || address.Postcode.IsEmpty)
			{
				return null;
			}

			var postCodeRangeQuery = new ZQuery();
			postCodeRangeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_FromPostCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			postCodeRangeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_ToPostCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			postCodeRangeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_FromPostCode, SQLComparisonOperator.LessThanOrEqualTo, address.Postcode);
			postCodeRangeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_ToPostCode, SQLComparisonOperator.GreaterThanOrEqualTo, address.Postcode);

			var citySubQuery = new ZDBOnlySubQuery(typeof(RefCityTown), RefCityTownSchema.PK);
			citySubQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, address.City);

			var cityPostCodeQuery = new ZDBOnlyQuery(typeof(IRateTransportZoneItem));
			cityPostCodeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_FromPostCode, address.Postcode);
			cityPostCodeQuery.AddToFilter(RateTransportZoneItemSchema.TQ_ToPostCode, ZString.Empty);
			cityPostCodeQuery.AddSubQuery(RateTransportZoneItemSchema.TQ_R9_CityTown, citySubQuery, JoinCondition.And);
			cityPostCodeQuery.AddToFilter(postCodeRangeQuery, JoinCondition.Or);

			var transportZoneItemQuery = new ZDBOnlySubQuery(typeof(IRateTransportZoneItem), RateTransportZoneItemSchema.TQ_TZ_DomesticZone);
			transportZoneItemQuery.AddToFilter(RateTransportZoneItemSchema.TQ_RN_NKCountry, address.Country.Code);
			transportZoneItemQuery.AddToFilter(cityPostCodeQuery);
			return transportZoneItemQuery;
		}

		static ZQuery AddTransitHoursSubQuery(ZQuery query)
		{
			var transitTimeSubQuery = new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, null);
			transitTimeSubQuery.AddToFilter(JoinCondition.And, TransitTimeServiceLevelCombinationViewSchema.TSC_TransitHours, SQLComparisonOperator.GreaterThan, 0);
			query.AddToFilter(transitTimeSubQuery, JoinCondition.Or);
			return query;
		}

		protected override string GetErrorMessageForNonOptionalRecord(BusinessObject selectedBusinessObject) =>
			Res.GetString("16EC0263-CD6A-47EB-B1B1-52803A5CD904", "This {0} cannot be chosen here. Please make changes within a job first to allow selection of applicable service level.", GetRecordDescription(selectedBusinessObject));
	}
}
