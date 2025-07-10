using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class LearningCentreCampaignForm : ZTemplateForm
	{
		public LearningCentreCampaignForm(LearningCentreCampaign examCampaign)
			: base(examCampaign)
		{
			var menuItemImportQuestion = new ZMenuItem(ResString.GetMultilingualString("352B8EB6-2D58-411F-9404-F7D532DE69FB", "Import Questions"), ImportQuestionsHandler);
			ActionsMenuItem.MenuItems.Add(menuItemImportQuestion);

			var translateMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("D48DBADF-D75C-451A-900A-62AD366B7831", "Translate Text Elements"), OnTranslateTextElements);

			PlugIns.Add(ControllerIDs.LearningCentreExamPlugIn);
			PlugIns.Add(ControllerIDs.LearningCentreScaledTestPlugIn);

			if (Globals.IsTest)
			{
				importQuestionMenuItemForTest = menuItemImportQuestion;
			}
			BusinessEntity.LoadVoteExamSurveySummaries += HookProgressFormUpdate;
		}

		ProgressForm progressForm;

		public event EventHandler progressCancelled;

		void HookProgressFormUpdate(object sender, EventArgs e)
		{
			LearningCentreTestSummaryPerQuestionCollection summaries = (sender as LearningCentreTestSummaryPerQuestionCollection);
			if (summaries != null)
			{
				summaries.SummaryAnalyzeBegin += SummaryAnalyzeBegin;
				summaries.SummaryAnalyzeEnd += SummaryAnalyzeEnd;
				summaries.SummaryAnalyzedUpdate += SummaryAnalyzedUpdate;
				progressCancelled += summaries.CancelAnalyzeProgress;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void SummaryAnalyzeBegin(object sender, EventArgs e)
		{
			progressForm = new ProgressForm();
			progressForm.Status = Res.GetString("b9004b00-3ef0-4eea-88c1-78ff5dac19f6", "Analyzing the selected question...");
			progressForm.ShowCancelButton = true;
			progressForm.ShowProgressBar = true;
			progressForm.Cancelled += progressCancelled;
			progressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

		void SummaryAnalyzeEnd(object sender, EventArgs e)
		{
			progressForm.Close();
			progressForm.Dispose();
			LearningCentreTestSummaryPerQuestionCollection summaries = (sender as LearningCentreTestSummaryPerQuestionCollection);
			if (summaries != null)
			{
				summaries.SummaryAnalyzeBegin -= SummaryAnalyzeBegin;
				summaries.SummaryAnalyzeEnd -= SummaryAnalyzeEnd;
				summaries.SummaryAnalyzedUpdate -= SummaryAnalyzedUpdate;
				progressCancelled -= summaries.CancelAnalyzeProgress;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void SummaryAnalyzedUpdate(object sender, LearningCentreTestSummaryPerQuestionCollection.SummaryAnalyzedEventArgs e)
		{
			if (progressForm != null)
			{
				progressForm.Status = Res.GetString("d486f8e8-3368-4efc-99da-882a3bf3e76c", "Analyzing the answers ({0} of {1}).", e.current, e.total);
				progressForm.PercentComplete = (int)((e.current / (decimal)e.total) * 100m);
				Application.DoEvents();
			}
		}

		void OnTranslateTextElements(object sender, EventArgs e)
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(BusinessEntity.G0_CampaignNameInfo);
			infos.Add(BusinessEntity.G0_CampaignCommentInfo);

			infos.AddRange(BusinessEntity.Questions.Select(q => q.HY_QuestionInfo));
			infos.AddRange(BusinessEntity.Questions.SelectMany(quest => quest.SubQuestions).Select(q => q.HY_QuestionInfo));

			var source = new MultipleDataCaptionSource(infos.ToArray(), BusinessEntity.G0_CampaignNameMultilingual);

			ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(
				new CustomizableDataResourceStrings(source),
				null, DataSource);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			CampaignNameTextBox.Focus();
		}

		void ImportQuestionsHandler(object sender, EventArgs e)
		{
			ExamSurveyQuestionImporterBizO importer = new ExamSurveyQuestionImporterBizO(BusinessEntity, new FileMapper());
			ZFormModaliser.ShowDialogAndDispose(new ExamSurveyQuestionImporterForm(importer));
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}

		public new LearningCentreCampaign BusinessEntity
		{
			get { return (LearningCentreCampaign)base.BusinessEntity; }
		}

		internal MenuItem importQuestionMenuItemForTest;

		void zGrid1_DoubleClick(object sender, EventArgs e)
		{
		}

		[SuppressControlRequiresTextBasher]
		public class ZGroupBoxNoText : ZGroupBox
		{
		}
	}
}
