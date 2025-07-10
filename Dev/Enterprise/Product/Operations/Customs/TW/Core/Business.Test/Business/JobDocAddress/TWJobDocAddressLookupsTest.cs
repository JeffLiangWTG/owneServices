using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWJobDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIDCodeTypeList()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var controllingMessageHeaders = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var jobDocAddress = declaration.JobDocAddress;
			var jobDocAddressLookups = jobDocAddress.Lookups;
			var importerDocumentaryAddressLookups = declaration.ImporterDocumentaryAddress.Lookups;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress.Lookups;
			var localProcessorAddressLookups = controllingMessageHeaders.LocalProcessorAddress.Lookups;
			var manuFacturerAddressLookups = declaration.InvoiceLines.AddNew().ManufacturerDocAddress.Lookups;

			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var cmApplicant = controllingMessageHeader.ApplicantDocumentaryAddress;
			var cmApplicantLookup = cmApplicant.Lookups;
			var cmSupplier = controllingMessageHeader.SupplierDocumentaryAddress;
			var cmSupplierLookup = cmSupplier.Lookups;
			var cmImporter = controllingMessageHeader.ImporterDocumentaryAddress;
			var cmImporterLookup = cmImporter.Lookups;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CombineAssertions("Import", () =>
			{
				AssertEquals("jobDocAddressLookups", "VAT, PAS, PID, ZZZ", jobDocAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("supplierDocumentaryAddress", "VAT, PAS, PID, ZZZ", supplierDocumentaryAddress.IDCodeTypeList.CodesAsString);
				AssertEquals("importerDocumentaryAddressLookups", "VAT, PAS, PID", importerDocumentaryAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("localProcessorAddressLookups", "FRI, VAT, PID, PAS, ZZZ", localProcessorAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("manuFacturerAddressLookups", "VAT, PAS, PID", manuFacturerAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("Applicant Import IDCodeTypeList", "VAT, PAS, PID", cmApplicantLookup.IDCodeTypeList.CodesAsString);
				AssertEquals("Supplier Import IDCodeTypeList", "VAT, PAS, PID", cmSupplierLookup.IDCodeTypeList.CodesAsString);
				AssertEquals("Importer Import IDCodeTypeList", "VAT, PID, PAS", cmImporterLookup.IDCodeTypeList.CodesAsString);
			});

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			CombineAssertions("Export", () =>
			{
				AssertEquals("jobDocAddressLookups", "VAT, PAS, PID, ZZZ", jobDocAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("supplierDocumentaryAddress", "VAT, PAS, PID", supplierDocumentaryAddress.IDCodeTypeList.CodesAsString);
				AssertEquals("importerDocumentaryAddressLookups", "VAT, PAS, PID", importerDocumentaryAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("localProcessorAddressLookups", "FRI, VAT, PID, PAS, ZZZ", localProcessorAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("manuFacturerAddressLookups", "VAT, PAS, PID", manuFacturerAddressLookups.IDCodeTypeList.CodesAsString);
				AssertEquals("Applicant Import IDCodeTypeList", "VAT, PAS, PID", cmApplicantLookup.IDCodeTypeList.CodesAsString);
				AssertEquals("Supplier Import IDCodeTypeList", "VAT, PAS, PID", cmSupplierLookup.IDCodeTypeList.CodesAsString);
				AssertEquals("Importer Import IDCodeTypeList", "VAT, PID, PAS", cmImporterLookup.IDCodeTypeList.CodesAsString);
			});
		}

		public void TestAEOCodeTypeList()
		{
			var jobDocAddressLookups = Factory.New<TWJobDocAddress>().Lookups;
			AssertEquals("AEOCodeTypeList", "AEO", jobDocAddressLookups.AEOCodeTypeList.CodesAsString);
		}

		public void TestTPCCodeTypeList()
		{
			var jobDocAddressLookups = Factory.New<TWJobDocAddress>().Lookups;
			AssertEquals("TPCCodeTypeList", "TPC", jobDocAddressLookups.TPCCodeTypeList.CodesAsString);
		}

		public void TestCBPCodeTypeList()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var importerDocumentaryAddressLookups = declaration.ImporterDocumentaryAddress.Lookups;
			AssertEquals("CBPCodeTypeList", "EPZ, CBF, FTZ, ATP, SPK", importerDocumentaryAddressLookups.CBPCodeTypeList.CodesAsString);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.CusEntryInstruction.CEI_Style = "L1";
			AssertEquals("CBPCodeTypeList", "CCP, EPZ, CBF, FTZ, ATP, SPK", importerDocumentaryAddressLookups.CBPCodeTypeList.CodesAsString);
		}

		public void TestGovRegNumTypes()
		{
			var jobDocAddressLookups = Factory.New<TWJobDocAddress>().Lookups;
			AssertEquals("GovRegNumTypes", "VAT, PAS, PID, ZZZ, AEO, EPZ, CBF, FTZ, ATP, SPK, TPC", jobDocAddressLookups.GovRegNumTypes.CodesAsString);
		}

		public void TestFRICodeTypeList()
		{
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			var controllingMessageHeaders = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var jobDocAddress = declaration.JobDocAddress;
			var jobDocAddressLookups = jobDocAddress.Lookups;
			var manuFacturerAddressLookups = declaration.InvoiceLines.AddNew().ManufacturerDocAddress.Lookups;

			CombineAssertions(() =>
			{
				AssertEquals("jobDocAddressLookups", "FRI", jobDocAddressLookups.FRICodeTypeList.CodesAsString);
				AssertEquals("manuFacturerAddressLookups", "FRI, ZZZ", manuFacturerAddressLookups.FRICodeTypeList.CodesAsString);
			});
		}
	}
}
