using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolDGRestrictions))]
	sealed class ConsolDGRestrictionsTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJKD_Calc_Substance()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			AssertNoExceptionThrown(() => consolDGRestrictions.JKD_Calc_Substance = "THISISTOOLONG");
			AssertNoExceptionThrown(() => consolDGRestrictions.JKD_Calc_Substance = "hi");
		}

		public void TestSupportsCloneCore()
		{
			var consolDGRestrictions = Factory.NewWithValidTestData<ConsolDGRestrictions>();
			Assert(consolDGRestrictions.SupportsClone());
		}

		public void TestCloneExcludesParentConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDGRestrictions = consol.ConsolDGRestrictionCollection.AddNew();
			consolDGRestrictions.JKD_UNNO = "1001";
			Factory.Save();

			var clone = (ConsolDGRestrictions)consolDGRestrictions.Clone();
			AssertEquals(ZGuid.Empty, clone.JKD_JK);
		}
	}
}
