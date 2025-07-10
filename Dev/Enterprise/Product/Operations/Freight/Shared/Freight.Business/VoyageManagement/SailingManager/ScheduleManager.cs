using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	internal sealed class ScheduleManager : BaseSailingManager
	{
		public ScheduleManager(ISailingManaged parent)
			: base(parent) { }

		#region Implementation

		protected override bool HasSufficientInformationCore
		{
			get
			{
				bool result = false;

				if (HasCommonMandatoryValues)
				{
					if (parent.IsCharter)
					{
						result = !parent.RegistrationNo.IsEmpty;
					}
					else
					{
						result = !parent.Voyage.IsEmpty;
					}
				}
				return result;
			}
		}

		#region Existing Voyage

		protected override JobVoyage GetExistingVoyage(bool ignoreActiveFilter = false)
		{
			JobVoyage existingVoyage = GetMostCloselyMatchingExistingVoyage(ignoreActiveFilter);

			if (!CanUpdateExistingVoyage(existingVoyage))
			{
				existingVoyage = null;
			}

			if (existingVoyage != null)
			{
				UpdateAircraftTypeIfNecessary(existingVoyage);
				UpdateCharteredStatusIfNecessary(existingVoyage);
			}

			return existingVoyage;
		}

		void UpdateCharteredStatusIfNecessary(JobVoyage voyage)
		{
			bool shouldUpdateIsCharted = IsCharterDirty;
			if (voyage.JV_IsChartered != parent.IsCharter && shouldUpdateIsCharted)
			{
				voyage.JV_IsChartered = parent.IsCharter;
			}
		}

		JobVoyage GetMostCloselyMatchingExistingVoyage(bool ignoreActiveFilter)
		{
			JobVoyage result = null;

			if (MostRelevantFlightTime.IsValidSmallDateTime)
			{
				var filter = GetMostCloselyMatchingVoyageShortListFilter(ignoreActiveFilter);

				result = GetBestMatchingExistingVoyage(filter);
			}

			return result;
		}

		ZQuery GetMostCloselyMatchingVoyageShortListFilter(bool ignoreActiveFilter)
		{
			const int spanInHoursNeededToLoadFlightsBecauseFlightDateIsDependandOnLocationAndFlightCannotBeLongerThan24Hrs = 48;

			var voyageFilter = new ZQuery();
			voyageFilter.IgnoreActiveFilter = ignoreActiveFilter;
			voyageFilter.AddToFilter(TransportModeFilter);

			var hours = parent.TransportMode == Constants.TransportModes.Air && parent.IsImportingData
					? SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.Value
					: spanInHoursNeededToLoadFlightsBecauseFlightDateIsDependandOnLocationAndFlightCannotBeLongerThan24Hrs;

			voyageFilter.AddToFilter(VoyagesWithinHoursOf(hours, MostRelevantFlightTime));

			if (parent.IsCharter)
			{
				voyageFilter.AddToFilter(JobVoyageSchema.JV_RegistrationNo, parent.RegistrationNo);
			}
			else
			{
				voyageFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, parent.Voyage);
			}

			return voyageFilter;
		}

		bool CanUpdateExistingVoyage(JobVoyage existingVoyage)
		{
			if (MostRelevantFlightTime.IsValidSmallDateTime
				&& existingVoyage != null
				&& existingVoyage != Voyage)
			{
				keepScheduleDates = false; // needs to be cleared before quering ShouldUpdateET[AD]
				userSelectsUpdateSchedule = false;

				bool canUpdateDate = FilterOnETA ? ShouldUpdateETA : ShouldUpdateETD;
				if (canUpdateDate || VoyageDirty)
				{
					var foundDate = GetFlightDate(existingVoyage, false);

					var queryArgs = new QueryFreshMatchBehaviourArgs
					{
						AddCheckpoint = FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(parent.TransportMode),
						EditCheckpoint = FreightUtilities.GetEditScheduleSecurityCheckpoint(parent.TransportMode),
						TransportMode = parent.TransportMode,
						IsImportingData = parent.IsImportingData,
						DateName = FilterOnETA ? "ETA" : "ETD",
						RequestedDate = MostRelevantFlightTime,
						FoundDate = foundDate
					};

					var queryProvider = SailingManagerQueryProviderFactory.Get(parent.Factory);
					var updateMode = queryProvider.QueryFreshMatchBehaviour(queryArgs);

					userSelectsUpdateSchedule = queryProvider.SelectedByUser && updateMode == SailingManagerUpdateMode.UpdateSchedule;
					keepScheduleDates = (updateMode == SailingManagerUpdateMode.ScheduleUnchanged);

					forceNewSchedule = false;
					if (updateMode == SailingManagerUpdateMode.NewSchedule)
					{
						forceNewSchedule = true;
						return false;
					}
				}
			}

			return true;
		}

		void UpdateAircraftTypeIfNecessary(JobVoyage voyage)
		{
			if (voyage.JV_AircraftType != parent.AircraftType && IsAircraftTypeDirty)
			{
				voyage.JV_AircraftType = parent.AircraftType;
			}
		}

		#endregion

		protected override JobVoyage NewVoyage()
		{
			JobVoyage result = parent.Factory.New<JobVoyage>();
			result.JV_VoyageFlight = parent.Voyage;
			result.JV_RV_NKVessel = parent.Vessel;
			result.JV_AirSeaRoad = parent.TransportMode;
			result.JV_IsChartered = parent.IsCharter;
			result.JV_RegistrationNo = parent.RegistrationNo;
			result.JV_IsCargoOnly = parent.IsCargoOnly;
			result.JV_FlightDate = MostRelevantFlightTime;
			result.JV_AircraftType = parent.AircraftType;

			return result;
		}

		protected override bool ShouldUpdateETD
		{
			get
			{
				if (!FilterOnETA && userSelectsUpdateSchedule)
				{
					return true;
				}

				var shouldUpdateETD = DepartureDirty && !VoyageDirty && !LoadDirty && !DischargeDirty;
				if (!shouldUpdateETD)
				{
					return false;
				}

				if (Voyage != null && parent.ETD.IsValid)
				{
					var voyageOriginFilter = new ZQuery(JobVoyOriginSchema.JA_JV, Voyage.PK);
					voyageOriginFilter.AddToFilter(new ZQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, parent.Load));
					var origin = parent.Factory.LoadTop1<VoyageOrigin>(voyageOriginFilter);

					if (origin != null && !origin.IsInDatabase)
					{
						return true;
					}
				}

				return !keepScheduleDates;
			}
		}

		protected override bool ShouldUpdateETA
		{
			get
			{
				if (FilterOnETA && userSelectsUpdateSchedule)
				{
					return true;
				}

				var shouldUpdateETA = ArrivalDirty && !VoyageDirty && !LoadDirty && !DischargeDirty;
				if (!shouldUpdateETA)
				{
					return false;
				}

				if (Voyage != null && parent.ETA.IsValid)
				{
					var voyageDestinationFilter = new ZQuery(JobVoyDestinationSchema.JB_JV, Voyage.PK);
					voyageDestinationFilter.AddToFilter(new ZQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, parent.Discharge));
					var destination = parent.Factory.LoadTop1<VoyageDestination>(voyageDestinationFilter);

					if (destination != null && !destination.IsInDatabase)
					{
						return true;
					}
				}

				return !keepScheduleDates;
			}
		}

		protected override bool ShouldCheckUniqueVoyage
		{
			get { return !forceNewSchedule; }
		}

		protected override ZQuery GetExistingMatchingVoyageQuery()
		{
			var query = base.GetExistingMatchingVoyageQuery();
			query.AddToFilter(GetMostCloselyMatchingVoyageShortListFilter(false));

			return query;
		}

		protected override JobVoyage GetBestMatchingExistingVoyage(ZQuery voyageFilter)
		{
			JobVoyage result = null;

			if (!MostRelevantFlightTime.IsValidSmallDateTime)
			{
				return result;
			}

			var minimalTimeDifference = new TimeSpan(0, 23, 59, 59);

			foreach (var possibleVoyage in parent.Factory.Load<JobVoyage>(voyageFilter))
			{
				var voyageDate = GetFlightDate(possibleVoyage, true);
				if (voyageDate.IsValidSmallDateTime)
				{
					if (Voyage != null && Voyage.PK == possibleVoyage.PK
						&& Math.Abs((MostRelevantFlightTime - possibleVoyage.JV_FlightDate).Ticks) < TimeSpan.TicksPerDay)
					{
						result = possibleVoyage;
						break;
					}

					var timeDifference = voyageDate - MostRelevantFlightTime;
					if ((Math.Abs(timeDifference.Ticks) <= Math.Abs(minimalTimeDifference.Ticks)))
					{
						minimalTimeDifference = timeDifference;
						result = possibleVoyage;
					}
				}
			}

			return result;
		}

		bool HasCommonMandatoryValues
		{
			get
			{
				return !parent.Load.IsEmpty
					&& !parent.Discharge.IsEmpty
					&& MostRelevantFlightTime.IsValidSmallDateTime;
			}
		}

		ZDateTime GetFlightDate(JobVoyage possibleVoyage, bool allowFallback)
		{
			ZDateTime result;

			if (FilterOnETA)
			{
				VoyageDestination destination = possibleVoyage.Destinations.GetDestinationFromDischarge(parent.Discharge);
				result = (destination == null ? ZDateTime.Empty : destination.JB_E_ARV);
			}
			else
			{
				VoyageOrigin origin = possibleVoyage.Origins.GetOriginFromLoading(parent.Load);
				result = (origin == null ? ZDateTime.Empty : origin.JA_E_DEP);
			}

			if (!result.IsValidSmallDateTime && allowFallback)
			{
				result = possibleVoyage.JV_FlightDate;
			}

			return result;
		}

		bool FilterOnETA
		{
			get { return ImportExportHelper.IsImport(parent.Load, parent.Discharge); }
		}

		ZDateTime MostRelevantFlightTime => FilterOnETA ? parent.ETA : parent.ETD;

		ZQuery VoyagesWithinHoursOf(ZInt hours, ZDateTime eT)
		{
			var result = new ZQuery();
			if (eT.IsValidSmallDateTime)
			{
				var greaterThanDateTime = eT.AddHours(-hours);
				if (greaterThanDateTime.IsValidSmallDateTime)
				{
					result.AddToFilter(new ZQuery(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.GreaterThan, greaterThanDateTime));
				}

				var lessThanDateTime = eT.AddHours(hours);
				if (lessThanDateTime.IsValidSmallDateTime)
				{
					result.AddToFilter(new ZQuery(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.LessThan, lessThanDateTime));
				}
			}

			return result;
		}

		bool keepScheduleDates;

		bool forceNewSchedule;

		bool userSelectsUpdateSchedule;

		#endregion
	}
}
