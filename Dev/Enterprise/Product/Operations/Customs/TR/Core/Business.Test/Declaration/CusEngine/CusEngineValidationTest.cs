using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusEngineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEG_EngineType_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(Engine.CEG_EngineTypeInfo, "X", "1");
		}

		public void TestCheckCEG_CapacityCC_MandatoryValidation_NotEntered()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Engine.CEG_CapacityCCInfo);
		}

		public void TestCheckCEG_CapacityCC_MandatoryValidation_Negative()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(Engine.CEG_CapacityCCInfo);
		}

		public void TestCheckCEG_Cylinders_MandatoryValidation()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Engine.CEG_CylindersInfo);
		}
		public void TestCheckCEG_CapacityHP_MandatoryValidation_NotEntered()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Engine.CEG_CapacityHPInfo);
		}

		public void TestCheckCEG_CapacityHP_MandatoryValidation_Negative()
		{
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(Engine.CEG_CapacityHPInfo);
		}

		CusEngine Engine => engine ?? (engine = Factory.New<CusEngine>());
		CusEngine engine;
	}
}
