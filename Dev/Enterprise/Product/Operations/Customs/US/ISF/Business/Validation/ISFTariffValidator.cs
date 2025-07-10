using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class ISFTariffValidator
	{
		public static void ValidateFormattedHarmonisedNum(BusinessObjectFactory factory, ZPropertyInfo info, ZString harmonisedNum, int numberOfHarmonisedDigitsRequired)
		{
			if (!harmonisedNum.IsEmpty)
			{
				if (harmonisedNum.Length < numberOfHarmonisedDigitsRequired)
				{
					info.AddMessageError(string.Format(TariffLengthNotEnough, harmonisedNum, numberOfHarmonisedDigitsRequired));
				}
				else
				{
					if (IsHSVersionUpToDate(factory))
					{
						var query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, harmonisedNum);
						query.AddToFilter(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
						query.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);

						USCTariff tariff = factory.LoadTop1<USCTariff>(query);
						if (tariff == null)
						{
							info.AddMessageError(string.Format(TariffNotFound, harmonisedNum));
						}
					}
					else
					{
						var query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, harmonisedNum);
						var tariff = factory.LoadTop1<USCTariff>(query);

						if (tariff == null)
						{
							info.AddWarning(string.Format(TariffDataVersionNotMatch, harmonisedNum));
						}
					}
				}
			}
		}

		static bool IsHSVersionUpToDate(BusinessObjectFactory factory)
		{
			var version = USCDataVersion.GetLastHTSAttempt(factory);

			if (version != null)
			{
				var currentVersion = version.UZ_Version;
				var currentYearMatchNumber = (ZDateTime.Now.Year - 2000) * 100;
				var minimumVersion = 1403;

				return (currentVersion >= minimumVersion) && (currentVersion >= currentYearMatchNumber);
			}

			return false;
		}

		public const string TariffNotFound = "Tariff '{0}' is not recognized as a valid tariff. Please check the tariff or, if necessary, send a query to customs for the latest tariff information (Customs Declarations->Actions->Reference File Request->Tariff)";
		public const string TariffDataVersionNotMatch = "Tariff '{0}' is not recognized. This may indicate that it is not valid, please check the tariff before submitting to Customs.";
		public const string TariffLengthNotEnough = "Tariff '{0}' is too short; the minimum number of digits required is '{1}'.";
	}
}
