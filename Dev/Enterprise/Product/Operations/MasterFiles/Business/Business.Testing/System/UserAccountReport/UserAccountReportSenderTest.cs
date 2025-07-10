using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UserAccountReportSenderTest : TestCaseWithFactory
	{
		public void TestSendUserAccountWhenStaffListIsNotEmpty()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsResource = false;
			staff.GS_IsSystemAccount = false;
			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var userAccountReportSender = new UserAccountReportSender();
			SystemDataRegistry.Instance.UserAccountLastReportTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2024,1,1).ToDateTime());
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			userAccountReportSender.SendReport(lastRunTime);
			var interchangeType = ObjectFactory.GetType<IXmlEDIInterchange>();
			AssertEquals("ehub message created", 1, Factory.GetDatabaseCount(interchangeType));
		}

		public void TestShouldNotSendUserAccountWhenStaffListIsEmpty()
		{
			var userAccountReportSender = new UserAccountReportSender();
			SystemDataRegistry.Instance.UserAccountLastReportTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2024, 1, 1).ToDateTime());
			var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
			userAccountReportSender.SendReport(lastRunTime);
			var interchangeType = ObjectFactory.GetType<IXmlEDIInterchange>();
			AssertEquals("ehub message created", 0, Factory.GetDatabaseCount(interchangeType));
		}
	}
}
