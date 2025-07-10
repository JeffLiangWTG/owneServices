using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentExclusionDeleterTest : TestCaseWithFactory
	{
		public void TestJobDocumentExclusionDeleter()
		{
			var newFactory = new BusinessObjectFactory();

			var dummyDocumentSupportable = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocAddress = newFactory.NewWithValidTestData<JobDocAddress>();

			var jobDocumentExclusion1 = newFactory.NewWithValidTestData<JobDocumentExclusion>();
			jobDocumentExclusion1.JDE_ParentID = dummyDocumentSupportable.PK;
			jobDocumentExclusion1.JDE_ParentTableCode = dummyDocumentSupportable.TablePrefix;
			jobDocumentExclusion1.JDE_E2_Address = jobDocAddress.PK;

			var dummy1 = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var dummy2 = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var dummy3 = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var dummy4 = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var dummy5 = newFactory.NewWithValidTestData<DummyDocumentSupportable>();

			var dummyBusinessObjectCollection = new DummyBusinessObjectCollection(newFactory);
			dummyBusinessObjectCollection.Add(dummyDocumentSupportable);
			dummyBusinessObjectCollection.Add(dummy1);
			dummyBusinessObjectCollection.Add(dummy2);
			dummyBusinessObjectCollection.Add(dummy3);
			dummyBusinessObjectCollection.Add(dummy4);
			dummyBusinessObjectCollection.Add(dummy5);

			var jobDocumentExclusion2 = newFactory.NewWithValidTestData<JobDocumentExclusion>();
			jobDocumentExclusion2.JDE_ParentID = dummyDocumentSupportable.PK;
			jobDocumentExclusion2.JDE_ParentTableCode = dummyDocumentSupportable.TablePrefix;
			jobDocumentExclusion2.JDE_E2_Address = jobDocAddress.PK;

			newFactory.Save();

			dummyBusinessObjectCollection.RemoveAndDeleteAll();

			var expectedHitCounts = new Dictionary<string, int> {
				{ StmDocDataOverrideSchema.Constants.TableName, 6 },
				{ StmUniversalCopySchema.Constants.TableName, 3 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ JobShipmentSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ JobDocumentDeliverySchema.Constants.TableName, 1 },
				{ JobDocumentExclusionSchema.Constants.TableName, 1 },
			};

			CombineAssertions(() =>
			{
				AssertDbHits(expectedHitCounts, newFactory);
				AssertEquals(0, newFactory.Load<JobDocumentExclusion>(new ZQuery()).Length);
			});
		}

		public void TestFetchHintQueryDoesNotAddMultipleTimesAndQueryResultInAnInStatement()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var documentSupportable1 = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocumentExclusion1 = Factory.NewWithValidTestData<JobDocumentExclusion>();
			jobDocumentExclusion1.JDE_ParentID = documentSupportable1.PK;
			jobDocumentExclusion1.JDE_ParentTableCode = documentSupportable1.TablePrefix;
			jobDocumentExclusion1.JDE_E2_Address = jobDocAddress.PK;

			var documentSupportable2 = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocumentExclusion2 = Factory.NewWithValidTestData<JobDocumentExclusion>();
			jobDocumentExclusion2.JDE_ParentID = documentSupportable2.PK;
			jobDocumentExclusion2.JDE_ParentTableCode = documentSupportable2.TablePrefix;
			jobDocumentExclusion2.JDE_E2_Address = jobDocAddress.PK;
			var jobDocumentExclusion3 = Factory.NewWithValidTestData<JobDocumentExclusion>();
			jobDocumentExclusion3.JDE_ParentID = documentSupportable1.PK;
			jobDocumentExclusion3.JDE_ParentTableCode = documentSupportable2.TablePrefix;
			jobDocumentExclusion3.JDE_E2_Address = jobDocAddress.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			using (newFactory.EnableTableHitQueryCollection(new[] { JobDocumentExclusionSchema.Constants.TableName }))
			{
				var rowFactory = ((IBusinessObjectFactoryInternals)newFactory).RowFactory;
				var documentSupportable1InAnotherFactory = newFactory.Load<DummyDocumentSupportable>(documentSupportable1.PK);
				var documentSupportable2InAnotherFactory = newFactory.Load<DummyDocumentSupportable>(documentSupportable2.PK);

				for (var i = 0; i < 3; i++)
				{
					documentSupportable1InAnotherFactory.FetchStrategy.FetchForDelete();
					documentSupportable2InAnotherFactory.FetchStrategy.FetchForDelete();
				}
				rowFactory.ExecuteFetchHintsForTable(JobDocumentExclusionSchema.Constants.TableName);
				var tableSelect = newFactory.TableSelects.First(x => x.TableName == JobDocumentExclusionSchema.Constants.TableName);
				AssertEquals(1, tableSelect.Value);
				var queries = tableSelect.Queries.ToArray();
				AssertEquals(1, queries.Length);
				AssertContains("WHERE (JDE_ParentID in (CONVERT(", queries[0].Query);
			}
		}
	}
}
