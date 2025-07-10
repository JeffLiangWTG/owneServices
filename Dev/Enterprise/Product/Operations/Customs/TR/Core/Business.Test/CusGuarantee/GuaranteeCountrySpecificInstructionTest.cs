using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Testing
{
	class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			CombineAssertions(() =>
			{
				var list = instruction.GetTypeList(Core.Constants.CountryCodes.Turkey);
				AssertContainsExactElementsInAnyOrder("Guarantee Type List", new ZString[] { "BANKA", "DAC", "DACR2", "DIGER", "GAR", "GDS", "GLB", "GTR1", "GTR2", "GTRAN", "NAKIT", "RODER", "UND" }, list.GetAllCodes());
				AssertEquals("The Guarantee Types List has been defined for TR guarantees", 13, list.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			instruction = new GuaranteeCountrySpecificInstruction(Factory);
		}

		GuaranteeCountrySpecificInstruction instruction;
	}
}
