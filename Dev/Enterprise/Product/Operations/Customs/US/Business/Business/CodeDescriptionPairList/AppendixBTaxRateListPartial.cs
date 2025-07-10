using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	partial class AppendixBTaxRateList
	{
		public const string CBMAEligible = "CBMA Eligible";

		public AppendixBTaxRateList(string taxCode)
		{
			switch (taxCode)
			{
				case Core.Constants.USCustoms.FeeCodes.DistilledSpirits:
					AddPair(Codes.DistilledSpirits, Descriptions.DistilledSpirits);
					break;

				case Core.Constants.USCustoms.FeeCodes.Wines:
					AddPair(Codes.Wines_1, Descriptions.Wines_1);
					AddPair(Codes.Wines_2, Descriptions.Wines_2);
					AddPair(Codes.Wines_3, Descriptions.Wines_3);
					AddPair(Codes.Wines_4, Descriptions.Wines_4);
					AddPair(Codes.Wines_5, Descriptions.Wines_5);
					AddPair(Codes.Wines_6, Descriptions.Wines_6);
					break;

				case Core.Constants.USCustoms.FeeCodes.Tobacco:
					AddPair(Codes.Tobacco_1, Descriptions.Tobacco_1);
					AddPair(Codes.Tobacco_2, Descriptions.Tobacco_2);
					AddPair(Codes.Tobacco_3, Descriptions.Tobacco_3);
					AddPair(Codes.Tobacco_4, Descriptions.Tobacco_4);
					AddPair(Codes.Tobacco_5, Descriptions.Tobacco_5);
					AddPair(Codes.Tobacco_6, Descriptions.Tobacco_6);
					AddPair(Codes.Tobacco_7, Descriptions.Tobacco_7);
					AddPair(Codes.Tobacco_8, Descriptions.Tobacco_8);
					break;

				case Core.Constants.USCustoms.FeeCodes.OtherExcise:
					AddPair(Codes.Other_1, Descriptions.Other_1);
					AddPair(Codes.Other_2, Descriptions.Other_2);
					AddPair(Codes.Other_3, Descriptions.Other_3);
					AddPair(Codes.Other_4, Descriptions.Other_4);
					break;
			}

			AddPair(Codes.Specify, Descriptions.Specify);
		}

		public static string GetNormalTaxRateString(USCTariff importTariff, ZString taxCode, ZString rateType)
		{
			return importTariff != null ? importTariff.GetTaxFeeRateDescription(taxCode, rateType) : ZString.Empty;
		}

		public static string GetUQ(ZString rateDesc)
		{
			int indexOfPer = rateDesc.IndexOf("/");

			string result = "";

			if (indexOfPer > 0)
			{
				result = rateDesc.SubstringSafe(indexOfPer + 1);
			}

			return result;
		}

		public static ZDecimal GetRate(ZString rateDesc)
		{
			ZDecimal result = ZDecimal.ParseSafe(rateDesc.KeepCharsUntil(".1234567890", new char[] { '/' }), 0m);

			if (rateDesc == Codes.Tobacco_2 || rateDesc.Contains("c/"))
			{
				result /= 100m;
			}

			return result;
		}

		public static string GetComputationCode(ZString rateDesc, ZString importTariff, ZString firstUQ, ZString secondUQ)
		{
			string result = string.Empty;

			if (rateDesc == Codes.Tobacco_2)
			{
				result = ComputationCodeList.Codes.AdValorem;
			}
			else
			{
				ZString uQ = GetUQ(rateDesc);

				if (uQ.EqualsIgnoringCase(firstUQ)
					|| rateDesc.EqualsIgnoringCase(Codes.Specify)
					|| rateDesc.EqualsIgnoringCase(AppendixBTaxRateList.CBMAEligible)
					|| (uQ.EqualsIgnoringCase(firstUQ) && (importTariff == "2403102050" || importTariff == "2403102080"))
					|| (uQ == Fifty && IsConvertibleFrom_50(firstUQ))
					|| (uQ == WineLiters && firstUQ == Liters))
				{
					result = ComputationCodeList.Codes.SpecificRateFirstQuantity;
				}
				else if (uQ.EqualsIgnoringCase(secondUQ) || uQ == Fifty && IsConvertibleFrom_50(secondUQ))
				{
					result = ComputationCodeList.Codes.SpecificRateSecondQuantity;
				}
				else if (!uQ.IsEmpty && DoesUQMatchNoneOfCustomsUQsCore(uQ, firstUQ, secondUQ))
				{
					result = ComputationCodeList.CustomComputationCodeForCalculatingIRTax;
				}
			}

			return result;
		}

		static bool IsConvertibleFrom_50(ZString customsUQ)
		{
			return customsUQ == ABIUnitOfMeasureList.Codes.Thousand
				|| customsUQ == ABIUnitOfMeasureList.Codes.Number;
		}

		public const string Fifty = "50";
		public const string Liters = "L";
		public const string WineLiters = "WL";

		public static bool DoesUQMatchNoneOfCustomsUQs(ZString rateDesc, ZString firstUQ, ZString secondUQ)
		{
			var uq = GetUQ(rateDesc);

			return DoesUQMatchNoneOfCustomsUQsCore(uq, firstUQ, secondUQ);
		}

		internal static bool DoesUQMatchNoneOfCustomsUQsCore(ZString uq, ZString firstUQ, ZString secondUQ)
		{
			return !uq.IsEmpty
				&& !IsMatch(uq, firstUQ, secondUQ)
				&& uq != AppendixBTaxRateList.Codes.Specify
				&& uq != AppendixBTaxRateList.CBMAEligible
				&& (uq != Fifty || !IsConvertibleFrom_50(firstUQ) && !IsConvertibleFrom_50(secondUQ));
		}

		static bool IsMatch(ZString uq, ZString firstUQ, ZString secondUQ)
		{
			return uq == firstUQ || uq == secondUQ || (uq == WineLiters && (firstUQ == Liters || secondUQ == Liters));
		}
	}
}
