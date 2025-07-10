using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.VersionReport.Testing
{
	sealed class VersionReportBuilderFactoryTest : TestCaseWithFactory
	{
		public void TestSendDelivered()
		{
			VersionReportBuilderFactory reportBuilderFactory = new VersionReportBuilderFactory();
			var report = reportBuilderFactory.SendDelivered("1.2.3.4");
			AssertEquals("1.2.3.4", report.CurrentVersion);
			var interchangeType = ObjectFactory.GetType<IXmlEDIInterchange>();
			AssertEquals("ehub message created", 1, Factory.GetDatabaseCount(interchangeType));
			AssertEquals("no mail created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendCurrent()
		{
			VersionReportBuilderFactory reportBuilderFactory = new VersionReportBuilderFactory();
			var report = reportBuilderFactory.SendCurrent("some usage", false);
			AssertEquals("some usage", report.LicenceUsage);
			var interchangeType = ObjectFactory.GetType<IXmlEDIInterchange>();
			AssertEquals("ehub message created", 1, Factory.GetDatabaseCount(interchangeType));
			AssertEquals("no mail created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 6, 16, 5, 0)]
		public void TestSendBasic()
		{
			VersionReportBuilderFactory reportBuilderFactory = new VersionReportBuilderFactory();
			var report = reportBuilderFactory.SendBasic();
			AssertEquals(TestDateAttribute.Date, report.CurrentDate);
			AssertEquals(ReleaseInfo.Instance.VersionNumber.ToString(), report.CurrentVersion);
			var interchangeType = ObjectFactory.GetType<IXmlEDIInterchange>();
			AssertEquals("ehub message created", 1, Factory.GetDatabaseCount(interchangeType));
			AssertEquals("no mail created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}
	}
}
