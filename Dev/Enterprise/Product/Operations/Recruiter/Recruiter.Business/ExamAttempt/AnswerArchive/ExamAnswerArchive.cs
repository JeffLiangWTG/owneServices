using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class ExamAnswerArchive : AutoExamAnswerArchive, ILearningCentreSubmittedAnswer
	{
		public ExamAnswerArchive(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new sealed class Schema : AutoExamAnswerArchive.Schema
		{
			Schema() { }
			public const string QuestionNumber = "QuestionNumber";
			public const string SelectedQuestions = "SelectedQuestions";
			public const string OrderedOptions = "OrderedOptions";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SelectedQuestionPKs = Array.Empty<ZGuid>();
			OrderedMultipleChoiceOptionPKs = Array.Empty<ZGuid>();
		}

		public override bool ReadOnly
		{
			get { return true; }
			set { }
		}

		#region Properties

		public VoteExamSurveyQuestion Question
		{
			get { return Factory.Load<LearningCentreQuestion>(QuestionPK); }
		}

		public ZString QuestionText
		{
			get
			{
				if (!QuestionAsString.IsEmpty)
				{
					return QuestionAsString;
				}
				else
				{
					var question = Question;
					return question != null ? question.HY_QuestionMultilingual : ZString.Empty;
				}
			}
		}

		public ZString QuestionActualOrder
		{
			get
			{
				if (QuestionActualNumber != 0)
				{
					return QuestionActualNumber.ToString().PadLeft(4, ' ');
				}
				else
				{
					var question = Question;
					return question != null ? question.ActualOrderForBinding.PadLeft(4, ' ') : ZString.Empty;
				}
			}
		}

		public ZString Answer
		{
			get { return this.GetAnswer(); }
		}

		public ZPropertyInfo AnswerInfo
		{
			get { return GetZPropertyInfo(nameof(Answer)); }
		}

		public ZString AnswerFieldType
		{
			get { return this.GetAnswerFieldType(); }
		}

		public ZPropertyInfo AnswerFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AnswerFieldType)); }
		}

		public ZInt QuestionNumber
		{
			get;
			private set;
		}

		public IEnumerable<ZGuid> SelectedQuestionPKs
		{
			get;
			private set;
		}

		public IEnumerable<ZGuid> OrderedMultipleChoiceOptionPKs
		{
			get;
			private set;
		}

		public ZBool IsAnsweredIncorrectly
		{
			get { return !IsAnsweredCorrectly && this.IsPopulated(); }
		}

		public ZBool IsPopulated
		{
			get { return this.IsPopulated(); }
		}

		public ZString ResultAsText
		{
			get { return this.GetResultAsText(); }
		}

		#region IVoteExamSurveySubmittedAnswer Members

		ZString IVoteExamSurveySubmittedAnswer.PersistedAnswer
		{
			get { return AnswerAsString; }
		}

		public IEnumerable<VoteExamSurveyQuestion> SelectedMultipleChoiceOptions
		{
			get { return SelectedQuestionPKs.Select(pk => Factory.Load<VoteExamSurveyQuestion>(pk)); }
		}

		#endregion

		#endregion

		#region XML Serialization / Deserialization

		protected override void WriteQuestionNumber(XmlWriter writer)
		{
			writer.WriteString(QuestionNumber.ToString());
		}

		protected override void ReadQuestionNumber(XmlReader reader)
		{
			if (reader.IsStartElement("QuestionNumber"))
			{
				QuestionNumber = ZInt.ParseSafe(reader.ReadElementString("QuestionNumber"), 0);
			}
		}

		protected override void WriteSelectedQuestions(XmlWriter writer)
		{
			if (SelectedQuestionPKs != null)
			{
				string selectedQuestions = string.Join(",", Array.ConvertAll(SelectedQuestionPKs.ToArray(), (s) => s.ToString()));
				writer.WriteString(selectedQuestions);
			}
		}

		protected override void ReadSelectedQuestions(XmlReader reader)
		{
			string[] selectedOptionGuids = reader.ReadElementString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			SelectedQuestionPKs = Array.ConvertAll(selectedOptionGuids, delegate(string selectedOption)
				{
					ZGuid result;
					ZGuid.TryParse(selectedOption, out result);
					return result;
				});
		}

		protected override void WriteOrderedOptions(XmlWriter writer)
		{
			if (OrderedMultipleChoiceOptionPKs != null)
			{
				string orderedOptions = string.Join(",", Array.ConvertAll(OrderedMultipleChoiceOptionPKs.ToArray(), (s) => s.ToString()));
				writer.WriteString(orderedOptions);
			}
		}

		protected override void ReadOrderedOptions(XmlReader reader)
		{
			if (reader.IsStartElement("OrderedOptions"))
			{
				string[] orderedOptionGuids = reader.ReadElementString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				OrderedMultipleChoiceOptionPKs = Array.ConvertAll(orderedOptionGuids, delegate(string orderedOption)
				{
					ZGuid result;
					ZGuid.TryParse(orderedOption, out result);
					return result;
				});
			}
		}

		#endregion

		public void Populate(LearningCentreSubmittedAnswer submittedAnswer)
		{
			Argument.NotNull(submittedAnswer, "submittedAnswer");

			var question = submittedAnswer.Question;
			QuestionPK = question.PK;
			QuestionAsString = question.HY_QuestionMultilingual;
			QuestionActualNumber = question.ActualOrder;
			QuestionNumber = submittedAnswer.QuestionNumber;

			if (question.IsMultipleChoiceQuestion)
			{
				List<ZGuid> optionGuids = new List<ZGuid>();
				foreach (VoteExamSurveyQuestion option in submittedAnswer.SelectedMultipleChoiceOptions)
				{
					optionGuids.Add(option.PK);
				}
				SelectedQuestionPKs = optionGuids.ToArray();
				OrderedMultipleChoiceOptionPKs = submittedAnswer.OrderedMultipleChoiceOptionPKs;
			}
			else
			{
				AnswerAsString = ((IVoteExamSurveySubmittedAnswer)submittedAnswer).PersistedAnswer.Left(AnswerAsStringInfo.MaxLength);
			}
			IsAnsweredCorrectly = submittedAnswer.IsAnsweredCorrectly;
		}

#if DEBUG
		public void SetSelectedQuestionPKsForTest(ZGuid[] pks)
		{
			SelectedQuestionPKs = pks;
		}

		public void SetOrderedOptionPKsForTest(ZGuid[] pks)
		{
			OrderedMultipleChoiceOptionPKs = pks;
		}
#endif

	}
}
