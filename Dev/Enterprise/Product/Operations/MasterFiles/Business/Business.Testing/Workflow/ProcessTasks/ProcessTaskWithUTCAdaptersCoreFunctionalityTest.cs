using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskWithUTCAdaptersCoreFunctionalityTest : TestCaseWithFactory
	{
		[TestUtcOffset(10, 0, 0)]
		public void TestScheduledDateMinValueValid()
		{
			ProcessTaskForTest task = GetNewBusinessObject();

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ZDateTime date = new ZDateTime(1800, 1, 1, 0, 0, 0);
			task.P9_ScheduledDate = date;

			AssertExceptionThrown<ArgumentOutOfRangeException>(Factory.Save);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestUtcOffset(-10, 0, 0)]
		public void TestScheduledDateMaxValueValid()
		{
			ProcessTaskForTest task = GetNewBusinessObject();

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ZDateTime date = new ZDateTime(2100, 1, 1, 0, 0, 0);
			task.P9_ScheduledDate = date;

			AssertExceptionThrown<ArgumentOutOfRangeException>(Factory.Save);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestUtcOffset(-10, 0, 0)]
		public void TestScheduledDateUtcMinValueValid()
		{
			ProcessTaskForTest task = GetNewBusinessObject();

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ZDateTime date = new ZDateTime(1800, 1, 1, 0, 0, 0);
			task.P9_ScheduledDateUtc = date;

			AssertExceptionThrown<ArgumentOutOfRangeException>(Factory.Save);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestScheduledDateUtcMaxValueValid()
		{
			ProcessTaskForTest task = GetNewBusinessObject();

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ZDateTime date = new ZDateTime(2100, 1, 1, 0, 0, 0);
			task.P9_ScheduledDateUtc = date;

			AssertExceptionThrown<ArgumentOutOfRangeException>(Factory.Save);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestConcurrencyPolicies()
		{
			var processTask = GetNewBusinessObject();

			AssertEquals("P9_ActualDate is set by the batch processor very often, conflicting value should overwrite",
				ConcurrencyPolicy.Ignore,
				processTask.P9_ActualDateInfo.ConcurrencyPolicy);

			AssertEquals("P9_ActualDateUtc is set by the batch processor very often, conflicting value should overwrite",
				ConcurrencyPolicy.Ignore,
				processTask.P9_ActualDateUtcInfo.ConcurrencyPolicy);

			AssertEquals("P9_OriginalScheduledDateUtc is set by the batch processor very often, conflicting value should overwrite",
				ConcurrencyPolicy.Ignore,
				processTask.P9_OriginalScheduledDateUtcInfo.ConcurrencyPolicy);

			AssertEquals("P9_MilestoneExceptionAdded is set by the batch processor very often, conflicting value should overwrite",
				ConcurrencyPolicy.Ignore,
				processTask.P9_MilestoneExceptionAddedInfo.ConcurrencyPolicy);

			AssertEquals("P9_ExceptionAddedUtc is set by the batch processor very often, conflicting value should overwrite",
				ConcurrencyPolicy.Ignore,
				processTask.P9_ExceptionAddedUtcInfo.ConcurrencyPolicy);
		}

		public void TestFetchHintAttribute()
		{
			var pkList = new List<ZGuid>();
			for (int i = 0; i < 5; i++)
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_JobNum = i.ToString();
				var processTask = Factory.New<ProcessTaskForTest>();
				processTask.P9_ParentID = jobHeader.PK;
				processTask.P9_ParentTableCode = "JH";
				pkList.Add(processTask.PK);
			}
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(0, factory2.DatabaseLoadCount);
			var tasks = factory2.Load<ProcessTaskForTest>(new ZQuery(ProcessTasksSchema.PK, pkList));
			AssertEquals(1, factory2.DatabaseLoadCount);
			foreach (ProcessTaskForTest task in tasks)
			{
				// This load should use in memory data (or run the fetch hint) so no extra db hit.
				factory2.Load<JobHeader>(task.P9_ParentID);
			}
			AssertEquals(2, factory2.DatabaseLoadCount);
		}

		public void TestHtmlProperty()
		{
			ProcessTaskForTest task = GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, task.P9_Notes);
			AssertEquals(ZBlob.Empty, task.P9_Notes_HTML);
			task.P9_Notes_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(task.P9_Notes.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", task.P9_Notes_HTML.ToUTF8());
		}

		ProcessTaskForTest GetNewBusinessObject()
		{
			return Factory.New<ProcessTaskForTest>();
		}
	}
}
