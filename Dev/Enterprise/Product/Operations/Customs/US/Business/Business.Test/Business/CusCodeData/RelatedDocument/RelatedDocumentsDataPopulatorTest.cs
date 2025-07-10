using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RelatedDocumentsDataPopulatorTest : TestCaseWithFactory
	{
		public void TestPopulateRelatedDocuments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "M081232123";
			declaration.JE_HouseBill = "H023223434";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(0, invoice.RelatedDocuments.Count);
			RelatedDocumentsDataPopulator populator = new RelatedDocumentsDataPopulator(invoice);
			populator.PopulateRelatedDocuments();
			AssertEquals(2, invoice.RelatedDocuments.Count);
			Bill masterBill = declaration.PrimaryMasterBill;
			Bill houseBill = declaration.PrimaryHouseBill;
			Bill subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "SB23423234";
			populator.PopulateRelatedDocuments();
			AssertEquals(3, invoice.RelatedDocuments.Count);
			AssertEquals("M081232123", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.AirWaybillNumber).CY_Data);
			AssertEquals("H023223434", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber).CY_Data);
			AssertEquals("SB23423234", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.SubhouseBillOfLading).CY_Data);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoice.RelatedDocuments.RemoveAndDeleteAll();
			populator.PopulateRelatedDocuments();
			AssertEquals(3, invoice.RelatedDocuments.Count);
			AssertEquals("M081232123", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.BillOfLadingNumber).CY_Data);
			AssertEquals("H023223434", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber).CY_Data);
			AssertEquals("SB23423234", invoice.RelatedDocuments.GetFirstElementHaving(RelatedDocumentIdentifierList.Codes.SubhouseBillOfLading).CY_Data);
		}
	}
}
