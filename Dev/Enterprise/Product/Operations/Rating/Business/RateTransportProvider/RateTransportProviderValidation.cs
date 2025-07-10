using System.Linq;

namespace Enterprise.Rating.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class RateTransportProviderValidation : AutoRateTransportProviderValidation
	{
		public RateTransportProviderValidation(AutoRateTransportProvider parent)
			: base(parent)
		{
			transportZoneSet = (RateTransportProvider)parent;
		}

		readonly RateTransportProvider transportZoneSet;

		#region Properties

		protected override void CheckTP_DefaultDeliveryDueTime()
		{
		}

		protected override void CheckTP_DefaultDeliveryDueTimeIsValidZDateTimeRange()
		{
		}

		protected override void CheckTP_DefaultHoldForPickupTime()
		{
		}

		protected override void CheckTP_DefaultHoldForPickupTimeIsValidZDateTimeRange()
		{
		}

		protected override void CheckTP_OH_RelatedParty()
		{
			base.CheckTP_OH_RelatedParty();
			ValidateDuplicatedTransportZonesSets();
		}

		protected override void CheckTP_RN_NKCountry()
		{
			base.CheckTP_RN_NKCountry();

			if (Parent.ZoneHubLocation == null)
			{
				MandatoryValidation.CheckEntered(Parent.TP_RN_NKCountryInfo);
				ListValidation.ErrorIfInvalidCode(Parent.TP_RN_NKCountryInfo);
			}
			ValidateTP_OH_RelatedParty();
		}

		protected override void CheckTP_ZoneType()
		{
			base.CheckTP_ZoneType();
			MandatoryValidation.CheckEntered(Parent.TP_ZoneTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TP_ZoneTypeInfo);

			ValidateTP_OH_RelatedParty();
		}

		protected override void CheckTP_ZoneMode()
		{
			base.CheckTP_ZoneMode();
			ListValidation.ErrorIfInvalidCode(Parent.TP_ZoneModeInfo);
		}

		#endregion

		#region Validate Duplicated Transport Zone Set Information

		void ValidateDuplicatedTransportZonesSets()
		{
			var zoneOwner = transportZoneSet.TP_OH_RelatedParty.IsEmpty ? null : (object)transportZoneSet.TP_OH_RelatedParty;
			var zoneHub = transportZoneSet.TP_R9_ZoneHubLocation.IsEmpty ? null : (object)transportZoneSet.TP_R9_ZoneHubLocation;

			var filter = new ZQuery(RateTransportProviderSchema.PK, SQLComparisonOperator.NotEqual, transportZoneSet.PK);
			filter.AddToFilter(RateTransportProviderSchema.TP_OH_RelatedParty, zoneOwner);
			filter.AddToFilter(RateTransportProviderSchema.TP_R9_ZoneHubLocation, zoneHub);
			filter.AddToFilter(RateTransportProviderSchema.TP_RN_NKCountry, transportZoneSet.TP_RN_NKCountry);
			filter.AddToFilter(RateTransportProviderSchema.TP_IsActive, true);

			var otherProviders = transportZoneSet.Factory.Load<RateTransportProvider>(filter);
			if (otherProviders.Any())
			{
				if (otherProviders.Any(x => x.TP_ZoneType == Parent.TP_ZoneType && x.TP_ZoneMode == Parent.TP_ZoneMode))
				{
					Parent.TP_OH_RelatedPartyInfo.AddError(Res.GetString("43d00e86-0529-4ae6-a3b9-5abca842d57a", "Transport zone set cannot be created because there is an existing transport zone set with same owner, location, zone type and zone mode."));
				}
			}
		}

		#endregion
	}
}

