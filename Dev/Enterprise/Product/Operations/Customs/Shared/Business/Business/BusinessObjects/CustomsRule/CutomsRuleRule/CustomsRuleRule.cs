using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleRule : CommonCusPermitRule
	{
		public static readonly CustomsRuleRuleTypeDecider TypeDecider = new CustomsRuleRuleTypeDecider();

		public CustomsRuleRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			valuesExtension = ReGenerateValuesExtension();
		}

		#region Override

		[ResourceStringData("6DEDF334-D700-4EC5-B7D5-21EFEF5713B0", Caption = "Rule Code")]
		[List(nameof(Lookups) + "." + nameof(CustomsRuleRuleLookups.RuleCodes))]
		public override ZString CPR_RuleCode
		{
			get => base.CPR_RuleCode;
			set
			{
				var oldValue = base.CPR_RuleCode;
				if (oldValue != value && !IsCopying)
				{
					base.CPR_RuleCode = value;
					valuesExtension = ReGenerateValuesExtension();
					RestCPR_Values();
				}
			}
		}

		[ResourceStringData("8CB29D22-88DE-48F2-BECA-774A3FB10E1B", Caption = "Rule Code Description")]
		public ZString RuleCodeDescription => Lookups.RuleCodes.GetDescriptionFromCode(CPR_RuleCode);

		[ResourceStringData("E47A2EC1-950B-410C-BC6D-0573A9377FA9", Caption = "Value From")]
		[List(nameof(Lookups) + "." + nameof(CustomsRuleRuleLookups.ValueFromCodes))]
		[ReadOnlyMember(nameof(CPR_ValueFrom_ReadOnly))]
		public override ZString CPR_ValueFrom
		{
			get => base.CPR_ValueFrom;
			set => base.CPR_ValueFrom = value;
		}

		[ResourceStringData("3488CA3F-13A7-4F86-969F-80AECB7790C1", Caption = "Value To")]
		[ReadOnlyMember(nameof(CPR_ValueTo_ReadOnly))]
		public override ZString CPR_ValueTo
		{
			get => base.CPR_ValueTo;
			set => base.CPR_ValueTo = value;
		}

		[RelatedBusinessObject(nameof(CustomsRuleRule.CustomsRule))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get => base.CPR_CPH_PermitHeader;
			set => base.CPR_CPH_PermitHeader = value;
		}

		public virtual ZString CPR_ValueFrom_FieldType => valuesExtension.ValueType.ToString();

		public virtual bool CPR_ValueFrom_ReadOnly => valuesExtension.FromReadonly;

		public virtual ZString CPR_ValueTo_FieldType => valuesExtension.ValueType.ToString();

		public virtual bool CPR_ValueTo_ReadOnly => valuesExtension.ToReadonly;

		public virtual bool UnUseCPR_ValueFrom => valuesExtension.FromReadonly && FieldType.Decimal.Equals(valuesExtension.ValueType);
		#endregion

		public CustomsRule CustomsRule => Factory.Load<CustomsRule>(CPR_CPH_PermitHeader);

		public new CustomsRuleRuleLookups Lookups => (CustomsRuleRuleLookups)base.Lookups;

		protected override CusPermitRuleValidation GetNewValidation()
		{
			return new CustomsRuleRuleValidation(this);
		}

		protected override CusPermitRuleLookups GetNewLookups()
		{
			return new CustomsRuleRuleLookups(this);
		}

		void RestCPR_Values()
		{
			if (FieldType.Decimal.Equals(valuesExtension.ValueType))
			{
				CPR_ValueFrom = ZDecimal.Zero.ToString();
				CPR_ValueTo = ZString.Empty;
			}
			else
			{
				CPR_ValueFrom = ZString.Empty;
				CPR_ValueTo = ZString.Empty;
			}
		}

		#region RuleCodeValues Extension Properties
		protected struct RuleCodeValuesExtension
		{
			public bool FromReadonly;
			public bool ToReadonly;
			public FieldType ValueType;
		}

		RuleCodeValuesExtension valuesExtension;

		protected virtual RuleCodeValuesExtension ReGenerateValuesExtension()
		{
			switch (CPR_RuleCode)
			{
				case CustomsRuleRuleCodeList.Codes.PaymentType:
					return new RuleCodeValuesExtension
					{
						ValueType = FieldType.TextDropEdit,
						FromReadonly = false,
						ToReadonly = true
					};
				case CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement:
				case CustomsRuleRuleCodeList.Codes.TotalCustomsValue:
				case CustomsRuleRuleCodeList.Codes.TotalDuty:
					return new RuleCodeValuesExtension
					{
						ValueType = FieldType.Decimal,
						FromReadonly = true,
						ToReadonly = false
					};
				case CustomsRuleRuleCodeList.Codes.TariffNumber:
					return new RuleCodeValuesExtension
					{
						ValueType = FieldType.Text,
						FromReadonly = false,
						ToReadonly = false
					};
				default:
					return GenerateDefaultValuesExtension();
			}
		}

		protected static RuleCodeValuesExtension GenerateDefaultValuesExtension()
		{
			return new RuleCodeValuesExtension
			{
				ValueType = FieldType.Text,
				FromReadonly = false,
				ToReadonly = false
			};
		}
		#endregion
	}
}
