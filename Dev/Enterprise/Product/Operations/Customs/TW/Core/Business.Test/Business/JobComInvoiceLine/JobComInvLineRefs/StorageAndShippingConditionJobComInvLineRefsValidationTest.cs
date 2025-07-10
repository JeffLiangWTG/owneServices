using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(StorageAndShippingConditionJobComInvLineRefsValidation))]
	sealed class StorageAndShippingConditionJobComInvLineRefsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJG_ReferenceNumber_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageAndShippingCondition.JG_ReferenceNumberInfo);
		}

		public void TestCheckJG_ReferenceNumber_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(storageAndShippingCondition.JG_ReferenceNumberInfo, "A", CPT_122_StorageShippingConditionList.Codes._1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclaration = controllingMsgHeaderHelper.New(new string[] { ControllingAgencyList.Codes.CD });
			var header = jobDeclaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line, ControllingAgencyList.Codes.CD, true);
			storageAndShippingCondition = line.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
		}
		StorageAndShippingConditionJobComInvLineRefs storageAndShippingCondition;
	}
}
