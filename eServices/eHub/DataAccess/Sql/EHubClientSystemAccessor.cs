using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class EHubClientSystemAccessor : IEHubClientSystemAccessor
	{
		public EHubClientSystemAccessor() : this(new eServices.eHubDataAccess.Sql.EHubClientSystemAccessor()) { }

		public EHubClientSystemAccessor(eServices.eHubDataAccess.Integration.IEHubClientSystemAccessor eHubClientSystemAccessor)
		{
			this.eHubClientSystemAccessor = eHubClientSystemAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IEHubClientSystemAccessor eHubClientSystemAccessor;

		public virtual void InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage(string systemID, string url, Guid trackingID, string senderID)
			=> eHubClientSystemAccessor.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage(systemID, url, trackingID, senderID);
	}
}
