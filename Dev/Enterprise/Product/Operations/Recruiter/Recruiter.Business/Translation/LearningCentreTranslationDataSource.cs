using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreTranslationDataSource : IContentTranslationDataSource
	{
		public IEnumerable<IContentTranslationDataItem> GetTranslatableResources()
		{
			var factory = new BusinessObjectFactory();

			var campaignQuery = new ZQuery(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, Constants.Recruiter.LearningCentreCampaignType);
			var reader = new FilteredBusinessObjectReader<LearningCentreCampaign>(campaignQuery, factory);
			foreach (LearningCentreCampaign exam in reader)
			{
				var item = new LearningCentreTranslationDataItem(exam.G0_CampaignNameMultilingual + "-" + exam.G0_CampaignID);
				item.SafeAdd(exam.G0_CampaignNameInfo);
				item.SafeAdd(exam.G0_CampaignCommentInfo);

				foreach (var question in exam.Questions)
				{
					item.SafeAdd(question.HY_QuestionInfo);
				}

				foreach (var question in exam.Questions.SelectMany(quest => quest.SubQuestions))
				{
					item.SafeAdd(question.HY_QuestionInfo);
				}

				yield return item;
			}
		}

		protected virtual BusinessObjectFactory GetNewFactory(DbConnection connection)
		{
			var factory = new BusinessObjectFactory(connection);
			return factory;
		}
	}

	public class LearningCentreTranslationDataItem : IContentTranslationDataItem
	{
		public LearningCentreTranslationDataItem(string examId)
		{
			Id = examId;
			Resources = new Dictionary<string, string>();
		}

		public string Id { get; }
		public IDictionary<string, string> Resources { get; }

		public void SafeAdd(ZPropertyInfo info)
		{
			if (!(info.Value is ZString))
			{
				return;
			}

			var value = (ZString)info.Value;
			if (value.IsEmpty)
			{
				return;
			}

			string key = info.CustomizableDataResourceStrings.Source.GetKey(info.BizObj, value);

			if (Resources.ContainsKey(key))
			{
				return;
			}

			Resources.Add(key, value);
		}
	}
}
