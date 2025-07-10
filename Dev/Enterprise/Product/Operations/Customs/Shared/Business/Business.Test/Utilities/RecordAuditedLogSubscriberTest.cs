using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(RecordAuditedLogSubscriber))]
	class RecordAuditedLogSubscriberTest : LogSubscriberTest<RecordAuditedLogSubscriber>
	{
		[TestDate(2022, 5, 1)]
		public void TestPorcessLogs()
		{
			var now = ZDateTime.Now;
			var utcNowInCurrentBranch = StmALog.ToDateTimeOffset(Factory, now, GlbBranch.CurrentBranch.GB_Code).ToUtcZDateTime();

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			var newFactory = NewFactory();
			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix, SJ_ParentID = declaration1.PK, SJ_Reference = "ref1", SJ_SE_NKEvent = AutoEvents.RecordAuditedCode, SJ_EventTime = ZDateTime.BrettsBirthday },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix, SJ_ParentID = declaration1.PK, SJ_Reference = "ref2", SJ_SE_NKEvent = AutoEvents.RecordAuditedCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix, SJ_ParentID = declaration2.PK, SJ_Reference = "ref3", SJ_SE_NKEvent = AutoEvents.RecordAuditedCode, SJ_EventTime = now },
			};

			using (newFactory.AddDisposableService())
			{
				var subscriber = new RecordAuditedLogSubscriberForTest();
				subscriber.ProcessLogs(logs);
				newFactory.Save();
			}

			declaration1.Reload();
			declaration2.Reload();
			CombineAssertions(() =>
			{
				AssertEquals(utcNowInCurrentBranch, declaration1.AuditDateUtc);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, declaration1.AuditLogUser);
				AssertEquals("ref2", declaration1.AuditReference);
				AssertEquals(utcNowInCurrentBranch, declaration2.AuditDateUtc);
				AssertEquals("ref3", declaration2.AuditReference);
			});
		}

		#region Implement

		[Serializable]
		sealed class RecordAuditedLogSubscriberForTest : RecordAuditedLogSubscriber
		{
			public void ProcessLogs(IQueuedLog[] queuedLogs)
			{
				var allPrefix = TableNames.Select(c => EnterpriseSchema.GetTableSchema(c)?.PK.ColumnPrefix ?? string.Empty);
				var filterLogs = queuedLogs.Where(c => allPrefix.Any(d => d == c.SJ_ParentTableCode) && EventTypes.Any(d => d == c.SJ_SE_NKEvent)).ToArray();

				base.ProcessLogQueueItems(filterLogs);
			}
		}

		#endregion
	}
}
