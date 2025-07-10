using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementRecipientModifiedLogsManagerTest : TestCaseWithFactory
	{
		public void TestLogs()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();

			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var recipient = agreement.Recipients.AddNew();
			recipient.CAR_GS_NKStaff = "ADL";
			Factory.Save();
			var modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(1, modifiedLogs.Length);
			AssertEquals("Attached Commission Agreement Entity (ADL)", modifiedLogs[0].SL_Reference);

			recipient.Delete();
			Factory.Save();
			modifiedLogs = agreement.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusChangeCode).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals(2, modifiedLogs.Length);
			AssertEquals("Detached Commission Agreement Entity (ADL)", modifiedLogs[1].SL_Reference);
		}
	}
}
