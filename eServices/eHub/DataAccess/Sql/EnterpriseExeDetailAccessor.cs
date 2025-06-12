using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class EnterpriseExeDetailAccessor : IEnterpriseExeDetailAccessor
	{
		public EnterpriseExeDetailAccessor() : this(new eServices.eHubDataAccess.Sql.EnterpriseExeDetailAccessor()) { }

		public EnterpriseExeDetailAccessor(eServices.eHubDataAccess.Integration.IEnterpriseExeDetailAccessor enterpriseExeDetailAccessor)
		{
			this.enterpriseExeDetailAccessor = enterpriseExeDetailAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IEnterpriseExeDetailAccessor enterpriseExeDetailAccessor;

		public EnterpriseExeDetail GetExeDetail(string senderID)
			=> enterpriseExeDetailAccessor.GetExeDetail(senderID);

		public string GetLicenceType(string senderID)
			=> enterpriseExeDetailAccessor.GetLicenceType(senderID);
	}
}
