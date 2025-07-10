using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAddressForMatchingTest : TestCase
	{
		public void TestPKsAreDifferent()
		{
			Dictionary<int, ZGuid> pks = new Dictionary<int, ZGuid>();
			int count = 10;
			for (int i = 0; i < count; i++)
			{
				pks.Add(i, new OrgAddressForMatching().PK);
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

		public void TestHasChanges()
		{
			Assert("should be true", new OrgAddressForMatching().HasChanges);
		}

		public void TestOA_RL_NKRelatedPortCodeInfoHasChanges()
		{
			Assert("should be true", new OrgAddressForMatching().OA_RL_NKRelatedPortCodeInfoHasChanges);
		}

		public void TestOA_IsActive()
		{
			Assert("should be true", new OrgAddressForMatching().OA_IsActive);
		}

		public void TestIsMainAddress()
		{
			Assert("default value should be false", !new OrgAddressForMatching().IsMainAddress);
		}

		public void TestSetMainAddress()
		{
			var adr = new OrgAddressForMatching();
			Assert(!adr.IsMainAddress);
			adr.SetMainAddress();
			Assert(adr.IsMainAddress);
		}

		public void TestPortAndCountryNames()
		{
			var adr = new OrgAddressForMatching();
			AssertEquals("", adr.PortName);
			AssertEquals("", adr.CountryName);

			var org = new OrgHeaderForMatching(null);
			(org.MainAddress as OrgAddressForMatching).PortName = "123";
			(org.MainAddress as OrgAddressForMatching).CountryName = (NoResString)"456";

			adr.HeaderForMatching = org;
			AssertEquals("123", adr.PortName);
			AssertEquals("456", adr.CountryName);

			adr.PortName = "qqq";
			adr.CountryName = (NoResString)"www";
			AssertEquals("qqq", adr.PortName);
			AssertEquals("www", adr.CountryName);
		}

		public void TestPortAndCountryNames_NoStackOverflow()
		{
			var org = new OrgHeaderForMatching(null);
			AssertEquals("", org.MainAddress.PortName);
			AssertEquals("", org.MainAddress.CountryName);
		}
	}
}
