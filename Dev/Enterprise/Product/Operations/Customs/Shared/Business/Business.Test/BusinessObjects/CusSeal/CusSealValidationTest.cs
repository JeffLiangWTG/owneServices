using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusSealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBK_SequenceNumber()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(seal.BK_SequenceNumberInfo);
		}

		public void TestCheckBK_SealNumber()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(seal.BK_SealNumberInfo);
		}

		public void TestCheckBK_UnloadingState()
		{
			var info = seal.BK_UnloadingStateInfo;
			seal.BK_UnloadingState = "ZZZ";
			AssertHasMessageErrorContaining("When invalid list entry", info, ListValidation.InvalidCodeMessageError.ToString());

			seal.BK_UnloadingState = UnloadingStates.Codes.New;
			AssertNoMessageErrorContaining("When valid list entry", info, ListValidation.InvalidCodeMessageError.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			seal = Factory.New<CusSeal>();
		}
		CusSeal seal;
	}
}
