using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RefCountry = Enterprise.MasterFiles.Business.RefCountry;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(FinancialAccountNumberPortMapCollection))]
	sealed class FinancialAccountNumberPortMapCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FinancialAccountNumberPortMapCollection>
	{
		public void TestGetCreditorFor()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping1 = collection.AddNew();
			mapping1.OrganizationPK = org2.PK;
			mapping1.CustomsOfficeCode = "BIA";
			mapping1.CreditorPK = org1.PK;
			var mapping2 = collection.AddNew();
			mapping2.OrganizationPK = org3.PK;
			mapping2.CustomsOfficeCode = "BBR";
			mapping2.CreditorPK = org2.PK;
			var mapping3 = collection.AddNew();
			mapping3.OrganizationPK = org1.PK;
			mapping3.CustomsOfficeCode = "BBR";
			mapping3.CreditorPK = org3.PK;
			AssertEquals(org3.PK, collection.GetCreditorFor(org1.PK, "BBR"));
			AssertEquals(org2.PK, collection.GetCreditorFor(org3.PK, "BBR"));
			AssertEquals(ZGuid.Empty, collection.GetCreditorFor(org2.PK, "BFN"));
		}

		public void TestGetFinancialAccountNumberFor()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping1 = collection.AddNew();
			mapping1.OrganizationPK = org2.PK;
			mapping1.CustomsOfficeCode = "BIA";
			mapping1.FinancialAccountNumber = "FIN1";
			var mapping2 = collection.AddNew();
			mapping2.OrganizationPK = org3.PK;
			mapping2.CustomsOfficeCode = "BBR";
			mapping2.FinancialAccountNumber = "FIN2";
			var mapping3 = collection.AddNew();
			mapping3.OrganizationPK = org1.PK;
			mapping3.CustomsOfficeCode = "BBR";
			mapping3.FinancialAccountNumber = "FIN3";
			AssertEquals("FIN3", collection.GetFinancialAccountNumberFor(org1.PK, "BBR"));
			AssertEquals("FIN2", collection.GetFinancialAccountNumberFor(org3.PK, "BBR"));
			AssertEquals(ZString.Empty, collection.GetFinancialAccountNumberFor(org2.PK, "BFN"));
		}

		public void TestGetFinancialAccountNumberFor_FromAgentCode()
		{
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var countryZA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ZA");
			var countryCA = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CA");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org1.SetAgentCode(countryAU, "AGT1");
			org2.SetAgentCode(countryZA, "AGT1");
			org3.SetAgentCode(countryZA, "AGT2");
			org4.SetAgentCode(countryZA, "AGT3");
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping1 = collection.AddNew();
			mapping1.OrganizationPK = org2.PK;
			mapping1.CustomsOfficeCode = "BIA";
			mapping1.FinancialAccountNumber = "FIN1";
			mapping1.ImporterPays = true;
			var mapping2 = collection.AddNew();
			mapping2.OrganizationPK = org3.PK;
			mapping2.CustomsOfficeCode = "BBR";
			mapping2.FinancialAccountNumber = "FIN2";
			mapping2.ImporterPays = true;
			var mapping3 = collection.AddNew();
			mapping3.OrganizationPK = org1.PK;
			mapping3.CustomsOfficeCode = "BBR";
			mapping3.FinancialAccountNumber = "FIN3";
			mapping3.ImporterPays = true;
			var mapping4 = collection.AddNew();
			mapping4.OrganizationPK = org4.PK;
			mapping4.CustomsOfficeCode = "BBR";
			mapping4.FinancialAccountNumber = "FIN4";
			mapping4.ImporterPays = true;
			var mapping5 = collection.AddNew();
			mapping5.OrganizationPK = org4.PK;
			mapping5.CustomsOfficeCode = ZString.Empty;
			mapping5.FinancialAccountNumber = "FIN5";
			mapping5.Cash = true;
			mapping5.ImporterPays = true;
			CombineAssertions(() =>
			{
				AssertEquals("Test:AU-XXX-BBR", ZString.Empty, collection.GetMappingFor(countryAU, "XXX", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-XXX-BBR", ZString.Empty, collection.GetMappingFor(countryZA, "XXX", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:CA-XXX-BBR", ZString.Empty, collection.GetMappingFor(countryCA, "XXX", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:AU-XXX-BFN", ZString.Empty, collection.GetMappingFor(countryAU, "XXX", "BFN", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-XXX-BFN", ZString.Empty, collection.GetMappingFor(countryZA, "XXX", "BFN", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:CA-XXX-BFN", ZString.Empty, collection.GetMappingFor(countryCA, "XXX", "BFN", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:AU-AGT1-BBR", "FIN3", collection.GetMappingFor(countryAU, "AGT1", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT1-BBR", ZString.Empty, collection.GetMappingFor(countryZA, "AGT1", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT1-BIA", "FIN1", collection.GetMappingFor(countryZA, "AGT1", "BIA", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:CA-AGT1-BBR", ZString.Empty, collection.GetMappingFor(countryCA, "AGT1", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT2-BBR", "FIN2", collection.GetMappingFor(countryZA, "AGT2", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT2-BBR", "FIN2", collection.GetMappingFor(countryZA, "AGT2", "BBR", PaymentMethodCodeList.Codes.Cash)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT3-BBR-D", "FIN4", collection.GetMappingFor(countryZA, "AGT3", "BBR", PaymentMethodCodeList.Codes.Defer)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT3-BBR-C", "FIN5", collection.GetMappingFor(countryZA, "AGT3", "BBR", PaymentMethodCodeList.Codes.Cash)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT3-BBR-F", "FIN4", collection.GetMappingFor(countryZA, "AGT3", "BBR", PaymentMethodCodeList.Codes.Free)?.FinancialAccountNumber ?? ZString.Empty);
				AssertEquals("Test:ZA-AGT3-???-F", "FIN5", collection.GetMappingFor(countryZA, "AGT3", "???", PaymentMethodCodeList.Codes.Free)?.FinancialAccountNumber ?? ZString.Empty);
			});
		}

		public void TestGetMappingForWithNullOrganization()
		{
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SetAgentCode(countryAU, "AGT1");
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.CustomsOfficeCode = "BIA";
			mapping.FinancialAccountNumber = "FIN1";
			mapping.ImporterPays = true;
			AssertNoExceptionThrown(() => collection.GetMappingFor(countryAU, "XXX", "BBR", PaymentMethodCodeList.Codes.Cash));
			AssertNoExceptionThrown(() => collection.GetMappingFor(countryAU, "XXX", "BBR", "XXX"));
		}

		public void TestGetByFinancialAccountNumber()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping1 = collection.AddNew();
			mapping1.OrganizationPK = org2.PK;
			mapping1.CustomsOfficeCode = "BIA";
			mapping1.FinancialAccountNumber = "0123456789";
			mapping1.CreditorPK = org1.PK;
			var mapping2 = collection.AddNew();
			mapping2.OrganizationPK = org3.PK;
			mapping2.CustomsOfficeCode = "BBR";
			mapping2.FinancialAccountNumber = "0101010101";
			mapping2.CreditorPK = org2.PK;
			var mapping3 = collection.AddNew();
			mapping3.OrganizationPK = org1.PK;
			mapping3.CustomsOfficeCode = "BBR";
			mapping3.FinancialAccountNumber = "2013201356";
			mapping3.CreditorPK = org3.PK;
			AssertEquals(mapping1, collection.GetByFinancialAccountNumber("0123456789"));
			AssertEquals(mapping2, collection.GetByFinancialAccountNumber("0101010101"));
			AssertEquals(mapping3, collection.GetByFinancialAccountNumber("2013201356"));
			AssertEquals(null, collection.GetByFinancialAccountNumber("6541236580"));
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override FinancialAccountNumberPortMapCollection GetCollectionToTest()
		{
			return new FinancialAccountNumberPortMapCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FinancialAccountNumberPortMap();
		}
	}
}
