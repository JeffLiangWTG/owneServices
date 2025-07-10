using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using static CargoWise.Definitions.LicenceFeatureCodeList;

namespace Enterprise.MasterFiles.Business.VersionReport.Testing
{
	sealed class VersionReportForTesting : VersionReport
	{
		public VersionReportForTesting(IProductRegistrationKey regKey, bool isVerbose,
			ZString dbServerName,
			ZString dbName,
			ZString currentVersion,
			ZString releaseRing,
			ZDateTime currentDate,
			List<String> additionalInfoList,
			string licenceUsage)
			: base(regKey, isVerbose, dbServerName, dbName, currentVersion, releaseRing, currentDate, additionalInfoList, licenceUsage)
		{
		}

		public VersionReportForTesting(ZString xmlData)
			: base(xmlData)
		{
		}

		internal void AddCompaniesForTest(GlbCompany[] companies)
		{
			CompanyList.Clear();
			AddCompanies(companies);
		}

		protected override IEnumerable<LicenceFeatureCodePair> PopulateSupportedFeatureCodes()
		{
			return new List<LicenceFeatureCodePair>
			{
				new LicenceFeatureCodePair("CR5RESWIZ", "CR5 Resolution Wizard", FeatureStage.Active),
				new LicenceFeatureCodePair("ACCRBKFTR", "Accounting Reporting Book Feature", FeatureStage.Development),
				new LicenceFeatureCodePair("ACCGLDFTR", "Accounting General Ledger Data Feature Control", FeatureStage.Development),
				new LicenceFeatureCodePair("CWNext", "CargoWise Next", FeatureStage.Active),
			};
		}
	}
}
