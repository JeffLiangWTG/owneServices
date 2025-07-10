using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class SlaughterDateValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var packingDate = Factory.New<PackingDate>();
			packingDate.CY_Code = ZString.Empty;
			AssertNoErrors(packingDate.CY_CodeInfo);
		}
	}
}
