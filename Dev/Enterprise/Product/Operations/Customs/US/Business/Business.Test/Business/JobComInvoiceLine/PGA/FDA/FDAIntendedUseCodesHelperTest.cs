using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class FDAIntendedUseCodesHelperTest : TestCaseWithFactory
	{
		public void TestIsAOCRequiredForDRU_DA()
		{
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._100000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._130000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._180017));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._970000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._150013));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DA(FDAIntendedUseCodesHelper.Codes._080000));
		}

		public void TestIsAOCRequiredForDRU_REG()
		{
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._100000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._180017));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._970000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._080000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_REG(FDAIntendedUseCodesHelper.Codes._130000));
		}

		public void TestIsAOCRequiredForDRU_DLS()
		{
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._100000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._180017));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._970000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._080000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_DLS(FDAIntendedUseCodesHelper.Codes._130000));
		}

		public void TestIsAOCRequiredForDRU_IND()
		{
			AssertEquals(true, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_IND(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_IND(FDAIntendedUseCodesHelper.Codes._180017));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_IND(FDAIntendedUseCodesHelper.Codes._970000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_IND(FDAIntendedUseCodesHelper.Codes._080000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsAOCRequiredForDRU_IND(FDAIntendedUseCodesHelper.Codes._130000));
		}

		public void TestIsFinishedDosageFormDrugs()
		{
			AssertEquals(true, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._080000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._130000));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._155009));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._130000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsFinishedDosageFormDrugs(FDAIntendedUseCodesHelper.Codes._180017));
		}

		public void TestIsActivePharmaceuticalIngredients()
		{
			AssertEquals(true, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._150007));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._150013));
			AssertEquals(true, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._150017));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._130000));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._180009));
			AssertEquals(false, FDAIntendedUseCodesHelper.IsActivePharmaceuticalIngredients(FDAIntendedUseCodesHelper.Codes._180017));
		}
	}
}
