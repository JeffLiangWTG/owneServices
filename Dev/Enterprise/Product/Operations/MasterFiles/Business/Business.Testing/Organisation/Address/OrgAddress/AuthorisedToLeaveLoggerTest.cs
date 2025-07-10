using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AuthorisedToLeaveLoggerTest : TestCaseWithFactory
	{
		public void Test_OA_AuthorityToLeave_Log()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.Z0_NVarChar = "DEF";
			var log = AuthorisedToLeaveLogger.Log(dummy, dummy.Z0_NVarCharInfo);
			AssertNull("Should be no logs, as the object is not in the DB yet.", log);

			Factory.Save();
			log = AuthorisedToLeaveLogger.Log(dummy, dummy.Z0_NVarCharInfo);
			AssertNull("Should be no logs, as DEF is default. (first value)", log);

			dummy.Z0_NVarChar = "YES";
			log = AuthorisedToLeaveLogger.Log(dummy, dummy.Z0_NVarCharInfo);
			AssertEquals(Events.Authorised.Code, log.SL_SE_NKEvent);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
			AssertEquals("Parent is Dummy", dummy.PK, log.SL_Parent);

			Factory.Save();
			log = AuthorisedToLeaveLogger.Log(dummy, dummy.Z0_NVarCharInfo);
			AssertNull("Should be no new log, as there are no changes.", log);
		}
	}
}
