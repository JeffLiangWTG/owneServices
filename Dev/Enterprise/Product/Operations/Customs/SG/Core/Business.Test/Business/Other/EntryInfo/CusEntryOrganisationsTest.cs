using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusEntryOrganisationsTest : TestCaseWithFactory
	{
		public void TestOrganisation()
		{
			OrgHeader testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Singapore Exporter (Pte.) Ltd.";
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Ave";
			testExporter.MainAddress.OA_Address2 = "Central";
			testExporter.MainAddress.OA_City = "Singapore";
			testExporter.MainAddress.OA_PostCode = "59200";
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "S68MC0001B");
			EntryOrganisationsInfo validOrganisation = new EntryOrganisationsInfo(testExporter);
			AssertEquals("Exporter Name", "Singapore Exporter (Pte.) Ltd.", validOrganisation.Name);
			AssertEquals("Exporter Address", "1792 RAFFLES AVE CENTRAL SINGAPORE 59200", validOrganisation.Address.FullAddress);
			AssertEquals("Exporter UEN should be reference returned", "S68MC0001B", validOrganisation.UEN);
		}

		public void TestSubdivisionName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Australian";
			org.MainAddress.OA_Address1 = "1792 Doody Ave";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var orgInfo = new EntryOrganisationsInfo(org);
			AssertEquals("New South Wales", orgInfo.Address.SubdivisionName);
		}

		public void TestCountryCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Australian";
			org.MainAddress.OA_Address1 = "1792 Doody Ave";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_RL_NKRelatedPortCode = "";
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var orgInfo = new EntryOrganisationsInfo(org);
			AssertEquals("CountryCode should not blow up if related country cannot be found", "", orgInfo.Address.CountryCode);
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgInfo = new EntryOrganisationsInfo(org);
			AssertEquals("CountryCode", "AU", orgInfo.Address.CountryCode);
		}

		public void TestNameOverride()
		{
			OrgHeader exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.OH_FullName = "Singapore Exporter (Pte.) Ltd.";
			EntryOrganisationsInfo orgInfo = new EntryOrganisationsInfo(exporter, "Name Override");
			AssertEquals("Name Override", orgInfo.Name);
			orgInfo = new EntryOrganisationsInfo(exporter, "");
			AssertEquals("Singapore Exporter (Pte.) Ltd.", orgInfo.Name);
		}

		public void TestOrganisationLongName()
		{
			var testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Singapore Exporter (Pte.) Ltd. Trading As GST SECTION 33(1) AGENT FOR ECA2";
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Ave";
			testExporter.MainAddress.OA_Address2 = "Central";
			testExporter.MainAddress.OA_City = "Singapore";
			testExporter.MainAddress.OA_PostCode = "59200";
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "S68MC0001B");
			Assert("Pre-condition: Org Name needs to be greater than 50 characters for this test", testExporter.OH_FullName.Length > 50);
			var validOrganisation = new EntryOrganisationsInfo(testExporter);
			AssertEquals("Exporter Name should be returned in full", "Singapore Exporter (Pte.) Ltd. Trading As GST SECTION 33(1) AGENT FOR ECA2", validOrganisation.Name);
		}

		public void TestAddressExcludesAdditionalAddressInfo()
		{
			var testExporter = Factory.NewWithValidTestData<OrgHeader>();
			testExporter.OH_FullName = "Singapore Exporter (Pte.) Ltd.";
			testExporter.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "C/O Gary O'Dea, 3rd Floor, Build C";
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Ave";
			testExporter.MainAddress.OA_Address2 = " ";
			testExporter.MainAddress.OA_City = "Singapore";
			testExporter.MainAddress.OA_PostCode = "59200";
			testExporter.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "S68MC0001B");
			var validOrganisation = new EntryOrganisationsInfo(testExporter);
			AssertEquals("Exporter Name", "Singapore Exporter (Pte.) Ltd.", validOrganisation.Name);
			AssertEquals("Short address can include Additional Address Information if Address does not exceeds capacity", "C/O GARY O'DEA, 3RD FLOOR, BUILD C 1792 RAFFLES AVE SINGAPORE 59200", validOrganisation.Address.FullAddress);
			testExporter.MainAddress.OA_Address1 = "1792 Raffles Boulevarde, Downtown CBD East River";
			testExporter.MainAddress.OA_Address2 = "Ambassadoral Circuit, Special Processing Precinct";
			Assert("Pre-condition: Org Address line 1 & 2 need to be 50 characters for this test", testExporter.MainAddress.OA_Address1.Length > 45);
			Assert("Pre-condition: Org Address line 1 & 2 need to be 50 characters for this test", testExporter.MainAddress.OA_Address2.Length > 45);
			validOrganisation = new EntryOrganisationsInfo(testExporter);
			AssertEquals("Long address should not include Additional Address Information when Address exceeds capacity", "1792 RAFFLES BOULEVARDE, DOWNTOWN CBD EAST RIVER AMBASSADORAL CIRCUIT, SPECIAL PROCESSING PRECINCT SINGAPORE 59200", validOrganisation.Address.FullAddressWithoutAdditionalInfo);
		}
	}
}
