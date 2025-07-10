using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSADocumentCollection))]
	public class NHTSADocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasMultipleDocumentsWithSameType()
		{
			Header.NHTSADocuments.RemoveAndDeleteAll();

			var documentOne = Header.NHTSADocuments.AddNew();
			documentOne.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;

			var documentTwo = Header.NHTSADocuments.AddNew();
			documentTwo.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			Assert(Header.NHTSADocuments.HasMultipleDocumentsWithSameType(NHTSADocumentTypeList.Codes._946));

			documentTwo.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			Assert(!Header.NHTSADocuments.HasMultipleDocumentsWithSameType(NHTSADocumentTypeList.Codes._946));
		}

		public void TestHasDocument()
		{
			var documentOne = Header.NHTSADocuments.AddNew();
			documentOne.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			Assert(Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._946));
			Assert(!Header.NHTSADocuments.HasDocument(NHTSADocumentTypeList.Codes._875));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Header.NHTSADocuments;
		}

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
					Factory.Save();
				}
				return header;
			}
		}
		NHTSAHeader header;

		#endregion
	}
}
