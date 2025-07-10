using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	class ScheduleAndPrincipalUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public ScheduleAndPrincipalUniqueIndexFailureHandler(VoyageAccount voyageAccount)
		{
			this.voyageAccount = voyageAccount;
		}

		readonly VoyageAccount voyageAccount;

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var otherVoyageAccountQuery = voyageAccount.IsDistinctQuery;
			otherVoyageAccountQuery.FetchOnlyFromLocalCache = true;

			var otherVoyageAccount = voyageAccount.Factory.LoadTop1<VoyageAccount>(otherVoyageAccountQuery);

			if (otherVoyageAccount != null)
			{
				otherVoyageAccount.Reload();
			}
			else
			{
				var otherVoyageAccountDBQuery = new ZDBOnlyQuery(voyageAccount.GetType());
				otherVoyageAccountDBQuery.AddToFilter(otherVoyageAccountQuery);
				voyageAccount.Factory.LoadTop1<VoyageAccount>(otherVoyageAccountDBQuery);
			}

			voyageAccount.Validation.ValidateNA_Calc_Voyage();
			voyageAccount.Validation.ValidateNA_Calc_Vessel();
			voyageAccount.Validation.ValidateNA_OH();
			notifier.ReportInformation(voyageAccount.IsNotDistinctErrorMessage, Res.GetString("66e4b0cb-0cd2-4362-875a-2e0fcec7598e", "Information"));
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return JobVoyAccountSchema.Constants.Indexes.NR_UX__NA_JV_NA_OH_NA_GC; }
		}
	}
}
