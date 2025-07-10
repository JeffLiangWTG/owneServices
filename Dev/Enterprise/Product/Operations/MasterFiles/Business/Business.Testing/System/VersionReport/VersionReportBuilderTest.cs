using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.VersionReport.Testing
{
	sealed class VersionReportBuilderTest : TestCaseWithFactory
	{
		public void TestCreateVersionReport_ExtractsSystemInfoProperly()
		{
			GlbCompany.CurrentCompany.GC_Code = "COM";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SRV";

			VersionReport report = VersionReportBuilder.CreateCurrent("SomeUsage", false);
			var entryKeyRegex = new Regex("^.+(?=:)");

			Assert(report.AdditionalDatabaseSystemInfoList.Count > 0);

			foreach (var entry in report.AdditionalDatabaseSystemInfoList)
			{
				// Extract the key
				var match = entryKeyRegex.Match(entry);
				Assert(String.Format("Database information entry without a key: {0}", entry), match.Success);

				Assert(String.Format("Database information entry is not part of the standard entries: {0}", match.Value), VersionReport.DbConfigFieldKeys.Values.Concat(VersionReport.VersionFieldKeys.Values).Contains(match.Value));
			}

			AssertEquals("SomeUsage", report.LicenceUsage);
		}

		public void TestCreateVersionReport_Current()
		{
			GlbCompany.CurrentCompany.GC_Code = "COM";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SRV";

			var currentVersion = ReleaseInfo.Instance.VersionNumber.ToString();

			var report = VersionReportBuilder.CreateCurrent("SomeUsage", false);
			AssertEquals("EnterpriseCode", "ENT", report.EnterpriseCode);
			AssertEquals("PhysicalServerID", "SRV", report.PhysicalServerID);
			AssertEquals("DBServerName", Db.Connection.ServerNameReportedByDatabase, report.DBServerName);
			AssertEquals("DBName", Db.DatabaseName, report.DBName);
			AssertEquals("CurrentVersion", currentVersion, report.CurrentVersion);
			AssertEquals("CurrentRelease", ReleaseInfo.Instance.ReleaseDisplayText, report.CurrentRelease);

			var expectedDateTime = ZDateTime.UtcNow;
			var actualDateTime = report.CurrentDate;
			Assert("CurrentDate should be " + expectedDateTime + " (+/- 2 minutes), but was " + actualDateTime + ".",
				expectedDateTime.AddMinutes(-2) <= actualDateTime &&
				expectedDateTime.AddMinutes(2) >= actualDateTime);

			Assert(report.AdditionalDatabaseSystemInfoList.Count > 0);
			AssertEquals("SomeUsage", report.LicenceUsage);
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("MAXDOP")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("CostOfParallelism")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("OptimizeForAdHocWorkloads")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("MinMemory")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("MaxMemory")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("TotalAvailableMemory")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("TraceFlags")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("DbAlwaysOn")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("ParameterizationForced")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("AutoCreate")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("AutoUpdate")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("AutoUpdateAsync")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("CompatibilityLevel")));
			AssertEquals(true, report.AdditionalDatabaseSystemInfoList.Any(i => i.Contains("Cardinality")));
		}

		public void TestCreateVersionReport_Delivered()
		{
			const string DeliveredVersion = "1.1.1900.11111";

			GlbCompany.CurrentCompany.GC_Code = "COM";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SRV";

			ZDateTime dateTimeToUse = ZDateTime.UtcNow;

			VersionReport report = VersionReportBuilder.CreateDelivered(DeliveredVersion);

			AssertEquals("EnterpriseCode", "ENT", report.EnterpriseCode);
			AssertEquals("PhysicalServerID", "SRV", report.PhysicalServerID);
			AssertEquals("DBServerName", Db.Connection.ServerNameReportedByDatabase, report.DBServerName);
			AssertEquals("DBName", Db.DatabaseName, report.DBName);
			AssertEquals("CurrentVersion", DeliveredVersion, report.CurrentVersion);
			AssertEquals("CurrentRelease", ReleaseInfo.GetReleaseDisplayText("", new VersionNumber(1, 4, 1900, 11111)), report.CurrentRelease);

			ZDateTime expectedDateTime = dateTimeToUse;
			ZDateTime actualDateTime = report.CurrentDate;
			Assert("CurrentDate should be " + expectedDateTime + " (+/- 2 minutes), but was " + actualDateTime + ".",
				expectedDateTime.AddMinutes(-2) <= actualDateTime &&
				expectedDateTime.AddMinutes(2) >= actualDateTime);

			AssertNull("no licence usage in a delivered version report", report.LicenceUsage);
		}
	}
}
