using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class ImportCodesForm3520_1ListTest : TestCase
	{
		public void TestIsImportedByICI()
		{
			Assert(ImportCodesForm3520_1List.IsImportedByICI(ImportCodesForm3520_1List.Codes.A));
			Assert(ImportCodesForm3520_1List.IsImportedByICI(ImportCodesForm3520_1List.Codes.C));
			Assert(ImportCodesForm3520_1List.IsImportedByICI(ImportCodesForm3520_1List.Codes.J));
			Assert(ImportCodesForm3520_1List.IsImportedByICI(ImportCodesForm3520_1List.Codes.Z));
			Assert(!ImportCodesForm3520_1List.IsImportedByICI(ImportCodesForm3520_1List.Codes.FF));
		}

		public void TestIsPolicyNumberRequired()
		{
			Assert(ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.G));
			Assert(ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.I));
			Assert(ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.K));
			Assert(ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.J));
			Assert(!ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.A));
			Assert(!ImportCodesForm3520_1List.IsPolicyNumberRequired(ImportCodesForm3520_1List.Codes.B));
		}

		public void TestIsEngineFamilyNumberRequired()
		{
			Assert(ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.B));
			Assert(ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.F));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.A));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.C));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.J));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.W));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.Z));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.E));
			Assert(!ImportCodesForm3520_1List.IsEngineFamilyNumberRequired(ImportCodesForm3520_1List.Codes.G));
		}

		public void TestIsDISFilingRequired()
		{
			Assert(ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.EE));
			Assert(ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.FF));
			Assert(ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.M));
			Assert(ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.N));
			Assert(!ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.A));
			Assert(!ImportCodesForm3520_1List.IsDISFilingRequired(ImportCodesForm3520_1List.Codes.B));
		}

		public void TestIsExemptionNumberRequired()
		{
			Assert(ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.G));
			Assert(ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.I));
			Assert(ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.L));
			Assert(ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.K));
			Assert(ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.O));
			Assert(!ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.A));
			Assert(!ImportCodesForm3520_1List.IsExemptionNumberRequired(ImportCodesForm3520_1List.Codes.B));
		}

		public void TestIsModelYearRequired()
		{
			Assert(ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.A));
			Assert(ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.C));
			Assert(ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.U));
			Assert(ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.Y));
			Assert(ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.Z));
			Assert(!ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.B));
			Assert(!ImportCodesForm3520_1List.IsModelYearRequired(ImportCodesForm3520_1List.Codes.E));
		}
	}
}
