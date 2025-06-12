using System;
namespace CargoWise.eHub.DataAccess.Integration
{
	public class EnterpriseExeDetail
	{
		public EnterpriseExeDetail(string releaseStatus, DateTime versionDate, string licenceType)
		{
			ReleaseStatus = releaseStatus;
			VersionDate = versionDate;
			LicenceType = licenceType;
		}

		public string ReleaseStatus { get; private set; }
		public DateTime VersionDate { get; private set; }
		public string LicenceType { get; private set; }

		public static implicit operator EnterpriseExeDetail(eServices.eHubDataAccess.Integration.EnterpriseExeDetail enterpriseExeDetail)
			=> enterpriseExeDetail is null ? null : new EnterpriseExeDetail(enterpriseExeDetail.ReleaseStatus, enterpriseExeDetail.VersionDate, enterpriseExeDetail.LicenceType);
	}
}
