using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineRelatedControllingMsgHeadersGenPivot))]
	sealed class InvoiceLineRelatedControllingMsgHeadersGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRefreshControllingMessageHeaderLinkInvoiceLineLinkIfNeeded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			var invoiceLineLinkControllingMsgHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First();
			invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = true;
			var controllingMessageHeaderLinkInvoiceLine = controllingMessageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			AssertEquals(true, controllingMessageHeaderLinkInvoiceLine.Link);

			invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader = false;
			controllingMessageHeaderLinkInvoiceLine = controllingMessageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			AssertEquals(false, controllingMessageHeaderLinkInvoiceLine.Link);
		}

		public void TestRefreshInvoiceLineLinkControllingMsgHeaderLinkIfNeeded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			var testCollection1 = messageHeader1.ControllingMessageHeaderLinkInvoiceLines;
			var testCollection2 = messageHeader2.ControllingMessageHeaderLinkInvoiceLines;
			var line2ToControllingMessageHeaderLinkInvoiceLine = testCollection2.Cast<ControllingMessageHeaderLinkInvoiceLine>().First(x => x.InvoicelinePK == line2.PK);
			var invoiceLineLinkControllingMsgHeader = line2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingMessageHeaderPK == messageHeader2.PK);
			Assert("Should be false", !invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader);

			line2ToControllingMessageHeaderLinkInvoiceLine.Link = true;
			invoiceLineLinkControllingMsgHeader = line2.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingMessageHeaderPK == messageHeader2.PK);
			Assert("Should be true", invoiceLineLinkControllingMsgHeader.IsLinkedCMHeader);
		}

		public void TestDefaultValues()
		{
			AssertEquals(GenPivotTypeDecider.Types.InvoiceLineRelatedControllingMessageHeaderPivot, InvoiceLineRelatedControllingMsgHeadersGenPivot.XX_RelationType);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, InvoiceLineRelatedControllingMsgHeadersGenPivot.XX_Relation1TableCode);
			AssertEquals(CusTWControllingMessageHeaderSchema.Constants.Prefix, InvoiceLineRelatedControllingMsgHeadersGenPivot.XX_Relation2TableCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceLineRelatedControllingMsgHeadersGenPivot;
		}

		InvoiceLineRelatedControllingMsgHeadersGenPivot InvoiceLineRelatedControllingMsgHeadersGenPivot
		{
			get
			{
				return fInvoiceLineRelatedControllingMsgHeadersGenPivot ?? (fInvoiceLineRelatedControllingMsgHeadersGenPivot = Factory.New<InvoiceLineRelatedControllingMsgHeadersGenPivot>());
			}
		}

		InvoiceLineRelatedControllingMsgHeadersGenPivot fInvoiceLineRelatedControllingMsgHeadersGenPivot;
	}
}
