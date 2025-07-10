//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationAttemptValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationAttemptValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptValidation : AutoGlbAccreditationAttemptValidation
	{
		public GlbAccreditationAttemptValidation(AutoGlbAccreditationAttempt parent) : base(parent)
		{
		}

		protected override void CheckHAA_CompletionDueDate()
		{
			base.CheckHAA_CompletionDueDate();
			if (Parent.HAA_CommencementDate.IsValid && Parent.HAA_CompletionDueDate.IsValid && (!Parent.IsInDatabase || Parent.HAA_CompletionDueDateInfo.HasChanges))
			{
				if (Parent.HAA_CompletionDueDate < Parent.HAA_CommencementDate)
				{
					Parent.HAA_CompletionDueDateInfo.AddError(Res.GetString("1f17ac4f-97ae-416d-a539-aa0bc779e0c0", "Completion Due Date Cannot be earlier than Commencement Date."));
				}
			}
		}
	}
}
