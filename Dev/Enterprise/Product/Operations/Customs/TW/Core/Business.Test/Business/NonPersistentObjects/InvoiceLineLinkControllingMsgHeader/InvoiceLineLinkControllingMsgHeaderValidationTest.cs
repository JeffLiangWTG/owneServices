using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class InvoiceLineLinkControllingMsgHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			var invoiceLineLinkControllingMsgHeader = new InvoiceLineLinkControllingMsgHeader(controllingMessageHeader, invoiceLine);
			AssertType<InvoiceLineLinkControllingMsgHeaderValidation>(invoiceLineLinkControllingMsgHeader.Validation);
		}

		public void TestCheckIsLinkedCMHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line4 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line5 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line6 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line7 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line8 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line9 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line10 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line11 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line12 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line13 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line14 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line15 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line16 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line17 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line18 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line19 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line20 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line21 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line22 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();

			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;

			var message = ValidationConstants.CusTWControllingMessageHeader.CanNotAssignECFAHeaderToLines;
			line1.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line1, messageHeader.PK), message);

			line2.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line2, messageHeader.PK), message);

			line3.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line3, messageHeader.PK), message);

			line4.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line4, messageHeader.PK), message);

			line5.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line5, messageHeader.PK), message);

			line6.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line6, messageHeader.PK), message);

			line7.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line7, messageHeader.PK), message);

			line8.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line8, messageHeader.PK), message);

			line9.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line9, messageHeader.PK), message);

			line10.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line10, messageHeader.PK), message);

			line11.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line11, messageHeader.PK), message);

			line12.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line12, messageHeader.PK), message);

			line13.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line13, messageHeader.PK), message);

			line14.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line14, messageHeader.PK), message);

			line15.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line15, messageHeader.PK), message);

			line16.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line16, messageHeader.PK), message);

			line17.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line17, messageHeader.PK), message);

			line18.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line18, messageHeader.PK), message);

			line19.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line19, messageHeader.PK), message);

			line20.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line20, messageHeader.PK), message);

			line21.AssignCMHeaderToInvoices(messageHeader);
			AssertHasMessageError(GetIsLinkedCMHeaderInfo(line21, messageHeader.PK), message);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			line22.AssignCMHeaderToInvoices(messageHeader);
			AssertNoMessageError(GetIsLinkedCMHeaderInfo(line22, messageHeader.PK), message);
		}

		ZPropertyInfo GetIsLinkedCMHeaderInfo(JobComInvoiceLine line, ZGuid pk)
		{
			return line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault(x => x.ControllingMessageHeaderPK == pk).IsLinkedCMHeaderInfo;
		}
	}
}
