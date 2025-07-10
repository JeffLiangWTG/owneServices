using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
	[Serializable]  // Only to make the code analyser STFU (CA2237)
	public class VoteExamSurveyAnswerCollectionDictionary : Dictionary<ZGuid, VoteExamSurveyAnswerCollection>
	{
		public VoteExamSurveyAnswerSet Master;

		public VoteExamSurveyAnswerCollectionDictionary(VoteExamSurveyAnswerSet master)
		{
			this.Master = master;
			foreach (var campaignItem in Master.ExamCampaignItems)
			{
				Add(campaignItem.PK, campaignItem.PersistedAnswers);
			}
		}

#if NETFRAMEWORK
		protected VoteExamSurveyAnswerCollectionDictionary(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public void Load()
		{
			Clear();
			foreach (var campaignItem in Master.ExamCampaignItems)
			{
				campaignItem.PersistedAnswers.Load();
				Add(campaignItem.PK, campaignItem.PersistedAnswers);
			}
		}

		public VoteExamSurveyAnswer LoadOrCreateNew(VoteExamSurveyQuestion question, GlbCompanyCampaignItem campaignItem)
		{
			VoteExamSurveyAnswer result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers.LoadOrCreateNew(question);
			}
			return result;
		}

		public IEnumerable<VoteExamSurveyAnswer> AllPersistedAnswers
		{
			get
			{
				foreach (var campaignItemPersistedAnswers in this)
				{
					foreach (VoteExamSurveyAnswer answer in campaignItemPersistedAnswers.Value)
					{
						yield return answer;
					}
				}
			}
		}

		public VoteExamSurveyAnswer FindByQuestion(VoteExamSurveyQuestion question, GlbCompanyCampaignItem campaignItem)
		{
			VoteExamSurveyAnswer result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers.FindByQuestion(question);
			}
			return result;
		}

		public IEnumerable<VoteExamSurveyAnswer> Find(ZQuery query, GlbCompanyCampaignItem campaignItem)
		{
			IEnumerable<VoteExamSurveyAnswer> result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers.Find(query);
			}
			return result;
		}

		public VoteExamSurveyAnswer CreateNew(VoteExamSurveyQuestion question, GlbCompanyCampaignItem campaignItem)
		{
			VoteExamSurveyAnswer result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers.CreateNew(question);
			}
			return result;
		}

		public IEnumerable<VoteExamSurveyAnswer> GetCompletedAnswers(GlbCompanyCampaignItem campaignItem)
		{
			IEnumerable<VoteExamSurveyAnswer> result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers.GetCompletedAnswers();
			}
			return result;
		}

		public VoteExamSurveyAnswerCollection GetCampaignItemAnswers(GlbCompanyCampaignItem campaignItem)
		{
			VoteExamSurveyAnswerCollection result = null;
			VoteExamSurveyAnswerCollection persistedAnswers;
			if (TryGetValue(campaignItem.PK, out persistedAnswers))
			{
				result = persistedAnswers;
			}
			return result;
		}
	}
}
