using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(USCTariffSchema.Constants.UE_Tariff), DescriptionProperty(USCTariffSchema.Constants.UE_ShortDescription)]
	public sealed class USCTariff : AutoUSCTariff, ITariff, Universal.ITariff, Integration.Customs.US.IUSCTariff
	{
		#region Schema

		public new class Schema : AutoUSCTariff.Schema
		{
			public const string UE_FormattedTariff = "UE_FormattedTariff";
			public const string UE_ValueEditCode = "UE_ValueEditCode";
			public const string UE_ValueLowBounds = "UE_ValueLowBounds";
			public const string UE_ValueHighBounds = "UE_ValueHighBounds";
			public const string UE_QuantityEditCode = "UE_QuantityEditCode";
			public const string UE_QuantityEditLowerBound = "UE_QuantityEditLowerBound";
			public const string UE_QuantityEditUpperBound = "UE_QuantityEditUpperBound";
		}

		#endregion

		#region FilterSchema

		public static class FilterSchema
		{
			public const string Date = "Effective Date";
			public const string Tariff = "Tariff";
			public const string Description = "Description";
			public const string PGACode = "PGA Code";

			public const string SPI = "SPI";
			public const string PermitLicenseCode = "Permit License Code";

			public const string DatePropertyNameToDefault = "PropertySearch";
		}

		#endregion

		#region Constructors

		public USCTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public ZString TaxFeeCode
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (USCTariffDutyRate dutyRate in DutyRates)
				{
					if (!dutyRate.UD_TaxFeeClassCode.IsEmpty)
					{
						result += dutyRate.UD_TaxFeeClassCode + ",";
					}
				}
				return result.TrimEnd(',');
			}
		}

		public ZString TaxFeeComputationCode
		{
			get
			{
				return TaxFeeDutyRateObject != null ? TaxFeeDutyRateObject.UD_TaxFeeComputationCode : ZString.Empty;
			}
		}

		public ZString TaxFeeRate
		{
			get
			{
				ZString result = ZString.Empty;
				if (TaxFeeDutyRateObject != null)
				{
					if (TaxFeeDutyRateObject.UD_TaxFeeSpecificRate > 0.0m)
					{
						if (TaxFeeDutyRateObject.UD_TaxFeeAdvalorem > 0.0m)
						{
							result = TaxFeeDutyRateObject.UD_TaxFeeSpecificRate.ToStringTrimZeros() + " or " + TaxFeeDutyRateObject.UD_TaxFeeAdvalorem.ToStringTrimZeros();
						}
						else
						{
							result = TaxFeeDutyRateObject.UD_TaxFeeSpecificRate.ToStringTrimZeros();
						}
					}
					else
					{
						if (TaxFeeDutyRateObject.UD_TaxFeeAdvalorem > 0.0m)
						{
							result = TaxFeeDutyRateObject.UD_TaxFeeAdvalorem.ToStringTrimZeros();
						}
					}
				}
				return result;
			}
		}

		USCTariffDutyRate TaxFeeDutyRateObject
		{
			get
			{
				if (taxFeeDutyRateObject == null)
				{
					foreach (USCTariffDutyRate dutyRate in DutyRates)
					{
						if (!dutyRate.UD_TaxFeeClassCode.IsEmpty)
						{
							taxFeeDutyRateObject = dutyRate;
							break;
						}
					}
				}
				return taxFeeDutyRateObject;
			}
		}
		USCTariffDutyRate taxFeeDutyRateObject;

		/// <summary>
		/// For CBTPA Textile Benefits, no category number is required except for and 98201112
		/// and tariff under 98206 - refer CS00127987
		/// </summary>
		public bool NoCategoryNumberToBeEntered(ZDate effectiveDateForDutyRate)
		{
			return IsEligibleForCBTPATextileBenefits(effectiveDateForDutyRate)
					&& !UE_Tariff.StartsWith("98206");
		}

		public bool Applies(ZString ruleCode, ZDateTime effectiveDate)
		{
			var result = USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, UE_Tariff, effectiveDate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, ruleCode) != null;
			if (!result && TariffRules.Applies(ruleCode, UE_Tariff, effectiveDate))
			{
				result = ruleCode != TariffRuleList.Codes.InLieuTariffs || !TariffRules.Applies(TariffRuleList.Codes.AdditionalTariffs, UE_Tariff, effectiveDate)
					&& USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, UE_Tariff, effectiveDate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs) == null;
			}
			return result;
		}

		public USCTariffRule GetTariffRuleIfApplies(ZString ruleCode, ZDateTime effectiveDate)
		{
			return TariffRules.GetTariffRuleIfApplies(ruleCode, UE_Tariff, effectiveDate);
		}

		[ChildEditable(false)]
		internal USCTariffRuleAdhocCollection TariffRules
		{
			get
			{
				if (tariffRules == null)
				{
					tariffRules = new USCTariffRuleAdhocCollection(Factory);
					tariffRules.Load(UE_Tariff);
				}
				return tariffRules;
			}
		}
		USCTariffRuleAdhocCollection tariffRules;

		public override void OnSaving()
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			base.OnSaving();
		}

		#region Statics

#if DEBUG
		public const string TariffNumberNotInDatabase = "0199999999";

		public const string FDAPriorNoticeRequiredTariff = "0804504040";
		public const string FDAAdmissibilityReviewRequiredTariff = "8516500090";
		public const string FDAPriorNoticeMayBeRequiredTariff = "3004505040";
		public const string FDAAdmissibilityReviewMayBeRequiredTariff = "2853000095";
		public const string FDAAdmissibilityReviewDONOTSUBMITTariff = "8438909090";

		public const string DOTIsApplicable = "8703240054";
		public const string DOTMayBeApplicable = "7007110010";

		public const string FCCApplicable = "8528123228";
		public const string FCCMayBeApplicable = "8543709620";

		public const string CottonFeeApplicable = "6206900040";

		public const string AGOABenefitsApplicable = "98191103";
		public const string CBTPABenefitsApplicable = "9802008044";
		public const string ATPDEABenefitsApplicable = "9802008048";
		public const string CAFTABenefitsApplicable = "99150205";

		public const string AdditionalDutyCalculationApplicable = "99040237";

		public const string LumberPermitApplicable = "4407100165";

		/// <summary>
		/// Expires at 2009/12/31
		/// </summary>
		public const string DistilledSpiritsFeeApplicable = "2208402000";
		public const string TaxConditional = "2008112200";

		/// <summary>
		/// From 2011/08/01
		/// </summary>
		public const string DairyFeeApplicable = "0401100000";
		public const string DairyFeeApplicable2 = "0402210200";
#endif

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USCTariff tariff)
				: base(tariff)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(USCTariffDutyRateSchema.UD_UE, BusinessObject.PK);
				Factory.AddFetchHint(USCTariffQuantitySchema.UQ_UE, BusinessObject.PK);
				Factory.AddFetchHint(USCTariffValueSchema.UA_UE, BusinessObject.PK);
				Factory.AddFetchHint(USCTariffDateRestrictionSchema.UF_UE, BusinessObject.PK);
			}
		}

		#region Boolean Flags

		public bool IsDutyFree
		{
			get { return UE_DutyComputationCode == ComputationCodeList.Codes.Free; }
		}

		public bool IsDutyFreeFor(ZString countryOfOrigin, ZDateTime dutyDate)
		{
			IRateWrapper dutyRate = DutyRateWrapper.GetWrapper(dutyDate, this, ZString.Empty, ZString.Empty, countryOfOrigin);

			return dutyRate.Advalorem == 0m && dutyRate.Specific == 0m && dutyRate.Other == 0m;
		}

		public bool IsFeeApplicable(params string[] feeCodes)
		{
			return CheckFeeApplicability(x => x.IsFeeApplicable(feeCodes), feeCodes);
		}

		public bool IsTaxComputationUnknown
		{
			get
			{
				bool result = false;

				string[] taxCodes = CusFeeCodeConstants.GetTaxCodes();
				foreach (USCTariffDutyRate dutyRate in DutyRates)
				{
					result = dutyRate.IsFeeApplicable(taxCodes) && dutyRate.UD_TaxFeeComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable;
					if (result)
					{
						break;
					}
				}

				return result;
			}
		}

		public bool IsTaxApplicable
		{
			get { return IsFeeApplicable(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public bool IsTaxRequired
		{
			get { return CheckFeeApplicability(x => x.IsFeeRequired, CusFeeCodeConstants.GetTaxCodes()); }
		}

		public bool PRCoffeeFeeMightBeRequired
		{
			get { return CheckFeeApplicability(x => x.IsFeeRequired, Core.Constants.USCustoms.FeeCodes.Coffee); }
		}

		public bool IsTaxConditional
		{
			get { return CheckFeeApplicability(x => x.IsFeeConditional, CusFeeCodeConstants.GetTaxCodes()); }
		}

		bool CheckFeeApplicability(Func<USCTariffDutyRate, bool> apply, params string[] feeCodes)
		{
			bool result = false;

			foreach (USCTariffDutyRate dutyRate in DutyRates)
			{
				result = dutyRate.CheckFeeApplicability(delegate
				{ return apply(dutyRate); }, feeCodes);
				if (result)
				{
					break;
				}
			}

			return result;
		}

		public IEnumerable<ZString> GetRequiredFeeCodes()
		{
			foreach (USCTariffDutyRate dutyRate in DutyRates)
			{
				ZString result = dutyRate.GetRequiredFeeCode();
				if (!result.IsEmpty)
				{
					yield return result;
				}
			}
		}

		public ZString GetUniqueTaxCode()
		{
			ZString result = ZString.Empty;

			foreach (ZString code in GetRelatedFeeCodes())
			{
				if (CusFeeCodeConstants.IsExciseTax(code))
				{
					if (result.IsEmpty)
					{
						result = code;
					}
					else if (result != code)
					{
						result = ZString.Empty;
						break;
					}
				}
			}
			return result;
		}

		public IEnumerable<ZString> GetRelatedFeeCodes()
		{
			foreach (USCTariffDutyRate dutyRate in DutyRates)
			{
				if (!dutyRate.UD_TaxFeeClassCode.IsEmpty)
				{
					yield return dutyRate.UD_TaxFeeClassCode;
				}
			}
		}

		public bool IsSPICountryValid(ZString sPICountry)
		{
			bool result = UE_SPICode.IsEmpty;

			if (!result)
			{
				ZString spiCountryToCompare = SpecialProgramList.IsNAFTASPI(sPICountry) ? sPICountry.Left(1).PadRight(2) : sPICountry;
				foreach (string spiCode in UE_SPICode.Split(2))
				{
					result = spiCode.PadRight(2).Equals(spiCountryToCompare, StringComparison.OrdinalIgnoreCase);
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		public bool IsValidForGSP(USCCountry countryOfOrigin, ZDateTime effectiveDate)
		{
			bool result = false;

			if (countryOfOrigin != null)
			{
				//general GSP requirement
				result = countryOfOrigin.IsValidForGSP(effectiveDate) && !UE_SPICode.IsEmpty;

				if (result)
				{
					foreach (string spiCode in UE_SPICode.Split(2))
					{
						if (spiCode.Equals("A+", StringComparison.OrdinalIgnoreCase))
						{
							result &= countryOfOrigin.GetRateIndicator(effectiveDate.Date) == "3";
							if (result)
							{
								break;
							}
						}
						else if (spiCode.Equals("A*", StringComparison.OrdinalIgnoreCase))
						{
							bool isExcluded = false;
							foreach (ZString countryExcluded in UE_GSPExcludedCountries.Split(2))
							{
								isExcluded = countryExcluded == countryOfOrigin.UC_Code;
								if (isExcluded)
								{
									break;
								}
							}
							result &= !isExcluded;
							if (result)
							{
								break;
							}
						}
					}
				}
			}

			return result;
		}

		public bool HasSpecialProgramsIndicator(ZString indicator)
		{
			bool result = false;

			if (PrimarySpecProgramIndicatorList.IndicatorIsExcludedFromDBCheck(indicator))
			{
				//For some 99 tariffs, a set of tariffs is dedicated to AU, SG, JO, MA or MX etc and UE_ISOCountryofOriginEditCode is the same as UE_SPICode
				//For 9909.04.10, N(valid for IL, JO, EG WE and GZ) should be excluded from a valid SPI for JO Country of origin
				result = !UE_Tariff.StartsWith("99") || (indicator == UE_SPICode && UE_SPICode == UE_ISOCountryofOriginEditCode);
			}

			if (!result && !indicator.IsEmpty)
			{
				indicator = indicator.PadRight(2);
				foreach (string spiCode in UE_SPICode.Split(2))
				{
					string paddedCode = spiCode.PadRight(2);

					switch (indicator)
					{
						case "A ":
							result = paddedCode == "A " || paddedCode == "A+" || paddedCode == "A*";
							break;

						case "E ":
							result = paddedCode == "E " || paddedCode == "E*";
							break;

						case "J ":
							result = paddedCode == "J " || paddedCode == "J*";
							break;

						case SpecialProgramList.Codes.BSharp:
						case SpecialProgramList.Codes.CSharp:
						case SpecialProgramList.Codes.KSharp:
						case SpecialProgramList.Codes.LSharp:
							result = paddedCode.TrimEnd() == indicator.Left(1);
							break;

						default:
							result = paddedCode.Equals(indicator, StringComparison.OrdinalIgnoreCase);
							break;
					}

					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		public bool SecondaryTariffsAreDutyFree
		{
			get { return UE_DutyComputationCode == ComputationCodeList.Codes.Free; }
		}

		public bool IsSpecificSpecificDutyRate
		{
			get { return UE_DutyComputationCode == ComputationCodeList.Codes.SpecificSpecific; }
		}

		public bool BecomesDutyFreeDueTo(ZString spiCode)
		{
			bool result = false;

			if (!IsDutyFree && !spiCode.IsEmpty && HasSpecialProgramsIndicator(spiCode) && DutyRates.GetRateForSPI(spiCode) == null)
			{
				result = true;
			}

			return result;
		}

		public bool MayRequireADD
		{
			get { return UE_AntiDumping; }
		}

		public bool MayRequireCVD
		{
			get { return UE_CountervailingDutyFlag; }
		}

		public bool IsRepairOrAssembly(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.RepairTariffs, effectiveDate) || Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, effectiveDate);
		}

		public bool FDAAdmissibilityReviewDONOTSUBMIT
		{
			get { return TariffChecker.HasSpecifiedCode("FD0", OGARequirements); }
		}

		public bool FDAAdmissibilityReviewMayBeRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD1", OGARequirements); }
		}

		public bool FDAAdmissibilityReviewRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD2", OGARequirements); }
		}

		public bool FDAPriorNoticeAndAdmissibilityReviewMayBeRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD3", OGARequirements); }
		}

		public bool FDAPriorNoticeAndAdmissibilityReviewRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD4", OGARequirements); }
		}

		public bool HasFDARequirement
		{
			get { return FDAAdmissibilityReviewMayBeRequired || FDAAdmissibilityReviewRequired || FDAPriorNoticeAndAdmissibilityReviewMayBeRequired || FDAPriorNoticeAndAdmissibilityReviewRequired; }
		}

		public bool ACEFDAAdmissibilityReviewMayBeRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD1", PGARequirements); }
		}

		public bool ACEFDAAdmissibilityReviewRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD2", PGARequirements); }
		}

		public bool ACEFDAPriorNoticeAndAdmissibilityReviewMayBeRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD3", PGARequirements); }
		}

		public bool ACEFDAPriorNoticeAndAdmissibilityReviewRequired
		{
			get { return TariffChecker.HasSpecifiedCode("FD4", PGARequirements); }
		}

		public bool HasACEFDARequirement
		{
			get { return ACEFDAAdmissibilityReviewMayBeRequired || ACEFDAAdmissibilityReviewRequired || ACEFDAPriorNoticeAndAdmissibilityReviewMayBeRequired || ACEFDAPriorNoticeAndAdmissibilityReviewRequired; }
		}

		public bool HasDOTRequirement
		{
			get { return MayRequireDOT || DoesRequireDOT; }
		}

		/// <summary>
		/// May require department of transport data
		/// </summary>
		public bool MayRequireDOT
		{
			get { return TariffChecker.HasSpecifiedCode("DT1", OGARequirements); }
		}

		public bool DoesRequireFDA
		{
			get { return FDAAdmissibilityReviewRequired || FDAPriorNoticeAndAdmissibilityReviewRequired; }
		}

		public bool DoesRequireACEFDA
		{
			get { return ACEFDAAdmissibilityReviewRequired || ACEFDAPriorNoticeAndAdmissibilityReviewRequired; }
		}

		/// <summary>
		/// Requires department of transport data
		/// </summary>
		public bool DoesRequireDOT
		{
			get { return TariffChecker.HasSpecifiedCode("DT2", OGARequirements); }
		}

		internal bool MayRequireVNE
		{
			get { return TariffChecker.HasSpecifiedCode("EP3", PGARequirements); }
		}

		internal bool MayRequireODS
		{
			get { return TariffChecker.HasSpecifiedCode("EP1", PGARequirements); }
		}

		internal bool MayRequireFSIS
		{
			get { return TariffChecker.HasSpecifiedCode("FS3", PGARequirements); }
		}

		internal bool MayRequirePST
		{
			get { return TariffChecker.HasSpecifiedCode("EP5", PGARequirements); }
		}

		internal bool MayRequireHFC
		{
			get { return TariffChecker.HasSpecifiedCode("EH1", PGARequirements); }
		}

		internal bool MayRequireTSCA
		{
			get { return TariffChecker.HasSpecifiedCode("EP7", PGARequirements); }
		}

		internal bool DoesRequireFSIS
		{
			get { return TariffChecker.HasSpecifiedCode("FS4", PGARequirements); }
		}

		internal bool DoesRequireODS
		{
			get { return TariffChecker.HasSpecifiedCode("EP2", PGARequirements); }
		}

		internal bool DoesRequireVNE
		{
			get { return TariffChecker.HasSpecifiedCode("EP4", PGARequirements); }
		}

		internal bool DoesRequirePST
		{
			get { return TariffChecker.HasSpecifiedCode("EP6", PGARequirements); }
		}

		internal bool DoesRequireHFC
		{
			get { return TariffChecker.HasSpecifiedCode("EH2", PGARequirements); }
		}

		internal bool DoesRequireTSCA
		{
			get { return TariffChecker.HasSpecifiedCode("EP8", PGARequirements); }
		}

		public bool HasAPHISRequirement
		{
			get { return MayRequireAPHIS || DoesRequireAPHIS || MayRequireAPHISNoDisclaimRequired; }
		}

		internal bool MayRequireAPHIS
		{
			get { return TariffChecker.HasSpecifiedCode("AQ1", PGARequirements); }
		}

		internal bool DoesRequireAPHIS
		{
			get { return TariffChecker.HasSpecifiedCode("AQ2", PGARequirements); }
		}

		public bool MayRequireAPHISNoDisclaimRequired
		{
			get { return TariffChecker.HasSpecifiedCode("AQX", PGARequirements); }
		}

		public bool HasFWSRequirement
		{
			get { return MayRequireFWS || DoesRequireFWS; }
		}

		internal bool MayRequireFWS
		{
			get { return TariffChecker.HasSpecifiedCode("FW1", PGARequirements) || TariffChecker.HasSpecifiedCode("FW3", PGARequirements); }
		}

		internal bool DoesRequireFWS
		{
			get { return TariffChecker.HasSpecifiedCode("FW2", PGARequirements); }
		}

		internal bool MayRequireNMFS370
		{
			get { return TariffChecker.HasSpecifiedCode("NM1", PGARequirements); }
		}

		internal bool DoesRequireNMFS370
		{
			get { return TariffChecker.HasSpecifiedCode("NM2", PGARequirements); }
		}

		internal bool DoesRequireNMFSCOA(ZString countryOfOrigin, ZDateTime valuationDate)
		{
			return USRefTariffDataLoader.IsTariffMatchCondition(Factory, UE_Tariff, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA, TariffConditionValueTypes.Codes.PGA, GovernmentAgencyProgramCodeList.Codes.COA, countryOfOrigin, valuationDate);
		}

		internal bool MayRequireNMFSAMR
		{
			get { return TariffChecker.HasSpecifiedCode("NM3", PGARequirements); }
		}

		internal bool DoesRequireNMFSAMR
		{
			get { return TariffChecker.HasSpecifiedCode("NM4", PGARequirements); }
		}

		internal bool MayRequireNMFSHMS
		{
			get { return TariffChecker.HasSpecifiedCode("NM5", PGARequirements); }
		}

		internal bool DoesRequireNMFSHMS
		{
			get { return TariffChecker.HasSpecifiedCode("NM6", PGARequirements); }
		}

		internal bool DoesRequireNMFSSIM
		{
			get { return TariffChecker.HasSpecifiedCode("NM8", PGARequirements); }
		}

		internal bool MayRequireNHTSA
		{
			get { return TariffChecker.HasSpecifiedCode("DT1", PGARequirements); }
		}

		internal bool DoesRequireNHTSA
		{
			get { return TariffChecker.HasSpecifiedCode("DT2", PGARequirements); }
		}

		internal bool MayRequireAMSEG
		{
			get { return TariffChecker.HasSpecifiedCode("AM1", PGARequirements); }
		}

		internal bool DoesRequireAMSEG
		{
			get { return TariffChecker.HasSpecifiedCode("AM2", PGARequirements); }
		}

		internal bool MayRequireAMSMO
		{
			get { return TariffChecker.HasSpecifiedCode("AM3", PGARequirements); }
		}

		internal bool DoesRequireAMSMO
		{
			get { return TariffChecker.HasSpecifiedCode("AM4", PGARequirements); }
		}

		internal bool DoesRequireAMSPeanuts
		{
			get { return TariffChecker.HasSpecifiedCode("AM6", PGARequirements); }
		}

		internal bool MayRequireAMSOrganics
		{
			get { return TariffChecker.HasSpecifiedCode("AM7", PGARequirements); }
		}

		internal bool DoesRequireAMSOrganics
		{
			get { return TariffChecker.HasSpecifiedCode("AM8", PGARequirements); }
		}

		internal bool DoesRequireOMC
		{
			get { return TariffChecker.HasSpecifiedCode("OM2", PGARequirements); }
		}

		internal bool MayRequireOMC
		{
			get { return TariffChecker.HasSpecifiedCode("OM1", PGARequirements); }
		}

		internal bool MayRequireTTB
		{
			get { return TariffChecker.HasSpecifiedCode("TB1", PGARequirements) || TariffChecker.HasSpecifiedCode("TB3", PGARequirements); }
		}

		internal bool DoesRequireTTB
		{
			get { return TariffChecker.HasSpecifiedCode("TB2", PGARequirements); }
		}

		internal bool MayRequireDEA
		{
			get { return TariffChecker.HasSpecifiedCode("DE1", PGARequirements); }
		}

		internal bool MayRequireCPSC
		{
			get { return TariffChecker.HasSpecifiedCode("CP1", PGARequirements); }
		}

		internal bool DoesRequireCPSC
		{
			get { return TariffChecker.HasSpecifiedCode("CP2", PGARequirements); }
		}

		internal bool IsCOLANumberRequiredForTTB(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.COLANumberTTBProgramRequirement, effectiveDate);
		}

		internal bool IsPermitNumberRequiredForTTB(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.PermitNumberTTBProgramRequirement, effectiveDate);
		}

		internal bool IsForeignCertificateRequiredForTTB(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.ForeignCertificateTTBProgramRequirement, effectiveDate);
		}

		internal bool IsCigarRequiredForTTB(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.TTBPriceRequirement, effectiveDate);
		}

		public bool HasAMSRequirement
		{
			get { return DoesRequireAMSEG || MayRequireAMSEG || DoesRequireAMSMO || MayRequireAMSMO || DoesRequireAMSPeanuts; }
		}

		public bool HasAMSEGGRequirement
		{
			get { return MayRequireAMSEG || DoesRequireAMSEG; }
		}

		public bool HasAMSORDRequirement
		{
			get { return MayRequireAMSMO || DoesRequireAMSMO; }
		}

		public bool HasAMSPNTRequirement
		{
			get { return DoesRequireAMSPeanuts; }
		}

		public bool HasNOPRequirement
		{
			get { return MayRequireAMSOrganics || DoesRequireAMSOrganics; }
		}

		public bool HasFSISRequirement
		{
			get { return DoesRequireFSIS || MayRequireFSIS; }
		}

		public bool HasODSRequirement
		{
			get { return DoesRequireODS || MayRequireODS; }
		}

		public bool HasVNERequirement
		{
			get { return DoesRequireVNE || MayRequireVNE; }
		}

		public bool HasPSTRequirement
		{
			get { return DoesRequirePST || MayRequirePST; }
		}

		public bool HasHFCRequirement
		{
			get { return DoesRequireHFC || MayRequireHFC; }
		}

		public bool HasNMFS370Requirement
		{
			get { return DoesRequireNMFS370 || MayRequireNMFS370; }
		}

		public bool HasNMFSAMRRequirement
		{
			get { return DoesRequireNMFSAMR || MayRequireNMFSAMR; }
		}

		public bool HasNMFSHMSRequirement
		{
			get { return DoesRequireNMFSHMS || MayRequireNMFSHMS; }
		}

		public bool HasNMFSSIMRequirement
		{
			get { return DoesRequireNMFSSIM; }
		}

		public bool HasTSCARequirement
		{
			get { return DoesRequireTSCA || MayRequireTSCA; }
		}

		public bool HasOMCRequirement
		{
			get { return DoesRequireOMC || MayRequireOMC; }
		}

		public bool HasNHTSARequirement
		{
			get { return DoesRequireNHTSA || MayRequireNHTSA; }
		}

		public bool HasTTBRequirement
		{
			get { return MayRequireTTB || DoesRequireTTB; }
		}

		public bool HasDEARequirement
		{
			get { return MayRequireDEA; }
		}

		public bool HasCPSCRequirement
		{
			get { return MayRequireCPSC || DoesRequireCPSC; }
		}

		public bool IsGSPExcluded(ZString countryCode)
		{
			bool result = false;
			if (!countryCode.IsEmpty)
			{
				countryCode = countryCode.PadRight(2);
				foreach (string country in UE_GSPExcludedCountries.Split(2))
				{
					result = country.PadRight(2).Equals(countryCode, StringComparison.OrdinalIgnoreCase);
					if (result)
					{
						break;
					}
				}
			}
			return result;
		}

		public bool HasValidCountryOfOrigin(ZString countryOfOrigin)
		{
			ZString validCountryOfOrigin = ValidCountryOfOrigin;

			return validCountryOfOrigin.IsEmpty ||
				validCountryOfOrigin == countryOfOrigin ||
				(validCountryOfOrigin == Core.Constants.CountryCodes.Canada &&
				(CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) ||
				CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(countryOfOrigin)));
		}

		public ZString ValidCountryOfOrigin
		{
			get
			{
				ZString result = ZString.Empty;

				if (!UE_ISOCountryofOriginEditCode.IsEmpty && UE_ISOCountryofOriginEditCode != "01" && UE_ISOCountryofOriginEditCode != "02")
				{
					result = UE_ISOCountryofOriginEditCode;
				}

				return result;
			}
		}

		public bool RequiresCanadianLumberPermit
		{
			get { return UE_PermitLicenseIndicator == MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber; }
		}

		public bool IsValueToBeDeclaredInAlternateTariff(ZDateTime effectiveDate)
		{
			return
				UE_DutyComputationCode == ComputationCodeList.Codes.Derived ||
				IsTIBTariff ||
				Applies(TariffRuleList.Codes.ValueShouldBeDeclaredAtAlternateTariffLine, effectiveDate);
		}

		public bool CountryOfOriginAndExportShouldBeSame(ZDateTime effectiveDate)
		{
			return IsEligibleForCBTPATextileBenefits(effectiveDate);
		}

		public bool IsEligibleForCAFTAClaims(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.EligibleForCAFTAClaims, effectiveDate);
		}

		public bool IsEligibleForAGOATextileBenefits(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.EligibleForAGOATextileClaims, effectiveDate);
		}

		public bool IsEligibleForCBTPATextileBenefits(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.EligibleForCBTPATextileClaims, effectiveDate);
		}

		public bool IsEligibleForATPDEATextileAndTunaClaims(ZDateTime effectiveDate)
		{
			return Applies(TariffRuleList.Codes.EligibleForATPDEATextileAndTunaClaims, effectiveDate);
		}

		public bool IsPastaTariff
		{
			get
			{
				return UE_Tariff.StartsWith("1902112010") ||
					UE_Tariff.StartsWith("1902112020") ||
					UE_Tariff.StartsWith("1902112030") ||
					UE_Tariff.StartsWith("1902192010") ||
					UE_Tariff.StartsWith("1902192020") ||
					UE_Tariff.StartsWith("1902192030");
			}
		}

		public bool RequiresFirstQuantity()
		{
			return
				!UE_Unit1.IsEmpty && UE_Unit1 != ABIUnitOfMeasureList.Codes.NoUnitRequired &&
				(
				ComputationCodeList.IsFirstQuantityRequired(UE_DutyComputationCode) ||
				IsQuantityRequiredForUnitPriceBound(QuantityCode.FirstQuantity) ||
				IsQuantityRequiredForQuantityBound(QuantityCode.FirstQuantity)
				);
		}

		public bool RequiresSecondQuantity()
		{
			return
				!UE_Unit2.IsEmpty && UE_Unit2 != ABIUnitOfMeasureList.Codes.NoUnitRequired &&
				(
				ComputationCodeList.IsSecondQuantityRequired(UE_DutyComputationCode) ||
				IsQuantityRequiredForUnitPriceBound(QuantityCode.SecondQuantity) ||
				IsQuantityRequiredForQuantityBound(QuantityCode.SecondQuantity)
				);
		}

		public bool RequiresThirdQuantity()
		{
			return !UE_Unit3.IsEmpty && UE_Unit3 != ABIUnitOfMeasureList.Codes.NoUnitRequired &&
				(
				ComputationCodeList.IsThirdQuantityRequired(UE_DutyComputationCode) ||
				IsQuantityRequiredForUnitPriceBound(QuantityCode.ThirdQuantity) ||
				IsQuantityRequiredForQuantityBound(QuantityCode.ThirdQuantity)
				);
		}

		bool IsQuantityRequiredForUnitPriceBound(char quantityChar)
		{
			ZString valueEditCode = UE_ValueEditCode;

			return valueEditCode.StartsWith(ValueQuantityBoundValidator.ValueEditType.HTSRestriction) &&
				valueEditCode.Length == 3 &&
				valueEditCode[1] == quantityChar;
		}

		bool IsQuantityRequiredForQuantityBound(char quantityChar)
		{
			ZString quantityEditCode = UE_QuantityEditCode;

			return quantityEditCode.Length == 3 &&
				!quantityEditCode.StartsWith(ValueQuantityBoundValidator.ValueEditType.NoValueEdit) &&
				!quantityEditCode.EndsWith(ValueQuantityBoundValidator.ValueEditType.NoValueEdit) &&
				(quantityEditCode[0] == quantityChar || quantityEditCode[1] == quantityChar);
		}

		/// <summary>
		/// Determines whether a tariff number is used for Temporary Importation under Bond
		/// </summary>
		/// <param name="tariff">A tariff number to be tested</param>
		/// <returns>true if the <paramref name="tariff"/> starts from "9813"; otherwise, false.</returns>
		public static bool IsTIB(ZString tariff)
		{
			return tariff.StartsWith("9813");
		}

		/// <summary>
		/// Determines whether this tariff is used for Temporary Importation under Bond
		/// </summary>
		public bool IsTIBTariff
		{
			get { return IsTIB(UE_Tariff); }
		}

		public bool IsTIB110PercentTariff
		{
			get
			{
				return UE_Tariff.StartsWith("98130020") ||
					   UE_Tariff.StartsWith("98130025") ||
					   UE_Tariff.StartsWith("98130050");
			}
		}

		public bool DutyMightBeOverridable(ZDateTime effectiveDateForDutyRate)
		{
			return UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable &&
				!Applies(TariffRuleList.Codes.RepairTariffs, effectiveDateForDutyRate) &&
				!Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, effectiveDateForDutyRate);
		}

		internal bool HasLaceyActRequirement
		{
			get { return MayRequireLaceyAct || DoesRequireLaceyAct; }
		}

		internal bool DoesRequireLaceyAct
		{
			get { return false; }
		}

		internal bool MayRequireLaceyAct
		{
			get { return TariffChecker.HasSpecifiedCode("AL1", PGARequirements) || TariffChecker.HasSpecifiedCode("AL2", PGARequirements); }
		}

		public bool IsProvDutyAlwaysRequired
		{
			get
			{
				return UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem &&
				(
					UE_Tariff.StartsWith("990380", StringComparison.OrdinalIgnoreCase)
					|| UE_Tariff.StartsWith("990385", StringComparison.OrdinalIgnoreCase)
					|| UE_Tariff.StartsWith("990388", StringComparison.OrdinalIgnoreCase)
				);
			}
		}

		public ZBool IsEmbroideryTariff(ZDateTime effectiveDate) => USRefTariffDataLoader.IsEmbroideryTariff(Factory, UE_Tariff, effectiveDate);

		#region AMS Regulated Period

		internal ZString RegulatedPeriodForTariff(ZString programCode, ZDateTime effectiveDate)
		{
			return Factory.GetCachedValue("RegulatedPeriodForTariff" + UE_Tariff + programCode + effectiveDate.ToLongTimeString(), () =>
			{
				var result = ZString.Empty;
				if (!programCode.IsEmpty && !effectiveDate.IsEmpty)
				{
					var tariffCode = UE_Tariff;
					var yearForValidation = effectiveDate.Year;
					if (tariffCode.StartsWith("0805100040", StringComparison.OrdinalIgnoreCase))
					{
						var regulatedPeriodRequired = false;
						if (effectiveDate >= new ZDateTime(yearForValidation, 07, 01) && effectiveDate <= new ZDateTime(yearForValidation, 08, 31))
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO1 || programCode == AMSProgramList.Codes.MO6;
						}
						else
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO7;
						}

						if (regulatedPeriodRequired)
						{
							result = "1/Sep through 30/Jun";
						}
					}
					else if (tariffCode.StartsWith("080610", StringComparison.OrdinalIgnoreCase))
					{
						var regulatedPeriodRequired = false;
						if (!(effectiveDate >= new ZDateTime(yearForValidation, 04, 10) && effectiveDate <= new ZDateTime(yearForValidation, 07, 10)))
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO1 || programCode == AMSProgramList.Codes.MO6;
						}
						else
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO7;
						}

						if (regulatedPeriodRequired)
						{
							result = "10/Apr through 10/Jul";
						}
					}
					else if (tariffCode.StartsWith("0702002099", StringComparison.OrdinalIgnoreCase) || tariffCode.StartsWith("0702004098", StringComparison.OrdinalIgnoreCase) || tariffCode.StartsWith("0702006099", StringComparison.OrdinalIgnoreCase))
					{
						var regulatedPeriodRequired = false;
						if (effectiveDate >= new ZDateTime(yearForValidation, 06, 16) && effectiveDate <= new ZDateTime(yearForValidation, 10, 9))
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO1 || programCode == AMSProgramList.Codes.MO6;
						}
						else
						{
							regulatedPeriodRequired = programCode == AMSProgramList.Codes.MO7;
						}

						if (regulatedPeriodRequired)
						{
							result = "10/Oct through 15/Jun";
						}
					}
				}
				return result;
			});
		}

		#endregion
		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				DutyRates.RemoveAndDeleteAll();
				TariffDateRestrictions.DeleteAll();
				var query = new ZQuery(USCTariffValueSchema.UA_UE, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				Factory.Load<USCTariffValue>(query).DeleteAll();
				query = new ZQuery(USCTariffQuantitySchema.UQ_UE, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				Factory.Load<USCTariffQuantity>(query).DeleteAll();
			}
			base.Delete();
		}

		[BusinessObjectTestExclude]
		public ZString UE_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(UE_Tariff); }
		}

		public ZPropertyInfo UE_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.UE_FormattedTariff, x => UE_TariffInfo); }
		}

		TariffFormatter TariffFormatter
		{
			get { return new Loader(Factory).TariffFormatter; }
		}

		#region Duty Rates

		public USCTariffDutyRateCollection DutyRates
		{
			get
			{
				if (dutyRates == null)
				{
					dutyRates = new USCTariffDutyRateCollection(this, Factory);
					dutyRates.Load();
				}
				return dutyRates;
			}
		}
		USCTariffDutyRateCollection dutyRates;

		#endregion

		#region Proxied Properties

		#region TariffValue

		public bool HasHTSValueRestriction
		{
			get
			{
				return UE_ValueEditCode.StartsWith(ValueQuantityBoundValidator.ValueEditType.HTSRestriction) && UE_ValueEditCode.Length == 3;
			}
		}

		public ZString UE_ValueEditCode
		{
			get
			{
				var tariffValue = TariffValue;
				return tariffValue == null ? ZString.Empty : tariffValue.UA_ValueEditCode;
			}
		}

		public ZPropertyInfo UE_ValueEditCodeInfo
		{
			get { return GetZPropertyInfo(Schema.UE_ValueEditCode); }
		}

		public ZDecimal UE_ValueLowBounds
		{
			get
			{
				var tariffValue = TariffValue;
				return tariffValue == null ? ZDecimal.Zero : tariffValue.UA_ValueLowBounds;
			}
		}

		public ZPropertyInfo UE_ValueLowBoundsInfo
		{
			get { return GetZPropertyInfo(Schema.UE_ValueLowBounds); }
		}

		public ZDecimal UE_ValueHighBounds
		{
			get
			{
				var tariffValue = TariffValue;
				return tariffValue == null ? ZDecimal.Zero : tariffValue.UA_ValueHighBounds;
			}
		}

		public ZPropertyInfo UE_ValueHighBoundsInfo
		{
			get { return GetZPropertyInfo(Schema.UE_ValueHighBounds); }
		}

		public USCTariffValue GetOrCreateNewTariffValue()
		{
			var result = TariffValue;
			if (result == null)
			{
				result = Factory.New<USCTariffValue>();
				result.UA_UE = PK;
			}
			return result;
		}

		USCTariffValue TariffValue
		{
			get
			{
				ZQuery query = new ZQuery(USCTariffValueSchema.UA_UE, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.LoadTop1<USCTariffValue>(query);
			}
		}

		#endregion

		#region TariffDateRestriction

		public bool ConformToDateRestriction(ZDateTime date)
		{
			bool result = true;

			if (TariffDateRestrictions.Count > 0)
			{
				USCTariffDateRestriction restriction = TariffDateRestrictions.GetRestrictionFor(date);
				result = restriction != null;
			}

			return result;
		}

		[ChildEditable(true)]
		public USCTariffDateRestrictionCollection TariffDateRestrictions
		{
			get
			{
				if (fTariffDateRestrictions == null)
				{
					fTariffDateRestrictions = new USCTariffDateRestrictionCollection(this);
					RegisterEditableChildObject(fTariffDateRestrictions);
				}
				return fTariffDateRestrictions;
			}
		}
		USCTariffDateRestrictionCollection fTariffDateRestrictions;

		#endregion

		#region TariffQuantity

		public ZString UE_QuantityEditCode
		{
			get
			{
				var tariffQuantity = TariffQuantity;
				return tariffQuantity == null ? ZString.Empty : tariffQuantity.UQ_QuantityEditCode;
			}
		}

		public ZPropertyInfo UE_QuantityEditCodeInfo
		{
			get { return GetZPropertyInfo(Schema.UE_QuantityEditCode); }
		}

		public ZDecimal UE_QuantityEditLowerBound
		{
			get
			{
				var tariffQuantity = TariffQuantity;
				return tariffQuantity == null ? ZDecimal.Zero : tariffQuantity.UQ_LowerBound;
			}
		}

		public ZPropertyInfo UE_QuantityEditLowerBoundInfo
		{
			get { return GetZPropertyInfo(Schema.UE_QuantityEditLowerBound); }
		}

		public ZDecimal UE_QuantityEditUpperBound
		{
			get
			{
				var tariffQuantity = TariffQuantity;
				return tariffQuantity == null ? ZDecimal.Zero : tariffQuantity.UQ_UpperBound;
			}
		}

		public ZPropertyInfo UE_QuantityEditUpperBoundInfo
		{
			get { return GetZPropertyInfo(Schema.UE_QuantityEditUpperBound); }
		}

		public USCTariffQuantity GetOrCreateNewTariffQuantity()
		{
			var result = TariffQuantity;
			if (result == null)
			{
				result = Factory.New<USCTariffQuantity>();
				result.UQ_UE = PK;
			}
			return result;
		}

		USCTariffQuantity TariffQuantity
		{
			get
			{
				var query = new ZQuery(USCTariffQuantitySchema.UQ_UE, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.LoadTop1<USCTariffQuantity>(query);
			}
		}

		#endregion

		#region Tariff Permit Desc

		public ZString MiscLicenseTypeLabel
		{
			get
			{
				ZString result = "Misc. License No.:";

				switch (UE_PermitLicenseIndicator)
				{
					case MiscellaneousPermitLicenseList.Codes.SteelImportLicense:
						result = "Steel License No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.SingaporeTPLCertificate:
						result = "SG TPL License No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.CANAFTATPLCertificate:
						result = "CA NAFTA Cert. No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.MXNAFTATPLCertificate:
						result = "MX NAFTA Cert. No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.BeefExportCertificate:
						result = "Beef Certificate No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.DiamondCertificate:
						result = "Diamond Cert. No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.ATPDEACertificateHTS98211119:
						result = "ATPDEA Cert No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.AustraliaFreeTradeExportCertificate:
						result = "AU FT Export Cert.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense:
						result = "MX Cement Imp. Lic.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.CAFTATPLCertificate:
						result = "NI CAFTA TPL Cert.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber:
						result = "Lumber Permit No.:";
						break;
					case MiscellaneousPermitLicenseList.Codes.CottonShirtingFabricLicenseNumber:
						result = "Cotton Shirting Lic.:";
						break;
					default:
						break;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Loader

		public new class Loader : Customs.Business.ReferenceFileLoader<USCTariff>
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new TariffFormatter TariffFormatter
			{
				get { return (TariffFormatter)base.TariffFormatter; }
			}

			protected override SchemaColumn CodeSchema
			{
				get { return USCTariffSchema.UE_Tariff; }
			}

			protected override SchemaColumn DateFromSchema
			{
				get { return USCTariffSchema.UE_DateFrom; }
			}

			protected override SchemaColumn DateToSchema
			{
				get { return USCTariffSchema.UE_DateTo; }
			}

			public USCTariff[] LoadUniqueStartsWith(ZString tariffNumber, ZDateTime effectiveDutyDate)
			{
				ZQuery query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, TariffFormatter.Format(tariffNumber));
				query.AddToFilter(DateFromSchema, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDutyDate);
				query.AddToFilter(DateToSchema, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDutyDate);
				query.OrderBy = USCTariffSchema.UE_Tariff.Name;

				Dictionary<ZString, USCTariff> tariffs = new Dictionary<ZString, USCTariff>();
				foreach (USCTariff tariff in Factory.Load<USCTariff>(query))
				{
					USCTariff matchingTariff;
					if (!tariffs.TryGetValue(tariff.UE_Tariff, out matchingTariff))
					{
						tariffs[tariff.UE_Tariff] = tariff;
					}
					else
					{
						if (matchingTariff.UE_DateFrom < tariff.UE_DateFrom)
						{
							tariffs[tariff.UE_Tariff] = tariff;
						}
					}
				}

				List<USCTariff> sortedTariffs = new List<USCTariff>(tariffs.Values);
				sortedTariffs.Sort((t1, t2) => string.Compare(t1.UE_Tariff, t2.UE_Tariff));
				return sortedTariffs.ToArray();
			}

			public USCTariff[] LoadCurrentlyValidTariffs(ZString tariffNumber, ZDateTime assessmentDate)
			{
				ZQuery query = new ZQuery(USCTariffSchema.UE_Tariff, TariffFormatter.Format(tariffNumber));
				query.AddToFilter(DateFromSchema, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assessmentDate);
				query.AddToFilter(DateToSchema, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assessmentDate);
				query.OrderBy = DateFromSchema.Name + " desc";

				return Factory.Load<USCTariff>(query);
			}

			public USCTariff LoadCachedBestMatch(ZString tariffNumber, ZDateTime assessmentDate)
			{
				var formattedTariff = TariffFormatter.Format(tariffNumber);
				return Factory.GetCachedValue(formattedTariff + assessmentDate.ToString(), () => LoadBestMatch(formattedTariff, assessmentDate));
			}

			public USCTariff LoadBestMatchOrMostRecent(ZString tariffNumber, ZDateTime assessmentDate)
			{
				var result = LoadCachedBestMatch(tariffNumber, assessmentDate);
				if (result == null)
				{
					var formattedTariff = TariffFormatter.Format(tariffNumber);
					result = Factory.GetCachedValue(formattedTariff, () =>
					{
						var query = new ZQuery(USCTariffSchema.UE_Tariff, formattedTariff);
						query.OrderBy = USCTariffSchema.Constants.UE_DateTo + " desc";
						return Factory.LoadTop1<USCTariff>(query);
					});
				}
				return result;
			}

			protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
			{
				return new TariffFormatter();
			}
		}

		#endregion

		#region ITariff Members

		ZString ITariff.Code
		{
			get { return UE_Tariff; }
			set { UE_Tariff = value; }
		}

		ZString ITariff.Unit1
		{
			get { return UE_Unit1; }
			set { UE_Unit1 = value; }
		}

		ZString ITariff.Unit2
		{
			get { return UE_Unit2; }
			set { UE_Unit2 = value; }
		}

		ZString ITariff.Unit3
		{
			get { return UE_Unit3; }
			set { UE_Unit3 = value; }
		}

		ZString ITariff.ShortDescription
		{
			get { return UE_ShortDescription; }
			set { UE_ShortDescription = value; }
		}

		public IReadOnlyList<ZString> OGARequirements
		{
			get { return UE_OGACodes.Split(3); }
		}

		public IReadOnlyList<ZString> PGARequirements
		{
			get { return UE_PGACodes.Split(3); }
		}

		#endregion

		#region Enterprise.Customs.Universal.ITariff Members
		ZString Universal.ITariff.Code => UE_Tariff;
		ZString Universal.ITariff.Description => UE_ShortDescription;
		ZString Universal.ITariff.UQ1 => UE_Unit1;
		ZString Universal.ITariff.UQ2 => UE_Unit2;
		ZString Universal.ITariff.UQ3 => UE_Unit3;
		ZString Universal.ITariff.UQ4 => ZString.Empty;
		ZString Universal.ITariff.UQ5 => ZString.Empty;
		#endregion

		public void CheckTaxApply(ZPropertyInfo propertyInfo, ZString selectedTaxCode)
		{
			bool isTaxApplicable = false;
			bool isTaxRequired = false;
			bool mustBeOverriddenForUnknownFormula = false;
			bool mustBeOverriddenForDiffTaxCode = false;

			isTaxApplicable |= this.IsTaxApplicable;
			isTaxRequired |= this.IsTaxRequired;
			mustBeOverriddenForUnknownFormula |= this.IsTaxComputationUnknown && this.IsTaxRequired;
			mustBeOverriddenForDiffTaxCode |= !selectedTaxCode.IsEmpty && !this.GetRelatedFeeCodes().Contains(selectedTaxCode);

			ZString value = (ZString)propertyInfo.Value;

			if (isTaxRequired)
			{
				if (value == ZString.Empty || value == TaxApplyList.Codes.No)
				{
					propertyInfo.AddMessageError(TaxIsRequired);
				}
			}
			else if (isTaxApplicable)
			{
				if (value == ZString.Empty)
				{
					propertyInfo.AddMessageError(TaxApplicabilityMustBeSelected);
				}
			}
			else if (!isTaxApplicable)
			{
				if (value == TaxApplyList.Codes.Yes || value == TaxApplyList.Codes.Override)
				{
					propertyInfo.AddMessageError(TaxIsNotApplicable);
				}
			}

			if (value != TaxApplyList.Codes.Override)
			{
				if (mustBeOverriddenForUnknownFormula)
				{
					propertyInfo.AddMessageError(TaxRateMustBeOverridenForUnknownFormula);
				}
				else if (mustBeOverriddenForDiffTaxCode)
				{
					propertyInfo.AddMessageError(TaxRateMustBeOverridenForDiffTaxCode);
				}
			}
		}
		public const string TaxRateMustBeOverridenForDiffTaxCode = "A Tax code for which there is no published Tax Rate has been selected. Select 'O' override and enter the Tax Rate";
		public const string TaxRateMustBeOverridenForUnknownFormula = "The Tax calculation formula is not published by Customs. Select 'O' override and enter the Tax Rate";
		public const string TaxIsRequired = "Tax is mandatory for the selected tariff.";
		public const string TaxIsNotApplicable = "Tax is not applicable for the selected tariff.";
		public const string TaxApplicabilityMustBeSelected = "Please indicate whether Tax is applicable or not.";

		public static CodeDescriptionPairList GetTaxCodeList(USCTariff importTariff, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(GetCachedKeyForTaxCodeList(importTariff),
				delegate
				{
					var result = new CodeDescriptionPairList();

					if (importTariff != null)
					{
						IEnumerable<ZString> feeCodes = importTariff.GetRelatedFeeCodes();

						if (feeCodes.Contains(Core.Constants.USCustoms.FeeCodes.DistilledSpirits) || feeCodes.Contains(Core.Constants.USCustoms.FeeCodes.Wines))
						{
							AddTaxCodeIfDoesNotContain(result, Core.Constants.USCustoms.FeeCodes.DistilledSpirits, factory);
							AddTaxCodeIfDoesNotContain(result, Core.Constants.USCustoms.FeeCodes.Wines, factory);
						}
						foreach (ZString feeCode in feeCodes)
						{
							AddTaxCodeIfDoesNotContain(result, feeCode, factory);
						}
					}

					if (result.Count == 0)
					{
						foreach (string code in CusFeeCodeConstants.GetTaxCodes())
						{
							result.AddPair(code, CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory).GetDescriptionFromCode(code));
						}
					}

					return result;
				});
		}

		public static CodeDescriptionPairList GetTaxRateList(USCTariff importTariff, ZString taxCode, ZString rateType, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>(GetCachedKeyForTaxRateList(importTariff, taxCode, rateType), delegate
			{
				var result = taxCode.IsEmpty ? new AppendixBTaxRateList() : new AppendixBTaxRateList(taxCode);

				if (importTariff != null && importTariff.IsTaxApplicable)
				{
					var code = AppendixBTaxRateList.GetNormalTaxRateString(importTariff, taxCode, rateType);

					if (!string.IsNullOrWhiteSpace(code) && !result.ContainsCode(code) && code != USCTariffExtensionMethods.SelectTaxRateType)
					{
						var pair = new CodeDescriptionPair(code, "As published in HTS fiels");
						result.Insert(0, pair);
					}
				}

				return result;
			});
		}

		static string GetCachedKeyForTaxCodeList(USCTariff importTariff)
		{
			return (importTariff != null ? importTariff.PK.ToStringKey() : "USDefault") + "TaxCode";
		}

		static void AddTaxCodeIfDoesNotContain(CodeDescriptionPairList list, string taxCode, BusinessObjectFactory factory)
		{
			if (!list.ContainsCode(taxCode) && CusFeeCodeConstants.IsExciseTax(taxCode))
			{
				list.AddPair(taxCode, CusFeeCodeConstants.GetAccountingClassFeeCodeList(factory).GetDescriptionFromCode(taxCode));
			}
		}

		static string GetCachedKeyForTaxRateList(USCTariff importTariff, ZString taxCode, ZString rateType)
		{
			return (importTariff != null ? importTariff.PK.ToStringKey() : string.Empty) + taxCode + rateType + "TaxRate";
		}
	}
}
