using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Recruiter.Business
{
	class CsvExamQuestionsImporter : ExamQuestionsImporter
	{
		public CsvExamQuestionsImporter(ExamSurveyQuestionImporterBizO importerBizO)
			: base(importerBizO)
		{
		}

		protected override int ImportQuestionsCore(INotifications notificationSubscriber)
		{
			int result = 0;

			using (StreamReader reader = new StreamReader(importerBizO.OpenFile()))
			{
				try
				{
					string csvLineRaw;
					int index = 0;
					while ((csvLineRaw = ImportWizard.GetNextLine(reader)) != null)
					{
						if (++index < importerBizO.StartingRowIndex)
						{
							if (index == 1)
							{
								CheckHeaderText(csvLineRaw);
							}

							continue;
						}

						if (ImportOneQuestion(csvLineRaw))
						{
							result++;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("213d3317-b359-433f-a456-ff6e7f108238", "The CSV file contains invalid content: {0}", ex.Message)));
				}
			}

			return result;
		}

		void CheckHeaderText(string csvLineRaw)
		{
			var csvLine = new OCsvLine(csvLineRaw);
			var errorMessage = new ZStringBuilder();

			if (csvLine.FieldValues[ImportQuestionColumnIndexes.QuestionText].Trim() != ImportQuestionColumnHeaders.QuestionText)
			{
				errorMessage.AppendLine(FormattableString.Invariant($"Header \"{csvLine.FieldValues[ImportQuestionColumnIndexes.QuestionText]}\" should read \"{ImportQuestionColumnHeaders.QuestionText}\"")); // Error message for English Template
			}
			if (csvLine.FieldValues.Length > 2)
			{
				if (csvLine.FieldValues[ImportQuestionColumnIndexes.CountryCode].Trim() != ImportQuestionColumnHeaders.CountryCode)
				{
					errorMessage.AppendLine(FormattableString.Invariant($"Header \"{csvLine.FieldValues[ImportQuestionColumnIndexes.CountryCode]}\" should read \"{ImportQuestionColumnHeaders.CountryCode}\"")); // Error message for English Template
				}

				if (csvLine.FieldValues[ImportQuestionColumnIndexes.IsRandomisable].Trim() != ImportQuestionColumnHeaders.IsRandomisable)
				{
					errorMessage.AppendLine(FormattableString.Invariant($"Header \"{csvLine.FieldValues[ImportQuestionColumnIndexes.IsRandomisable]}\" should read \"{ImportQuestionColumnHeaders.IsRandomisable}\"")); // Error message for English Template
				}

				if (csvLine.FieldValues.Length > 3)
				{
					if (csvLine.FieldValues[ImportQuestionColumnIndexes.Comment].Trim() != ImportQuestionColumnHeaders.Comment)
					{
						errorMessage.AppendLine(FormattableString.Invariant($"Header \"{csvLine.FieldValues[ImportQuestionColumnIndexes.Comment]}\" should read \"{ImportQuestionColumnHeaders.Comment}\"")); // Error message for English Template
					}

					if (csvLine.FieldValues.Length > 4)
					{
						if (csvLine.FieldValues[ImportQuestionColumnIndexes.CorrectAnswerIndex].Trim() != ImportQuestionColumnHeaders.CorrectAnswerIndex)
						{
							errorMessage.AppendLine(FormattableString.Invariant($"Header \"{csvLine.FieldValues[ImportQuestionColumnIndexes.CorrectAnswerIndex]}\" should read \"{ImportQuestionColumnHeaders.CorrectAnswerIndex}\"")); // Error message for English Template
						}
					}
				}
			}

			if (!errorMessage.IsEmpty)
			{
				throw new ArgumentException(
					FormattableString.Invariant($"This file has errors in its header text:\r\n{errorMessage.ToString()}\r\nChange these headers to match the specified text or use the template accessible from the Import Questions Form")); // Error message for English Template
			}
		}

		#region SuppressResourceStringsCheckRegion

		static class ImportQuestionColumnHeaders
		{
			public const string QuestionText = "Question";
			public const string CountryCode = "Country";
			public const string IsRandomisable = "Randomisable";
			public const string Comment = "Comment";
			public const string CorrectAnswerIndex = "Correct Answer";
		}

		#endregion

		static class ImportQuestionColumnIndexes
		{
			public const int QuestionText = 0;
			public const int CountryCode = 1;
			public const int IsRandomisable = 2;
			public const int Comment = 3;
			public const int CorrectAnswerIndex = 4;
			public const int StartOptionsIndex = 5;
		}

		bool ImportOneQuestion(string csvLineRaw)
		{
			bool result = false;

			OCsvLine csvLine = new OCsvLine(csvLineRaw);
			string questionText = (csvLine.FieldValues.Length > 0) ? csvLine.FieldValues[ImportQuestionColumnIndexes.QuestionText].Trim() : "";

			if (!string.IsNullOrEmpty(questionText))
			{
				result = true;
				VoteExamSurveyQuestion question = null;
				if (csvLine.FieldValues.Length > 2)
				{
					question = AddNewQuestion(VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, questionText);
					if (csvLine.FieldValues[ImportQuestionColumnIndexes.CountryCode].Length == 2)
					{
						question.HY_RN_NKCountryCode = csvLine.FieldValues[ImportQuestionColumnIndexes.CountryCode];
					}
					question.HY_IsRandomisable = !csvLine.FieldValues[ImportQuestionColumnIndexes.IsRandomisable].IsNullOrEmpty() ?
						new ZBool(csvLine.FieldValues[ImportQuestionColumnIndexes.IsRandomisable]) : ZBool.True;

					if (csvLine.FieldValues.Length > 3)
					{
						question.HY_Comment = ((ZString)csvLine.FieldValues[ImportQuestionColumnIndexes.Comment]).Left(AutoVoteExamSurveyQuestion.Schema.HY_CommentMaxLength);

						if (csvLine.FieldValues.Length > 4)
						{
							int correctAnswerIndex;
							var correctAnswerParseSucceeded =
								int.TryParse(csvLine.FieldValues[ImportQuestionColumnIndexes.CorrectAnswerIndex], out correctAnswerIndex);

							if (!correctAnswerParseSucceeded)
							{
								throw new ArgumentException(FormattableString.Invariant($"No correct answer has been supplied for question: {questionText}"));
							}

							for (int i = ImportQuestionColumnIndexes.StartOptionsIndex; i < csvLine.FieldValues.Length; i++)
							{
								string subQuestionText = csvLine.FieldValues[i].Trim();
								if (!string.IsNullOrEmpty(subQuestionText))
								{
									LearningCentreQuestion option = AddNewQuestion(question.SubQuestions,
										VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption, subQuestionText);
									option.CorrectAnswerAsBool = ((i - ImportQuestionColumnIndexes.StartOptionsIndex + 1) == correctAnswerIndex);
								}
							}
						}
					}
				}
				else
				{
					question = AddNewQuestion(VoteExamSurveyAnswerTypeList.Codes.Header, questionText);
				}

				question?.Validation.ValidateAll();
			}

			return result;
		}

		VoteExamSurveyQuestion AddNewQuestion(string type, string text)
		{
			return AddNewQuestion(importerBizO.campaign.Questions, type, text);
		}

		LearningCentreQuestion AddNewQuestion(VoteExamSurveyQuestionSet questionSet, string type, string text)
		{
			LearningCentreQuestion result = (LearningCentreQuestion)questionSet.AddNew();
			result.HY_AnswerType = type;
			result.HY_Question = text;
			return result;
		}
	}
}
