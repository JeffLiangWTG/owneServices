using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class ExportPGARequirementIndicator
	{
		public ExportPGARequirementIndicator(BusinessObjectFactory factory, Func<TariffView> getTariff, ZDateTime effectiveDate)
		{
			this.factory = factory;
			this.getTariff = getTariff;
			this.effectiveDate = effectiveDate;
		}
		readonly Func<TariffView> getTariff;
		readonly BusinessObjectFactory factory;
		readonly ZDateTime effectiveDate;

		public bool RequireAMS
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.AMS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireATF
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.ATF, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireFWS
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.FWS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireEPA
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireNMFS
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.NMFS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireTTB
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.TTB, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool RequireDEA
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory); }
		}

		public bool MayRequireEPA
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Optional); }
		}

		public bool MayRequireDEA
		{
			get { return PGARequirement(GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Optional); }
		}

		public bool HasEPARequirement
		{
			get { return RequireEPA || MayRequireEPA; }
		}

		bool PGARequirement(ZString conditionValueType, ZString conditionValue)
		{
			var effectiveValuationDate = effectiveDate.IsValid ? effectiveDate : ZDateTime.Today;
			if (getTariff() is TariffView tariff)
			{
				return factory.GetCachedValue($"PGARequirement|{tariff.ZZ1_TariffCode}|{conditionValueType}|{conditionValue}|{effectiveValuationDate}", () =>
				{
					var result = false;
					var criteria = new ZZConditionSelectionCriteria(effectiveValuationDate, ZString.Empty, ZString.Empty, null, ZString.Empty, Core.Constants.CountryCodes.UnitedStates, ConditionChecker.ConditionDirection.Export, RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.PGA);
					foreach (var condition in ConditionChecker.GetApplicableConditions(factory, tariff, criteria))
					{
						if (condition.ConditionValues.Any(x => x.ValueType == conditionValueType && x.ZX3_Value == conditionValue))
						{
							result = true;
							break;
						}
					}
					return result;
				});
			}
			return false;
		}
	}
}
