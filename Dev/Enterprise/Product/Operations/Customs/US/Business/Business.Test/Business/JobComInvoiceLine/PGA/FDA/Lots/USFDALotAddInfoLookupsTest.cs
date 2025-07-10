using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USFDALotAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var lot = fda.Lots.AddNew();
			AssertEquals(typeof(TemperatureQualifierList), lot.AddInfoLookups.TemperatureQualifierList.GetType());
			AssertEquals(typeof(DegreeTypeList), lot.AddInfoLookups.DegreeTypeList.GetType());
			AssertEquals(typeof(PGAStorageTypeList), lot.AddInfoLookups.LocationOfTempList.GetType());
		}
	}
}
