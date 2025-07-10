using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(ExamSurveyQuestionImporterForm))]
	class ExamSurveyQuestionImporterForm_Test : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (ExamSurveyQuestionImporterForm form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				AssertEquals("Import Questions", form.FormHeading);
			}
		}

		public void TestDownloadTemplate()
		{
			using (var form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				form.Show();
				var tempFileName = Env.GetTempFileName();
				try
				{
					form.DownloadTemplateDialogShowing += (sender, e) =>
					{
						AssertEquals("Comma delimited (*.csv)|*.csv", e.DownloadTemplateDialog.Filter);
						e.DownloadTemplateDialog.FileName = tempFileName;
						e.DialogResultOverride = DialogResult.OK;
					};
					form.DownloadTemplateButton.PerformClick();
					Assert("Should have created file", File.Exists(tempFileName));
					AssertEquals(FormattableString.Invariant($"Template has been saved to {tempFileName}"), UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					using (File.OpenWrite(tempFileName))
					{
						form.DownloadTemplateButton.PerformClick();
						AssertContains("Cannot write the file to the disk.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		public void TestCsvFileBrowseButton_Click()
		{
			using (ExamSurveyQuestionImporterForm form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = "meh.csv";
				form.BrowseButton.PerformClick();
				AssertEquals("meh.csv", form.BusinessEntity.FileLocation);
			}
		}

		public void TestXmlFileBrowseButton_Click()
		{
			using (ExamSurveyQuestionImporterForm form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = "import.xml";
				form.BrowseButton.PerformClick();
				AssertEquals("import.xml", form.BusinessEntity.FileLocation);
			}
		}

		public void TestCsvFileImportButton_Click()
		{
			using (ExamSurveyQuestionImporterForm form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				form.Show();
				form.ImportButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				string fileName = Env.GetTempFileName("", "csv");
				try
				{
					using (StreamWriter writer = File.CreateText(fileName))
					{
						writer.Write("Test Question");
					}

					form.BusinessEntity.FileLocation = fileName;
					form.BusinessEntity.StartingRowIndex = 1;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("1 questions successfully imported.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					File.Delete(fileName);
				}
			}
		}

		public void TestXmlFileImportButton_Click()
		{
			using (ExamSurveyQuestionImporterForm form = (ExamSurveyQuestionImporterForm)GetFormToBash())
			{
				form.Show();
				form.ImportButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				string fileName = Env.GetTempFileName("", "xml");
				try
				{
					using (StreamWriter writer = File.CreateText(fileName))
					{
						writer.Write(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
								<GlbCompanyCampaign>
								<VoteExamSurveyQuestionCollection>
									<VoteExamSurveyQuestion>
										<QuestionOrder>1</QuestionOrder>
										<Question>Question 1:</Question>
										<AnswerType>MUL</AnswerType>
										<IsRandomisable>Y</IsRandomisable>
										<IsValid>Y</IsValid>
										<IsActive>Y</IsActive>
										<OptionalAnswerExplanation>Question 1 Explanation</OptionalAnswerExplanation>
										<AnswerCollection>
											<VoteExamSurveyQuestion>
												<QuestionOrder>1</QuestionOrder>
												<SubQuestionOrder>1</SubQuestionOrder>
												<Question>Option A</Question>
												<AnswerType>MCP</AnswerType>
												<ExamCorrectAnswer>Y</ExamCorrectAnswer>
												<IsRandomisable>Y</IsRandomisable>
												<IsValid>Y</IsValid>
												<IsActive>Y</IsActive>
											</VoteExamSurveyQuestion>
											<VoteExamSurveyQuestion>
												<QuestionOrder>1</QuestionOrder>
												<SubQuestionOrder>2</SubQuestionOrder>
												<Question>Option B</Question>
												<AnswerType>MCP</AnswerType>
												<ExamCorrectAnswer>N</ExamCorrectAnswer>
												<IsRandomisable>Y</IsRandomisable>
												<IsValid>Y</IsValid>
												<IsActive>Y</IsActive>
											</VoteExamSurveyQuestion>
											<VoteExamSurveyQuestion>
												<QuestionOrder>1</QuestionOrder>
												<SubQuestionOrder>3</SubQuestionOrder>
												<Question>Option C</Question>
												<AnswerType>MCP</AnswerType>
												<ExamCorrectAnswer>N</ExamCorrectAnswer>
												<IsRandomisable>Y</IsRandomisable>
												<IsValid>Y</IsValid>
												<IsActive>Y</IsActive>
											</VoteExamSurveyQuestion>
											<VoteExamSurveyQuestion>
												<QuestionOrder>1</QuestionOrder>
												<SubQuestionOrder>4</SubQuestionOrder>
												<Question>Option D</Question>
												<AnswerType>MCP</AnswerType>
												<ExamCorrectAnswer>N</ExamCorrectAnswer>
												<IsRandomisable>Y</IsRandomisable>
												<IsValid>Y</IsValid>
												<IsActive>Y</IsActive>
											</VoteExamSurveyQuestion>
										</AnswerCollection>
									</VoteExamSurveyQuestion>
								</VoteExamSurveyQuestionCollection>
								</GlbCompanyCampaign>");
					}

					form.BusinessEntity.FileLocation = fileName;
					form.ImportButton.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("1 questions successfully imported.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					File.Delete(fileName);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			ExamSurveyQuestionImporterBizO bizO = new ExamSurveyQuestionImporterBizO(campaign, new FileMapper());
			return new ExamSurveyQuestionImporterForm(bizO);
		}
	}
}
