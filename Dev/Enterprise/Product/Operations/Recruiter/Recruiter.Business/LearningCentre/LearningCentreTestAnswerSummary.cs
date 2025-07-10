using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreTestAnswerSummary : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string OptionNumber = "OptionNumber";
			public const string AnswerText = "AnswerText";
			public const int AnswerTextMaxLength = 8000;
		}
		public LearningCentreTestAnswerSummary()
		{
		}

		ZString anwserText;
		[MaxLength(Schema.AnswerTextMaxLength)]
		public ZString AnswerText
		{
			get
			{
				return anwserText;
			}
			set
			{
				if (anwserText != value)
				{
					CheckMaximumLength(AnswerTextInfo, value);
					anwserText = value;
					AnswerTextInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo AnswerTextInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.AnswerText);
			}
		}

		public int OptionNumber { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public ZString OptionNumberText
		{
			get
			{
				ZString result = "";
				if (OptionNumber > 0)
				{
					result = OptionNumber.ToString() + "#";
				}
				return result;
			}
		}
		public ZInt AnsweredCount { get; set; }
	}

	public class AnswerSummaryComparer : IComparer<LearningCentreTestAnswerSummary>
	{
		public AnswerSummaryComparer()
		{
		}

		int IComparer<LearningCentreTestAnswerSummary>.Compare(LearningCentreTestAnswerSummary x, LearningCentreTestAnswerSummary y)
		{
			int answerComparisonResult = x.OptionNumber.CompareTo(y.OptionNumber);
			return answerComparisonResult;
		}
	}

	public class LearningCentreTestAnswerSummaryCollection : NonPersistentBusinessObjectCollection<LearningCentreTestAnswerSummary>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LearningCentreTestAnswerSummary();
		}
	}
}
