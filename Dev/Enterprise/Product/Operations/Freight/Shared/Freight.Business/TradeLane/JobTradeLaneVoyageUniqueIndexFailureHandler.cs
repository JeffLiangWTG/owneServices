using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneVoyageUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public JobTradeLaneVoyageUniqueIndexFailureHandler(JobTradeLaneVoyage tradeLaneVoyage)
		{
			this.TradeLaneVoyage = tradeLaneVoyage;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return JobTradeLaneVoyageSchema.Constants.Indexes.FK_UX__NB_OH_NB_JV; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			JobVoyage voyage = TradeLaneVoyage.Voyage;
			string message = GenerateMessage();
			voyage.TradeLanes.Factory.ClearQueryCache();
			voyage.TradeLanes.Factory.Load<JobTradeLaneVoyage>(new ZQuery(JobTradeLaneVoyageSchema.NB_JV, voyage.PK));
			((IActiveBusinessObjectCollection)voyage.TradeLanes).Refresh();
			TradeLaneVoyage.Delete();
			notifier.ReportError(message, Res.GetString("f804039d-4f67-44b0-8afa-dfb466e27f18", "Save Error"));
		}

		string GenerateMessage()
		{
			return Res.GetString("79f3ac9a-ff71-40d5-a602-e5bbf5d1783b", "Another user has added the principal {0} to this schedule, please review your changes and save again.", TradeLaneVoyage.Principal.OH_Code) + "\r\n";
		}

		readonly JobTradeLaneVoyage TradeLaneVoyage;
	}
}
