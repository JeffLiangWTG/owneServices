using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_TransportChargesMethodOfPayment()
	{
		var invoiceHeader = Factory.New<JobDeclaration>().Invoices.AddNew();
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, "1", ExportTransportMethodOfPaymentList.Codes.A);
	}

	public void TestZG_ValuationMethod_IsNotMandatory()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();

		invoiceHeader.ZG_ValuationMethod = ZString.Empty;
		ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.ZG_ValuationMethodInfo);
	}
}
