using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateTransportZoneItemValidation : AutoRateTransportZoneItemValidation
	{
		public RateTransportZoneItemValidation(AutoRateTransportZoneItem parent)
			: base(parent)
		{
		}

		public new RateTransportZoneItem Parent
		{
			get { return (RateTransportZoneItem)base.Parent; }
		}

		#region Country

		protected override void CheckTQ_RN_NKCountry()
		{
			base.CheckTQ_RN_NKCountry();

			MandatoryValidation.CheckEntered(Parent.TQ_RN_NKCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TQ_RN_NKCountryInfo);

			var providerCountryCode = Parent.Zone.TransportProvider.CountryCode;
			if (!Parent.TQ_RN_NKCountry.IsEmpty && providerCountryCode != Parent.TQ_RN_NKCountry)
			{
				Parent.TQ_RN_NKCountryInfo.AddWarning(Res.GetString("0a8fb99e-07d2-462f-91af-7b08ebf5abb1", "Overriding zone country/region ({0}).", providerCountryCode));
			}
		}

		#endregion

		#region City/Town

		protected override void CheckTQ_R9_CityTown()
		{
			base.CheckTQ_R9_CityTown();
			if (Parent.CityTown != null && !Parent.HasRowErrors)
			{
				if (Parent.CityTown.R9_RN_NKCountry != Parent.TQ_RN_NKCountry)
				{
					Parent.TQ_R9_CityTownInfo.AddError(Res.GetString("01fbd615-2e62-4c54-93f6-4c291afdc7be", "Must belong to the country/region {0}.", Parent.TQ_RN_NKCountry));
				}
				ValidateOverlappingZoneItemsByCityTown(Parent.TQ_R9_CityTownInfo);
			}

			ValidateZoneHasCityTownOrDistancesOrPostCodes();
			CheckFromPostCodeBelongsToCityTown(Parent.TQ_R9_CityTownInfo);
		}

		#endregion

		#region Distance

		protected override void CheckTQ_FromDistance()
		{
			base.CheckTQ_FromDistance();
			CheckDistance(Parent.TQ_FromDistanceInfo);
			ValidateTQ_ToDistance();
		}

		protected override void CheckTQ_ToDistance()
		{
			base.CheckTQ_ToDistance();
			CheckDistance(Parent.TQ_ToDistanceInfo);
			ValidateTQ_FromDistance();
		}

		void CheckDistance(ZPropertyInfo info)
		{
			if (Parent.TQ_ToDistance > 0 && Parent.TQ_FromDistance >= Parent.TQ_ToDistance)
			{
				info.AddError(Res.GetString("676ca3b3-ff42-4d09-9f24-f94e58fc8c1d", "'From Distance' must be less than 'To Distance'."));
			}

			ValidateZoneHasCityTownOrDistancesOrPostCodes();

			if (!Parent.HasRowErrors)
			{
				ValidateOverlappingZoneItemsByDistance(info);
			}
		}

		#endregion

		#region Postcode

		protected override void CheckTQ_FromPostCode()
		{
			base.CheckTQ_FromPostCode();
			var info = Parent.TQ_FromPostCodeInfo;
			CheckPostCodeBelongsToCountry(info, Parent.TQ_FromPostCode);
			CheckPostCode(info);
			CheckFromPostCodeBelongsToCityTown(info);
			ValidateTQ_ToPostCode();
		}

		void CheckPostCodeBelongsToCountry(ZPropertyInfo info, ZString postCode)
		{
			if (!postCode.IsEmpty)
			{
				var refPostCode = Parent.LoadRefPostCode(postCode);
				if (refPostCode == null)
				{
					info.AddError(Res.GetString("cfcff20b-c5dd-4b0e-a341-5db9445b2bf3", "Postcodes must belong to the country/region {0}.", Parent.TQ_RN_NKCountry));
				}
			}
		}

		protected override void CheckTQ_ToPostCode()
		{
			base.CheckTQ_ToPostCode();
			var info = Parent.TQ_ToPostCodeInfo;
			CheckPostCodeBelongsToCountry(info, Parent.TQ_ToPostCode);
			CheckPostCode(info);

			if (Parent.TQ_FromPostCode.IsEmpty && !Parent.TQ_ToPostCode.IsEmpty)
			{
				info.AddError(Res.GetString("a1640f3e-d418-4a1a-a07e-647481cd73e1", "'To Postcode' can only be entered if 'From Postcode' is also entered."));
			}

			ValidateTQ_FromPostCode();
		}

		void CheckFromPostCodeBelongsToCityTown(ZPropertyInfo info)
		{
			if (Parent.TQ_R9_CityTown != Guid.Empty && !Parent.TQ_FromPostCode.IsEmpty)
			{
				var refPostCode = Parent.FromPostCode;
				if (refPostCode != null)
				{
					var query = new ZQuery(RefCityPCodePivotSchema.R0_RK, refPostCode.PK);
					query.AddToFilter(RefCityPCodePivotSchema.R0_R9, Parent.TQ_R9_CityTown);
					var pivot = Parent.Factory.Load<RefCityPCodePivot>(query);
					if (pivot == null || pivot.Length == 0)
					{
						info.AddError(Res.GetString("E69AE150-ED60-4626-A095-1E9DFE247B8E", "'From Postcode' must belong to the City/Town specified."));
					}
				}
			}
		}

		void CheckPostCode(ZPropertyInfo info)
		{
			if (!Parent.TQ_FromPostCode.IsEmpty && !Parent.TQ_ToPostCode.IsEmpty)
			{
				var comparer = new PostcodeOrderComparer();
				if (comparer.Compare(Parent.TQ_FromPostCode, Parent.TQ_ToPostCode) > 0)
				{
					info.AddError(Res.GetString("88dcf3f8-e2cb-439f-9421-19ba7a88976d", "'From Postcode' must be less than 'To Postcode'."));
				}
			}

			ValidateZoneHasCityTownOrDistancesOrPostCodes();

			if (!Parent.TQ_FromPostCode.IsEmpty && !Parent.HasRowErrors)
			{
				ValidateOverlappingZoneItemsByPostCode(info);
			}
		}

		#endregion

		protected override void CheckTQ_IsExcludingPostCode()
		{
			if (!Parent.TQ_IsExcludingPostCode_ReadOnly &&
				Parent.TQ_IsExcludingPostCode &&
				(!Parent.TQ_FromPostCode.IsEmpty || !Parent.TQ_ToPostCode.IsEmpty))
			{
				Parent.TQ_IsExcludingPostCodeInfo.AddError(Res.GetString("A32B24F4-D50C-4E2C-970B-BCDB83080637", "Can only be checked if both From Postcode and To Postcode are blank."));
			}
		}

		#region Overlap

		protected override void CheckTQ_DeliveryDueTime()
		{
		}

		protected override void CheckTQ_DeliveryDueTimeIsValidZDateTimeRange()
		{
		}

		void ValidateOverlappingZoneItemsByCityTown(ZPropertyInfo propertyInfo)
		{
			AddErrorsIfOverlapping(propertyInfo, x => OverlapsCityTown(Parent, x));
		}

		void ValidateOverlappingZoneItemsByDistance(ZPropertyInfo propertyInfo)
		{
			AddErrorsIfOverlapping(propertyInfo, x => OverlapsDistance(Parent, x));
		}

		void ValidateOverlappingZoneItemsByPostCode(ZPropertyInfo propertyInfo)
		{
			AddErrorsIfOverlapping(propertyInfo, x => OverlapsPostCode(Parent, x));
		}

		void AddErrorsIfOverlapping(ZPropertyInfo info, Func<RateTransportZoneItem, bool> checkFunc)
		{
			if (Parent.Zone.TZ_IsActive)
			{
				if (info.HasChanges || !Parent.IsInDatabase || Parent.Zone.TZ_IsActiveInfo.HasChanges)
				{
					var items = Parent.Zone.TransportProvider.Zones
						.Where(z => z.TZ_IsActive)
						.SelectMany(x => x.Items)
						.Where(x => x != Parent && checkFunc(x));

					foreach (var item in items)
					{
						info.AddError(Res.GetString("92c5a79a-991b-4f45-920c-174405583f99", "This {0} is already part of zone ({1}) as a {2}.", Parent.ToDescriptiveString(), item.Zone.TZ_ZoneName, item.ToDescriptiveString()));
					}
				}
			}
		}

#if DEBUG
		public
#endif
		static bool OverlapsCityTown(RateTransportZoneItem parentItem, RateTransportZoneItem item)
		{
			var result = false;
			if (parentItem.CityTown != null)
			{
				result = parentItem.TQ_FromPostCode.IsEmpty
					? RateTransportZoneHelper.IsCityTownMatching(item, parentItem.CityTown)
					: RateTransportZoneHelper.IsCityTownMatching(item, parentItem.CityTown, true, parentItem.TQ_FromPostCode);
			}

			return result;
		}

#if DEBUG
		public
#endif
		static bool OverlapsDistance(RateTransportZoneItem parentItem, RateTransportZoneItem item)
		{
			return parentItem.TQ_ToDistance != 0
					&& item.TQ_ToDistance != 0
					&& item.TQ_FromDistance < parentItem.TQ_ToDistance
					&& parentItem.TQ_FromDistance < item.TQ_ToDistance;
		}

#if DEBUG
		public
#endif
		static bool OverlapsPostCode(RateTransportZoneItem parentItem, RateTransportZoneItem item)
		{
			var result = false;

			if (!item.TQ_FromPostCode.IsEmpty && !parentItem.TQ_FromPostCode.IsEmpty && item.TQ_RN_NKCountry == parentItem.TQ_RN_NKCountry)
			{
				var comparer = new PostcodeOrderComparer();
				if (!item.TQ_ToPostCode.IsEmpty)
				{
					if (!parentItem.TQ_ToPostCode.IsEmpty)
					{
						result = !(comparer.Compare(item.TQ_FromPostCode, parentItem.TQ_ToPostCode) > 0 || comparer.Compare(parentItem.TQ_FromPostCode, item.TQ_ToPostCode) > 0);
					}
					else
					{
						var citiesPKsWithPostCode = RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(item.Factory, parentItem.TQ_FromPostCode);
						result = RateTransportZoneHelper.IsPostCodeWithinRange(item, parentItem.TQ_FromPostCode, checkPostCodeCities: true, citiesPKsWithPostCode);
					}
				}
				else
				{
					if (!parentItem.TQ_ToPostCode.IsEmpty)
					{
						var citiesPKsWithPostCode = RateTransportZoneHelper.GetAllCitiesPKsWithPostCode(parentItem.Factory, item.TQ_FromPostCode);
						result = RateTransportZoneHelper.IsPostCodeWithinRange(parentItem, item.TQ_FromPostCode, checkPostCodeCities: true, citiesPKsWithPostCode);
					}
					else
					{
						result = (comparer.Compare(item.TQ_FromPostCode, parentItem.TQ_FromPostCode) == 0
							&& item.TQ_R9_CityTown == parentItem.TQ_R9_CityTown);
					}
				}
			}
			else if (item.CityTown != null)
			{
				result = RateTransportZoneHelper.IsCityTownMatching(parentItem, item.CityTown);
			}

			return result;
		}

		#endregion

		#region Has City/Town or Distance or Postcode(s)

		void ValidateZoneHasCityTownOrDistancesOrPostCodes()
		{
			var error = Res.GetString("0537BDE5-6090-4B92-BBA2-80CB8E150CFA", @"You must specify either a city/town, postcode, a city/town postcode combination, postcode range or distance range.
Please set distance range to 0 if not required.");

			Parent.RemoveRowError(error);

			var hasCityTown = !Parent.TQ_R9_CityTown.IsEmpty;
			var hasDistanceRange = Parent.TQ_FromDistance > 0 || Parent.TQ_ToDistance > 0;
			var hasPostCode = !Parent.TQ_FromPostCode.IsEmpty;

			if ((new[] { hasPostCode, hasDistanceRange, hasCityTown }).Count(x => x) != 1)
			{
				if (!(hasPostCode && hasCityTown && !hasDistanceRange))
				{
					Parent.AddRowError(error);
				}
			}
		}

		#endregion

		#region BeyondHours

		public void ValidateBeyondDays()
		{
			ValidateCalculatedProperty(Parent.BeyondDaysInfo);
		}

		protected virtual void CheckBeyondDays()
		{
			MandatoryValidation.CheckNotNegative(Parent.BeyondDaysInfo);
		}

		public void ValidateBeyondHours()
		{
			ValidateCalculatedProperty(Parent.BeyondHoursInfo);
		}

		protected virtual void CheckBeyondHours()
		{
			MandatoryValidation.CheckNotNegative(Parent.BeyondHoursInfo);

			if (Parent.BeyondHours >= 24)
			{
				Parent.BeyondHoursInfo.AddError(Res.GetString("393879a2-aa59-4763-a327-652d3cdeacd4", "Hours must be less than 24. Please use the 'Days' component for longer times."));
			}
		}

		#endregion
	}
}

