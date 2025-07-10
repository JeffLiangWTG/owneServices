using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class QuestionCategoryValidation : AutoQuestionCategoryValidation
	{
		public QuestionCategoryValidation(AutoQuestionCategory parent)
			: base(parent) { }

		protected override void CheckCode()
		{
			base.CheckCode();
			MandatoryValidation.CheckEntered(Parent.CodeInfo);
		}

		protected override void CheckDescription()
		{
			base.CheckDescription();
			MandatoryValidation.CheckEntered(Parent.DescriptionInfo);
		}

		#region Implementation

		public new QuestionCategory Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (QuestionCategory)base.Parent; }
		}

		#endregion
	}
}
