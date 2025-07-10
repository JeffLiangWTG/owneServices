using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Invoicing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInvoiceDocManagerInfo))]
	class WhsInvoiceDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var postedInvoice = Factory.New<ARInvoice>();
			postedInvoice.AH_ConsolidatedInvoiceRef = "I00001001";
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory);
			var whsInvoice = Factory.New<WhsInvoice>();
			whsInvoice.ET_StorageJobNumber = "I00001001";

			var receiveDocManagerInfo = new WhsInvoiceDocManagerInfo(whsInvoice);
			AssertEquals("One Transaction should be found", 1, receiveDocManagerInfo.RelatedObjects.Length);
			AssertEquals("I00001001", ((AccTransactionHeader)receiveDocManagerInfo.RelatedObjects[0]).AH_ConsolidatedInvoiceRef);
		}

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsInvoice>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var whsInvoice = Factory.New<WhsInvoice>();
			whsInvoice.ET_StorageJobNumber = "TestInvoiceNumber";

			return whsInvoice;
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var postedInvoice = Factory.New<ARInvoice>();
			postedInvoice.AH_ConsolidatedInvoiceRef = "I00001001";
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory);
			var whsInvoice = Factory.New<WhsInvoice>();
			whsInvoice.ET_StorageJobNumber = "I00001001";

			var invoiceDocManagerInfo = new WhsInvoiceDocManagerInfo(whsInvoice);
			AssertNotNull(invoiceDocManagerInfo);
			var invoiceDocManagerInfoSupportReadonly = invoiceDocManagerInfo as ISupportReadOnlyOverride;
			AssertNotNull(invoiceDocManagerInfoSupportReadonly);

			invoiceDocManagerInfoSupportReadonly.SetReadOnly(true);
			AssertEquals("Should be equal", true, invoiceDocManagerInfo.ReadOnly);

			invoiceDocManagerInfoSupportReadonly.SetReadOnly(false);
			AssertEquals("Should be equal", false, invoiceDocManagerInfo.ReadOnly);
		}

#endregion
	}
}
