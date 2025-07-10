using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentRecipientCollection))]
	public class JobDocumentRecipientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDocumentRecipientCollection>
	{
		public void TestCreateNonPersistentBusinessObject()
		{
			var collection = GetCollectionToTest();
			var newItem = (JobDocumentRecipientWrapperForJobDocumentDelivery)collection.AddNew();
			AssertEquals(dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK, newItem.JobDocumentDelivery.JDC_ParentID);
			AssertEquals(dummyDocumentSupportable.DocumentSupporter.BusinessObject.TablePrefix, newItem.JobDocumentDelivery.JDC_ParentTableCode);
		}

		public void TestCreateNonPersistentBusinessObject_PerformsValidation()
		{
			var collection = GetCollectionToTest();
			var newItem = (JobDocumentRecipientWrapperForJobDocumentDelivery)collection.AddNew();
			Assert("Pre-condition", newItem.EmailToRecipientsAsString.IsEmpty);
			Assert(newItem.EmailToRecipientsAsStringInfo.HasErrors());
		}

		public void TestAddNewExclusion()
		{
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			var collection = GetCollectionToTest();
			var wrapper = collection.AddNewExclusion(orgDocument);
			AssertEquals(dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK, wrapper.Exclusion.JDE_ParentID);
			AssertEquals(dummyDocumentSupportable.DocumentSupporter.BusinessObject.TablePrefix, wrapper.Exclusion.JDE_ParentTableCode);
			AssertEquals(orgDocument.PK, wrapper.Exclusion.JDE_OD_Document);
		}

		public void TestAllowNewAndRemove()
		{
			var collection = new JobDocumentRecipientCollection(Factory, configuration, true);
			Assert(collection.AllowNew);
			Assert(collection.AllowRemove);

			collection = new JobDocumentRecipientCollection(Factory, configuration, false);
			Assert(!collection.AllowNew);
			Assert(!collection.AllowRemove);
		}

		#region Implementation

		protected override JobDocumentRecipientCollection GetCollectionToTest()
		{
			return new JobDocumentRecipientCollection(Factory, configuration, true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			return new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, configuration);
		}

		protected override void SetUp()
		{
			base.SetUp();

			dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
		}

		IDocumentSupportable dummyDocumentSupportable;
		JobDocumentRecipientConfiguration configuration;

		#endregion
	}
}
