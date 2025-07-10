using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCountryDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestAviationSecuritySchemeMembershipList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var org = Factory.New<OrgHeader>();
				AssertEquals("Only 2 Codes in the aviation security list", 2, org.CountryData.Lookups.AviationSecuritySchemeMembershipList.Count);
				Assert("List contains YES", org.CountryData.Lookups.AviationSecuritySchemeMembershipList.IndexOfCode("YES") >= 0);
				Assert("List contains NO", org.CountryData.Lookups.AviationSecuritySchemeMembershipList.IndexOfCode("NO") >= 0);
			}
		}

		public void TestBondedWarehouseList()
		{
			OrgHeader bondedOrganisation = Factory.New<OrgHeader>();
			bondedOrganisation.OH_IsWarehouseClient = true;
			bondedOrganisation.OH_Code = "BondedOrg";
			bondedOrganisation.OH_FullName = "Bonded Warehouse Organisation";
			Factory.Save();

			OrgHeader organisation = Factory.New<OrgHeader>();
			var bondedWarehouses = organisation.CountryData.Lookups.BondedWarehouseList;
			bondedWarehouses.Load();
			AssertEquals(true, bondedWarehouses.Contains(bondedOrganisation.PK));
		}
	}
}
