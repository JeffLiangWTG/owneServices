using System;
using System.Drawing;
using System.Threading;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class QuestionsUserControl : ZUserControl
	{
		public QuestionsUserControl()
		{
			InitializeComponent();

			QuestionsGrid.AfterBind += new EventHandler(QuestionsGrid_AfterBind);
			QuestionsGrid.FontDeciding += new EventHandler<FontDecidingEventArgs>(QuestionsGrid_FontDeciding);
			InactiveQuestionsGrid.FontDeciding += new EventHandler<FontDecidingEventArgs>(QuestionsGrid_FontDeciding);
		}

		void PreviewButton_Click(object sender, EventArgs e)
		{
			if (!DataSource.HasChanges)
			{
				if (DataSource.IsCampaignURLSettingsValid)
				{
					string url = VoteExamSurveyUrlHelper.GetCampaignPreviewUrl(DataSource);
					WebUrlLauncher.Launch(url);
				}
				else
				{
					Globals.Message.ShowError(UnsubscribeUrlHelper.GetUriFormatErrorMessage(DataSource.Company), Res.GetString("4adccb21-44f6-4b5e-b6c9-ab25bb58f2b8", "Cannot Preview Campaign"));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("d95073f9-690f-4f8e-9b51-86110cccd61f", "Please save before previewing this campaign"), Res.GetString("4adccb21-44f6-4b5e-b6c9-ab25bb58f2b8", "Cannot Preview Campaign"));
			}
		}

		new GlbCompanyCampaign DataSource
		{
			get { return (GlbCompanyCampaign)base.DataSource; }
		}

		#region Questions Grid

		void QuestionsGrid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			if (e.DataMember == VoteExamSurveyQuestionSchema.Constants.HY_Question)
			{
				VoteExamSurveyQuestion question = (VoteExamSurveyQuestion)e.ObjectAtRow;
				if (question.IsHeader)
				{
					e.Font = new Font(e.OriginalFont, FontStyle.Bold);
				}
			}
		}

		void QuestionsGrid_AfterBind(object sender, EventArgs e)
		{
			ZGridColumn gridColumn = QuestionsGrid.Columns[VoteExamSurveyQuestionSchema.Constants.HY_AnswerType];
			if (gridColumn != null)
			{
				ZDropEditColumnStyle columnStyle = gridColumn.ColumnStyle as ZDropEditColumnStyle;
				ZDropEdit dropEditControl = (ZDropEdit)columnStyle.EditControl;
				dropEditControl.TextChanged += delegate
				{
					SynchronizationContext.Current.Post(delegate
					{ QuestionsGrid.EndEdit(columnStyle, QuestionsGrid.CurrentRowIndex, false); }, null);
				};
			}

			QuestionsGrid.AfterBind -= QuestionsGrid_AfterBind;
		}

		#endregion

		#region CurrentQuestionsTabPageChanged

		public string CurrentGridBindingMember
		{
			get
			{
				ZGrid grid = (QuestionsTabControl.SelectedTab == ActiveQuestionsTabPage) ? QuestionsGrid : InactiveQuestionsGrid;
				return BindingSource.GetBindingMember(grid);
			}
		}

		public bool IsQuestionsTabPageSelected
		{
			get { return QuestionsTabControl.SelectedTab == ActiveQuestionsTabPage || QuestionsTabControl.SelectedTab == InactiveQuestionsTabPage; }
		}

		void QuestionsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (CurrentQuestionsTabPageChanged != null)
			{
				CurrentQuestionsTabPageChanged(sender, e);
			}
		}

		internal event EventHandler CurrentQuestionsTabPageChanged;

		#endregion
	}
}
