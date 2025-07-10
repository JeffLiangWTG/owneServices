using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementItemConditionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModes()
		{
			var item = Factory.NewWithValidTestData<OrgCommissionAgreementItem>();

			Factory.Save();

			item.CAI_Code = "SHP";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "SEA", "AIR", "RAI", "ROA", "COU", "ALL" }, item.ConditionCollection.AddNew().Lookups.Modes.GetAllCodesZString());

			item.CAI_Code = "BRK";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "IMP", "EXP", "ALL" }, item.ConditionCollection.AddNew().Lookups.Modes.GetAllCodesZString());

			var containers = new ZString[]
			{
				"FCL", "BLK", "LQD", "BBK", "ROR", "ALL"
			};

			item.CAI_Code = "AGS";
			AssertContainsExactElementsInAnyOrder(containers, item.ConditionCollection.AddNew().Lookups.Modes.GetAllCodesZString());

			item.CAI_Code = "AGB";
			AssertContainsExactElementsInAnyOrder(containers, item.ConditionCollection.AddNew().Lookups.Modes.GetAllCodesZString());

			item.CAI_Code = "";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ALL" }, item.ConditionCollection.AddNew().Lookups.Modes.GetAllCodesZString());
		}
	}
}
