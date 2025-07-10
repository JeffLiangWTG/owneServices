using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(BaseCusPermitHeader), "CusPermitRules")]
	public class BaseCusPermitRule : SharedCusPermitRule
	{
		public BaseCusPermitRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly CusPermitRuleTypeDecider TypeDecider = new CusPermitRuleTypeDecider();

		#region Override Properties

		[RelatedBusinessObject(nameof(BaseCusPermitRule.PermitHeader))]
		public override ZGuid CPR_CPH_PermitHeader
		{
			get { return base.CPR_CPH_PermitHeader; }
			set { base.CPR_CPH_PermitHeader = value; }
		}

		#endregion

		#region New Properties

		public new BaseCusPermitHeader PermitHeader => Factory.Load<BaseCusPermitHeader>(CPR_CPH_PermitHeader);

		#endregion

		#region Business Object Overrides

		public override ZString ShortName => Res.GetString("302DB3BE-D78A-473D-93A6-C8FC3B9FD6B5", "Permit Rule");

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPR_RuleCode = "ABC";
			CPR_ValueFrom = "DEF";
		}
#endif

		#region Validation and Lookups

		public new BaseCusPermitRuleLookups Lookups => (BaseCusPermitRuleLookups)base.Lookups;

		protected override CusPermitRuleLookups GetNewLookups() => new BaseCusPermitRuleLookups(this);

		public new BaseCusPermitRuleValidation Validation => (BaseCusPermitRuleValidation)base.Validation;

		protected override CusPermitRuleValidation GetNewValidation()
		{
			switch (CPR_RuleCode)
			{
				case RuleCodes.CountryOfOrigin:
					return new CountryOfOriginRuleValidation(this);
				default:
					return new BaseCusPermitRuleValidation(this);
			}
		}

		#endregion Validation and Lookups
	}
}
