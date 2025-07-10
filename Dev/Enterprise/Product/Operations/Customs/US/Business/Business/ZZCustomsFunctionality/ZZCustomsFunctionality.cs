using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class ZZCustomsFunctionality
	{
		public static bool IsEnableFWSEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.EnableFWS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsFWSEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.PGAFWS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsCargRlsCESEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.CargRlsCES, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
#if DEBUG
		public static IDisposable TemporarilySetupFWSEffective(bool isEffective = true) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.PGAFWS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, isEffective);
#endif

		public static ZBool IsNewFTZe214Valid => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.NewFTZe214, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static ZBool FTZNoLengthCouleBe9 => !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.FTZZoneID9, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsAMSHBREffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool GBIACTIVE => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool USFTAReconIndIsValid => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.USFTARECONIND, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool PostalCodeIsRequiredForChinaMF => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsAESJurisdictionNumberEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.AESJurisdictionNumber, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsAluminumSmeltEffective(ZDateTime effectiveDate) => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.USSmelt, Core.Constants.CountryCodes.UnitedStates, effectiveDate);

		public static bool IsNMFSCOAACTIVEEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NMFSCOAACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsNewEntrySummaryQueryEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsEDA86Effective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.EDA86, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsAPHIS2024Effective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.APHIS2024, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsCPSCEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsOMCEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsPGADataCorrection2ndPhaseffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.PGADataCorrection2ndPhase, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public static bool IsSanctionsEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
	}
}


