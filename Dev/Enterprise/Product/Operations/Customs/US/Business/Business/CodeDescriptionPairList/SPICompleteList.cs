using System.Collections.Generic;
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
	public class SPICompleteList : CodeDescriptionPairList,
		Integration.Customs.US.IBrokerPaymentTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public SPICompleteList() : this(true)
		{
		}

		public SPICompleteList(bool addNotApplicable = true)
		{
			AddRange(new PrimarySpecProgramIndicatorList());
			AddRange(new SpecialProgramList());
			if (addNotApplicable)
			{
				AddPair(MoreCodes.NotApplicable, MoreDescriptions.NotApplicable);
			}
			this.Sort();
		}

		public static SPICompleteList GetCachedList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WithNotApplicable", () => new SPICompleteList());
		}

		public static SPICompleteList GetCachedListWithoutNotApplicable(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("NoNotApplicable", () => new SPICompleteList(addNotApplicable: false));
		}

		public static class MoreCodes
		{
			public const string NotApplicable = "N/A";
			public const string APlus = "A+";
			public const string AAsterisk = "A*";
			public const string EAsterisk = "E*";
			public const string JAsterisk = "J*";
		}

		public static class MoreDescriptions
		{
			public const string NotApplicable = "Not Applicable";
			public const string APlus = "GSP for the least developed countries";
			public const string AAsterisk = "GSP not applicable for excluded countries";
			public const string EAsterisk = "CBI Eligibility excluded from certain articles";
			public const string JAsterisk = "ATPA Eligibility excluded from certain articles";
		}

		public static bool CanBeClaimedForMPFExemptionForDutyFreeTariffs(string spiCode, USCCountry countryOfOrigin, ZDateTime effectiveDate)
		{
			return spiCode == SpecialProgramList.Codes.SG ||
				spiCode == SpecialProgramList.Codes.AU ||
				spiCode == SpecialProgramList.Codes.KR && countryOfOrigin != null && countryOfOrigin.IsValidForSPI(spiCode, effectiveDate) ||
				spiCode == SpecialProgramList.Codes.CA ||
				spiCode == SpecialProgramList.Codes.MX ||
				spiCode == SpecialProgramList.Codes.CO ||
				spiCode == SpecialProgramList.Codes.PA ||
				spiCode == SpecialProgramList.Codes.CL ||
				spiCode == SpecialProgramList.Codes.SG ||
				spiCode == SpecialProgramList.Codes.BH ||
				spiCode == SpecialProgramList.Codes.OM ||
				spiCode == SpecialProgramList.Codes.PE ||
				spiCode == SpecialProgramList.Codes.PPlus ||
				spiCode == PrimarySpecProgramIndicatorList.Codes.P ||
				spiCode == SpecialProgramList.Codes.S ||
				spiCode == SpecialProgramList.Codes.SPlus ||
				spiCode == PrimarySpecProgramIndicatorList.Codes.Y;
		}

		/// <summary>
		/// It returns a list of SPI codes relevant for tariff. Tariff and Country of origin being null are taken into consideration
		/// </summary>
		public static CodeDescriptionPairList GetRelevantListFor(ISPILine spiLine)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			bool standAloneLine = spiLine.ParentTariffLine == null && !spiLine.HasSecondaryChildrenLines;

			ISPILine parentLineToPass = spiLine.ParentTariffLine ?? spiLine;
			IEnumerable<ISPILine> secondaryLines = parentLineToPass.SecondaryTariffLines;

			foreach (CodeDescriptionPair pair in SPICompleteList.GetCachedList(spiLine.Factory))
			{
				if (IsValidBothForParentAndChild(spiLine, pair.Code, standAloneLine, parentLineToPass, secondaryLines))
				{
					result.Add(pair);
				}
			}

			if (!result.ContainsCode(MoreCodes.NotApplicable))
			{
				USCTariff tariff = parentLineToPass.ImportTariff;

				IRateWrapper rates = DutyRateWrapper.GetWrapper(spiLine.EffectiveDate, tariff, ZString.Empty, ZString.Empty, spiLine.CountryOfOrigin != null ? spiLine.CountryOfOrigin.UC_Code : ZString.Empty);

				if (!rates.IsInvalidDutyRate() || tariff.UE_ISOCountryofOriginEditCode.IsEmpty)
				{
					CodeDescriptionPair notApplicable = new CodeDescriptionPair(MoreCodes.NotApplicable, MoreDescriptions.NotApplicable);
					result.Insert(0, notApplicable);
				}
			}

			return result;
		}

		/// <summary>
		///1.	Inlieu – check the validity of an SPI against 98/99 only.  
		///2.	98/99 with a single SPI - check the validity of an SPI against 98/99 only. 
		///3.	The rest of 98/99 – Check the validity of an SPI only against secondary lines unless the rule 'No SPI' does apply. This is where 99031724 falls in. 
		///4.	The rest of parent + secondary lines – Check the validity of an SPI against all lines. 
		/// </summary>
		static bool IsValidBothForParentAndChild(ISPILine spiLine, string spiCode, bool standAloneLine, ISPILine parentLine, IEnumerable<ISPILine> secondaryLines)
		{
			bool isValidSPI = false;

			var tariff = parentLine.ImportTariff;
			var isTariff9899 = tariff != null && (tariff.UE_Tariff.StartsWith("98") || tariff.UE_Tariff.StartsWith("99"));

			if (isTariff9899)
			{
				if (tariff.Applies(TariffRuleList.Codes.InLieuTariffs, spiLine.EffectiveDate) || tariff.UE_SPICode.Split(2).Length == 1 || standAloneLine)
				{
					isValidSPI = IsValidFor(spiCode, parentLine, standAloneLine, null);
				}
				else if (!tariff.Applies(TariffRuleList.Codes.NoSPIRequired, spiLine.EffectiveDate))
				{
					foreach (ISPILine childLine in secondaryLines)
					{
						isValidSPI = IsValidFor(spiCode, childLine, false, parentLine);
						if (!isValidSPI)
						{
							break;
						}
					}
				}
			}
			else
			{
				if (parentLine != null)
				{
					isValidSPI = IsValidFor(spiCode, parentLine, standAloneLine, null);
				}

				if (isValidSPI)
				{
					foreach (ISPILine childLine in secondaryLines)
					{
						isValidSPI = IsValidFor(spiCode, childLine, false, parentLine);
						if (!isValidSPI)
						{
							break;
						}
					}
				}
			}

			return isValidSPI;
		}

		static bool IsValidFor(ZString spiCode, ISPILine spiLine, bool standAloneLine, ISPILine parentSPILine)
		{
			bool result = false;

			USCTariff importTariff = spiLine.ImportTariff;
			ZDateTime effectiveDate = spiLine.EffectiveDate;
			USCCountry countryOfOrigin = spiLine.CountryOfOrigin;

			if (importTariff == null)
			{
				result = IsValidAgainstCountryOfOrigin(spiCode, effectiveDate, countryOfOrigin);
				if (IsCAFTA(spiCode))
				{
					result = spiLine?.CountryOfExport?.IsValidForSPI(spiCode, effectiveDate) ?? false;
				}
			}
			else if (!importTariff.Applies(TariffRuleList.Codes.NoSPIRequired, effectiveDate))
			{
				if (importTariff.HasSpecialProgramsIndicator(spiCode) ||

					//for tariff and C/O which make goods duty-free, a certain SPIs are allowed even if the tariff does not indicate the SPI as applicable for the purpose of MPF exemption
					CanBeClaimedForMPFExemptionForDutyFreeTariffs(spiCode, countryOfOrigin, effectiveDate) && countryOfOrigin != null && importTariff.IsDutyFreeFor(countryOfOrigin.UC_Code, effectiveDate))
				{
					if (spiCode.KeepAlphanumericCharacters() == PrimarySpecProgramIndicatorList.Codes.A)
					{
						result = importTariff.IsValidForGSP(countryOfOrigin, effectiveDate);

						if (result && parentSPILine != null && parentSPILine.ImportTariff != null)
						{
							var parentTariff = parentSPILine.ImportTariffCode;
							if (parentTariff.StartsWith("990380", System.StringComparison.OrdinalIgnoreCase) || parentTariff.StartsWith("990385", System.StringComparison.OrdinalIgnoreCase) || parentTariff.StartsWith("990345", System.StringComparison.OrdinalIgnoreCase))
							{
								result = parentSPILine.ImportTariff.HasSpecialProgramsIndicator(spiCode) && parentSPILine.ImportTariff.IsValidForGSP(countryOfOrigin, effectiveDate);
							}
						}
					}
					else
					{
						result = IsValidAgainstCountryOfOrigin(spiCode, effectiveDate, countryOfOrigin);

						if (result)
						{
							if (spiCode == PrimarySpecProgramIndicatorList.Codes.Z)
							{
								result = !importTariff.Applies(TariffRuleList.Codes.FASDutyFreeTreatmentException, effectiveDate);
							}
							else if (IsCAFTA(spiCode))
							{
								if (spiLine != null && spiLine.CountryOfExport != null)
								{
									result = spiLine.CountryOfExport.IsValidForSPI(spiCode, effectiveDate);
								}
								else
								{
									result = importTariff.HasSpecialProgramsIndicator(spiCode);
								}
							}
							else if (spiCode == PrimarySpecProgramIndicatorList.Codes.D && parentSPILine != null && parentSPILine.ImportTariff != null)
							{
								var parentTariff = parentSPILine.ImportTariffCode;
								if (parentTariff.StartsWith("990380", System.StringComparison.OrdinalIgnoreCase) || parentTariff.StartsWith("990385", System.StringComparison.OrdinalIgnoreCase))
								{
									result = parentSPILine.ImportTariff.HasSpecialProgramsIndicator(spiCode);
								}
							}
						}
						else
						{
							if (spiCode == SpecialProgramList.Codes.S || spiCode == SpecialProgramList.Codes.SPlus)
							{
								var parentLine = parentSPILine ?? spiLine;

								if (parentLine != null && parentLine.ImportTariff is USCTariff tariffOnParentSPILine)
								{
									result = tariffOnParentSPILine.HasSpecialProgramsIndicator(spiCode) && tariffOnParentSPILine.Applies(UniversalReferenceConstants.TariffAttributeTypes.Values.USMCA_REPAIR, effectiveDate);
								}
							}
						}
					}
				}

				if (result && standAloneLine)//if it is a stand-alone or parent line, then SPI with 'Do not declare this duty' is not valid. Secondary line with 'Do not declare this duty rate' indicates it needs a parent line and the SPI is valid
				{
					IRateWrapper rates = DutyRateWrapper.GetWrapper(effectiveDate, importTariff, spiCode, spiCode, countryOfOrigin != null ? countryOfOrigin.UC_Code : ZString.Empty);

					if (rates.IsInvalidDutyRate())
					{
						result = false;
					}
				}
			}

			return result;
		}

		static bool IsCAFTA(string spiCode)
		{
			return spiCode == SpecialProgramList.Codes.PPlus || spiCode == PrimarySpecProgramIndicatorList.Codes.P;
		}

		static bool IsValidAgainstCountryOfOrigin(ZString spiCode, ZDateTime effectiveDate, USCCountry countryOfOrigin)
		{
			bool result = false;

			if (countryOfOrigin == null || PrimarySpecProgramIndicatorList.IsNonCountrySpecific(spiCode) || IsCAFTA(spiCode))
			{
				result = true;
			}
			else if (countryOfOrigin != null)
			{
				result = countryOfOrigin.IsValidForSPI(spiCode, effectiveDate);
			}

			return result;
		}

		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
