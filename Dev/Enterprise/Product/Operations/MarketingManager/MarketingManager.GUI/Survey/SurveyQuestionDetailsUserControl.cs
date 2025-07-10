using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SurveyQuestionDetailsUserControl : QuestionDetailsUserControl
	{
		public SurveyQuestionDetailsUserControl()
		{
			InitializeComponent();
			this.VisibilityRelationshipProvider.SetDependency(this.MaxCalcEdit, this.MinCalcEdit);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetupControlsVisibility();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				HookAnswerTypeValueChangedHandler(false);
			}
			base.OnCurrentDataItemChanged(e);
			SetupControlsVisibility();
			if (CurrentDataItem != null)
			{
				HookAnswerTypeValueChangedHandler(true);
			}
		}

		void HookAnswerTypeValueChangedHandler(bool hook)
		{
			if (hook)
			{
				CurrentDataItem.HY_AnswerTypeInfo.ValueChanged += HandleAnswerTypeValueChanged;
			}
			else
			{
				CurrentDataItem.HY_AnswerTypeInfo.ValueChanged -= HandleAnswerTypeValueChanged;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void HandleAnswerTypeValueChanged(object sender, EventArgs e)
		{
			Application.DoEvents();
			SetupControlsVisibility();
		}

		protected virtual void SetupControlsVisibility()
		{
			MinCalcEdit.Visible = false;
			OptionsBox.Visible = false;
			rowLayoutPanel1.Visible = true;

			VoteExamSurveyQuestion question = CurrentDataItem;
			if (question != null)
			{
				switch (question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
						MinCalcEdit.Visible = true;
						break;

					case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
						OptionsBox.Visible = true;
						rowLayoutPanel1.Visible = false;
						break;
				}
			}
		}

		new VoteExamSurveyQuestion CurrentDataItem
		{
			get { return (VoteExamSurveyQuestion)base.CurrentDataItem; }
		}
	}
}
