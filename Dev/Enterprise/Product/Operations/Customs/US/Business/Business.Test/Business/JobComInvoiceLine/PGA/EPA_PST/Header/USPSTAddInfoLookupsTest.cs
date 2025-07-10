using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USPSTAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var pesticide = invoiceLine.PSTLines.AddNew();
			var lookups = pesticide.AddInfoLookups;
			Assert(lookups.PSTCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.PSTCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Importer));
			Assert(lookups.PSTCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Shipper));
			Assert(!lookups.PSTCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.Owner));
			Assert(lookups.NotifyPartyList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.NotifyPartyList.ContainsCode(PartyTypeList.Codes.Importer));
			Assert(!lookups.NotifyPartyList.ContainsCode(PartyTypeList.Codes.Shipper));
			Assert(!lookups.NotifyPartyList.ContainsCode(PartyTypeList.Codes.Owner));
		}
	}
}
