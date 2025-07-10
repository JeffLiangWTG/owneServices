using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ControllingMessageHeaderLinkInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMessageHeaderLinkInvoiceLine = messageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			AssertType<ControllingMessageHeaderLinkInvoiceLineValidation>(controllingMessageHeaderLinkInvoiceLine.Validation);
		}

		public void TestCheckLink()
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

			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code15;

			var message = ValidationConstants.CusTWControllingMessageHeader.CanNotAssignECFAHeaderToLines;

			var controllingMessageHeaderLinkInvoiceLines = messageHeader1.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>();
			var messageHeader1AndLine1 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line1.PK));
			messageHeader1AndLine1.Link = true;
			AssertNoMessageError(messageHeader1AndLine1.LinkInfo, message);

			var messageHeader1AndLine2 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line2.PK));
			messageHeader1AndLine2.Link = true;
			AssertNoMessageError(messageHeader1AndLine2.LinkInfo, message);

			var messageHeader1AndLine3 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line3.PK));
			messageHeader1AndLine3.Link = true;
			AssertNoMessageError(messageHeader1AndLine3.LinkInfo, message);

			var messageHeader1AndLine4 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line4.PK));
			messageHeader1AndLine4.Link = true;
			AssertNoMessageError(messageHeader1AndLine4.LinkInfo, message);

			var messageHeader1AndLine5 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line5.PK));
			messageHeader1AndLine5.Link = true;
			AssertNoMessageError(messageHeader1AndLine5.LinkInfo, message);

			var messageHeader1AndLine6 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line6.PK));
			messageHeader1AndLine6.Link = true;
			AssertNoMessageError(messageHeader1AndLine6.LinkInfo, message);

			var messageHeader1AndLine7 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line7.PK));
			messageHeader1AndLine7.Link = true;
			AssertNoMessageError(messageHeader1AndLine7.LinkInfo, message);

			var messageHeader1AndLine8 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line8.PK));
			messageHeader1AndLine8.Link = true;
			AssertNoMessageError(messageHeader1AndLine8.LinkInfo, message);

			var messageHeader1AndLine9 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line9.PK));
			messageHeader1AndLine9.Link = true;
			AssertNoMessageError(messageHeader1AndLine9.LinkInfo, message);

			var messageHeader1AndLine10 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line10.PK));
			messageHeader1AndLine10.Link = true;
			AssertNoMessageError(messageHeader1AndLine10.LinkInfo, message);

			var messageHeader1AndLine11 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line11.PK));
			messageHeader1AndLine11.Link = true;
			AssertNoMessageError(messageHeader1AndLine11.LinkInfo, message);

			var messageHeader1AndLine12 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line12.PK));
			messageHeader1AndLine12.Link = true;
			AssertNoMessageError(messageHeader1AndLine12.LinkInfo, message);

			var messageHeader1AndLine13 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line13.PK));
			messageHeader1AndLine13.Link = true;
			AssertNoMessageError(messageHeader1AndLine13.LinkInfo, message);

			var messageHeader1AndLine14 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line14.PK));
			messageHeader1AndLine14.Link = true;
			AssertNoMessageError(messageHeader1AndLine14.LinkInfo, message);

			var messageHeader1AndLine15 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line15.PK));
			messageHeader1AndLine15.Link = true;
			AssertNoMessageError(messageHeader1AndLine15.LinkInfo, message);

			var messageHeader1AndLine16 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line16.PK));
			messageHeader1AndLine16.Link = true;
			AssertNoMessageError(messageHeader1AndLine16.LinkInfo, message);

			var messageHeader1AndLine17 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line17.PK));
			messageHeader1AndLine17.Link = true;
			AssertNoMessageError(messageHeader1AndLine17.LinkInfo, message);

			var messageHeader1AndLine18 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line18.PK));
			messageHeader1AndLine18.Link = true;
			AssertNoMessageError(messageHeader1AndLine18.LinkInfo, message);

			var messageHeader1AndLine19 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line19.PK));
			messageHeader1AndLine19.Link = true;
			AssertNoMessageError(messageHeader1AndLine19.LinkInfo, message);

			var messageHeader1AndLine20 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line20.PK));
			messageHeader1AndLine20.Link = true;
			AssertNoMessageError(messageHeader1AndLine20.LinkInfo, message);

			var messageHeader1AndLine21 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line21.PK));
			messageHeader1AndLine21.Link = true;
			AssertHasMessageError(messageHeader1AndLine21.LinkInfo, message);

			messageHeader1.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			var messageHeader1AndLine22 = controllingMessageHeaderLinkInvoiceLines.FirstOrDefault(x => x.InvoicelinePK.Equals(line22.PK));
			messageHeader1AndLine22.Link = true;
			AssertNoMessageError(messageHeader1AndLine22.LinkInfo, message);
		}
	}
}
