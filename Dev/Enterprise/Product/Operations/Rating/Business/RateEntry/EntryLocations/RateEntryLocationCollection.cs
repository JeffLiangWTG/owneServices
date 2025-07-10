using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class RateEntryLocationCollection : NonPersistentBusinessObjectCollection<RateEntryLocation>
	{
		public RateEntryLocationCollection(RateEntry parentRateEntry)
			: base(parentRateEntry?.Factory)
		{
			ParentRateEntry = Argument.NotNull(parentRateEntry, nameof(parentRateEntry));

			Load();
		}

		protected override bool AllowRemoveCore => true;

		public override void Load()
		{
			using (SuspendListChanged())
			using (SuspendSettingHasChanges())
			{
				SuspendValidation();
				try
				{
					RemoveAndDeleteAll();

					if (!ParentRateEntry.TI_FirstLoadLRC.IsEmpty)
					{
						var firstLoad = AddNew();
						using (firstLoad.SuspendSettingHasChanges())
						{
							firstLoad.Location = ParentRateEntry.TI_FirstLoadLRC;
							firstLoad.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstLoad;
						}
					}

					if (!ParentRateEntry.TI_LastDischargeLRC.IsEmpty)
					{
						var lastDischarge = AddNew();
						using (lastDischarge.SuspendSettingHasChanges())
						{
							lastDischarge.Location = ParentRateEntry.TI_LastDischargeLRC;
							lastDischarge.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.LastDischarge;
						}
					}

					if (!ParentRateEntry.TI_FirstRouteSetLoadPortLRC.IsEmpty)
					{
						var firstRouteSetLoad = AddNew();
						using (firstRouteSetLoad.SuspendSettingHasChanges())
						{
							firstRouteSetLoad.Location = ParentRateEntry.TI_FirstRouteSetLoadPortLRC;
							firstRouteSetLoad.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad;
						}
					}

					if (!ParentRateEntry.TI_LastRouteSetDischargePortLRC.IsEmpty)
					{
						var lastRouteSetDischarge = AddNew();
						using (lastRouteSetDischarge.SuspendSettingHasChanges())
						{
							lastRouteSetDischarge.Location = ParentRateEntry.TI_LastRouteSetDischargePortLRC;
							lastRouteSetDischarge.LocationSourceOption = RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge;
						}
					}
				}
				finally
				{
					ResumeValidation();
				}
			}
		}

		public void UpdateLocation(ZString locationSourceOption, ZString location)
		{
			var locationSettings = FindLocationSettings(locationSourceOption);

			if (locationSettings.IsNullOrEmpty())
			{
				if (!location.IsEmpty)
				{
					var locationSetting = AddNew();
					locationSetting.LocationSourceOption = locationSourceOption;
					locationSetting.Location = location;
				}

				return;
			}

			locationSettings.First().Location = location;

			foreach (var redundantLocationSetting in locationSettings.Skip(1).ToArray())
			{
				RemoveAndDelete(redundantLocationSetting);
			}
		}

		public bool PopulateBackToRateEntry()
		{
			RunPreSaveValidation();

			var hasErrors = this.HasErrors() || this.Any(x => x.HasErrors);

			if (!hasErrors)
			{
				using (new DisposableAction(
					() => ParentRateEntry.RateEntryLocationFieldsSuspended = true,
					() => ParentRateEntry.RateEntryLocationFieldsSuspended = false))
				{
					ParentRateEntry.TI_FirstLoadLRC = FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad);
					ParentRateEntry.TI_LastDischargeLRC = FindLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge);
					ParentRateEntry.TI_FirstRouteSetLoadPortLRC = FindLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad);
					ParentRateEntry.TI_LastRouteSetDischargePortLRC = FindLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge);
				}
			}

			return hasErrors;
		}

		IEnumerable<RateEntryLocation> FindLocationSettings(ZString locationSourceOption) =>
			this.Cast<RateEntryLocation>().Where(x => x.LocationSourceOption == locationSourceOption);

		public ZString FindLocation(ZString locationSourceOption)
		{
			var locationSettings = FindLocationSettings(locationSourceOption);

			return locationSettings.Count() == 1
				? locationSettings.First().Location
				: ZString.Empty;
		}

		public RateEntry ParentRateEntry { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new RateEntryLocation(this);
	}
}
