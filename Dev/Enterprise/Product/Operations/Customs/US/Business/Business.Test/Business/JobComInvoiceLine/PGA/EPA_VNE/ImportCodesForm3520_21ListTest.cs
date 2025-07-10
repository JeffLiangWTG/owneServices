using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportCodesForm3520_21ListTest : TestCase
	{
		public void TestIsEngineFamilyNumberRequired()
		{
			Assert(ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._22));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._02));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._03));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._04));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._05));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._06));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._07));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._08));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._16));
			Assert(!ImportCodesForm3520_21List.IsEngineFamilyNumberRequired(ImportCodesForm3520_21List.Codes._24A));
		}

		public void TestIsModelYearRequired()
		{
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._09));
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._17));
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._19));
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._22));
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._24A));
			Assert(ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._24B));
			Assert(!ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(!ImportCodesForm3520_21List.IsModelYearRequired(ImportCodesForm3520_21List.Codes._02));
		}

		public void TestIsDISFilingRequired()
		{
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._02));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._03));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._04));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._05));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._06));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._07));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._08));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._11));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._12));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._13));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._14));
			Assert(ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._15));
			Assert(!ImportCodesForm3520_21List.IsDISFilingRequired(ImportCodesForm3520_21List.Codes._01));
		}

		public void TestIsExemptionNumberRequired()
		{
			Assert(ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._10));
			Assert(ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._11));
			Assert(ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._12));
			Assert(ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._18));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._02));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._03));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._05));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._06));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._07));
			Assert(!ImportCodesForm3520_21List.IsExemptionNumberRequired(ImportCodesForm3520_21List.Codes._14));
		}

		public void TestIsExemptionRemarksRequired()
		{
			Assert(ImportCodesForm3520_21List.IsExemptionRemarksRequired(ImportCodesForm3520_21List.Codes._21));
			Assert(ImportCodesForm3520_21List.IsExemptionRemarksRequired(ImportCodesForm3520_21List.Codes._25));
			Assert(!ImportCodesForm3520_21List.IsExemptionRemarksRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(!ImportCodesForm3520_21List.IsExemptionRemarksRequired(ImportCodesForm3520_21List.Codes._02));
		}

		public void TestIsStorageLocationRequired()
		{
			Assert(ImportCodesForm3520_21List.IsStorageLocationRequired(ImportCodesForm3520_21List.Codes._24A));
			Assert(ImportCodesForm3520_21List.IsStorageLocationRequired(ImportCodesForm3520_21List.Codes._24B));
			Assert(!ImportCodesForm3520_21List.IsStorageLocationRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(!ImportCodesForm3520_21List.IsStorageLocationRequired(ImportCodesForm3520_21List.Codes._02));
		}

		public void TestIsEnginePowerRequired()
		{
			Assert(ImportCodesForm3520_21List.IsEnginePowerRequired(ImportCodesForm3520_21List.Codes._19));
			Assert(ImportCodesForm3520_21List.IsEnginePowerRequired(ImportCodesForm3520_21List.Codes._22));
			Assert(ImportCodesForm3520_21List.IsEnginePowerRequired(ImportCodesForm3520_21List.Codes._23));
			Assert(!ImportCodesForm3520_21List.IsEnginePowerRequired(ImportCodesForm3520_21List.Codes._01));
			Assert(!ImportCodesForm3520_21List.IsEnginePowerRequired(ImportCodesForm3520_21List.Codes._02));
		}
	}
}
