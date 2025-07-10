using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class LocationValidator : ValidationProvider
	{
		public LocationValidator(RateEntry entry, ZPropertyInfo propertyInfo)
			: base(entry)
		{
			this.entry = Argument.NotNull(entry, "entry");
			this.propertyInfo = Argument.NotNull(propertyInfo, "propertyInfo");

			if (propertyInfo.Name != RateEntrySchema.TI_OriginLRC.Name
				&& propertyInfo.Name != RateEntrySchema.TI_DestinationLRC.Name
				&& propertyInfo.Name != RateEntrySchema.TI_ViaLRC.Name
				&& propertyInfo.Name != RateEntrySchema.TI_PlannedLoadLRC.Name
				&& propertyInfo.Name != RateEntrySchema.TI_PlannedDischargeLRC.Name
				&& propertyInfo.Name != RateEntrySchema.TI_RateOrigin.Name
				&& propertyInfo.Name != RateEntrySchema.TI_RateDestination.Name)
			{
				throw new ArgumentException("This validator is only for TI_OriginLRC, TI_DestinationLRC, TI_ViaLRC, TI_PlannedLoadLRC, TI_PlannedDischargeLRC, TI_RateOrigin or TI_RateDestination properties");
			}
		}

		readonly RateEntry entry;
		readonly ZPropertyInfo propertyInfo;

		#region Validation

		public void CheckRateEntryLocations(ILocation location)
		{
			ValidatePortCorrectMode(location);

			// Not using ListValidation to load the location, since it's not smart enough to do it efficiently
			if ((location as ICancellable)?.IsCancelled ?? false)
			{
				ListValidation.ErrorIfCancelled(propertyInfo, true);
			}
		}

		void ValidatePortCorrectMode(ILocation location)
		{
			var loco = location as RefUNLOCO;

			if (loco != null)
			{
				if ((entry.IsAir() && !loco.RL_HasAirport) || (entry.IsSea() && !loco.RL_HasSeaport))
				{
					var message = Res.GetString("73b3a6cd-18b3-4713-ba02-1c53a2cfa095",
												"The port {0} is not {1} so should not be used here.",
												loco.Code,
												entry.IsAir()
													? Res.GetString("b048e0c6-0f37-447c-ac70-e53e63df7ad8", "an Airport")
													: Res.GetString("259089fe-9eea-49bb-aced-2571326bd67a", "a Seaport"));
					propertyInfo.AddWarning(message);
				}
			}
		}

		#endregion

		#region IATACityCode Validation

		public void ValidateIATACityCode(ILocation location)
		{
			var locationCode = propertyInfo.Value.ToString();

			if (LocationHelper.GetLocationType(locationCode) == LocationHelper.LocationType.IATACityCode)
			{
				if (location != null && !(entry.IsAirFreight() && entry.Parent.IsStandardCostRate()))
				{
					propertyInfo.AddError(Res.GetString("A9164DB8-01A1-45C4-AE21-0013D8FB1145", "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."));
				}
			}
		}

		#endregion

		#region Zone

		public void ValidateInternationalZone()
		{
			if (propertyInfo.Value.ToString().Length != 4)
			{
				return;
			}

			var zone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, propertyInfo.Value));
			if (zone == null)
			{
				return;
			}

			if (!zone.IsRatingAvailableZone)
			{
				propertyInfo.AddError(Res.GetString("e0b3b9fa-7625-4914-a539-4db5ce5050cc", "You have chosen a zone that is not available for Rating purposes. Please choose a rating zone."));
				return;
			}

			if (!IsZoneModeCompatibleWithRateMode(zone.FZ_ZoneMode))
			{
				propertyInfo.AddError(Res.GetString("C3F86515-6DF3-48F8-9026-4BC69A6A1BF2", "This {0} cannot be chosen as the International Zone Mode is incompatible with this Rate Mode.", propertyInfo.HumanReadableName));
				return;
			}

			if (zone.RelatedParty == null)
			{
				return;
			}

			var zoneOwners = new List<OrgHeader>
			{
				entry.Publisher.OrgProxy,
				entry.Parent != null ? entry.Parent.Header : null,
				entry.Supplier,
				entry.TransportProvider,
				entry.Consignee,
				entry.Consignor
			};

			if (zoneOwners.All(x => x != zone.RelatedParty))
			{
				propertyInfo.AddError(GetErrorMessageForZone(zone));
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		bool IsZoneModeCompatibleWithRateMode(string zoneMode)
		{
			if (zoneMode == entry.TI_Mode)
			{
				return true;
			}

			switch (zoneMode)
			{
				case Core.Constants.RateMode.ALL:
					return true;

				case Core.Constants.RateMode.FCL:
					return entry.IsSeaFreight() && entry.TI_Mode == Core.Constants.RateMode.SEA;

				case Core.Constants.RateMode.FRO:
					return entry.IsRoadFreight() && entry.TI_Mode == Core.Constants.RateMode.ROA;

				case Core.Constants.RateMode.FRA:
					return entry.IsRailFreight() && entry.TI_Mode == Core.Constants.RateMode.RAI;

				case Core.Constants.RateMode.FTL:
					return entry.IsPortTransport() && entry.TI_Mode == Core.Constants.RateMode.FRO;

				case Core.Constants.RateMode.AIR:
					return entry.TI_Mode == Core.Constants.RateMode.ULD
						|| entry.TI_Mode == Core.Constants.RateMode.LSE;

				case Core.Constants.RateMode.SEA:
					return entry.TI_Mode == Core.Constants.RateMode.LCL
						|| entry.TI_Mode == Core.Constants.RateMode.FCL;

				case Core.Constants.RateMode.ROA:
					return entry.TI_Mode == Core.Constants.RateMode.LRO
						|| entry.TI_Mode == Core.Constants.RateMode.FRO
						|| entry.TI_Mode == Core.Constants.RateMode.FTL;

				case Core.Constants.RateMode.RAI:
					return entry.TI_Mode == Core.Constants.RateMode.LRA
						|| entry.TI_Mode == Core.Constants.RateMode.FRA
						|| entry.TI_Mode == Core.Constants.RateMode.FWL;
			}
			return false;
		}

		static string GetErrorMessageForZone(RefZoneHeader zone)
		{
			return Res.GetString("3f1c3915-6d5d-4ec3-bb92-39459ed3a6f3", "You have chosen a zone that is specific for an Organization {0}.",
				zone.RelatedParty.OH_Code);
		}

		#endregion
	}
}

