using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSDocument))]
	public class NMFSDocumentTest : Customs.Business.Testing.CusCodeDataTest<NMFSDocument>
	{
		public void TestSetDefaultValues()
		{
			var documentDetail = (NMFSDocument)GetNewBusinessObject();
			AssertEquals("default value", documentDetail.CY_Type, CusCodeDataTypeList.Codes.NMFSDocument);
		}

		public void TestGetsCorrectValidation()
		{
			var documentDetail = (NMFSDocument)GetNewBusinessObject();
			AssertEquals("Correct validation", typeof(NMFSDocumentValidation), documentDetail.Validation.GetType());
		}

		public void TestINMFSDocument()
		{
			var documentDetail = (NMFSDocument)GetNewBusinessObject();
			documentDetail.CY_Code = "ABC";
			documentDetail.CY_Data = "CM123456";
			var nmfsDocument = documentDetail as INMFSDocument;
			AssertEquals("Document Type", "ABC", nmfsDocument.DocumentIdentifier);
			AssertEquals("DIS Document ID", "CM123456", nmfsDocument.DocumentNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NMFSLine.DocumentDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<NMFSDocument>();
		}

		protected override IEnumerable<NMFSDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<NMFSDocument>();

			var declaration = factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.B7_AddInfoData = "1";
			nmfsLine.DocumentDetails.Add(result);

			yield return result;
		}

		NMFSLine NMFSLine
		{
			get { return nmfsLine ?? (nmfsLine = Factory.New<NMFSLine>()); }
		}
		NMFSLine nmfsLine;
	}
}
