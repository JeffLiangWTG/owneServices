using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	[CodeProperty(Schema.CPR_ValueFrom)]
	public class CusAuthorisationRule : CommonCusPermitRule
	{
		public CusAuthorisationRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusAuthorisationRuleLookups.RuleCodeList))]
		[ResourceStringData("66621D89-F7B2-4E7B-87F6-A199366E8C34", Caption = "Rule Code")]
		public override ZString CPR_RuleCode
		{
			get => base.CPR_RuleCode;
			set
			{
				var oldValue = CPR_RuleCode;
				base.CPR_RuleCode = value;
				if (!IsCopying && oldValue != CPR_RuleCode)
				{
					CPR_ValueFrom = GetDefaultValueBasedOnType();
					if (!AllowLinkedRules)
					{
						LinkedCusAuthorisationRules.DeleteAll();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusAuthorisationRuleLookups.ValueList))]
		[ResourceStringData("EB5FEE26-85A6-40F1-B392-BDB5B2C889F6", Caption = "Value")]
		[MaxLength(nameof(CPR_ValueFromMaxLength))]
		public override ZString CPR_ValueFrom
		{
			get => base.CPR_ValueFrom;
			set
			{
				var oldValue = CPR_ValueFrom;
				base.CPR_ValueFrom = value;
				if (!IsCopying && oldValue != CPR_ValueFrom)
				{
					CPR_Description = GetDefaultDescription();
					AuthorisationHeader.Provider.AddOrUpdateLinkedRule(this);
				}
			}
		}

		public ZString CPR_ValueFromFieldType => AuthorisationHeader.Provider.GetRuleValueFromFieldType(this);

		public ZBool CPR_ValueFromIsCodeField => CPR_ValueFromFieldType == nameof(FieldType.Boolean) || CPR_ValueFromFieldType == nameof(FieldType.TextCodeFindBox) || CPR_ValueFromFieldType == nameof(FieldType.TextDropEdit);

		[List(nameof(Lookups) + "." + nameof(CusAuthorisationRuleLookups.DescriptionList))]
		[ResourceStringData("41C8623D-0E1C-4838-9349-AE71AC93D22B", Caption = "Description")]
		public override ZString CPR_Description
		{
			get => base.CPR_Description;
			set
			{
				var oldValue = CPR_Description;
				base.CPR_Description = value;
				if (!IsCopying && oldValue != CPR_Description)
				{
					AuthorisationHeader.Provider.AddOrUpdateLinkedRule(this);
				}
			}
		}

		public ZString CPR_DescriptionFieldType => AuthorisationHeader.Provider.GetRuleDescriptionFieldType(this);

		[RelatedBusinessObject(nameof(AuthorisationHeader))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get => base.CPR_CPH_PermitHeader;
			set => base.CPR_CPH_PermitHeader = value;
		}

		public override ZGuid CPR_CPR_Rule => ZGuid.Empty;

		[ChildEditable]
		public LinkedCusAuthorisationRuleCollection LinkedCusAuthorisationRules
		{
			get
			{
				if (linkedCusAuthorisationRules == null)
				{
					linkedCusAuthorisationRules = GetLinkedCusAuthorisationRules();
					RegisterEditableChildObject(linkedCusAuthorisationRules);
				}
				return linkedCusAuthorisationRules;
			}
		}
		LinkedCusAuthorisationRuleCollection linkedCusAuthorisationRules;

		public override void Delete()
		{
			LinkedCusAuthorisationRules.DeleteAll();
			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("347acd72-0530-48c0-b5eb-6f1b9db560d7", "Authorization Rule");

		public CusAuthorisationHeader AuthorisationHeader => Factory.Load<CusAuthorisationHeader>(CPR_CPH_PermitHeader);

		public new CusAuthorisationRuleLookups Lookups => (CusAuthorisationRuleLookups)base.Lookups;

		public bool AllowLinkedRules => AuthorisationHeader.Provider.RuleCodesWithLinkedRules(this).Contains(CPR_RuleCode);

		protected sealed override CusPermitRuleLookups GetNewLookups() => AuthorisationHeader.Provider.GetNewLookups(this);

		public new CusAuthorisationRuleValidation Validation => (CusAuthorisationRuleValidation)base.Validation;

		protected sealed override CusPermitRuleValidation GetNewValidation() => AuthorisationHeader?.Provider.GetNewValidation(this) ?? new CusAuthorisationRuleValidation(this);

		int CPR_ValueFromMaxLength => AuthorisationHeader?.Provider.GetRuleValueFromMaxLength(this) ?? CusAuthorisationRule.Schema.CPR_ValueFromMaxLength;

		ZString GetDefaultDescription() => AuthorisationHeader.Provider.GetRuleDescription(this);

		ZString GetDefaultValueBasedOnType()
		{
			return CPR_ValueFromFieldType == nameof(FieldType.Boolean) ? (ZString)YesNoList.Codes.No : ZString.Empty;
		}

		LinkedCusAuthorisationRuleCollection GetLinkedCusAuthorisationRules() => AuthorisationHeader?.Provider.GetLinkedCusAuthorisationRules(this) ?? new LinkedCusAuthorisationRuleCollection(this);
	}
}
