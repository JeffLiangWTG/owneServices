//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPermitRuleExceptionValidation
//
//    This class should be used for overriding validation in AutoCusPermitRuleExceptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusPermitRuleExceptionValidation : AutoCusPermitRuleExceptionValidation
	{
		public CusPermitRuleExceptionValidation(AutoCusPermitRuleException parent) : base(parent)
		{
		}

		protected new BaseCusPermitRuleException Parent
		{
			get { return (BaseCusPermitRuleException)base.Parent; }
		}

		#region Message

		internal static string ExceptionRangeMustBeContainedInRuleRequirement
		{
			get { return Res.GetString("593B0845-43BE-41DC-8044-3B9DFA7D4CF2", "The Exception value range must be contained within the Rule requirement value"); }
		}

		#endregion

		protected override void CheckCPE_ValueFrom()
		{
			var parentFrom = Parent.PermitRule?.CPR_ValueFrom ?? ZString.Empty;
			var parentTo = Parent.PermitRule?.CPR_ValueTo ?? ZString.Empty;
			var valueFrom = Parent.CPE_ValueFrom;
			var targetInfo = Parent.CPE_ValueFromInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (!valueFrom.IsEmpty)
			{
				if ((!parentFrom.IsEmpty && Parent.ValueComparer.Compare(parentFrom, valueFrom) > 0) || (!parentTo.IsEmpty && Parent.ValueComparer.Compare(valueFrom, parentTo) > 0))
				{
					targetInfo.AddError(ExceptionRangeMustBeContainedInRuleRequirement);
				}
			}
		}

		protected override void CheckCPE_ValueTo()
		{
			var valueTo = Parent.CPE_ValueTo;
			if (!valueTo.IsEmpty)
			{
				var valueFrom = Parent.CPE_ValueFrom;
				var parentTo = Parent.PermitRule?.CPR_ValueTo ?? ZString.Empty;
				var comparer = Parent.ValueComparer;
				if (!valueFrom.IsEmpty && comparer.Compare(valueTo, valueFrom) < 0)
				{
					Parent.CPE_ValueToInfo.AddError(BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
				}
				if (!parentTo.IsEmpty && comparer.Compare(parentTo, valueTo) < 0)
				{
					Parent.CPE_ValueToInfo.AddError(ExceptionRangeMustBeContainedInRuleRequirement);
				}
			}
		}
	}
}
