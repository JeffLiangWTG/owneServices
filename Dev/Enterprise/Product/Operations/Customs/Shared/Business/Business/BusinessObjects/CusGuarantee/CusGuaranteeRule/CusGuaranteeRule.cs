using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusGuaranteeHeader), "CusGuaranteeRules")]
	public class CusGuaranteeRule : SharedCusPermitRule, ICusGuaranteeRule
	{
		public CusGuaranteeRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new static readonly CusGuaranteeRuleTypeDecider TypeDecider = new CusGuaranteeRuleTypeDecider();
		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static CusGuaranteeRule LoadOrCreateMainAccessCode(BaseCusGuaranteeHeader header)
			{
				CusGuaranteeRule result = null;
				if (header != null)
				{
					result = LoadMainAccessCode(header);
					if (result == null)
					{
						result = CreateMainAccessCode(header);
					}
				}
				return result;
			}

			public static CusGuaranteeRule CreateMainAccessCode(BaseCusGuaranteeHeader header)
			{
				var result = header.Factory.New<CusGuaranteeRule>();
				result.CPR_CPH_PermitHeader = header.PK;
				result.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
				return result;
			}

			public static CusGuaranteeRule LoadMainAccessCode(BaseCusGuaranteeHeader header)
			{
				CusGuaranteeRule result = null;
				if (header != null)
				{
					var query = new ZQuery(CusPermitRuleSchema.CPR_CPH_PermitHeader, header.PK);
					query.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin);
					result = header.Factory.Load<CusGuaranteeRule>(query)
						.OrderBy(x => x.PK)
						.FirstOrDefault();
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusGuaranteeRule);
			}
		}

		#endregion

		#region Validation and Lookups

		public new CusGuaranteeRuleLookups Lookups => (CusGuaranteeRuleLookups)base.Lookups;

		protected override CusPermitRuleLookups GetNewLookups() => new CusGuaranteeRuleLookups(this);

		public new CusPermitRuleValidation Validation => base.Validation;

		protected override CusPermitRuleValidation GetNewValidation()
		{
			switch (CPR_RuleCode)
			{
				case PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin:
				case PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber:
					return AccessCodePinRuleValidation;
				default:
					return GuaranteeRuleValidation;
			}
		}

		protected virtual AccessCodePinRuleValidation AccessCodePinRuleValidation => new AccessCodePinRuleValidation(this);
		protected virtual CusGuaranteeRuleValidation GuaranteeRuleValidation => new CusGuaranteeRuleValidation(this);

		#endregion Validation and Lookups

		[RelatedBusinessObject(nameof(CusGuaranteeRule.GuaranteeHeader))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get { return base.CPR_CPH_PermitHeader; }
			set { base.CPR_CPH_PermitHeader = value; }
		}

		[Password(nameof(CPR_ValueFrom_IsPassword))]
		public override ZString CPR_ValueFrom
		{
			get => base.CPR_ValueFrom;
			set => base.CPR_ValueFrom = value;
		}

		protected virtual bool CPR_ValueFrom_IsPassword => CPR_RuleCode == PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin
			|| CPR_RuleCode == PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber
			|| CPR_RuleCode == ZString.Empty;

		protected override bool CPR_ValueTo_ReadOnly => base.CPR_ValueTo_ReadOnly
			|| CPR_RuleCode == PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin
			|| CPR_RuleCode == PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;

		public BaseCusGuaranteeHeader GuaranteeHeader => Factory.Load<BaseCusGuaranteeHeader>(CPR_CPH_PermitHeader);

		public override ZString ShortName => Res.GetString("6654BC24-96BC-440E-9D31-B3D4E957500B", "Guarantee Rule");

		public override void OnSaving()
		{
			if (IsDefaultAccessCodeDefaultPin && CPR_ValueFrom.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public ZBool IsDefaultAccessCodeDefaultPin => CPR_RuleCode == PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
	}
}
