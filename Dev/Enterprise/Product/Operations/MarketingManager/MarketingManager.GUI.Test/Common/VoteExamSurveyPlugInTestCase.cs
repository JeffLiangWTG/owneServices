using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public abstract class VoteExamSurveyPlugInTestCase : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			using (VoteExamSurveyPlugIn plugIn = GetPlugIn())
			using (VoteExamSurveyUserControl userControl = (VoteExamSurveyUserControl)plugIn.UserControl)
			{
				try
				{
					AssertControlType(ExpectedQuestionsControlType, userControl.QuestionsControl);
					AssertControlType(ExpectedQuestionDetailsControlType, userControl.QuestionDetailsControl);
					AssertControlType(ExpectedResultsByQuestionControlType, userControl.ResultsByQuestionControl);
					AssertControlType(ExpectedResultsByRecipientControlType, userControl.ResultsByRecipientControl);
					AssertControlType(ExpectedResultsControlOverrideType, userControl.ResultsControlOverride);
					AssertControlType(ExpectedResultsSummaryControlType, userControl.ResultsSummaryControl);
				}
				finally
				{
					if (userControl.QuestionsControl != null)
					{
						userControl.QuestionsControl.Dispose();
					}

					if (userControl.QuestionDetailsControl != null)
					{
						userControl.QuestionDetailsControl.Dispose();
					}

					if (userControl.ResultsByQuestionControl != null)
					{
						userControl.ResultsByQuestionControl.Dispose();
					}

					if (userControl.ResultsByRecipientControl != null)
					{
						userControl.ResultsByRecipientControl.Dispose();
					}

					if (userControl.ResultsControlOverride != null)
					{
						userControl.ResultsControlOverride.Dispose();
					}

					if (userControl.ResultsSummaryControl != null)
					{
						userControl.ResultsSummaryControl.Dispose();
					}
				}
			}
		}

		public void TestToggleEnabled()
		{
			using (VoteExamSurveyPlugIn plugIn = GetPlugIn())
			{
				TogglePlugInEnabled(true, plugIn.BusinessEntity);
				Assert(plugIn.Enabled);

				TogglePlugInEnabled(false, plugIn.BusinessEntity);
				Assert(!plugIn.Enabled);
			}
		}

		void AssertControlType(Type expectedType, Control control)
		{
			if (expectedType == null)
			{
				AssertNull(control);
			}
			else
			{
				AssertEquals(expectedType, control.GetType());
			}
		}

		protected virtual Type ExpectedQuestionsControlType { get { return typeof(QuestionsUserControl); } }
		protected virtual Type ExpectedQuestionDetailsControlType { get { return typeof(QuestionDetailsUserControl); } }
		protected virtual Type ExpectedResultsByRecipientControlType { get { return typeof(ResultsByRecipientUserControl); } }
		protected virtual Type ExpectedResultsByQuestionControlType { get { return typeof(ResultsByQuestionUserControl); } }
		protected virtual Type ExpectedResultsSummaryControlType { get { return typeof(ResultsSummaryUserControl); } }
		protected virtual Type ExpectedResultsControlOverrideType { get { return null; } }

		protected abstract VoteExamSurveyPlugIn GetPlugIn();
		protected abstract void TogglePlugInEnabled(bool enabled, GlbCompanyCampaign campaign);
	}
}
