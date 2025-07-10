using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TransportProvidersTest : TestCaseWithFactory
	{
		public void TestGroupUnspecific()
		{
			var factory2 = new BusinessObjectFactory();
			var org1 = OrgWithSource.New(factory2.NewWithValidTestData<OrgHeader>(), new List<string>() { "Provider1" });
			factory2.Save();

			var org2 = OrgWithSource.New(Factory.Load<OrgHeader>(org1.Org.PK), new List<string>() { "Provider2" });

			Assert("precondition", !org1.Equals(org2));
			Assert("precondition", org1.Org.PK.Equals(org2.Org.PK));

			OrgWithSource org3 = null;
			var org4 = GetNewOrgWithSourceWithValidTestData();

			var list = new List<OrgWithSource> { org4, org1, org2, org3, org4 };

			var providers = Creditors.New(list);
			AssertEquals(2, providers.AllOrgs.Count);

			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org1));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("XXX", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("YYY", org3));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("ZZZ", org4));

			providers = Creditors.New(org3, org4, org2, org1, org4);
			AssertEquals(2, providers.AllOrgs.Count);

			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org1));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("", org3));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org4));

			bool exceptionCaught = false;

			try
			{
				providers["FRT"] = new OrgPrioritizedList(OrgWithSource.New(org1, new List<string>() { "Provider" }));
			}
			catch (ArgumentException)
			{
				exceptionCaught = true;
			}

			Assert(exceptionCaught);

			providers[""] = new OrgPrioritizedList(OrgWithSource.New(org1, new List<string>() { "Provider" }));

			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org1));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("", org3));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("", org4));
		}

		public void TestGroupSpecific()
		{
			var factory2 = new BusinessObjectFactory();
			var org1 = OrgWithSource.New(factory2.NewWithValidTestData<OrgHeader>(), new List<string>() { "Provider1" });
			factory2.Save();

			var org2 = OrgWithSource.New(Factory.Load<OrgHeader>(org1.Org.PK), new List<string>() { "Provider2" });

			Assert("precondition", !org1.Equals(org2));
			Assert("precondition", org1.Org.PK.Equals(org2.Org.PK));

			OrgWithSource org3 = null;
			var org4 = GetNewOrgWithSourceWithValidTestData();
			var org5 = GetNewOrgWithSourceWithValidTestData();
			var org6 = GetNewOrgWithSourceWithValidTestData();

			var providers = new Creditors
									{
										{
											"FRT", new OrgPrioritizedList
													{
														{ 3, new OrgWithSource[] { org3, org3 } },
														{ 10, new OrgWithSource[] { null } },
														{ 2, new OrgWithSource[] { org6, } },
													}
											},
										{
											"DST", new OrgPrioritizedList
													{
														{ 1, new OrgWithSource[] { org2 } },
														{ 2, new OrgWithSource[] { org6, org5, null, org6, org1 } },
													}
											},
									};

			var providers2 = new Creditors
									{
										{
											"FRT", new OrgPrioritizedList
													{
														{ 1, new OrgWithSource[] { org1, org2 } },
														{ 3, new OrgWithSource[] { org3, org3 } },
														{ 10, new OrgWithSource[] { null, org4, org4 } },
														{ 2, new OrgWithSource[] { org6, org2 } },
													}
											},
										{ "ORG", new OrgPrioritizedList(org4, org5) },
									};

			providers.Merge(providers2);

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, providers.GetRank("FRT", org1));
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, providers.GetRank("FRT", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("FRT", org3));
			AssertContainsExactElementsInAnyOrder(new[] { 10 }, providers.GetRank("FRT", org4));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("FRT", org5));
			AssertContainsExactElementsInAnyOrder(new[] { 2 }, providers.GetRank("FRT", org6));

			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("ORG", org1));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("ORG", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("ORG", org3));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("ORG", org4));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("ORG", org5));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("ORG", org6));

			AssertContainsExactElementsInAnyOrder(new[] { 2, 1 }, providers.GetRank("DST", org1));
			AssertContainsExactElementsInAnyOrder(new[] { 2, 1 }, providers.GetRank("DST", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("DST", org3));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("DST", org4));
			AssertContainsExactElementsInAnyOrder(new[] { 2 }, providers.GetRank("DST", org5));
			AssertContainsExactElementsInAnyOrder(new[] { 2 }, providers.GetRank("DST", org6));

			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("", org6));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("BRK", org6));

			providers.Add("BRK", new OrgPrioritizedList { { 1, org5 }, { 5, org6 } });

			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("BRK", org1));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("BRK", org2));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("BRK", org3));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), providers.GetRank("BRK", org4));
			AssertContainsExactElementsInAnyOrder(new[] { 1 }, providers.GetRank("BRK", org5));
			AssertContainsExactElementsInAnyOrder(new[] { 5 }, providers.GetRank("BRK", org6));

			bool exceptionCaught = false;

			try
			{
				providers.Add("", new OrgPrioritizedList { { 1, org2 }, { 5, org4 } });
			}
			catch (ArgumentException)
			{
				exceptionCaught = true;
			}

			Assert(exceptionCaught);
		}

		OrgWithSource GetNewOrgWithSourceWithValidTestData()
		{
			return OrgWithSource.New(Factory.NewWithValidTestData<OrgHeader>(), new List<string>() { "Provider" });
		}
	}
}
