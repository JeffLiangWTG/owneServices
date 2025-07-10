using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderLookups))]
	sealed class CusAuthorisationHeaderLookupsBaseOnlyTest : CusAuthorisationHeaderLookupsAbstractTest<CusAuthorisationHeaderLookups>
	{
		public void TestAppliesToList()
		{
			AssertType<OrgHeaderCollection>(CusAuthorisationHeaderLookupsForTesting().AppliesToList);
		}

		public void TestBaseAuthorisationTypeList()
		{
			AssertEquals("Correct List", "ACE, ACP, ACR, ACT, AWB, CCL, CGU, CVA, CW1, CW2, CWP, DPO, EIR, ETD, EUS, IPO, OPO, RSS, SAS, SDE, SSE, TEA, TRD, TST", CusAuthorisationHeaderLookupsForTesting().AuthorisationTypeList.CodesAsString);
		}

		protected override CusAuthorisationHeaderLookups CusAuthorisationHeaderLookupsForTesting() => new CusAuthorisationHeaderLookups(Factory.New<CusAuthorisationHeader>());
	}
}
