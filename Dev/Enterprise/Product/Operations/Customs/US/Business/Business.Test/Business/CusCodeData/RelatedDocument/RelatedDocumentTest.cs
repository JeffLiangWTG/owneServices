using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedDocument))]
	sealed class RelatedDocumentTest : Customs.Business.Testing.CusCodeDataTest<RelatedDocument>
	{
		public void TestValidation()
		{
			RelatedDocument relatedDoc = Factory.New<RelatedDocument>();
			AssertEquals("Validation", typeof(RelatedDocumentValidation), relatedDoc.Validation.GetType());
		}

		public void TestLookups()
		{
			RelatedDocument relatedDoc = Factory.New<RelatedDocument>();
			AssertEquals("Validation", typeof(RelatedDocumentLookups), relatedDoc.Lookups.GetType());
		}

		public void TestDescription()
		{
			RelatedDocument relatedDoc = Factory.New<RelatedDocument>();
			relatedDoc.CY_Code = RelatedDocumentIdentifierList.Codes.AirWaybillNumber;
			AssertEquals(RelatedDocumentIdentifierList.Descriptions.AirWaybillNumber, relatedDoc.Description);
		}

		public void TestSetDefaultValues()
		{
			RelatedDocument relatedDoc = Factory.New<RelatedDocument>();
			AssertEquals(CusCodeDataTypeList.Codes.RelatedDocument, relatedDoc.CY_Type);
		}

		public void TestParent()
		{
			RelatedDocument relatedDoc = Factory.New<RelatedDocument>();
			relatedDoc.CY_ParentID = invoice.PK;
			relatedDoc.CY_ParentTableCode = invoice.TablePrefix;
			AssertEquals(invoice, relatedDoc.Parent);
		}

		protected override BusinessObject GetNewBusinessObject() => invoice.RelatedDocuments.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().Invoices.AddNew().RelatedDocuments.AddNew();

		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
	}
}
