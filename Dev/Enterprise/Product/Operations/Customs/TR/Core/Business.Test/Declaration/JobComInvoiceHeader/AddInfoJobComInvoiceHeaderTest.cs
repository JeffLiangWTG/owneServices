using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();

			AssertEquals("ZG_CommercialPaymentCode Default Value", ZString.Empty, invoice.ZG_CommercialPaymentCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			return invoiceHeaderAddInfo;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeaderAddInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}
		JobDeclaration declaration;
		AddInfoJobComInvoiceHeader invoiceHeaderAddInfo;
	}
}
