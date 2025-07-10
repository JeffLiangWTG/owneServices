using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DomesticTaxExemptionDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{

		public static void UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration)
		{
			var rate = new RefCusRate();
			rate.ZZ2_StartDate = tariff.ZZ1_StartDate;
			rate.ZZ2_EndDate = tariff.ZZ1_EndDate;
			rate.ZZ2_RateFormula = $"VFD";
			tariff.RefCusRates = new[] { rate };

			foreach (var attribute in tariff.RefCusTariffAttributes)
			{
				var ClassificationCodeValue = string.Empty;
				switch (attribute.ZZ3_Value)
				{
					case ClassificationCodeValues._1:
						ClassificationCodeValue = CodeListAttributeValues.SpecialConsumptionTax;
						break;
					case ClassificationCodeValues._2:
						ClassificationCodeValue = CodeListAttributeValues.LiquorTax;
						break;
					case ClassificationCodeValues._3:
						ClassificationCodeValue = CodeListAttributeValues.TransportationTax;
						break;
					case ClassificationCodeValues._4:
						ClassificationCodeValue = CodeListAttributeValues.VAT;
						break;
				}

				if (!string.IsNullOrEmpty(ClassificationCodeValue))
				{
					attribute.ZZ3_Value = ClassificationCodeValue;
				}
			}
		}

		public static void UpdateRule(RefCusTariff tariff, Rule rule)
		{
			if (rule.Name == RuleID.ExemptionCodes)
			{
				if (rule.Relationship == Relationship.OR.ToString())
				{
					var matchedRule = rule.RuleValues.FirstOrDefault(x => x.Value == tariff.ZZ1_TariffCode);
					if (matchedRule != null)
					{
						if (!string.IsNullOrEmpty(matchedRule.QuantityUnit))
						{
							tariff.RefCusTariffUOMs = new[] { new RefCusTariffUOM { ZZ8_UOM = matchedRule.QuantityUnit } };
							tariff.RefCusRates[0].ZZ2_RateFormula = $"[{matchedRule.QuantityUnit}] * {matchedRule.TaxReductionAmountPerUnit}";
						}
					}
				}
			}

			if (rule.Name == RuleID.InvolvesInstallationCost)
			{
				if (rule.Relationship == Relationship.OR.ToString())
				{
					var matchedRule = rule.RuleValues.FirstOrDefault(x => x.Value == tariff.ZZ1_TariffCode);
					if (matchedRule != null)
					{
						var newAttribute = new RefCusTariffAttribute
						{
							ZZ3_Name = AttributeValues.InvolvesInstallationCost,
							ZZ3_Value = Constants.YesNo.Yes
						};
						if (tariff.RefCusTariffAttributes != null)
						{
							tariff.RefCusTariffAttributes = tariff.RefCusTariffAttributes.Append(newAttribute).ToArray();
						}
						else
						{
							tariff.RefCusTariffAttributes = new[] { newAttribute };
						}
					}
				}
			}
		}

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration) => true;

		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration) => UpdateAdditionally(tariff, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff tariff, Rule rule) => UpdateRule(tariff, rule);
		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }

		class RuleID
		{
			public const string ExemptionCodes = "Exemption Codes";
			public const string InvolvesInstallationCost = "Involves Installation Cost";
		}
		static class ClassificationCodeValues
		{
			public const string _1 = "1";
			public const string _2 = "2";
			public const string _3 = "3";
			public const string _4 = "4";
		}
		static class AttributeValues
		{
			public const string InvolvesInstallationCost = "InvolvesInstallationCost";
		}
	}
}
