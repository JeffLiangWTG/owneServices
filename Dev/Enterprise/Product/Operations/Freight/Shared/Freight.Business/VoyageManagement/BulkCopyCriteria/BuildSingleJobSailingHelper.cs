using System;

namespace Enterprise.Freight.Business
{
	internal sealed class BuildSingleJobSailingHelper : BuildJobSailingHelper
	{
		public BuildSingleJobSailingHelper(BaseJobSailing sailing)
			: base(sailing.Factory)
		{
			this.sailing = sailing;
		}

		public BaseJobSailing CopySchedule(TimeSpan timeDifference)
		{
			BaseJobSailing result = null;

			if (!VoyageExists(timeDifference))
			{
				JobVoyage newVoyage = GetClonedVoyage(SourceVoyage);
				UpdateScheduleDates(newVoyage, timeDifference);
				newVoyage.PopulateSendersReferenceIfNeeded();

				if (!FilteredOrigin.IsEmpty && !FilteredDestination.IsEmpty)
				{
					result = newVoyage.Sailings.GetSailingFromLoadAndDischarge(FilteredOrigin, FilteredDestination);
				}
				else if (newVoyage.Sailings.Count > 0)
				{
					result = newVoyage.Sailings[0];
				}
			}

			return result;
		}

		#region Implementation

		protected override JobVoyage SourceVoyage
		{
			get { return sailing.Voyage; }
		}

		readonly BaseJobSailing sailing;

		#endregion
	}
}
