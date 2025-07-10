using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaPack))]
	public class USExportAsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBill()
		{
			var pack = (USExportAsycudaPack)GetNewBusinessObject();
			AssertType<USExportAsycudaBill>(pack.Bill);
		}

		public void TestUNDGs()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("0004a", pack.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
			Factory.Save();
			var packReloaded = new BusinessObjectFactory().Load<USExportAsycudaPack>(pack.PK);
			AssertEquals("0004a", packReloaded.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
		}

		public void TestPSN()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_PSN = "Test PSN";
			pack.UNDGs.FirstItemForBinding[0].DI_DG = undgSubstance.PK;

			AssertEquals("PSN", "Test PSN", pack.PSN);
		}

		public void TestContactNameAndPhone()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test OrgHeader";
			orgHeader.OH_Code = "Test";

			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_OH = orgHeader.PK;
			orgContact.OC_ContactName = "Test Contact Name";
			orgContact.OC_Phone = "Test Contact Phone";

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.UNDGs.FirstItemForBinding[0].DI_OC_DGContact = orgContact.PK;
			AssertEquals("Contack name", orgContact.PK, pack.ContactPK);
			AssertEquals("Contack phone", "Test Contact Phone", pack.ContactPhone);
		}

		public void TestFlashpointTemperatureCAndF()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.FlashpointTemperatureC = 4.4m;
			AssertEquals("Flash Point Temperature C", 4.4m, pack.FlashpointTemperatureC.Round(1));
			AssertEquals("Flash Point Temperature F", 39.9m, pack.FlashpointTemperatureF.Round(1));

			pack.FlashpointTemperatureF = 40m;
			AssertEquals("Flash Point Temperature F", 40m, pack.FlashpointTemperatureF.Round(1));
			AssertEquals("Flash Point Temperature C", 4.4m, pack.FlashpointTemperatureC.Round(1));

			Factory.Save();
			var packReloaded = new BusinessObjectFactory().Load<USExportAsycudaPack>(pack.PK);
			AssertEquals("Flash Point Temperature C", 4.4m, packReloaded.FlashpointTemperatureC.Round(1));
			AssertEquals("Flash Point Temperature F", 39.9m, packReloaded.FlashpointTemperatureF.Round(1));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}
	}
}
