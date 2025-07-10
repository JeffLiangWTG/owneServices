using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentDeliveryDeleterTest : TestCaseWithFactory
	{
		public void TestJobDocumentDeliveryDeleter()
		{
			var newFactory = new BusinessObjectFactory();

			var dummyDocumentSupportable = newFactory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocumentDelivery1 = newFactory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery1.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery1.JDC_ParentID = dummyDocumentSupportable.PK;
			jobDocumentDelivery1.JDC_ParentTableCode = dummyDocumentSupportable.TablePrefix;

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

			var jobDocumentDelivery2 = newFactory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery2.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery2.JDC_ParentID = dummyDocumentSupportable.PK;

			newFactory.Save();

			dummyBusinessObjectCollection.RemoveAndDeleteAll();

			var expectedHitCounts = new Dictionary<string, int> {
				{ StmDocDataOverrideSchema.Constants.TableName, 6 },
				{ StmUniversalCopySchema.Constants.TableName, 3 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ JobDocumentDeliverySchema.Constants.TableName, 1 },
				{ JobDocumentDeliveryCopyRecipientSchema.Constants.TableName, 6 },
				{ JobDocumentExclusionSchema.Constants.TableName, 1 },
			};

			CombineAssertions(() =>
			{
				AssertDbHits(expectedHitCounts, newFactory);
				AssertEquals(0, newFactory.Load<JobDocumentDelivery>(new ZQuery()).Length);
			});
		}

		public void TestFetchHintQueryDoesNotAddMultipleTimesAndQueryResultInAnInStatement()
		{
			var documentSupportable1 = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocumentDelivery1 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery1.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery1.JDC_ParentID = documentSupportable1.PK;
			jobDocumentDelivery1.JDC_ParentTableCode = documentSupportable1.TablePrefix;

			var documentSupportable2 = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			var jobDocumentDelivery2 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery2.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery2.JDC_ParentID = documentSupportable2.PK;
			jobDocumentDelivery2.JDC_ParentTableCode = documentSupportable2.TablePrefix;
			var jobDocumentDelivery3 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery3.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery3.JDC_ParentID = documentSupportable1.PK;
			jobDocumentDelivery3.JDC_ParentTableCode = documentSupportable2.TablePrefix;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			using (newFactory.EnableTableHitQueryCollection(new[] { JobDocumentDeliverySchema.Constants.TableName }))
			{
				var rowFactory = ((IBusinessObjectFactoryInternals)newFactory).RowFactory;
				var documentSupportable1InAnotherFactory = newFactory.Load<DummyDocumentSupportable>(documentSupportable1.PK);
				var documentSupportable2InAnotherFactory = newFactory.Load<DummyDocumentSupportable>(documentSupportable2.PK);

				for (var i = 0; i < 3; i++)
				{
					documentSupportable1InAnotherFactory.FetchStrategy.FetchForDelete();
					documentSupportable2InAnotherFactory.FetchStrategy.FetchForDelete();
				}
				rowFactory.ExecuteFetchHintsForTable(JobDocumentDeliverySchema.Constants.TableName);
				var tableSelect = newFactory.TableSelects.First(x => x.TableName == JobDocumentDeliverySchema.Constants.TableName);
				AssertEquals(1, tableSelect.Value);
				var queries = tableSelect.Queries.ToArray();
				AssertEquals(1, queries.Length);
				AssertContains("WHERE (JDC_ParentID in (CONVERT(", queries[0].Query);
			}
		}
	}
}
