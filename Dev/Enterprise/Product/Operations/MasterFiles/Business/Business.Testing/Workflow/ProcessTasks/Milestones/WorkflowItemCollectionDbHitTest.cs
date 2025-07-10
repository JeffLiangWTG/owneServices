using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowItemCollectionDbHitTest : TestCaseWithFactory
	{
		#region Setup

		DummyWithWorkflow CreateDummy()
		{
			var dummy = Factory.New<DummyWithStupidDbAccess>();
			dummy.InitRelatedDummyWithTasks();
			MakeTasksMilestonesTriggersAndExceptions(dummy);
			MakeTasksMilestonesTriggersAndExceptions(dummy.RelatedDummyWithTasks);
			MakeTasksMilestonesTriggersAndExceptions(dummy.RelatedDummyWithTasks2);

			return dummy;
		}

		void MakeTasksMilestonesTriggersAndExceptions(DummyWithWorkflow dummy)
		{
			for (int i = 0; i < 15; i++)
			{
				var task = dummy.WorkflowItems.Tasks.AddNew();
				var milestone = dummy.WorkflowItems.Milestones.AddNew();
				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				var exception = dummy.WorkflowItems.Exceptions.AddNew();
			}
		}

		#endregion

		public void TestSortMilestones()
		{
			var dummy = CreateDummy();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expected = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ DummyBizoSchema.Constants.TableName, 2 }
			};

			var loadedDummy = newFactory.Load<DummyWithStupidDbAccess>(dummy.PK);
			loadedDummy.WorkflowItems.MilestonesIncludingRelatedSortable.Rebuild();
			AssertDbHits(expected, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		public void TestSortTasks()
		{
			var dummy = CreateDummy();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expected = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ DummyBizoSchema.Constants.TableName, 1 }
			};

			var loadedDummy = newFactory.Load<DummyWithStupidDbAccess>(dummy.PK);
			loadedDummy.WorkflowItems.Tasks.Rebuild();
			AssertDbHits(expected, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		public void TestSortTriggers()
		{
			var dummy = CreateDummy();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expected = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ DummyBizoSchema.Constants.TableName, 2 }
			};

			var loadedDummy = newFactory.Load<DummyWithStupidDbAccess>(dummy.PK);
			loadedDummy.WorkflowItems.TriggersIncludingRelated.Rebuild();
			AssertDbHits(expected, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		class DummyWithStupidDbAccess : DummyWithWorkflow
		{
			public DummyWithStupidDbAccess(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override BusinessObject[] BusinessObjectsWithRelatedEvents
			{
				get
				{
					HitTheDatabase();
					return base.BusinessObjectsWithRelatedEvents;
				}
			}

			void HitTheDatabase()
			{
				Factory.Load<DummyWithStupidDbAccess>(new ZQuery(DummyBizoSchema.PK, PK) { ReLoadExistingRows = true });
			}

			public override DummyWithWorkflow RelatedDummyWithTasks
			{
				get
				{
					if (relatedDummy == null)
					{
						relatedDummy = Factory.Load<DummyWithWorkflow>(Z0_Guid) ?? base.RelatedDummyWithTasks;
						Z0_Guid = relatedDummy.PK;
					}
					return relatedDummy;
				}
			}

			DummyWithWorkflow relatedDummy;
		}
	}
}
