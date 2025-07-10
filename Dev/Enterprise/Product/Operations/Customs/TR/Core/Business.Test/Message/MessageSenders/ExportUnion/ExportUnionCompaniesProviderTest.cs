using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ExportUnionCompaniesProviderTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "xExporter";
			orgHeader.OH_FullName = "xExporter Full Name";
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890034441");
			var address = orgHeader.MainAddress;
			address.OA_OH = orgHeader.PK;
			address.CompanyName = "xExporter Company Name";
			AssertNoExceptionThrown("All ok", () => new ExportUnionCompaniesProvider(address, DocAddressTypes.Codes.SupplierDocumentaryAddress, ZDateTime.Now));
		});

		public void TestExportUnionCompaniesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);

				var exporter = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "Exporter");
				CombineAssertions("Exporter", () =>
				{
					AssertEquals("CompanyType", "Exporter", exporter.CompanyType);
					AssertEquals("TaxRegistrationCode", "8890024379", exporter.TaxRegistrationCode);
					AssertEquals("TaxOffice", "ANKARA", exporter.TaxOffice);
					AssertEquals("RegiteredName", "xSupplier Full Name", exporter.RegiteredName);
					AssertEquals("Address1", "xSupplierAdress1", exporter.Address1);
					AssertEquals("Address2", "xSupplierAdress2", exporter.Address2);
					AssertEquals("Town", ZString.Empty, exporter.Town);
					AssertEquals("Province", "ISTANBUL", exporter.Province);
					AssertEquals("Zipcode", "340301", exporter.Zipcode);
					AssertEquals("Country", "052", exporter.Country);
					AssertEquals("TelephoneNumber", "+90 212 212 26 91", exporter.TelephoneNumber);
					AssertEquals("FaxNumber", "+90 212 212 26 92", exporter.FaxNumber);
				});

				var importer = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "Importer");
				CombineAssertions("Importer", () =>
				{
					AssertEquals("CompanyType", "Importer", importer.CompanyType);
					AssertEquals("TaxRegistrationCode", "8890024399", importer.TaxRegistrationCode);
					AssertEquals("TaxOffice", "IZMIR", importer.TaxOffice);
					AssertEquals("RegiteredName", "xConsignee Full Name", importer.RegiteredName);
					AssertEquals("Address1", "xConsigneeAdress1", importer.Address1);
					AssertEquals("Address2", "xConsigneeAdress2", importer.Address2);
					AssertEquals("Town", ZString.Empty, importer.Town);
					AssertEquals("Province", "IZMIR", importer.Province);
					AssertEquals("Zipcode", "340302", importer.Zipcode);
					AssertEquals("Country", "052", importer.Country);
					AssertEquals("TelephoneNumber", "+90 212 212 26 93", importer.TelephoneNumber);
					AssertEquals("FaxNumber", "+90 212 212 26 94", importer.FaxNumber);
				});

				var declarationOwner = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "DeclarationOwner");
				CombineAssertions("DeclarationOwner", () =>
				{
					AssertEquals("CompanyType", "DeclarationOwner", declarationOwner.CompanyType);
					AssertEquals("TaxRegistrationCode", "8890024444", declarationOwner.TaxRegistrationCode);
					AssertEquals("TaxOffice", "ADANA", declarationOwner.TaxOffice);
					AssertEquals("RegiteredName", "xDeclarant Full Name", declarationOwner.RegiteredName);
					AssertEquals("Address1", "xDeclarantAdress1", declarationOwner.Address1);
					AssertEquals("Address2", "xDeclarantAdress2", declarationOwner.Address2);
					AssertEquals("Town", ZString.Empty, declarationOwner.Town);
					AssertEquals("Province", "ANKARA", declarationOwner.Province);
					AssertEquals("Zipcode", "340303", declarationOwner.Zipcode);
					AssertEquals("Country", "052", declarationOwner.Country);
					AssertEquals("TelephoneNumber", "+90 212 212 26 95", declarationOwner.TelephoneNumber);
					AssertEquals("FaxNumber", "+90 212 212 26 96", declarationOwner.FaxNumber);
				});

				var financialConsultant = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "FinancialConsultant");
				CombineAssertions("FinancialConsultant", () =>
				{
					AssertEquals("CompanyType", "FinancialConsultant", financialConsultant.CompanyType);
					AssertEquals("TaxRegistrationCode", "8890024555", financialConsultant.TaxRegistrationCode);
					AssertEquals("TaxOffice", "VAN", financialConsultant.TaxOffice);
					AssertEquals("RegiteredName", "xAdvisor Full Name", financialConsultant.RegiteredName);
					AssertEquals("Address1", "xAdvisorAdress1", financialConsultant.Address1);
					AssertEquals("Address2", "xAdvisorAdress2", financialConsultant.Address2);
					AssertEquals("Town", ZString.Empty, financialConsultant.Town);
					AssertEquals("Province", "BERLIN", financialConsultant.Province);
					AssertEquals("Zipcode", "340304", financialConsultant.Zipcode);
					AssertEquals("Country", "004", financialConsultant.Country);
					AssertEquals("TelephoneNumber", "+49 212 2122697", financialConsultant.TelephoneNumber);
					AssertEquals("FaxNumber", "+49 212 2122698", financialConsultant.FaxNumber);
				});

				var responsible = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "Responsible");
				CombineAssertions("Responsible", () =>
				{
					AssertEquals("CompanyType", "Responsible", responsible.CompanyType);
					AssertEquals("TaxRegistrationCode", "8890024444", responsible.TaxRegistrationCode);
					AssertEquals("TaxOffice", "ADANA", responsible.TaxOffice);
					AssertEquals("RegiteredName", "xDeclarant Full Name", responsible.RegiteredName);
					AssertEquals("Address1", "xDeclarantAdress1", responsible.Address1);
					AssertEquals("Address2", "xDeclarantAdress2", responsible.Address2);
					AssertEquals("Town", ZString.Empty, responsible.Town);
					AssertEquals("Province", "ANKARA", responsible.Province);
					AssertEquals("Zipcode", "340303", responsible.Zipcode);
					AssertEquals("Country", "052", responsible.Country);
					AssertEquals("TelephoneNumber", "+90 212 212 26 95", responsible.TelephoneNumber);
					AssertEquals("FaxNumber", "+90 212 212 26 96", responsible.FaxNumber);
				});

				var entryLines = declaration.EntryLines.ToArray()[0];
				var producer = entryLines.ExportUnionItems.Companies.FirstOrDefault(c => c.CompanyType == "Producer");
				CombineAssertions("Producer", () =>
				{
					AssertEquals("CompanyType", "Producer", producer.CompanyType);
					AssertEquals("TaxRegistrationCode", "8899665544", producer.TaxRegistrationCode);
					AssertEquals("TaxOffice", "SAMSUN", producer.TaxOffice);
					AssertEquals("RegiteredName", "xManufacturer Company Name", producer.RegiteredName);
					AssertEquals("Address1", "xAdress1", producer.Address1);
					AssertEquals("Address2", "xAdress2", producer.Address2);
					AssertEquals("Town", ZString.Empty, producer.Town);
					AssertEquals("Province", "IST", producer.Province);
					AssertEquals("Zipcode", "340300", producer.Zipcode);
					AssertEquals("Country", "052", producer.Country);
					AssertEquals("TelephoneNumber", "+90 212 212 26 92", producer.TelephoneNumber);
					AssertEquals("FaxNumber", "+90 212 212 26 92", producer.FaxNumber);
				});

				var orgSupplier = Factory.New<OrgHeader>();
				orgSupplier.OH_Code = "xExporter";
				orgSupplier.OH_FullName = "xExporter Full Name";
				orgSupplier.OH_RL_NKClosestPort = "TR";
				var addressSupplier = orgSupplier.MainAddress;
				addressSupplier.OA_OH = orgSupplier.PK;
				addressSupplier.CompanyName = "xExporter Company Name";
				headerJobDeclaration.JE_OH_Supplier = orgSupplier.PK;

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				exporter = declaration.ExportUnionDeclaration.Companies.FirstOrDefault(c => c.CompanyType == "Exporter");
				CombineAssertions("Exporter with non value", () =>
				{
					AssertEquals("CompanyType", "Exporter", exporter.CompanyType);
					AssertEquals("TaxRegistrationCode", ZString.Empty, exporter.TaxRegistrationCode);
					AssertEquals("TaxOffice", ZString.Empty, exporter.TaxOffice);
					AssertEquals("RegiteredName", "xExporter Full Name", exporter.RegiteredName);
				});
			}
		}
	}
}
