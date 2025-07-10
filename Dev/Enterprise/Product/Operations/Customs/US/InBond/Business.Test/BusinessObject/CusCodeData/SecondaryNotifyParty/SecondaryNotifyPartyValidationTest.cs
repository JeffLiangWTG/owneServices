using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class SecondaryNotifyPartyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var secondaryNotifyParty = Factory.New<SecondaryNotifyParty>();
			ValidationTestHelper.AssertErrorIfInvalidCode(secondaryNotifyParty.CY_CodeInfo, "0", SecondaryNotifyPartyCodeList.Codes.Third);
		}
	}
}
