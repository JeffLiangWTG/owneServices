using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.VersionReport.Testing
{
	sealed class VersionReportEHubBuilderTest : TestCaseWithFactory
	{
		public void TestSendCurrent()
		{
			GlbCompany.CurrentCompany.GC_Code = "COM";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SRV";

			ZDateTime dateTimeToUse = ZDateTime.UtcNow;

			var outgoing = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(outgoing);

			var versionReport = VersionReportBuilder.CreateCurrent("SomeUsage", false);
			VersionReportEHubBuilder.Send(versionReport, true);

			AssertEquals("MessageName", SystemMessageList.Descriptions.CurrentVersionReport, DebugOnlyOutgoingSystemMessage.MessageName);
			string bodyText = Encoding.UTF8.GetString(DebugOnlyOutgoingSystemMessage.MessageStream);
			VersionReport report = VersionReport.CreateForTest(bodyText);

			AssertEquals("EnterpriseCode", "ENT", report.EnterpriseCode);
			AssertEquals("PhysicalServerID", "SRV", report.PhysicalServerID);
			AssertEquals("DBServerName", Db.Connection.ServerNameReportedByDatabase, report.DBServerName);
			AssertEquals("DBName", Db.DatabaseName, report.DBName);

			ZDateTime expectedDateTime = dateTimeToUse;
			ZDateTime actualDateTime = report.CurrentDate;
			Assert("CurrentDate should be " + expectedDateTime + " (+/- 2 minutes), but was " + actualDateTime + ".",
				expectedDateTime.AddMinutes(-2) <= actualDateTime &&
				expectedDateTime.AddMinutes(2) >= actualDateTime);

			Assert(report.AdditionalDatabaseSystemInfoList.Count > 0);
			AssertEquals("SomeUsage", report.LicenceUsage);

			DebugOnlyOutgoingSystemMessage.Initialize();
		}

		public void TestSendDelivered()
		{
			const string DeliveredVersion = "1.1.1900.11111";
			GlbCompany.CurrentCompany.GC_Code = "COM";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SRV";

			ZDateTime dateTimeToUse = ZDateTime.UtcNow;

			var outgoing = new DebugOnlyOutgoingSystemMessage();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(outgoing);

			var versionReport = VersionReportBuilder.CreateDelivered(DeliveredVersion);
			VersionReportEHubBuilder.Send(versionReport, false);

			AssertEquals("MessageName", SystemMessageList.Descriptions.DeliveredVersionReport, DebugOnlyOutgoingSystemMessage.MessageName);
			string bodyText = Encoding.UTF8.GetString(DebugOnlyOutgoingSystemMessage.MessageStream);
			VersionReport report = VersionReport.CreateForTest(bodyText);

			AssertEquals("EnterpriseCode", "ENT", report.EnterpriseCode);
			AssertEquals("PhysicalServerID", "SRV", report.PhysicalServerID);
			AssertEquals("DBServerName", Db.Connection.ServerNameReportedByDatabase, report.DBServerName);
			AssertEquals("DBName", Db.DatabaseName, report.DBName);
			AssertEquals("CurrentVersion", DeliveredVersion, report.CurrentVersion);

			ZDateTime expectedDateTime = dateTimeToUse;
			ZDateTime actualDateTime = report.CurrentDate;
			Assert("CurrentDate should be " + expectedDateTime + " (+/- 2 minutes), but was " + actualDateTime + ".",
				expectedDateTime.AddMinutes(-2) <= actualDateTime &&
				expectedDateTime.AddMinutes(2) >= actualDateTime);
			AssertNull("no licence usage in a delivered version report", report.LicenceUsage);

			DebugOnlyOutgoingSystemMessage.Initialize();
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "HYEMELJKW");
		}

		#endregion
	}
}
