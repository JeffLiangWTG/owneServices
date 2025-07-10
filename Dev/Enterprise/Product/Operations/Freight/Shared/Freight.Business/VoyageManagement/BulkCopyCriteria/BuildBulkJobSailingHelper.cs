using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class BuildBulkJobSailingHelper : BuildJobSailingHelper
	{
		public BuildBulkJobSailingHelper(BulkCopyCriteria bulkCopy)
			: base(bulkCopy.Factory)
		{
			bulkCopyCriteria = bulkCopy;
		}

		public void BulkCopySchedule()
		{
			ZDateTime referenceDate = GetReferenceDate(SourceVoyage);

			if (!referenceDate.IsEmpty)
			{
				foreach (ZDateTime newDateTime in bulkCopyCriteria.RepititionSelection.GetDateTimes(referenceDate))
				{
					TimeSpan span = newDateTime - referenceDate;

					CloneJobVoyage(span);
				}
			}
		}

		#region Implementation

		void CloneJobVoyage(TimeSpan timeDifference)
		{
			if (!VoyageExists(timeDifference))
			{
				JobVoyage newVoyage = GetClonedVoyage(SourceVoyage);
				UpdateScheduleDates(newVoyage, timeDifference);
				newVoyage.PopulateSendersReferenceIfNeeded();
				InsertSailingIntoList(newVoyage);
			}
		}

		void InsertSailingIntoList(JobVoyage newVoyage)
		{
			foreach (JobSailing newSailing in newVoyage.Sailings)
			{
				bulkCopyCriteria.Schedules.Add(newSailing);
			}
		}

		ZDateTime GetReferenceDate(JobVoyage voyage)
		{
			RepititionSelection selection = bulkCopyCriteria.RepititionSelection;
			ZDateTime result = ZDateTime.Empty;

			if (selection.FromFirstETA)
			{
				foreach (VoyageDestination destination in voyage.Destinations)
				{
					ZDateTime date = destination.JB_E_ARV;

					if (!date.IsEmpty && (result.IsEmpty || date < result))
					{
						result = date;
					}
				}
			}
			else if (selection.FromFirstETD)
			{
				foreach (VoyageOrigin origin in voyage.Origins)
				{
					ZDateTime date = origin.JA_E_DEP;

					if (!date.IsEmpty && (result.IsEmpty || date < result))
					{
						result = date;
					}
				}
			}

			return result;
		}

		protected override JobVoyage SourceVoyage
		{
			get { return bulkCopyCriteria.Sailing.Voyage; }
		}

		readonly BulkCopyCriteria bulkCopyCriteria;

		#endregion
	}
}
