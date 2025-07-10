using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderForMatchingTest : TestCase
	{
		public void TestPKsAreDifferent()
		{
			Dictionary<int, ZGuid> pks = new Dictionary<int, ZGuid>();
			int count = 10;
			for (int i = 0; i < count; i++)
			{
				pks.Add(i, new OrgHeaderForMatching(null).PK);
			}
			foreach (var entry in pks)
			{
				foreach (var entry2 in pks)
				{
					if (entry.Key != entry2.Key && entry.Value == entry2.Value)
					{
						Fail("All should be different");
					}
				}
			}
			Assert(true);
		}

		public void TestOH_IsActive()
		{
			Assert("Default value should be true", new OrgHeaderForMatching(null).OH_IsActive);
		}

		public void TestOH_RL_NKClosestPortInfo()
		{
			AssertNull(new OrgHeaderForMatching(null).OH_RL_NKClosestPortInfo);
		}

		public void TestOH_RL_NKClosestPortInfoHasChanges()
		{
			Assert(new OrgHeaderForMatching(null).OH_RL_NKClosestPortInfoHasChanges);
		}

		public void TestCollectionsNotNull()
		{
			OrgHeaderForMatching org = new OrgHeaderForMatching(null);
			AssertNotNull(org.Addresses);
			AssertNotNull(org.CustomsCodes);
			AssertNotNull(org.Brands);
			AssertNotNull(org.OrganisationNamesExceptBrands);
			AssertNotNull(org.OrganisationNamesFromBrands);
			AssertNotNull(org.OriginalCollections);
		}

		public void TestPatternMatchRequiresFullRegen()
		{
			Assert("Should always be true", new OrgHeaderForMatching(null).PatternMatchRequiresFullRegen);
		}

		public void TestPatternMatchRequiresRegen()
		{
			Assert("Should always be true", new OrgHeaderForMatching(null).PatternMatchRequiresRegen);
		}

		public void TestMainAddress()
		{
			OrgHeaderForMatching org = new OrgHeaderForMatching(null);
			IMatchingAddress adr1 = org.AddNewAddress();
			IMatchingAddress adr2 = org.AddNewAddress();
			IMatchingAddress adr3 = org.AddNewAddress();
			AssertNotNull(null, org.MainAddress);

			org = new OrgHeaderForMatching(null);
			adr1 = org.AddNewAddress();
			adr2 = org.AddNewAddress();
			adr3 = org.AddNewAddress();
			adr2.SetMainAddress();
			AssertEquals(adr2, org.MainAddress);
		}

		public void TestHasAddress()
		{
			OrgHeaderForMatching org = new OrgHeaderForMatching(null);
			IMatchingAddress adr1 = org.AddNewAddress();
			IMatchingAddress adr2 = org.AddNewAddress();
			IMatchingAddress adr3 = new OrgAddressForMatching();
			Assert(org.HasAddress(adr1));
			Assert(org.HasAddress(adr2));
			Assert(!org.HasAddress(adr3));
		}

		public void TestAddNewAddress()
		{
			OrgHeaderForMatching org = new OrgHeaderForMatching(null);
			AssertEquals(0, org.Addresses.Count);
			IMatchingAddress adr1 = org.AddNewAddress();
			AssertEquals(1, org.Addresses.Count);
			IMatchingAddress adr2 = org.AddNewAddress();
			AssertEquals(2, org.Addresses.Count);
		}

		public void TestOrganisationNames()
		{
			var org = new OrgHeaderForMatching(null);
			AssertEquals(1, org.AllOrganisationNames().Count());

			var adr1 = org.AddNewAddress() as OrgAddressForMatching;
			AssertEquals(1, org.AllOrganisationNames().Count());
			adr1.OA_CompanyNameOverride = "111";
			AssertEquals(2, org.AllOrganisationNames().Count());

			var brand = new OrgBrandOrRelatedNameForMatching();
			org.Brands.Add(brand);
			AssertEquals(3, org.AllOrganisationNames().Count());
			brand.P1_RelatedName = "222";
			AssertEquals(3, org.AllOrganisationNames().Count());
		}

		public void TestPatternMatchesForThisOrg()
		{
			OrgHeaderForMatching org = new OrgHeaderForMatching(new BusinessObjectFactory());
			org.OH_Code = "111";
			org.OH_FullName = "name";
			(org.MainAddress as OrgAddressForMatching).OA_Address1 = "address 1";
			AssertEquals(1, org.PatternMatchesForThisOrg.Count);
			org.Delete();
			AssertEquals(0, org.PatternMatchesForThisOrg.Count);
		}
	}
}
