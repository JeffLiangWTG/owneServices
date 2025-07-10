using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class CensusOverrideCodeList
	{
		public static CodeDescriptionPairList GetListFor(ZString warningCode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			switch (warningCode)
			{
				case CensusWarningCodeList.Codes.ImprobableCountry:
					result.AddPair(CensusOverrideCodeList.Codes._01, CensusOverrideCodeList.Descriptions._01);
					result.AddPair(CensusOverrideCodeList.Codes._02, CensusOverrideCodeList.Descriptions._02);
					result.AddPair(CensusOverrideCodeList.Codes._03, CensusOverrideCodeList.Descriptions._03);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.Qty1DividedByQty2:
				case CensusWarningCodeList.Codes.Qty2DividedByQty1:
					result.AddPair(CensusOverrideCodeList.Codes._09, CensusOverrideCodeList.Descriptions._09);
					result.AddPair(CensusOverrideCodeList.Codes._20, CensusOverrideCodeList.Descriptions._20);
					result.AddPair(CensusOverrideCodeList.Codes._21, CensusOverrideCodeList.Descriptions._21);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.LowValueDividedByQty1:
				case CensusWarningCodeList.Codes.LowValueDividedByQty2:
					result.AddPair(CensusOverrideCodeList.Codes._09, CensusOverrideCodeList.Descriptions._09);
					result.AddPair(CensusOverrideCodeList.Codes._12, CensusOverrideCodeList.Descriptions._12);
					result.AddPair(CensusOverrideCodeList.Codes._13, CensusOverrideCodeList.Descriptions._13);
					result.AddPair(CensusOverrideCodeList.Codes._14, CensusOverrideCodeList.Descriptions._14);
					result.AddPair(CensusOverrideCodeList.Codes._15, CensusOverrideCodeList.Descriptions._15);
					result.AddPair(CensusOverrideCodeList.Codes._20, CensusOverrideCodeList.Descriptions._20);
					result.AddPair(CensusOverrideCodeList.Codes._27, CensusOverrideCodeList.Descriptions._27);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.HighValueDividedByQty1:
				case CensusWarningCodeList.Codes.HighValueDividedByQty2:
					result.AddPair(CensusOverrideCodeList.Codes._04, CensusOverrideCodeList.Descriptions._04);
					result.AddPair(CensusOverrideCodeList.Codes._05, CensusOverrideCodeList.Descriptions._05);
					result.AddPair(CensusOverrideCodeList.Codes._06, CensusOverrideCodeList.Descriptions._06);
					result.AddPair(CensusOverrideCodeList.Codes._07, CensusOverrideCodeList.Descriptions._07);
					result.AddPair(CensusOverrideCodeList.Codes._08, CensusOverrideCodeList.Descriptions._08);
					result.AddPair(CensusOverrideCodeList.Codes._09, CensusOverrideCodeList.Descriptions._09);
					result.AddPair(CensusOverrideCodeList.Codes._10, CensusOverrideCodeList.Descriptions._10);
					result.AddPair(CensusOverrideCodeList.Codes._11, CensusOverrideCodeList.Descriptions._11);
					result.AddPair(CensusOverrideCodeList.Codes._15, CensusOverrideCodeList.Descriptions._15);
					result.AddPair(CensusOverrideCodeList.Codes._21, CensusOverrideCodeList.Descriptions._21);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.ImprobableAirTariff:
					result.AddPair(CensusOverrideCodeList.Codes._05, CensusOverrideCodeList.Descriptions._05);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.GrossWeightAir:
				case CensusWarningCodeList.Codes.GrossWeightVessel:
					result.AddPair(CensusOverrideCodeList.Codes._20, CensusOverrideCodeList.Descriptions._20);
					result.AddPair(CensusOverrideCodeList.Codes._22, CensusOverrideCodeList.Descriptions._22);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.ChargesDividedByValue:
					result.AddPair(CensusOverrideCodeList.Codes._05, CensusOverrideCodeList.Descriptions._05);
					result.AddPair(CensusOverrideCodeList.Codes._12, CensusOverrideCodeList.Descriptions._12);
					result.AddPair(CensusOverrideCodeList.Codes._13, CensusOverrideCodeList.Descriptions._13);
					result.AddPair(CensusOverrideCodeList.Codes._14, CensusOverrideCodeList.Descriptions._14);
					result.AddPair(CensusOverrideCodeList.Codes._15, CensusOverrideCodeList.Descriptions._15);
					result.AddPair(CensusOverrideCodeList.Codes._16, CensusOverrideCodeList.Descriptions._16);
					result.AddPair(CensusOverrideCodeList.Codes._17, CensusOverrideCodeList.Descriptions._17);
					result.AddPair(CensusOverrideCodeList.Codes._18, CensusOverrideCodeList.Descriptions._18);
					result.AddPair(CensusOverrideCodeList.Codes._19, CensusOverrideCodeList.Descriptions._19);
					result.AddPair(CensusOverrideCodeList.Codes._20, CensusOverrideCodeList.Descriptions._20);
					result.AddPair(CensusOverrideCodeList.Codes._22, CensusOverrideCodeList.Descriptions._22);
					result.AddPair(CensusOverrideCodeList.Codes._49, CensusOverrideCodeList.Descriptions._49);
					result.AddPair(CensusOverrideCodeList.Codes._50, CensusOverrideCodeList.Descriptions._50);
					break;

				case CensusWarningCodeList.Codes.MaximumValueExceeded:
				case CensusWarningCodeList.Codes.MaximumChargeExceeded:
					result.AddPair(CensusOverrideCodeList.Codes._51, CensusOverrideCodeList.Descriptions._51);
					break;

				default:
					result = new CensusOverrideCodeList();
					break;
			}

			return result;
		}
	}
}
