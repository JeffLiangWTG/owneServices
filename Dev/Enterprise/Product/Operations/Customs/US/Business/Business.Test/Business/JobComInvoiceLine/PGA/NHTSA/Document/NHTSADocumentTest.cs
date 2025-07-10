using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NHTSADocument))]
	public class NHTSADocumentTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NHTSADocument>
	{
		public void TestUS_NHTDocumentType_ReadOnly()
		{
			Document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			Assert(Document.US_NHTDocumentTypeInfo.ReadOnly);

			Document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._875;
			Assert(!Document.US_NHTDocumentTypeInfo.ReadOnly);

			Document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			var newdoc = Header.NHTSADocuments.AddNew();
			newdoc.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
			Assert(!Document.US_NHTDocumentTypeInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Document;
		}

		protected override IEnumerable<NHTSADocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			header = invoiceLine.NHTSALines.AddNew();
			yield return header.NHTSADocuments.AddNew();
		}

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}
				return header;
			}
		}
		NHTSAHeader header;

		NHTSADocument Document
		{
			get { return document ?? (document = Header.NHTSADocuments.OfType<NHTSADocument>().FirstOrDefault() ?? Header.NHTSADocuments.AddNew()); }
		}
		NHTSADocument document;
	}
}
