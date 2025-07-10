using System;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Recruiter.Business
{
	class XmlExamQuestionsImporter : ExamQuestionsImporter
	{
		public XmlExamQuestionsImporter(ExamSurveyQuestionImporterBizO importerBizO)
			: base(importerBizO)
		{
		}

		protected override int ImportQuestionsCore(INotifications notificationSubscriber)
		{
			int result = 0;
			using (var fileStream = importerBizO.OpenFile())
			{
				try
				{
					XPathDocument xpathDoc = new XPathDocument(fileStream);
					XPathNavigator nav = xpathDoc.CreateNavigator();
					XPathNodeIterator nodes = nav.Select(string.Format("/{0}/{1}/{2}",
															XmlImportQuestionNodeName.GlbCompanyCampaign,
															XmlImportQuestionNodeName.VoteExamSurveyQuestionCollection,
															XmlImportQuestionNodeName.VoteExamSurveyQuestion));

					short questionOrder = (short)(importerBizO.campaign.Questions.Count + 1);
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(importerBizO.campaign.Factory))
					{
						while (nodes.MoveNext())
						{
							CreateOneQuestion(questionOrder++, nodes.Current);
							result++;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("f508bf27-b7ff-411a-b688-ef9b6b561ce4", "The XML file contains invalid content: {0}", ex.Message)));
				}
			}

			return result;
		}

		void CreateOneQuestion(short questionOrder, XPathNavigator nav)
		{
			LearningCentreQuestion question = AddNewQuestion(importerBizO.campaign.Questions, nav);
			question.HY_QuestionOrder = questionOrder;

			ZByte correctAnswersCount = 0;
			XPathNodeIterator nodes = nav.Select(XmlImportQuestionNodeName.VoteExamSurveyQuestion);
			while (nodes.MoveNext())
			{
				LearningCentreQuestion option = AddNewQuestion(question.SubQuestions, nodes.Current);
				if (option.CorrectAnswerAsBool)
				{
					correctAnswersCount++;
				}
			}

			if (correctAnswersCount > 1)
			{
				question.HY_Max = correctAnswersCount;
			}
		}

		LearningCentreQuestion AddNewQuestion(VoteExamSurveyQuestionSet questionSet, XPathNavigator nav)
		{
			LearningCentreQuestion question = (LearningCentreQuestion)questionSet.AddNew();
			nav.MoveToFirstChild();

			do
			{
				switch (nav.Name)
				{
					case XmlImportQuestionNodeName.Question:
						question.SetQuestionTextAndEncode(nav.Value);
						question.HY_Question = ReplaceSpecialCharactersInQuestionText(question.HY_Question);
						break;
					case XmlImportQuestionNodeName.AnswerType:
						question.HY_AnswerType = nav.Value;
						break;
					case XmlImportQuestionNodeName.IsRandomisable:
						question.HY_IsRandomisable = new ZBool(nav.Value);
						break;
					case XmlImportQuestionNodeName.IsActive:
						question.HY_IsActive = new ZBool(nav.Value);
						break;
					case XmlImportQuestionNodeName.OptionalAnswerExplanation:
						question.HY_OptionalAnswerExplanation = ((ZString)nav.Value).Left(AutoVoteExamSurveyQuestion.Schema.HY_OptionalAnswerExplanationMaxLength);
						break;
					case XmlImportQuestionNodeName.ExamCorrectAnswer:
						question.CorrectAnswerAsBool = new ZBool(nav.Value);
						break;
					case XmlImportQuestionNodeName.Comment:
						question.HY_Comment = ((ZString)nav.Value).Left(AutoVoteExamSurveyQuestion.Schema.HY_CommentMaxLength);
						break;
				}
			} while (nav.MoveToNext());

			return question;
		}

		ZString ReplaceSpecialCharactersInQuestionText(string htmlString)
		{
			ZString questionText = htmlString.TrimStart('\n', '\r').TrimEnd('\n', '\r');

			questionText = new CodeFormatterForWeb().HighlightCodeBlock(questionText);

			questionText = questionText.Replace("[b]", "<b>");
			questionText = questionText.Replace("[/b]", "</b>");
			questionText = questionText.Replace("[i]", "<i>");
			questionText = questionText.Replace("[/i]", "</i>");

			return questionText;
		}

		static class XmlImportQuestionNodeName
		{
			public const string GlbCompanyCampaign = "GlbCompanyCampaign";
			public const string VoteExamSurveyQuestionCollection = "VoteExamSurveyQuestionCollection";
			public const string VoteExamSurveyQuestion = "VoteExamSurveyQuestion";
			public const string QuestionOrder = "QuestionOrder";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is a XML node name.")]
			public const string Question = "Question";
			public const string AnswerType = "AnswerType";
			public const string IsRandomisable = "IsRandomisable";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is a XML node name.")]
			public const string Comment = "Comment";
			public const string IsValid = "IsValid";
			public const string IsActive = "IsActive";
			public const string OptionalAnswerExplanation = "OptionalAnswerExplanation";
			public const string AnswerCollection = "AnswerCollection";
			public const string SubQuestionOrder = "SubQuestionOrder";
			public const string ExamCorrectAnswer = "ExamCorrectAnswer";
		}
	}
}
