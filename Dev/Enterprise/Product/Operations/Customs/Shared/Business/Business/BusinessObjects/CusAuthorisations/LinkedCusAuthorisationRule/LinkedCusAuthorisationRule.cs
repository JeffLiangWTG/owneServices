using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	[SystemDefinedValues]
	public class LinkedCusAuthorisationRule : CommonCusPermitRule
	{
		public LinkedCusAuthorisationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnlyMember(nameof(CPR_RuleCode_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(LinkedCusAuthorisationRuleLookups.RuleCodeList))]
		[ResourceStringData("6F3E6093-6C8B-4F7F-9A2F-51AEB2DA40AA", Caption = "Rule Code")]
		public override ZString CPR_RuleCode
		{
			get => base.CPR_RuleCode;
			set => base.CPR_RuleCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(LinkedCusAuthorisationRuleLookups.ValueList))]
		[ResourceStringData("5C3A2A34-B1F7-4FB4-8D97-6831078BBA74", Caption = "Value")]
		public override ZString CPR_ValueFrom
		{
			get => base.CPR_ValueFrom;
			set
			{
				bool valueHasChanged = CPR_ValueFrom != value;
				base.CPR_ValueFrom = value;
				if (!IsCopying && valueHasChanged && CPR_Description.IsEmpty)
				{
					CPR_Description = GetDefaultDescription();
				}
			}
		}

		[ResourceStringData("EFF1F6C8-54BA-4CE4-A7AD-4E7A7FB06ABD", Caption = "Description")]
		public override ZString CPR_Description
		{
			get => base.CPR_Description;
			set => base.CPR_Description = value;
		}

		[RelatedBusinessObject(nameof(AuthorisationHeader))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get => base.CPR_CPH_PermitHeader;
			set => base.CPR_CPH_PermitHeader = value;
		}

		[RelatedBusinessObject(nameof(AuthorisationRule))]
		public override ZGuid CPR_CPR_Rule
		{
			get => base.CPR_CPR_Rule;
			set => base.CPR_CPR_Rule = value;
		}

		public ZBool IsMasterLinkedRule
		{
			get => this.GetSystemDefinedValue<ZBool>(nameof(IsMasterLinkedRule));
			set
			{
				var oldValue = IsMasterLinkedRule;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(nameof(IsMasterLinkedRule), value);
					IsMasterLinkedRuleInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo IsMasterLinkedRuleInfo => GetZPropertyInfo(nameof(IsMasterLinkedRule));

		public CusAuthorisationHeader AuthorisationHeader => Factory.Load<CusAuthorisationHeader>(CPR_CPH_PermitHeader);

		public CusAuthorisationRule AuthorisationRule => Factory.Load<CusAuthorisationRule>(CPR_CPR_Rule);

		public new LinkedCusAuthorisationRuleLookups Lookups => (LinkedCusAuthorisationRuleLookups)base.Lookups;

		protected override CusPermitRuleLookups GetNewLookups() => AuthorisationHeader?.Provider.GetNewLookups(this) ?? new LinkedCusAuthorisationRuleLookups(this);

		public new LinkedCusAuthorisationRuleValidation Validation => (LinkedCusAuthorisationRuleValidation)base.Validation;

		protected override CusPermitRuleValidation GetNewValidation() => AuthorisationHeader?.Provider.GetNewValidation(this) ?? new LinkedCusAuthorisationRuleValidation(this);

		ZString GetDefaultDescription() => AuthorisationHeader?.Provider.GetLinkedRuleDescription(this) ?? ZString.Empty;

		public ZString CPR_ValueFromFieldType => AuthorisationHeader?.Provider.GetLinkedRuleValueFromFieldType(this) ?? nameof(FieldType.Text);
		public ZBool CPR_ValueFromIsCodeField => CPR_ValueFromFieldType == nameof(FieldType.Boolean) || CPR_ValueFromFieldType == nameof(FieldType.TextCodeFindBox) || CPR_ValueFromFieldType == nameof(FieldType.TextDropEdit);

		public bool CPR_RuleCode_ReadOnly => AuthorisationHeader?.Provider.IsLinkedRuleCodeReadOnly(this) ?? true;

		public override bool ReadOnly => AuthorisationHeader?.Provider.IsLinkedRuleReadOnly(this) ?? false;
	}
}
