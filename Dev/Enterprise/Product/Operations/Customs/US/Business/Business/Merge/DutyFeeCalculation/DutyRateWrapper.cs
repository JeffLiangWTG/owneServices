using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	abstract class DutyRateWrapper : IRateWrapper
	{
		public static IRateWrapper GetWrapper(IDutyData dutyData)
		{
			return GetWrapper(dutyData.DateForDutyCalculation, dutyData.ImportTariff, dutyData.SpecialProgramsIndicatorCountry, dutyData.SpecialProgramsIndicatorPrimary, dutyData.CountryOfOrigin);
		}

		public static IFeeWrapper GetFeeWrapper(IDutyData dutyData, ZString feeCode)
		{
			if (dutyData.ImportTariff is USCTariff tariff)
			{
				foreach (USCTariffDutyRate rate in tariff.DutyRates)
				{
					if (rate.UD_TaxFeeClassCode == feeCode)
					{
						return new FeeWrapper(rate);
					}
				}
			}
			return null;
		}

		public static IRateWrapper GetWrapper(ZDateTime entryDate, USCTariff tariff, ZString spiCountry, ZString primarySPI, ZString isoCountryCode)
		{
			IRateWrapper result = null;
			if (tariff == null || tariff.IsProvDutyAlwaysRequired)
			{
				result = new GeneralWrapper(tariff, entryDate);
			}
			else
			{
				if (IsUnfriendly(tariff.Factory, entryDate, isoCountryCode))
				{
					result = new UnfriendlyWrapper(tariff);
				}
				else
				{
					var tariffNumber = tariff.UE_Tariff;
					var country = tariff.Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, isoCountryCode);
					if (tariff.HasSpecialProgramsIndicator(primarySPI) || (country != null && !Chapter98Helper.Is99Tariff(tariffNumber) && !Chapter98Helper.Is98Tariff(tariffNumber) && country.IsValidForSPI(primarySPI, entryDate)))
					{
						bool isDutyFreePrimarySPI = PrimarySpecProgramIndicatorList.IsDutyFreeSPI(primarySPI);

						//to be duty-free, tariff should support it or generically duty-free
						if (isDutyFreePrimarySPI)
						{
							result = new ZeroDutyWrapper(tariff);
						}
						else
						{
							result = new SpecialWrapper(tariff, isoCountryCode, primarySPI);
						}
					}
					else
					{
						ZString spiCountryCode = SpecialProgramList.IsNAFTASPI(spiCountry) ? spiCountry.Left(1) : spiCountry;

						if (tariff.HasSpecialProgramsIndicator(spiCountryCode))
						{
							result = new SpecialWrapper(tariff, isoCountryCode, spiCountryCode);
						}
						else
						{
							result = new GeneralWrapper(tariff, entryDate, isoCountryCode);
						}
					}
				}
			}
			return result;
		}

		static bool IsUnfriendly(BusinessObjectFactory factory, ZDateTime entryDate, ZString isoCountryCode)
		{
			USCCountry countryOfOrigin = factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, isoCountryCode);

			return countryOfOrigin != null && countryOfOrigin.GetRateIndicator(entryDate.Date) == "2";
		}

		#region Implementation

		protected DutyRateWrapper(USCTariff tariff)
		{
			this.tariff = tariff ?? new BusinessObjectFactory().GetNull<USCTariff>();
		}
		protected readonly USCTariff tariff;

		#region ITariffColumnWrapper Members

		public abstract ZDecimal Specific { get; }
		public abstract ZDecimal Advalorem { get; }
		public abstract ZDecimal Other { get; }

		#endregion

		sealed internal class ZeroDutyWrapper : DutyRateWrapper
		{
			public ZeroDutyWrapper(USCTariff tariff)
				: base(tariff)
			{
			}
			public override ZDecimal Specific
			{
				get { return ZDecimal.Zero; }
			}

			public override ZDecimal Advalorem
			{
				get { return ZDecimal.Zero; }
			}

			public override ZDecimal Other
			{
				get { return ZDecimal.Zero; }
			}
		}

		sealed class GeneralWrapper : DutyRateWrapper
		{
			public GeneralWrapper(USCTariff tariff, ZDateTime entryDate, string isoCountryCode = "")
				: base(tariff)
			{
				if (tariff != null)
				{
					var rateIndicator = tariff.Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, isoCountryCode)?.GetRateIndicator(entryDate.Date) ?? ZString.Empty;
					if (rateIndicator == "2")
					{
						specificRate = tariff.UE_Column2RateSpecific;
						advaloremRate = tariff.UE_Column2RateAdValorem;
						otherRate = tariff.UE_Column2RateOther;
					}
					else
					{
						specificRate = tariff.UE_Column1RateSpecific;
						advaloremRate = tariff.UE_Column1RateAdValorem;
						otherRate = tariff.UE_Column1RateOther;
					}
				}
			}

			readonly ZDecimal specificRate;
			readonly ZDecimal advaloremRate;
			readonly ZDecimal otherRate;

			public override ZDecimal Specific => specificRate;

			public override ZDecimal Advalorem => advaloremRate;

			public override ZDecimal Other => otherRate;
		}

		class FeeWrapper : IFeeWrapper
		{
			public FeeWrapper(USCTariffDutyRate rate)
			{
				if (rate == null)
				{
					throw new ArgumentNullException(nameof(rate));
				}

				this.rate = rate;
			}
			readonly USCTariffDutyRate rate;

			#region IFeeWrapper Members

			ZString IFeeWrapper.ComputationCode
			{
				get { return rate.UD_TaxFeeComputationCode; }
			}

			#endregion

			#region IDutyRateWrapper Members

			public ZDecimal Specific
			{
				get { return rate.UD_TaxFeeSpecificRate; }
			}

			public ZDecimal Advalorem
			{
				get { return rate.UD_TaxFeeAdvalorem; }
			}

			public ZDecimal Other
			{
				get { return ZDecimal.Zero; }
			}

			#endregion
		}

		public class SpecialWrapper : DutyRateWrapper
		{
			public SpecialWrapper(USCTariff tariff, ZString isoCountryCode, ZString specialProgramsIndicator)
				: base(tariff)
			{
				BusinessObject[] rates = tariff.DutyRates.Find(new ZQuery(USCTariffDutyRateSchema.UD_ISOCountryCode, isoCountryCode));
				if (rates.Length == 0 && specialProgramsIndicator.Length > 0)
				{
					rates = tariff.DutyRates.Find(new ZQuery(USCTariffDutyRateSchema.UD_ISOCountryCode, specialProgramsIndicator));
				}
				if (rates.Length > 0)
				{
					tariffDutyRate = (USCTariffDutyRate)rates[0];
				}
				else
				{
					tariffDutyRate = tariff.Factory.GetNull<USCTariffDutyRate>();
				}
			}
			public readonly USCTariffDutyRate tariffDutyRate;

			public override ZDecimal Specific
			{
				get { return tariffDutyRate.UD_SpecificSpecialRate; }
			}

			public override ZDecimal Advalorem
			{
				get { return tariffDutyRate.UD_AdValoremSpecialRate; }
			}

			public override ZDecimal Other
			{
				get { return tariffDutyRate.UD_OtherSpecialRate; }
			}
		}

		class UnfriendlyWrapper : DutyRateWrapper
		{
			public UnfriendlyWrapper(USCTariff tariff)
				: base(tariff)
			{
			}

			public override ZDecimal Specific
			{
				get { return tariff.UE_Column2RateSpecific; }
			}

			public override ZDecimal Advalorem
			{
				get { return tariff.UE_Column2RateAdValorem; }
			}

			public override ZDecimal Other
			{
				get { return tariff.UE_Column2RateOther; }
			}
		}

		#endregion
	}
}
