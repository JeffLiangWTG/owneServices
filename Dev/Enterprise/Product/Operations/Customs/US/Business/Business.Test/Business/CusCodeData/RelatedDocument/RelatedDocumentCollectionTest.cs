using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedDocumentCollection))]
	sealed class RelatedDocumentCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<RelatedDocument>
	{
		public void TestAddOrUpdate()
		{
			RelatedDocumentCollection collection = Invoice.RelatedDocuments;
			AssertEquals(0, collection.Count);
			collection.AddOrUpdate(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, "M32543545");
			AssertEquals(1, collection.Count);
			AssertEquals(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, collection[0].CY_Code);
			AssertEquals("M32543545", collection[0].CY_Data);
			collection.AddOrUpdate(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, "M9723422");
			AssertEquals(1, collection.Count);
			AssertEquals(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, collection[0].CY_Code);
			AssertEquals("M9723422", collection[0].CY_Data);
			collection.AddOrUpdate(RelatedDocumentIdentifierList.Codes.BillOfLadingNumber, "M9723422");
			AssertEquals(2, collection.Count);
			AssertEquals(RelatedDocumentIdentifierList.Codes.AirWaybillNumber, collection[0].CY_Code);
			AssertEquals("M9723422", collection[0].CY_Data);
			AssertEquals(RelatedDocumentIdentifierList.Codes.BillOfLadingNumber, collection[1].CY_Code);
			AssertEquals("M9723422", collection[1].CY_Data);
		}

		protected override Customs.Business.CusCodeDataCollection<RelatedDocument> GetCusCodeDataCollection() => new RelatedDocumentCollection(Invoice);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<RelatedDocument>();
			result.CY_ParentID = Invoice.PK;
			result.CY_ParentTableCode = Invoice.TablePrefix;
			return result;
		}

		JobComInvoiceHeader invoice;
		JobComInvoiceHeader Invoice => invoice ?? (invoice = Factory.New<JobDeclaration>().Invoices.AddNew());
	}
}
