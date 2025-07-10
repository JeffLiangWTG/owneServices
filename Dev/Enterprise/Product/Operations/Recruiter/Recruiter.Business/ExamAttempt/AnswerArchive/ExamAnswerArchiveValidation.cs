namespace Enterprise.Recruiter.Business
{
	public class ExamAnswerArchiveValidation : AutoExamAnswerArchiveValidation
	{
		public ExamAnswerArchiveValidation(AutoExamAnswerArchive parent)
			: base(parent) { }

		#region Implementation

		public new ExamAnswerArchive Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ExamAnswerArchive)base.Parent; }
		}

		#endregion
	}
}
