using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(StorageAndShippingConditionJobComInvLineRefs))]
	sealed class StorageAndShippingConditionJobComInvLineRefsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJG_ReferenceNumber_MaxLength()
		{
			AssertEquals(1, storageAndShippingCondition.JG_ReferenceNumberInfo.MaxLength);
		}

		[ExpectNoExceptions]
		public void TestJG_ReferenceNumber_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(storageAndShippingCondition.JG_ReferenceNumberInfo, "Code", "The code of storage and shipping conditions.");
		}

		public void TestDescription()
		{
			storageAndShippingCondition.JG_ReferenceNumber = CPT_122_StorageShippingConditionList.Codes._1;
			AssertEquals(CPT_122_StorageShippingConditionList.Descriptions._1, storageAndShippingCondition.Description);
		}

		public void TestDescription_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(storageAndShippingCondition.DescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Description", resourceStringData.Caption);
				AssertEquals("Short Caption", "Desc.", resourceStringData.ShortCaption);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(JobComInvLineRefsType.Codes.StorageAndShippingCondition, storageAndShippingCondition.JG_ReferenceType);
		}

		public void TestOnSaving_DeleteOnEmptyJG_ReferenceNumber()
		{
			var storageAndShippingCondition2 = storageAndShippingCondition.InvoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition2.JG_ReferenceNumber = "2";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Empty reference is Deleted", true, storageAndShippingCondition.IsDeleted);
				AssertEquals("With reference stays", false, storageAndShippingCondition2.IsDeleted);
			});
		}

		public void TestValidation()
		{
			AssertType<StorageAndShippingConditionJobComInvLineRefsValidation>(storageAndShippingCondition.Validation);
		}

		public void TestLookups()
		{
			AssertType<StorageAndShippingConditionJobComInvLineRefsLookups>(storageAndShippingCondition.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => storageAndShippingCondition;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var invoiceHeader = factory.New<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var result = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			result.JG_ReferenceNumber = "1";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			storageAndShippingCondition = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
		}
		StorageAndShippingConditionJobComInvLineRefs storageAndShippingCondition;
	}
}
