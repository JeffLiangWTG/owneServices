using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(LearningCentreCampaignForm))]
	class LearningCentreCampaignFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (LearningCentreCampaignForm form = (LearningCentreCampaignForm)GetFormToBash())
			{
				form.BusinessEntity.G0_CampaignID = "MEH001";
				AssertEquals("Learning Center MEH001", form.FormCaption);
			}
		}

		public void TestImportQuestions()
		{
			using (LearningCentreCampaignForm form = (LearningCentreCampaignForm)GetFormToBash())
			{
				form.Show();
				try
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					form.importQuestionMenuItemForTest.PerformClick();
					AssertEquals(form.DataSource, ((ExamSurveyQuestionImporterBizO)(ZFormModaliser.LastFormShownDialogForTest as ExamSurveyQuestionImporterForm).LastDataSourceForTest).campaign);
				}
				finally
				{
					if (ZFormModaliser.LastFormShownDialogForTest != null)
					{
						ZFormModaliser.LastFormShownDialogForTest.Dispose();
					}
				}
			}
		}

		public void TestPlugIns()
		{
			using (LearningCentreCampaignForm form = (LearningCentreCampaignForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.LearningCentreExamPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.LearningCentreScaledTestPlugIn));
			}
		}

		protected override Form GetFormToBashCore()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new LearningCentreCampaignForm(campaign);
		}
	}
}
