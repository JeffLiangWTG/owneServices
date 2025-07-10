using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ReadOnlySupportingDocumentCollection))]
	class ReadOnlySupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReadOnlySupportingDocumentCollection>
	{
		protected override ReadOnlySupportingDocumentCollection GetCollectionToTest()
		{
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc(1, "REF111"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc(2, "REF222"));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc(3, "REF333"));
			var result = new ReadOnlySupportingDocumentCollection(entryLine);
			result.LoadNew();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var supDoc = GetSupportingDoc(3, "REF333");
			return new ReadOnlySupportingDocument(supDoc);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
		{
			var supDoc = Factory.New<SupportingDocument>();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = "A";

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing";
			supDoc.CSI_ReferenceNumber2 = "REFNUM2";
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "X";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "QWE";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";
			supDoc.CSI_Procedure = "A";

			return supDoc;
		}
	}
}
