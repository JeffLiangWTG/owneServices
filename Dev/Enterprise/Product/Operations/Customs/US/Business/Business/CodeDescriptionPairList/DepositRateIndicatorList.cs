using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class DepositRateIndicatorList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string AdValorem = "A";
			public const string Specific = "S";
			public const string Override = "O";
			public const string OverrideAdValorem = "OA";
			public const string OverrideSpecific = "OS";
		}

		public static class Descriptions
		{
			public const string Rate = "{0}%";
			public const string OverrideAdValorem = "Override Ad Valorem Rate";
			public const string OverrideSpecific = "Override Specific Rate";
		}

		public DepositRateIndicatorList(USCACCaseRate caseRate)
		{
			if (caseRate != null)
			{
				if (caseRate.U6_AdValoremRate > 0 || caseRate.U6_SpecificRate == 0)
				{
					AddPair(Codes.AdValorem, GetRateDescription(Codes.AdValorem, caseRate));
					AddPair(Codes.OverrideAdValorem, Descriptions.OverrideAdValorem);
				}

				if (caseRate.U6_SpecificRate > 0m)
				{
					AddPair(Codes.Specific, GetRateDescription(Codes.Specific, caseRate));
					AddPair(Codes.OverrideSpecific, Descriptions.OverrideSpecific);
				}
			}
		}

		public static bool DepositRateIndContainOverride(ZString depositRateInd)
		{
			return depositRateInd.Contains(Codes.Override);
		}

		public static USCACCaseRate GetUSCACCaseRate(USCACCase adCase, ZDate dateForAD_CVD)
		{
			return adCase?.CaseRates.GetDepositRate(dateForAD_CVD);
		}

		public static string GetRateDescriptionFromCaseRecord(USCACCase adCase, ZDate dateForAD_CVD, string rateType)
		{
			var caseRate = GetUSCACCaseRate(adCase, dateForAD_CVD);
			return caseRate != null ? GetRateDescription(rateType, caseRate) : ZString.Empty;
		}

		public static string GetRateDescriptionFromCaseRateRecord(USCACCaseRate caseRate, string rateType)
		{
			return caseRate != null ? GetRateDescription(rateType, caseRate) : ZString.Empty;
		}

		public static string GetAdValoremRateDescription(ZString caseNo, ZDecimal rate)
		{
			var percentage = (ZDecimal)(rate * 100m);
			return GetAdValoremRateDescriptionFromPercentage(caseNo, percentage);
		}

		public static string GetAdValoremRateDescriptionFromPercentage(ZString caseNo, ZDecimal percentage)
		{
			return !caseNo.IsEmpty ? (percentage == 0m ? "0%" : percentage.ToString(2) + "%") : "";
		}

		public static ZString GetRateDescription(string rateType, USCACCaseRate caseRate)
		{
			var result = ZString.Empty;

			if (rateType == Codes.AdValorem)
			{
				result = GetAdValoremRateDescription(caseRate.U6_CaseNumber, caseRate.U6_AdValoremRate);
			}
			else if (rateType == Codes.Specific)
			{
				result = GetACERateDescriptionForSpecificOrOverrideSpecific(caseRate.U6_SpecificRate, caseRate.U6_Unit, caseRate.U6_UnitDesc);
			}

			return result;
		}

		public static ZString GetACERateDescriptionForSpecificOrOverrideSpecific(ZDecimal specificRate, ZString unit, ZString unitDesc)
		{
			ZDecimal amountPerUnit = specificRate < 1 ? (ZDecimal)(specificRate * 100) : specificRate;
			ZString dollarUnit = specificRate > 0 && specificRate < 1 ? "c" : "$";
			return amountPerUnit.ToStringTrimZeros() + dollarUnit + "/" + unit + (unitDesc.IsEmpty ? "" : "(" + unitDesc + ")");
		}

		public static ZDecimal GetDepositRate(USCACCaseRate rate, ZString rateIndicator)
		{
			ZDecimal result = ZDecimal.Zero;
			if (rate != null)
			{
				switch (rateIndicator)
				{
					case Codes.Specific:
						result = rate.U6_SpecificRate;
						break;
					case Codes.AdValorem:
						result = rate.U6_AdValoremRate;
						break;
					default:
						result = rate.U6_AdValoremRate;
						break;
				}
			}
			return result;
		}

		public static ZString GetDepositRateDescription(ZString depositRateIndicator, DepositRateIndicatorList depositRates, ZDecimal depositRateOverride, USCACCaseRate caseRate)
		{
			var result = ZString.Empty;
			if (depositRates != null && !DepositRateIndContainOverride(depositRateIndicator))
			{
				result = depositRates.GetDescriptionFromCode(depositRateIndicator);
			}
			else if (depositRateIndicator == Codes.OverrideAdValorem)
			{
				var percentage = (ZDecimal)(depositRateOverride * 100m);
				result = percentage == 0m ? "0%" : percentage.ToString(2) + "%";
			}
			else if (depositRateIndicator == Codes.OverrideSpecific)
			{
				if (caseRate != null && !caseRate.U6_Unit.IsEmpty)
				{
					result = GetACERateDescriptionForSpecificOrOverrideSpecific(depositRateOverride, caseRate.U6_Unit, caseRate.U6_UnitDesc);
				}
				else
				{
					result = depositRateOverride.ToString(2);
				}
			}

			return result;
		}

		public static void SetDepositRateDescription(ZString descriptionToSet, ZString depositRateIndicator, ZPropertyInfo depositRateOverrideInfo)
		{
			var depositRateOverride = ZDecimal.Zero;

			if (DepositRateIndContainOverride(depositRateIndicator))
			{
				var validDescriptionCharacters = descriptionToSet.KeepChars("0123456789.");
				if (depositRateIndicator == Codes.OverrideAdValorem)
				{
					depositRateOverride = ZDecimal.ParseSafe(validDescriptionCharacters, ZDecimal.Zero) / 100m;
				}
				else
				{
					depositRateOverride = ZDecimal.ParseSafe(validDescriptionCharacters, ZDecimal.Zero);
					if (descriptionToSet.ToUpper().Contains("C"))
					{
						depositRateOverride = depositRateOverride / 100m;
					}
				}
			}

			if (depositRateOverride >= ZDecimal.Zero)
			{
				depositRateOverrideInfo.Value = depositRateOverride;
			}
		}
	}
}
