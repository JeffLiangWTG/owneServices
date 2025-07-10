using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceChargeWrapperCollection))]
	sealed class CommercialInvoiceChargeWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommercialInvoiceChargeWrapperCollection>
	{
		public void TestLoadFromInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INVOICE";

			var collection = new CommercialInvoiceChargeWrapperCollection(Factory);
			collection.AddChargesFrom(invoiceHeader.Charges);
			AssertEquals("collection.Count", 0, collection.Count);

			invoiceHeader.Charges.AddNew();
			invoiceHeader.Charges.AddNew();
			invoiceHeader.Charges.AddNew();

			collection = new CommercialInvoiceChargeWrapperCollection(Factory);
			collection.AddChargesFrom(invoiceHeader.Charges);
			AssertEquals("collection.Count", 3, collection.Count);
		}

		#region Imeplementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var charge = Factory.NewWithValidTestData<InvoiceCharge>();
			var invoiceChargeBO = CommercialInvoiceChargeWrapper.New(charge, Factory);
			return invoiceChargeBO;
		}

		protected override CommercialInvoiceChargeWrapperCollection GetCollectionToTest()
		{
			return new CommercialInvoiceChargeWrapperCollection(Factory);
		}

		#endregion
	}
}
