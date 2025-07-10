using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CMRReferenceFileUpdateLog))]
	class CMRReferenceFileUpdateLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFixedPKAndTableName()
		{
			CMRReferenceFileUpdateLog bizObj = GetUpdateLog();
			Assert("PK was assigned", bizObj.PK.IsValid);
			AssertEquals("PK has the fixed value", new ZGuid(CMRReferenceFileUpdateLog.LogReferencePK), bizObj.PK);
			AssertEquals("Table Name", CMRReferenceFileUpdateLog.ReferenceTableName, bizObj.TableName);
		}

		public void TestLogUpdateSuccess()
		{
			CMRReferenceFileUpdateLog log = GetUpdateLog();
			int logRecordsBefore = log.Logs.GetAllLogs().Count;
			log.LogUpdateSuccess();

			AssertEquals("One record was added", logRecordsBefore + 1, log.Logs.GetAllLogs().Count);

			CMRReferenceFileUpdateLog logReload = GetUpdateLog(new BusinessObjectFactory());
			Assert("At least one Log Record", logReload.Logs.GetAllLogs().Count > 0);
			AssertEquals("Top Record is a Success record", Events.UpgradeSucceeded.Code, logReload.Logs.GetAllLogs()[0].SL_SE_NKEvent);
		}

		public void TestLogUpdateFailure()
		{
			CMRReferenceFileUpdateLog log = GetUpdateLog();
			int logRecordsBefore = log.Logs.GetAllLogs().Count;
			log.LogUpdateFailure();

			AssertEquals("One record was added", logRecordsBefore + 1, log.Logs.GetAllLogs().Count);

			CMRReferenceFileUpdateLog logReload = GetUpdateLog(new BusinessObjectFactory());
			Assert("At least one Log Record", logReload.Logs.GetAllLogs().Count > 0);
			AssertEquals("Top Record is a Failure record", Events.UpgradeFailed.Code, logReload.Logs.GetAllLogs()[0].SL_SE_NKEvent);
		}

		public void TestCheckLastSuccessfulUpdateDate()
		{
			CMRReferenceFileUpdateLog log = GetUpdateLog();
			int logRecordsBefore = log.Logs.GetAllLogs().Count;
			log.LoadLastSuccessfulUpdate();
			var lastSuccess = log.SuccessfulUpdateFileTimeStamp;
			ZDateTime lastSuccessPosted = log.SuccessfulUpdatePostedTime;

			Assert("Current state is consistent", (logRecordsBefore > 0 && !lastSuccess.IsEmpty) || lastSuccess.IsEmpty);
			log.LogUpdateSuccess();
			AssertEquals("One record was added", logRecordsBefore + 1, log.Logs.GetAllLogs().Count);
			log.LoadLastSuccessfulUpdate();
			AssertEquals("We have a last success datetime", false, log.SuccessfulUpdateFileTimeStamp.IsEmpty);
			AssertEquals("We have a last success datetime", false, log.SuccessfulUpdatePostedTime.IsEmpty);

			CMRReferenceFileUpdateLog logReload = GetUpdateLog(new BusinessObjectFactory());
			logReload.LoadLastSuccessfulUpdate();
			AssertEquals("We have a last success datetime", false, logReload.SuccessfulUpdateFileTimeStamp.IsEmpty);
			AssertEquals("We have a last success datetime", false, logReload.SuccessfulUpdatePostedTime.IsEmpty);
			Assert("It is different from the previous", lastSuccess != logReload.SuccessfulUpdateFileTimeStamp);
			Assert("It is different from the previous", lastSuccessPosted != logReload.SuccessfulUpdatePostedTime);
		}

		public void TestCheckLastSuccessfulUpgradeWithEventTimeAssigned()
		{
			CMRReferenceFileUpdateLog log = GetUpdateLog();
			int logRecordsBefore = log.Logs.GetAllLogs().Count;
			var lastSuccess = log.SuccessfulUpdateFileTimeStamp;
			ZDateTime lastSuccessPosted = log.SuccessfulUpdatePostedTime;
			var lastModified = lastSuccess.IsEmpty ? ZDateTimeOffset.Now : lastSuccess.AddMinutes(10);

			Assert("Current state is consistent", (logRecordsBefore > 0 && !lastSuccess.IsEmpty) || lastSuccess.IsEmpty);
			log.LogUpdateSuccess(lastModified);
			AssertEquals("One record was added", logRecordsBefore + 1, log.Logs.GetAllLogs().Count);
			log.LoadLastSuccessfulUpdate();
			AssertEquals("We have a last success datetime", false, log.SuccessfulUpdateFileTimeStamp.IsEmpty);
			AssertEquals("We have a last success datetime", false, log.SuccessfulUpdatePostedTime.IsEmpty);

			CMRReferenceFileUpdateLog logReload = GetUpdateLog(new BusinessObjectFactory());
			logReload.LoadLastSuccessfulUpdate();
			AssertEquals("We have a last success datetime", false, logReload.SuccessfulUpdateFileTimeStamp.IsEmpty);
			AssertEquals("We have a last success datetime", false, logReload.SuccessfulUpdatePostedTime.IsEmpty);
			Assert("It is different from the previous", lastSuccess != logReload.SuccessfulUpdateFileTimeStamp);
			Assert("It is different from the previous", lastSuccessPosted != logReload.SuccessfulUpdatePostedTime);
			AssertEquals("It should be time we logged", lastModified, logReload.SuccessfulUpdateFileTimeStamp);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetUpdateLog();
		}

		protected CMRReferenceFileUpdateLog GetUpdateLog()
		{
			return GetUpdateLog(Factory);
		}

		CMRReferenceFileUpdateLog GetUpdateLog(BusinessObjectFactory factory)
		{
			return new CMRReferenceFileUpdateLog(factory);
		}

		protected StmALog GetLogRecord(bool success)
		{
			return GetLogRecord(Factory, success);
		}

		StmALog GetLogRecord(BusinessObjectFactory factory, bool success)
		{
			StmALog result = factory.New<StmALog>();
			using (result.LockForUpdatingKeyFieldsForTesting())
			{
				result.SL_Parent = new ZGuid(CMRReferenceFileUpdateLog.LogReferencePK);
				result.SL_Table = CMRReferenceFileUpdateLog.ReferenceTableName;
				result.SL_Reference = CMRReferenceFileUpdateLog.Reference;
				result.SL_SE_NKEvent = success ? Events.UpgradeSucceeded.Code : Events.UpgradeFailed.Code;
			}
			return result;
		}

		#endregion
	}
}
