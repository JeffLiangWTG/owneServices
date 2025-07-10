using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Recruiter.GUI
{
	public partial class ExamResultsByQuestionUserControl : ResultsByQuestionUserControl, IExamResultsControl
	{
		public ExamResultsByQuestionUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			this.UnhookColourDecidingEventHandler();
			base.SetDataBinding(dataSource, dataMember);
			this.HookColourDecidingEventHandler();
		}

		#region IExamResultsControl Members

		ZGrid IExamResultsControl.AnswersGrid
		{
			get { return ResultItemAnswersGrid; }
		}

		#endregion
	}
}
