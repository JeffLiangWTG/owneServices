using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ImportFromAnotherFactoryTest : TestCaseWithFactory
	{
		public void TestSaveImportedLogInTwoFactories_RefreshEnabled()
		{
			SaveImportedLogInTwoFactories(refreshEnabled: true);
		}

		public void TestSaveImportedLogInTwoFactories_RefreshDisabled()
		{
			SaveImportedLogInTwoFactories(refreshEnabled: false);
		}

		static void SaveImportedLogInTwoFactories(bool refreshEnabled)
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = refreshEnabled };
			var f2 = new BusinessObjectFactory { RefreshEnabled = refreshEnabled };
			var pt1 = f1.New<ProcessTask>();
			var l1 = pt1.GetLogs().AddNew(Events.CustomisableEvent00);
			var l2 = f2.ImportFromAnotherFactory(l1);
			f1.Save();
			f2.Save();
			AssertEquals("Don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{l1.PK}'"));
			f1.Save();
			f2.Save();
			AssertEquals("Still don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{l1.PK}'"));
		}

		public void TestSaveImportedLogInTwoFactories_TheOtherFactoryOrder()
		{
			var f1 = new BusinessObjectFactory { };
			var f2 = new BusinessObjectFactory { };
			var pt1 = f1.New<ProcessTask>();
			var l1 = pt1.GetLogs().AddNew(Events.CustomisableEvent00);
			var l2 = f2.ImportFromAnotherFactory(l1);
			f2.Save(); // Saving factories in a different order to the test above.
			f1.Save();
			AssertEquals("Don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{l1.PK}'"));
			f2.Save();
			f1.Save();
			AssertEquals("Still don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{l1.PK}'"));
		}

		public void TestManyImports()
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = true };
			var f2 = new BusinessObjectFactory { RefreshEnabled = true };
			var f3 = new BusinessObjectFactory { RefreshEnabled = true };
			var pt1 = f1.New<ProcessTask>();
			var pt2 = f2.New<ProcessTask>();
			var pt3 = f3.New<ProcessTask>();
			var pt11 = f1.New<ProcessTask>();
			var pt22 = f2.New<ProcessTask>();
			var pt33 = f3.New<ProcessTask>();

			var l1_in_1 = pt1.GetLogs().AddNew(Events.ServiceCancelled);
			var l11_in_1 = pt11.GetLogs().AddNew(Events.ServiceCancelled);

			var l2_in_2 = pt2.GetLogs().AddNew(Events.ServiceCancelled);
			var l22_in_2 = pt22.GetLogs().AddNew(Events.ServiceCancelled);

			var l3_in_3 = pt3.GetLogs().AddNew(Events.ServiceCancelled);
			var l33_in_3 = pt33.GetLogs().AddNew(Events.ServiceCancelled);

			var l3_in_1 = f1.ImportFromAnotherFactory(l3_in_3);
			var l22_in_1 = f1.ImportFromAnotherFactory(l22_in_2);

			var l11_in_2 = f2.ImportFromAnotherFactory(l11_in_1);
			var l33_in_2 = f2.ImportFromAnotherFactory(l33_in_3);

			var l1_in_3 = f3.ImportFromAnotherFactory(l1_in_1);
			var l2_in_3 = f3.ImportFromAnotherFactory(l2_in_2);

			f1.Save();

			// All these guys got imported into the saved factory
			Assert(l1_in_1.IsInDatabase);
			Assert(l11_in_1.IsInDatabase);
			Assert(l11_in_2.IsInDatabase);
			Assert(l1_in_3.IsInDatabase);
			Assert(l22_in_2.IsInDatabase);
			Assert(l3_in_3.IsInDatabase);
			Assert(l3_in_1.IsInDatabase);
			Assert(l22_in_1.IsInDatabase);

			// These are only in factory 2 and 3
			Assert(!l2_in_2.IsInDatabase);
			Assert(!l33_in_3.IsInDatabase);
			Assert(!l33_in_2.IsInDatabase);
			Assert(!l2_in_3.IsInDatabase);
		}

		public void TestConflictResolution()
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = true };
			var f2 = new BusinessObjectFactory { RefreshEnabled = true };
			var pt1 = f1.New<ProcessTask>();
			pt1.P9_Description = "Chocco";
			var pt2 = (ProcessTask)f2.ImportFromAnotherFactory(pt1);
			pt2.P9_Description = "Rokko";
			f1.Save();
			AssertEquals("The reference is clobbered using the same mechanism as datarefreshbus", "Chocco", pt2.P9_Description);
		}

		public void TestConflictResolution_OnlyOnce()
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = true };
			var f2 = new BusinessObjectFactory { RefreshEnabled = true };
			var pt1 = f1.New<ProcessTask>();
			pt1.P9_Description = "Chocco";
			var pt2 = (ProcessTask)f2.ImportFromAnotherFactory(pt1);
			pt2.P9_Description = "Rokko";
			f1.Save();
			AssertEquals("The reference is clobbered using the same mechanism as datarefreshbus", "Chocco", pt2.P9_Description);
			pt2.P9_Description = "Belly";
			f1.Save();
			AssertEquals("The reference is clobbered using the same mechanism as datarefreshbus", "Belly", pt2.P9_Description);
		}

		public void TestCircularImportsAcrossThreeFactories()
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = true };
			var f2 = new BusinessObjectFactory { RefreshEnabled = true };
			var f3 = new BusinessObjectFactory { RefreshEnabled = true };
			var pt1 = f1.New<ProcessTask>();
			var l1 = pt1.GetLogs().AddNew(Events.ServiceCancelled);
			var l2 = f2.ImportFromAnotherFactory(l1);
			var l3 = f3.ImportFromAnotherFactory(l2);
			AssertExceptionThrown<ConstraintException>(() => f1.ImportFromAnotherFactory(l3));
		}

		public void TestLogImportedInTwoFactories()
		{
			var f1 = new BusinessObjectFactory { RefreshEnabled = true };
			var f2 = new BusinessObjectFactory { RefreshEnabled = true };
			var f3 = new BusinessObjectFactory { RefreshEnabled = true };
			var pt1 = f1.New<ProcessTask>();
			var l1 = pt1.GetLogs().AddNew(Events.ServiceCancelled);
			var l2 = f2.ImportFromAnotherFactory(l1);
			var l3 = f3.ImportFromAnotherFactory(l1);
			f1.Save();
			f2.Save();
			f3.Save();
			AssertEquals("Don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{l1.PK}'"));
		}

		public void TestImportIntoFactoryOnAnotherThread_ReportError()
		{
			var f1 = new BusinessObjectFactory();
			var f2 = new BusinessObjectFactory();
			var row = f1.New<DummyWithWorkflow>();

			f1.ThreadSentry.RelinquishThreadOwnership();
			f2.ImportFromAnotherFactory(row);
			AssertNotNull(ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestImportIntoFactoryFromAnotherThread_ReportError()
		{
			var f1 = new BusinessObjectFactory();
			var f2 = new BusinessObjectFactory();
			var row = f1.New<DummyWithWorkflow>();

			f2.ThreadSentry.RelinquishThreadOwnership();
			f2.ImportFromAnotherFactory(row);
			AssertNotNull(ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestImportIntoFactoryThenMoveFactoryToAnotherThread()
		{
			var f1 = new BusinessObjectFactory();
			var f2 = new BusinessObjectFactory();
			var row = f1.New<DummyWithWorkflow>();
			f2.ImportFromAnotherFactory(row);
			f2.ThreadSentry.RelinquishThreadOwnership();
			f2.Save();
			AssertNotNull(ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}
	}
}
/* 
 * Multi threaded tests that we may want in the future, but aren't applicable now, 
 * given that we are not supporting multithreading for ImportFromAnotherFactory
 * when the factories are importing rows not in the db.
 * 

	class MultiThreadedDuplicateStmALogTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestMultiThreadedFactoriesSimple()
		{
			var n = 25;
			var factories = new BusinessObjectFactory[n];
			var logs = new StmALog[n];
			var threads = new Thread[n];
			factories[0] = new BusinessObjectFactory { RefreshEnabled = false };
			var pt0 = factories[0].New<ProcessTask>();
			logs[0] = pt0.GetLogs().AddNew(Events.ServiceCancelled);
			factories[0].RelinquishThreadOwnership();
			for (int i = 1; i < factories.Length; i++)
			{
				factories[i] = new BusinessObjectFactory { RefreshEnabled = false };
				logs[i] = (StmALog)factories[i].ImportFromAnotherFactory(logs[i - 1]);
				factories[i].RelinquishThreadOwnership();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				var local = i;
				threads[i] = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						factories[local].TakeThreadOwnership();
						factories[local].Save();
					}
				});
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Start();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Join();
			}
			AssertEquals("Don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{logs[0].PK}'"));
		}

		[UseSnapshotProtection]
		public void TestMultiThreadedFactoriesWithManyFactories()
		{
			var n = 100;
			var factories = new BusinessObjectFactory[n];
			var logs = new StmALog[n];
			var threads = new Thread[n];
			factories[0] = new BusinessObjectFactory { RefreshEnabled = false };
			var pt0 = factories[0].New<ProcessTask>();
			logs[0] = pt0.GetLogs().AddNew(Events.ServiceCancelled);
			factories[0].RelinquishThreadOwnership();
			for (int i = 1; i < factories.Length; i++)
			{
				factories[i] = new BusinessObjectFactory { RefreshEnabled = false };
				logs[i] = (StmALog)factories[i].ImportFromAnotherFactory(logs[i - 1]);
				factories[i].RelinquishThreadOwnership();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				var local = i;
				threads[i] = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						factories[local].TakeThreadOwnership();
						factories[local].Save();
					}
				});
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Start();
			}
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i].Join();
			}
			AssertEquals("Don't expect a duplicate log.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{logs[0].PK}'"));
		}

		[UseSnapshotProtection]
		public void TestMultiThreadedFactoriesWithManyRowsButOnlyTwoFactories()
		{
			var n = 100;
			var f1 = new BusinessObjectFactory { RefreshEnabled = false };
			var f2 = new BusinessObjectFactory { RefreshEnabled = false };
			var logs = new StmALog[n];
			var importedLogs = new StmALog[n];
			var processTasks = new ProcessTask[n];
			for (int i = 0; i < processTasks.Length; i++)
			{
				processTasks[i] = f1.New<ProcessTask>();
				logs[i] = processTasks[i].GetLogs().AddNew(Events.ServiceCancelled);
				importedLogs[i] = (StmALog)f2.ImportFromAnotherFactory(logs[i]);
			}
			f1.RelinquishThreadOwnership();
			f2.RelinquishThreadOwnership();
			var thread1 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					f1.TakeThreadOwnership();
					f1.Save();
				}
			});
			var thread2 = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					f2.TakeThreadOwnership();
					f2.Save();
				}
			});
			thread1.Start();
			thread2.Start();
			thread1.Join();
			thread2.Join();
			for (int i = 0; i < logs.Length; i++)
			{
				AssertEquals($"Don't expect a duplicate log for log {i}.", 1, Db.Connection.ExecuteScalar<int>($"select count(*) from dbo.StmALog where SL_PK = '{logs[i].PK}'"));
			}
		}
	}
	*/

