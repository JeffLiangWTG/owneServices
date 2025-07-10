using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.VersionReport
{
	public interface IVersionReportSender
	{
		VersionReport SendCurrent(string licenceUsage, bool isLicenceUsageRequest);
	}

	public class VersionReportBuilderFactory : IVersionReportSender, IVersionUpgradeReportSender
	{
		void IVersionUpgradeReportSender.Send()
		{
			SendBasic();
		}

		public VersionReport SendBasic()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			VersionReport report = VersionReport.CreateBasic(registrationKey,
				Db.Connection.ServerNameReportedByDatabase,
				Db.DatabaseName,
				ReleaseInfo.Instance.VersionNumber.ToString(),
				ZDateTime.UtcNow);
			Send(report, true);
			return report;
		}

		public VersionReport SendDelivered(string deliveredVersion)
		{
			VersionReport report = VersionReportBuilder.CreateDelivered(deliveredVersion);
			Send(report, false);
			return report;
		}

		public VersionReport SendCurrent(string licenceUsage, bool isLicenceUsageRequest)
		{
			VersionReport report = VersionReportBuilder.CreateCurrent(licenceUsage, isLicenceUsageRequest);
			Send(report, true);
			return report;
		}

		void Send(VersionReport report, bool isCurrent)
		{
#if DEBUG
			ReportsSent.Add(report);
#endif
			VersionReportEHubBuilder.Send(report, isCurrent);
		}

#if DEBUG
		static public void ClearSent()
		{
			ReportsSent.Clear();
		}
		static public List<VersionReport> ReportsSent = new List<VersionReport>();
		static public int SentCount { get { return ReportsSent.Count; } }
		static public VersionReport LastSent { get { return SentCount > 0 ? ReportsSent[SentCount - 1] : null; } }
#endif
	}
}
