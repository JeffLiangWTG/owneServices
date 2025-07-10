using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCCountry.Schema.UC_Code), DescriptionProperty(USCCountry.Schema.UC_Name)]
	public class USCCountry : AutoUSCCountry
	{
		public const string Unknown = "**";
		public const string WestBank = "WE";
		public const string GazaStrip = "GZ";
		public const string Burma = "BU";

		[ThreadStatic]
		static IList<string> israelEligibleCountries;

		public static IList<string> IsraelEligibleCountries
		{
			get { return israelEligibleCountries ?? (israelEligibleCountries = new string[] { WestBank, GazaStrip, Core.Constants.CountryCodes.Israel, Core.Constants.CountryCodes.Jordan, Core.Constants.CountryCodes.Egypt }); }
		}

		public bool IssuesStandardisedVisas
		{
			get
			{
				return UC_Code == Core.Constants.CountryCodes.Bahrain ||
					UC_Code == Core.Constants.CountryCodes.Bangladesh ||
					UC_Code == Core.Constants.CountryCodes.Brazil ||
					UC_Code == Core.Constants.CountryCodes.China ||
					UC_Code == Core.Constants.CountryCodes.CostaRica ||
					UC_Code == Core.Constants.CountryCodes.DominicanRepublic ||
					UC_Code == Core.Constants.CountryCodes.Egypt ||
					UC_Code == Core.Constants.CountryCodes.Guatemala ||
					UC_Code == Core.Constants.CountryCodes.Haiti ||
					UC_Code == Core.Constants.CountryCodes.HongKong ||
					UC_Code == Core.Constants.CountryCodes.India ||
					UC_Code == Core.Constants.CountryCodes.Indonesia ||
					UC_Code == Core.Constants.CountryCodes.Jamaica ||
					UC_Code == Core.Constants.CountryCodes.Japan ||
					UC_Code == Core.Constants.CountryCodes.KoreaSouth ||
					UC_Code == Core.Constants.CountryCodes.Lesotho ||
					UC_Code == Core.Constants.CountryCodes.Macau ||
					UC_Code == Core.Constants.CountryCodes.Panama ||
					UC_Code == Core.Constants.CountryCodes.Malaysia ||
					UC_Code == Core.Constants.CountryCodes.Mexico ||
					UC_Code == Core.Constants.CountryCodes.Nepal ||
					UC_Code == Core.Constants.CountryCodes.Oman ||
					UC_Code == Core.Constants.CountryCodes.Pakistan ||
					UC_Code == Core.Constants.CountryCodes.Peru ||
					UC_Code == Core.Constants.CountryCodes.Philippines ||
					UC_Code == Core.Constants.CountryCodes.Romania ||
					UC_Code == Core.Constants.CountryCodes.Singapore ||
					UC_Code == Core.Constants.CountryCodes.SriLanka ||
					UC_Code == Core.Constants.CountryCodes.Taiwan ||
					UC_Code == Core.Constants.CountryCodes.Thailand ||
					UC_Code == Core.Constants.CountryCodes.TrinidadAndTobago ||
					UC_Code == Core.Constants.CountryCodes.Turkey ||
					UC_Code == Core.Constants.CountryCodes.UnitedArabEmirates ||
					UC_Code == Core.Constants.CountryCodes.Uruguay;
			}
		}

		public USCCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString GetRateIndicator(ZDate effectiveDate)
		{
			ZString result = ZString.Empty;

			if ((UC_RateColumnBeginDate.IsEmpty || UC_RateColumnBeginDate.Date <= effectiveDate) &&
					(UC_RateColumnEndDate.IsEmpty || UC_RateColumnEndDate.Date >= effectiveDate))
			{
				result = UC_RateColumnIndicator;
			}

			return result;
		}

		public bool IsNAFTACountry
		{
			get
			{
				return UC_Code == Core.Constants.CountryCodes.Canada ||
					UC_Code == Core.Constants.CountryCodes.Mexico ||
				CanadaProvinceTerritoryCodes.IsCanadianProvince(UC_Code);
			}
		}

		internal bool IsValidForGSP(ZDateTime effectiveDate)
		{
			return UC_GSPIndicator &&
				(UC_GSPBeginDate.IsEmpty || UC_GSPBeginDate.Date <= effectiveDate.Date) &&
				(UC_GSPEndDate.IsEmpty || UC_GSPEndDate.Date >= effectiveDate.Date);
		}

		public bool IsValidForSPI(ZString primaryOrProgramSPICode, ZDateTime effectiveDate)
		{
			bool result = PrimarySpecProgramIndicatorList.IsNonCountrySpecific(primaryOrProgramSPICode);

			if (!result)
			{
				switch (primaryOrProgramSPICode)
				{
					case SpecialProgramList.Codes.AU:
					case SpecialProgramList.Codes.BH:
					case SpecialProgramList.Codes.CL:
					case SpecialProgramList.Codes.IL:
					case SpecialProgramList.Codes.JO:
					case SpecialProgramList.Codes.MA:
					case SpecialProgramList.Codes.MX:
					case SpecialProgramList.Codes.OM:
					case SpecialProgramList.Codes.PE:
					case SpecialProgramList.Codes.SG:
					case SpecialProgramList.Codes.CO:
					case SpecialProgramList.Codes.PA:
					case SpecialProgramList.Codes.NP:
					case SpecialProgramList.Codes.JP:
						result = primaryOrProgramSPICode == UC_Code;
						break;
					case SpecialProgramList.Codes.S:
					case SpecialProgramList.Codes.SPlus:
						result = UC_Code == Core.Constants.CountryCodes.UnitedStates
							|| UC_Code == Core.Constants.CountryCodes.Mexico
							|| (CanadaProvinceTerritoryCodes.IsCanadianProvince(UC_Code)
							|| CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(UC_Code)
							|| UC_Code == Core.Constants.CountryCodes.Canada);
						break;
					case SpecialProgramList.Codes.KR:
						result = primaryOrProgramSPICode == UC_Code && !UC_SPICode.IsEmpty && IsValidForSPICode(effectiveDate.Date);
						break;

					case SpecialProgramList.Codes.CA:
					case PrimarySpecProgramIndicatorList.Codes.B:
					case SpecialProgramList.Codes.BSharp:
						result = CanadaProvinceTerritoryCodes.IsCanadianProvince(UC_Code) ||
							CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(UC_Code) ||
							UC_Code == Core.Constants.CountryCodes.Canada;
						break;

					case SpecialProgramList.Codes.CSharp:
					case SpecialProgramList.Codes.KSharp:
					case SpecialProgramList.Codes.LSharp:
						result = IsNAFTACountry;
						break;

					case PrimarySpecProgramIndicatorList.Codes.N:
						result = IsraelEligibleCountries.Contains(UC_Code);
						break;

					case PrimarySpecProgramIndicatorList.Codes.A:
						result = IsValidForGSP(effectiveDate);
						break;

					case SPICompleteList.MoreCodes.APlus:
						result = IsValidForGSP(effectiveDate) && GetRateIndicator(effectiveDate.Date) == "3";
						break;

					case SpecialProgramList.Codes.PPlus:
					case PrimarySpecProgramIndicatorList.Codes.P:
						result = IsEligibleForCAFTA(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.D:
						result = IsEligibleForAGOA(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.E:
					case SPICompleteList.MoreCodes.EAsterisk:
						result = IsEligibleForCBI(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.J:
					case SPICompleteList.MoreCodes.JAsterisk:
						result = IsEligibleForATPA(effectiveDate);
						break;

					case SpecialProgramList.Codes.JPlus:
						result = IsEligibleForATPDEA(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.R:
						result = IsEligibleForCBTPA(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.W:
						result = IsEligibleForCBI(effectiveDate);
						break;

					case PrimarySpecProgramIndicatorList.Codes.Y:
					case PrimarySpecProgramIndicatorList.Codes.Z:
						result = primaryOrProgramSPICode == UC_SPICode && IsValidForSPICode(effectiveDate.Date);
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Is eligible for African Growth & Opportunity Act
		/// </summary>
		public bool IsEligibleForAGOA(ZDateTime date)
		{
			return UC_SpecialTradeProgramsIndicator == "D" && IsValidForSpecialTradePrograms(date);
		}

		/// <summary>
		/// Is eligible for Caribbean Basin Trade Partnership Act
		/// </summary>
		public bool IsEligibleForCBTPA(ZDateTime date)
		{
			return UC_SpecialTradeProgramsIndicator == "R" && IsValidForSpecialTradePrograms(date);
		}

		/// <summary>
		/// Is eligible for Caribbean Basin Initiative (CBI)
		/// </summary>
		public bool IsEligibleForCBI(ZDateTime date)
		{
			return UC_SPICode == "E" && IsValidForSPICode(date);
		}

		/// <summary>
		/// Is eligible for Andian Trade Preference Act 
		/// </summary>
		public bool IsEligibleForATPA(ZDateTime date)
		{
			return UC_SPICode == "J" && IsValidForSPICode(date);
		}

		bool IsValidForSPICode(ZDateTime date)
		{
			return
				(UC_SPIBeginDate.IsEmpty || UC_SPIBeginDate.Date <= date.Date) &&
				(UC_SPIEndDate.IsEmpty || UC_SPIEndDate.Date >= date.Date);
		}

		/// <summary>
		/// Is eligible for Andean Trade Promotion & Drug Eradication Act 
		/// </summary>
		public bool IsEligibleForATPDEA(ZDateTime date)
		{
			return UC_SpecialTradeProgramsIndicator == "J" && IsValidForSpecialTradePrograms(date);
		}

		bool IsValidForSpecialTradePrograms(ZDateTime date)
		{
			return
				(UC_SpecialTradeProgramsBeginDate.IsEmpty || UC_SpecialTradeProgramsBeginDate.Date <= date.Date) &&
				(UC_SpecialTradeProgramsEndDate.IsEmpty || UC_SpecialTradeProgramsEndDate.Date >= date.Date);
		}

		public bool IsEligibleForCAFTA(ZDateTime date)
		{
			return UC_MiscellaneousSPIIndicator == "P" && IsValidForMiscSPIPrograms(date);
		}

		bool IsValidForMiscSPIPrograms(ZDateTime date)
		{
			return
				(UC_MiscellaneousSPIBeginDate.IsEmpty || UC_MiscellaneousSPIBeginDate.Date <= date.Date) &&
				(UC_MiscellaneousSPIEndDate.IsEmpty || UC_MiscellaneousSPIEndDate.Date >= date.Date);
		}

		public bool IsLeastDevelopedCountry(ZDateTime date)
		{
			return GetRateIndicator(date.Date) == "3";
		}

		public bool IsFolkloreAgreementsCountry
		{
			get
			{
				return UC_Code == Core.Constants.CountryCodes.Bangladesh ||
					UC_Code == Core.Constants.CountryCodes.Colombia ||
					UC_Code == Core.Constants.CountryCodes.India ||
					UC_Code == Core.Constants.CountryCodes.Japan ||
					UC_Code == Core.Constants.CountryCodes.KoreaSouth ||
					UC_Code == Core.Constants.CountryCodes.Malaysia ||
					UC_Code == Core.Constants.CountryCodes.Mexico ||
					UC_Code == Core.Constants.CountryCodes.Pakistan ||
					UC_Code == Core.Constants.CountryCodes.Peru ||
					UC_Code == Core.Constants.CountryCodes.Philippines ||
					UC_Code == Core.Constants.CountryCodes.Taiwan ||
					UC_Code == Core.Constants.CountryCodes.Thailand;
			}
		}

		internal bool IsInsularPossessionsCountry(ZDateTime date)
		{
			return UC_SPICode == "Y" && IsValidForSPICode(date);
		}

		public bool IsRestrictedCountry(ZDateTime entryDate = default(ZDateTime))
		{
			var result = false;
			var effectiveEntryDate = entryDate.IsEmpty ? ZDateTime.Today : entryDate;
			if (UC_RestrictionIndicator
					&& (effectiveEntryDate >= UC_RestrictionIndicatorBeginDate || UC_RestrictionIndicatorBeginDate.IsEmpty)
					&& (effectiveEntryDate <= UC_RestrictionIndicatorEndDate || UC_RestrictionIndicatorEndDate.IsEmpty))
			{
				result = true;
			}
			return result;
		}
	}
}
