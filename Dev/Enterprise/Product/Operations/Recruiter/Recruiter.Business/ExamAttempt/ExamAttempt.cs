using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	public class ExamAttempt : AutoExamAttempt, IExamAttempt
	{
		public ExamAttempt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CampaignItem")]
		public override ZGuid EXA_G8
		{
			get { return base.EXA_G8; }
			set { base.EXA_G8 = value; }
		}

		public LearningCentreCampaignItem CampaignItem
		{
			get { return Factory.Load<LearningCentreCampaignItem>(EXA_G8); }
		}

		public ExamAnswerArchiveCollection ExamAnswers
		{
			get
			{
				if (examAnswers == null)
				{
					examAnswers = new ExamAnswerArchiveCollection(Factory);
					if (!EXA_AnswersXML.IsEmpty)
					{
						examAnswers.LoadFromBlob(EXA_AnswersXML);
					}
				}
				return examAnswers;
			}
		}
		ExamAnswerArchiveCollection examAnswers;

		public void ExamBegin(LearningCentreCampaignItem campaignItem)
		{
			EXA_G8 = campaignItem.PK;
		}

		public void ExamEnd(LearningCentreCampaignItem campaignItem)
		{
			EXA_Status = StatusCodes.Queued;
			EXA_Score = campaignItem.ExamScore;
			PopulateExamAnswers(campaignItem);
		}

		public void Rollback()
		{
			EXA_Score = 0;
			EXA_TestCompletedUtc = ZDateTime.Empty;
			EXA_Status = StatusCodes.InProgress;
			EXA_AnswersXML = ZBlob.Empty;
			examAnswers = new ExamAnswerArchiveCollection(Factory);
		}

		public void PopulateExamAnswers(LearningCentreCampaignItem campaignItem)
		{
			examAnswers = new ExamAnswerArchiveCollection(Factory);
			foreach (LearningCentreSubmittedAnswer submittedAnswer in campaignItem.SubmittedAnswers)
			{
				var examAnswerArchive = examAnswers.AddNew();
				examAnswerArchive.Populate(submittedAnswer);
			}
			EXA_AnswersXML = (examAnswers.Count > 0) ? examAnswers.ConvertToXmlBlob() : ZBlob.Empty;
		}

		public static class StatusCodes
		{
			public const string InProgress = "INP";
			public const string Queued = "QUE";
			public const string Processed = "PRS";
			public const string Failed = "FAL";
		}
	}
}
