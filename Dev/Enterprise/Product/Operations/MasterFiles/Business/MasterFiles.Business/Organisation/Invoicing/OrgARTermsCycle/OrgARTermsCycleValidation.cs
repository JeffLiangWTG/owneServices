//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgARTermsCycleValidation
//
//    This class should be used for overriding validation in AutoOrgARTermsCycleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;

	public class OrgARTermsCycleValidation : AutoOrgARTermsCycleValidation
	{
		public OrgARTermsCycleValidation(AutoOrgARTermsCycle parent) : base(parent)
		{
		}

		protected override void CheckP5_ToDay()
		{
			base.CheckP5_ToDay();

			if (!OrgARTermsCycleValidationHelper.IsValidCalendarDay(Parent.P5_ToDay))
			{
				Parent.P5_ToDayInfo.AddError(Res.GetString("978f010f-8021-4454-a99d-c56f40b55720", "The value must be between 1 and 31."));
			}
			if (Parent.ARTerms != null)
			{
				if (Parent.ARTerms.ARTermsCycles.Find((termsCycle) => termsCycle.P5_ToDay == Parent.P5_ToDay && termsCycle.PK != Parent.PK).Any())
				{
					Parent.P5_ToDayInfo.AddError(Res.GetString("963C7FC7-5474-4426-AF12-0BBFA157A03F", "Terms cycle with the same 'To Day' value already exists."));
				}
			}
		}

		protected override void CheckP5_PaymentDay()
		{
			base.CheckP5_PaymentDay();

			if (!OrgARTermsCycleValidationHelper.IsValidCalendarDay(Parent.P5_PaymentDay))
			{
				Parent.P5_PaymentDayInfo.AddError(Res.GetString("25731183-921F-4BA8-BA65-C98735910E1B", "The value must be between 1 and 31."));
			}
		}
	}
}
