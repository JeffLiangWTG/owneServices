using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.Test
{
	class StatusTransactionScopeTest : TestCaseWithFactory
	{
		public void TestRollback()
		{
			var mawb = Factory.New<CusMAWB>();
			var scope = new StatusTransactionScope();
			scope.Add(mawb.CM_CustomsStatusInfo);

			mawb.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			var log = mawb.Logs.AddNew(Events.MessageSent, "SND", ZDateTimeOffset.Now, false);
			scope.AddLog(log);

			AssertEquals(1, mawb.Logs.GetAllLogs().Count);

			scope.Rollback();

			AssertEquals(LowValueManifestStatusList.Codes.NotSentToCustoms, mawb.CM_CustomsStatus);
			AssertEquals(0, mawb.Logs.GetAllLogs().Count);
		}
	}
}
