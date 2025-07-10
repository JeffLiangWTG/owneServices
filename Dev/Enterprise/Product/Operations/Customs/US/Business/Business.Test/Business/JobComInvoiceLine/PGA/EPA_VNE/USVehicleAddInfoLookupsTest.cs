using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USVehicleAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			var lookups = vehicle.AddInfoLookups;
			AssertEquals(typeof(EPAVNEDocumentIdentifierList), lookups.FormTypeList.GetType());
			AssertEquals(typeof(IndustryCodesList), lookups.IndustryCodeList.GetType());
			AssertEquals(typeof(IndustryCodesList), lookups.IndustryCodeList.GetType());
			AssertEquals(typeof(USStatesList), lookups.USStatesList.GetType());
			AssertEquals(typeof(EnginePowerUQList), lookups.EnginePowerUQ.GetType());
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertEquals(typeof(ImportCodesForm3520_1List), lookups.ImportCodeList.GetType());

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertEquals(typeof(ImportCodesForm3520_21List), lookups.ImportCodeList.GetType());
			Assert(!lookups.BodyTypeList.ContainsCode(CommodityVehicleQualifierCodesList.Codes.V03));
			Assert(lookups.BodyTypeList.ContainsCode(CommodityVehicleQualifierCodesList.Codes.V00));
			Assert(lookups.VNECertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.VNECertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
			Assert(lookups.VNECertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Owner));
			Assert(!lookups.VNECertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Shipper));
		}
	}
}
