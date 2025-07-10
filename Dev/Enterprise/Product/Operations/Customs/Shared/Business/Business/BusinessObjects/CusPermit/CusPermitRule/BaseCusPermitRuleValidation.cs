using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseCusPermitRuleValidation : SharedCusPermitRuleValidation
	{
		public BaseCusPermitRuleValidation(BaseCusPermitRule parent) : base(parent)
		{
		}

		protected new BaseCusPermitRule Parent => (BaseCusPermitRule)base.Parent;

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();
			var targetInfo = Parent.CPR_RuleCodeInfo;
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			if (Parent.CPR_ValueFromIsCodeField)
			{
				ListValidation.ErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
			}
		}

		protected override void CheckCPR_ValueTo()
		{
			var valueTo = Parent.CPR_ValueTo;
			var valueFrom = Parent.CPR_ValueFrom;
			if (!Parent.IsSingleValueRule && !valueTo.IsEmpty && Parent.ValueComparer.Compare(valueTo, valueFrom) < 0)
			{
				Parent.CPR_ValueToInfo.AddError(ValueToMustBeGreaterThanOrEqualValueFrom);
			}
		}

		internal static string ValueToMustBeGreaterThanOrEqualValueFrom
		{
			get { return Res.GetString("D92EAEB1-E1B7-4C7D-A49A-82790D61187A", "'Value To' must be greater than or equal to 'Value From'."); }
		}
	}
}
