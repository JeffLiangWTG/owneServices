using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.PL;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_AgreedPlaceCode()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
		AssertHasNotifications("Export declaration", invoiceHeader.ZG_AgreedPlaceCodeInfo);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
		AssertNoNotifications("ExitSummaryDeclaration declaration", invoiceHeader.ZG_AgreedPlaceCodeInfo);
	}

	public void TestCheckZG_TransportChargesMethodOfPayment()
	{
		var expectedMessage = "Method of Payment is required for EXS - Exit Summary Declaration.";

		JobDeclaration declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.ZG_TransportChargesMethodOfPayment = ZString.Empty;
		AssertHasMessageError("when ZG_TransportChargesMethodOfPayment empty for ExitSummaryDeclaration declaration", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, expectedMessage);

		invoiceHeader.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.D;
		AssertNoMessageError("when ZG_TransportChargesMethodOfPayment not empty for ExitSummaryDeclaration declaration", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, expectedMessage);
	}
}
