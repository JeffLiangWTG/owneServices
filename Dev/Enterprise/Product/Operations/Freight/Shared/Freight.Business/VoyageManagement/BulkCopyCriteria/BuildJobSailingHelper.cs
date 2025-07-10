using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public abstract class BuildJobSailingHelper
	{
		public BuildJobSailingHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; private set; }
		public ZString FilteredOrigin { get; set; }
		public ZString FilteredDestination { get; set; }

		#region Implementation

		protected bool VoyageExists(TimeSpan timeDifference)
		{
			ZDateTime flightDate = SourceVoyage.JV_FlightDate;

			if (flightDate.IsEmpty || !flightDate.IsValid)
			{
				return false;
			}
			else
			{
				ZDateTime target = flightDate.Add(timeDifference);

				ZQuery sqlQuery = new ZQuery();
				sqlQuery.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, SourceVoyage.JV_AirSeaRoad);
				sqlQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, SourceVoyage.JV_VoyageFlight);
				sqlQuery.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.GreaterThan, target.AddHours(-12));
				sqlQuery.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.LessThan, target.AddHours(12));

				return Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(JobVoyage)), sqlQuery);
			}
		}

		protected JobVoyage GetClonedVoyage(JobVoyage sourceVoyage)
		{
			JobVoyage newVoyage = (JobVoyage)sourceVoyage.Clone();

			if (!FilteredOrigin.IsEmpty)
			{
				for (int i = newVoyage.Origins.Count - 1; i >= 0; i--)
				{
					VoyageOrigin origin = newVoyage.Origins[i];
					if (origin.JA_RL_NKPortOfLoading != FilteredOrigin)
					{
						newVoyage.Origins.RemoveAndDelete(origin);
					}
				}
			}

			if (!FilteredDestination.IsEmpty)
			{
				for (int i = newVoyage.Destinations.Count - 1; i >= 0; i--)
				{
					VoyageDestination destination = newVoyage.Destinations[i];
					if (destination.JB_RL_NKPortOfDischarge != FilteredDestination)
					{
						newVoyage.Destinations.RemoveAndDelete(destination);
					}
				}
			}

			return newVoyage;
		}

		protected void UpdateScheduleDates(JobVoyage newVoyage, TimeSpan timeSpan)
		{
			foreach (VoyageOrigin origin in newVoyage.Origins)
			{
				if (origin.JA_E_DEP.IsValid)
				{
					origin.JA_E_DEP = origin.JA_E_DEP.Add(timeSpan);
				}

				if (origin.JA_S_DEP.IsValid)
				{
					origin.JA_S_DEP = origin.JA_S_DEP.Add(timeSpan);
				}

				VoyageOrigin sourceOrigin = SourceVoyage.Origins.GetOriginFromLoading(origin.JA_RL_NKPortOfLoading);

				if (sourceOrigin.JA_CutOff.IsValid)
				{
					origin.JA_CutOff = sourceOrigin.JA_CutOff.Add(timeSpan);
				}

				if (sourceOrigin.JA_DocumentaryCutoff.IsValid)
				{
					origin.JA_DocumentaryCutoff = sourceOrigin.JA_DocumentaryCutoff.Add(timeSpan);
				}

				if (sourceOrigin.JA_VGMCutOff.IsValid)
				{
					origin.JA_VGMCutOff = sourceOrigin.JA_VGMCutOff.Add(timeSpan);
				}

				if (sourceOrigin.JA_ReceivalCommences.IsValid)
				{
					origin.JA_ReceivalCommences = sourceOrigin.JA_ReceivalCommences.Add(timeSpan);
				}

				if (sourceOrigin.JA_DGReceivalCommences.IsValid)
				{
					origin.JA_DGReceivalCommences = sourceOrigin.JA_DGReceivalCommences.Add(timeSpan);
				}

				if (sourceOrigin.JA_DGCutOff.IsValid)
				{
					origin.JA_DGCutOff = sourceOrigin.JA_DGCutOff.Add(timeSpan);
				}

				origin.JA_A_DEP = ZDateTime.Empty;
			}

			foreach (VoyageDestination dest in newVoyage.Destinations)
			{
				if (dest.JB_E_ARV.IsValid)
				{
					dest.JB_E_ARV = dest.JB_E_ARV.Add(timeSpan);
				}

				if (dest.JB_S_ARV.IsValid)
				{
					dest.JB_S_ARV = dest.JB_S_ARV.Add(timeSpan);
				}

				VoyageDestination sourceDest = SourceVoyage.Destinations.GetDestinationFromDischarge(dest.JB_RL_NKPortOfDischarge);

				if (sourceDest.JB_AvailabilityDate.IsValid)
				{
					dest.JB_AvailabilityDate = sourceDest.JB_AvailabilityDate.Add(timeSpan);
				}

				if (sourceDest.JB_StorageDate.IsValid)
				{
					dest.JB_StorageDate = sourceDest.JB_StorageDate.Add(timeSpan);
				}

				dest.JB_A_ARV = ZDateTime.Empty;
			}

			foreach (JobSailing newSailing in newVoyage.Sailings)
			{
				JobSailing sourceSailing = SourceVoyage.Sailings.GetSailingFromLoadAndDischarge(newSailing.JX_JA_RL_NKPortOfLoading, newSailing.JX_JB_RL_NKPortOfDischarge);

				if (sourceSailing != null)
				{
					if (sourceSailing.JX_DepotReceivalCommences.IsValid)
					{
						newSailing.JX_DepotReceivalCommences = sourceSailing.JX_DepotReceivalCommences.Add(timeSpan);
					}

					if (sourceSailing.JX_DepotCutOff.IsValid)
					{
						newSailing.JX_DepotCutOff = sourceSailing.JX_DepotCutOff.Add(timeSpan);
					}

					if (sourceSailing.JX_DepotAvailabilityDate.IsValid)
					{
						newSailing.JX_DepotAvailabilityDate = sourceSailing.JX_DepotAvailabilityDate.Add(timeSpan);
					}

					if (sourceSailing.JX_DepotStorageDate.IsValid)
					{
						newSailing.JX_DepotStorageDate = sourceSailing.JX_DepotStorageDate.Add(timeSpan);
					}
				}
			}

			if (SourceVoyage.JV_FlightDate.IsValid)
			{
				newVoyage.JV_FlightDate = SourceVoyage.JV_FlightDate.Add(timeSpan);
			}
		}

		protected abstract JobVoyage SourceVoyage { get; }

		#endregion
	}
}
