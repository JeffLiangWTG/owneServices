namespace Enterprise.Customs.US.Business.Testing
{
	class FDAProcessingCodeListTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestIsAOCRequiredForFOO()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForFOO(FDAProcessingCodeList.Codes.FOO_NSF));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForFOO(FDAProcessingCodeList.Codes.FOO_PRO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForFOO(FDAProcessingCodeList.Codes.FOO_FEE));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForFOO(FDAProcessingCodeList.Codes.FOO_ADD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForFOO(FDAProcessingCodeList.Codes.FOO_DSU));
		}

		public void TestIsBrandNameMandatory()
		{
			AssertEquals(true, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(false, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(false, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.VME_ADR));
			AssertEquals(false, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.VME_ADE));
			AssertEquals(true, FDAProcessingCodeList.IsBrandNameMandatory(FDAProcessingCodeList.Codes.RAD_REP));
		}

		public void TestIsSpeficiAOCRequiredForBIO()
		{
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(false, FDAProcessingCodeList.IsSpeficiAOCRequiredForBIO(FDAProcessingCodeList.Codes.FOO_DSU));
		}

		public void TestIsAOCRequiredForBIO_ALG_180009()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180009(FDAProcessingCodeList.Codes.BIO_BLD));
		}

		public void TestIsAOCRequiredForBIO_ALG_080000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_080000(FDAProcessingCodeList.Codes.BIO_PVE));
		}

		public void TestIsAOCRequiredForBIO_BBA_080000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_BBA_080000(FDAProcessingCodeList.Codes.BIO_XEN));
		}

		public void TestIsAOCRequiredForBIO_HCT_082000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(FDAProcessingCodeList.Codes.BIO_HCT));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_HCT_082000(FDAProcessingCodeList.Codes.BIO_XEN));
		}

		public void TestIsAOCRequiredForBIO_ALG_180016()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_180016(FDAProcessingCodeList.Codes.BIO_HCT));
		}

		public void TestIsAOCRequiredForBIO_ALG_155000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_155000(FDAProcessingCodeList.Codes.BIO_HCT));
		}

		public void TestIsAOCRequiredForBIO_ALG_150007()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_HCT));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_150007(FDAProcessingCodeList.Codes.BIO_PVE));
		}

		public void TestIsAOCRequiredForBIO_ALG_970000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_ALG));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_CGT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_XEN));
			AssertEquals(false, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_HCT));
		}

		public void IsAOCRequiredForBIO_ALG_170000()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BDP));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BLD));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_HCT));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_PVE));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_VAC));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_XEN));
		}

		public void IsAOCRequiredForBIO_BBA_150007()
		{
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_BBA));
			AssertEquals(true, FDAProcessingCodeList.IsAOCRequiredForBIO_ALG_970000(FDAProcessingCodeList.Codes.BIO_PVE));
		}
	}
}
