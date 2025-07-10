using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class ExamSettingCodeHelper : IExamSettingCodeHelper
	{
		public string GetExamVersion(string examSettingsCode, ZGuid campaignItemPk, BusinessObjectFactory factory)
		{
			if (!string.IsNullOrEmpty(examSettingsCode))
			{
				var settings = factory.LoadFromNaturalKey<ExamSetting>(ExamSettingSchema.EXS_Code, examSettingsCode);
				if (settings != null)
				{
					return settings.EXS_ExamVersion;
				}
			}

			if (!campaignItemPk.IsEmpty)
			{
				var examSettingsQuery = new ZDBOnlyQuery(typeof(ExamSetting));
				var campaignQuery = new ZDBOnlySubQuery(typeof(LearningCentreCampaign), ExamSettingSchema.EXS_G0);
				var campaignItemSubQuery = new ZDBOnlySubQuery(typeof(LearningCentreCampaignItem), GlbCompanyCampaignItemSchema.G8_G0);
				campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.PK, campaignItemPk);
				campaignQuery.AddSubQuery(campaignItemSubQuery, JoinCondition.And);
				examSettingsQuery.AddSubQuery(campaignQuery, JoinCondition.And);
				examSettingsQuery.AddToFilter(ExamSettingSchema.EXS_IsDefault, true);

				var examSetting = factory.LoadTop1<ExamSetting>(examSettingsQuery);
				if (examSetting != null)
				{
					return examSetting.EXS_ExamVersion;
				}
			}

			return RecruiterDataRegistry.Instance.JobSkillTestVersionList.Value[0].Code;
		}

		public string GetExamSettingsCode(LearningCentreCampaign exam, ZString jobSkillCode, BusinessObjectFactory factory)
		{
			if (exam == null)
			{
				return ZString.Empty;
			}

			return exam.DefaultExamSetting?.EXS_Code ?? ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Ambiguous const string")]
		public const string CodeAmbiguous = "~Ambiguous~";
	}
}
