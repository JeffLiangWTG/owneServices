using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business
{
	public class CustomsRuleRule : Customs.Business.CustomsRuleRule, Integration.Customs.US.ICustomsRuleRule
	{
		public CustomsRuleRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CustomsRuleRuleLookups Lookups => (CustomsRuleRuleLookups)base.Lookups;

		protected override CusPermitRuleLookups GetNewLookups()
		{
			return new CustomsRuleRuleLookups(this);
		}

		protected override CusPermitRuleValidation GetNewValidation()
		{
			return new CustomsRuleRuleValidation(this);
		}

		protected override RuleCodeValuesExtension ReGenerateValuesExtension()
		{
			switch (CPR_RuleCode)
			{
				case CustomsRuleRuleCodeList.Codes.ADCEligible:
					return new RuleCodeValuesExtension
					{
						ValueType = FieldType.TextDropEdit,
						FromReadonly = false,
						ToReadonly = true
					};
				case CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount:
					return new RuleCodeValuesExtension
					{
						ValueType = FieldType.Decimal,
						FromReadonly = true,
						ToReadonly = false
					};
				default:
					return base.ReGenerateValuesExtension();
			}
		}
	}
}
