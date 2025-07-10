using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class VoteExamSurveyUserControl : ZUserControl
	{
		public VoteExamSurveyUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			InitialiseQuestionAndResultControls();
			base.SetDataBinding(dataSource, dataMember);
		}

		void InitialiseQuestionAndResultControls()
		{
			if (!hasQuestionAndResultsControlBeenInitialised)
			{
				QuestionsSplitContainer.Panel1.Controls.Add(QuestionsControl);
				QuestionsControl.CurrentQuestionsTabPageChanged += new EventHandler(QuestionsControl_CurrentQuestionsTabPageChanged);
				QuestionsControl.Dock = DockStyle.Fill;

				QuestionsSplitContainer.Panel2.Controls.Add(QuestionDetailsControl);
				BindingSource.SetBindingMember(QuestionDetailsControl, QuestionsControl.CurrentGridBindingMember);
				QuestionDetailsControl.Dock = DockStyle.Fill;

				if (ResultsControlOverride == null)
				{
					ResultsByRecipientsTabPage.Controls.Add(ResultsByRecipientControl);
					ResultsByRecipientControl.Dock = DockStyle.Fill;

					ResultsByItemsTabPage.Controls.Add(ResultsByQuestionControl);
					ResultsByQuestionControl.Dock = DockStyle.Fill;

					ResultsSummaryTabPage.Controls.Add(ResultsSummaryControl);
					ResultsSummaryControl.Dock = DockStyle.Fill;
				}
				else
				{
					foreach (Control control in ResultsTabPage.Controls)
					{
						control.Dispose();
					}
					ResultsTabPage.Controls.Clear();
					ResultsTabPage.Controls.Add(ResultsControlOverride);
					ResultsControlOverride.Dock = DockStyle.Fill;
				}
				hasQuestionAndResultsControlBeenInitialised = true;
			}
		}

		bool hasQuestionAndResultsControlBeenInitialised;

		void QuestionsControl_CurrentQuestionsTabPageChanged(object sender, EventArgs e)
		{
			QuestionsSplitContainer.Panel2Collapsed = !QuestionsControl.IsQuestionsTabPageSelected;
			BindingSource.SetBindingMember(QuestionDetailsControl, QuestionsControl.CurrentGridBindingMember);
			((CargoWise.EntityFramework.IBusiness)QuestionDetailsControl.BindingSource.Current).RefreshBindingIncludingChildren();
		}

		public QuestionsUserControl QuestionsControl { get; set; }
		public QuestionDetailsUserControl QuestionDetailsControl { get; set; }
		public ResultsByRecipientUserControl ResultsByRecipientControl { get; set; }
		public ResultsByQuestionUserControl ResultsByQuestionControl { get; set; }
		public ZUserControl ResultsSummaryControl { get; set; }
		public Control ResultsControlOverride { get; set; }
	}
}
