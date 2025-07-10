
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CensusOverrideCodeListTest : NUnit.Framework.TestCase
	{
		public void TestGetListFor()
		{
			CodeDescriptionPairList list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.ImprobableCountry);
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._01));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._02));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._03));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.Qty1DividedByQty2);
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._21));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.LowValueDividedByQty1);
			AssertEquals(9, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._12));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._13));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._14));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._15));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._27));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.Qty2DividedByQty1);
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._21));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.HighValueDividedByQty2);
			AssertEquals(12, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._04));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._05));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._06));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._07));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._08));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._11));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._15));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._21));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));
			AssertEquals(CensusWarningCodeList.Codes.HighValueDividedByQty2, "27F");

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.GrossWeightVessel);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._22));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.LowValueDividedByQty2);
			AssertEquals(9, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._12));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._13));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._14));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._15));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._27));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.HighValueDividedByQty1);
			AssertEquals(12, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._04));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._05));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._06));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._07));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._08));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._09));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._11));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._15));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._21));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.ImprobableAirTariff);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._05));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.GrossWeightAir);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._22));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.MaximumValueExceeded);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._51));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.ChargesDividedByValue);
			AssertEquals(13, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._05));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._12));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._13));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._14));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._15));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._16));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._17));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._18));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._19));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._20));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._22));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._50));

			list = CensusOverrideCodeList.GetListFor(CensusWarningCodeList.Codes.MaximumChargeExceeded);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(CensusOverrideCodeList.Codes._51));
		}
	}
}
