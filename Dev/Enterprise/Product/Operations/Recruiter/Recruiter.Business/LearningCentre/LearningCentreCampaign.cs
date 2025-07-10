using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(AutoGlbCompanyCampaign.Schema.G0_CampaignID), DescriptionProperty(AutoGlbCompanyCampaign.Schema.G0_CampaignName)]
	public class LearningCentreCampaign : GlbCompanyCampaign,
		ILearningCentreCampaign
	{
		public LearningCentreCampaign(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CampaignTypeCaption
		{
			get { return Res.GetString("4d4db6bf-cc46-467d-a8d8-a1064ab81c45", "Learning Center"); }
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G0_BroadcastVoteSurveyExam = Constants.Recruiter.LearningCentreCampaignType;
			G0_Type = LearningCentreTestTypes.Codes.Exam;
			G0_QuestionsPerWebPage = 5;
		}

		protected override void SetDefaultAnswerType()
		{
			G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
		}

		protected override void OnFactorySaving()
		{
			if (questionCategories != null && QuestionCategories.HasChanges) // only do the check if the collection has been loaded
			{
				QuestionCategories.Save();
			}
			base.OnFactorySaving();
		}

		protected override bool ShouldCreateTasksAndMilestonesFromTemplate
		{
			get { return false; }
		}

		protected override GlbCompanyCampaignValidation GetNewValidation()
		{
			return new LearningCentreCampaignValidation(this);
		}

		public new LearningCentreCampaignLookups Lookups
		{
			get { return (LearningCentreCampaignLookups)base.Lookups; }
		}

		protected override GlbCompanyCampaignLookups GetNewLookups()
		{
			return new LearningCentreCampaignLookups(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("082281bf-7bf4-40ef-8b31-2b1ead7fbe4b", "Learning Center {0}", G0_CampaignID).Trim(); }
		}

		protected override INumberFountainProxy NumberFountain => Env.Instance.NumberFountains.CertificateExamCampaignID;

		#endregion

		#region Base Campaign Overrides

		protected override void DeleteCore()
		{
			ExamSettingCollection.DeleteAll();
			base.DeleteCore();
		}

		[List("Lookups.TestTypes"), MetaDataValue(MetaDataTypes.MaxLength, 3)]
		public override ZString G0_Type
		{
			get { return base.G0_Type; }
			set
			{
				if (base.G0_Type != value)
				{
					base.G0_Type = value;
					Questions.MarkAsNeedingValidation();
				}
			}
		}

		public override bool RandomiseQuestionAndMultipleChoiceOrder
		{
			get { return true; }
		}

		public override ZString CampaignID
		{
			get { return G0_CampaignID; }
		}

		public override ZString G0_CampaignID
		{
			get { return base.G0_CampaignID; }
			set
			{
				base.G0_CampaignID = value;
				foreach (var setting in ExamSettingCollection)
				{
					setting.RegenerateCode();
				}
			}
		}

		protected override ZString VoteExamSurveyNameCaption
		{
			get { return new ZString(Res.GetString("5d88a7da-ed57-4cbc-acee-1dc8a42e3645", "Certification")); }
		}

		ZShort ExamExpiryTimeInMinutes
		{
			get
			{
				return ZShort.Zero;
			}
		}

		protected override GlbCompanyCampaignItemCampaignDependentCollection GetNewCampaignItemCollection()
		{
			return new LearningCentreCampaignItemCollection(this);
		}

		public override Type TypeOfSentItem => typeof(LearningCentreCampaignItem);

		public new LearningCentreCampaignItemCollection CampaignsItemsSent
		{
			get { return (LearningCentreCampaignItemCollection)base.CampaignsItemsSent; }
		}

		public new LearningCentreCampaignItemCollection CampaignsItemsSentForDisplayOnly
		{
			get { return (LearningCentreCampaignItemCollection)base.CampaignsItemsSentForDisplayOnly; }
		}

		public new LearningCentreQuestionCollection Questions
		{
			get { return (LearningCentreQuestionCollection)base.Questions; }
		}

		public new LearningCentreQuestionCollection InactiveQuestions
		{
			get { return (LearningCentreQuestionCollection)base.InactiveQuestions; }
		}

		public new LearningCentreQuestionCollection ActualQuestionsForBinding
		{
			get { return (LearningCentreQuestionCollection)base.ActualQuestionsForBinding; }
		}

		protected override VoteExamSurveyQuestionCollection GetNewQuestionCollection(GlbCompanyCampaign master, bool isActive)
		{
			return new LearningCentreQuestionCollection((LearningCentreCampaign)master, isActive);
		}

		public new LearningCentreTestSummaryCollection VoteExamSurveySummaries
		{
			get
			{
				if (surveySummaries == null)
				{
					surveySummaries = new LearningCentreTestSummaryCollection(ActualQuestionsForBinding);
					OnLoadVoteExamSurveySummaries(surveySummaries, EventArgs.Empty);
					surveySummaries.Load();
				}
				return surveySummaries;
			}
		}
		LearningCentreTestSummaryCollection surveySummaries;

		#endregion

		#region New Properties

		ExamSettingCollection examSettingCollection;
		[ChildEditable(true)]
		public ExamSettingCollection ExamSettingCollection
		{
			get
			{
				if (examSettingCollection == null)
				{
					examSettingCollection = new ExamSettingCollection(this);
					RegisterEditableChildObject(examSettingCollection);
				}

				return examSettingCollection;
			}
		}

		public ExamSetting DefaultExamSetting
		{
			get { return ExamSettingCollection.FirstOrDefault(s => s.EXS_IsDefault); }
		}

		public override ZString DefaultExamVersion
		{
			get
			{
				var defaultSetting = DefaultExamSetting;

				return defaultSetting?.EXS_ExamVersion
						?? RecruiterDataRegistry.Instance.JobSkillTestVersionList.Value[0].Code;
			}
		}

		public ZBool IsExam
		{
			get { return G0_Type == LearningCentreTestTypes.Codes.Exam; }
		}

		public ZBool IsScaledTest
		{
			get { return G0_Type == LearningCentreTestTypes.Codes.Scaled; }
		}

		#endregion

		public LearningCentreCampaignItem CreateCampaignItemIfNotExist()
		{
			return null;
		}

		public TimeSpan MaxDuration
		{
			get { return new TimeSpan(0, ExamExpiryTimeInMinutes, 0); }
		}

		#region Scaled Test

		[ChildEditable(true)]
		public QuestionCategoryCollection QuestionCategories
		{
			get
			{
				if (questionCategories == null)
				{
					questionCategories = new QuestionCategoryCollection(this);
					RegisterEditableChildObject(questionCategories);
					using (SuspendSettingHasChanges())
					{
						questionCategories.Load();
					}
				}
				return questionCategories;
			}
		}

		QuestionCategoryCollection questionCategories;

		#endregion

		#region Campaign Items Action
		public void FixIncorrectScoresAndCompletedOnMostRecentTestResults() // Temporary code for EDI users
		{
		}
		#endregion

		public event EventHandler LoadVoteExamSurveySummaries;

		public void OnLoadVoteExamSurveySummaries(object sender, EventArgs e)
		{
			LoadVoteExamSurveySummaries?.Invoke(sender, e);
		}

		protected override void OnSettingsChanged()
		{
			relatedAccreditations = null;
		}

		GlbAccreditationCollection relatedAccreditations;
		public GlbAccreditationCollection RelatedAccreditations
		{
			get
			{
				if (relatedAccreditations == null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbAccreditation));
					var skillGroupSubQueryAccred = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.HJG_ParentID);
					var skillGroupSubQueryGroup = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.HJG_ParentID);
					var skillPivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillPivot), GlbAccreditationJobSkillPivotSchema.HAJ_HJG);
					var examSettingsSubQuery = new ZDBOnlySubQuery(typeof(ExamSetting), HRJobSkillTestSchema.HT_EXS);
					examSettingsSubQuery.AddToFilter(ExamSettingSchema.EXS_G0, PK);
					if (!CurrentExamVersion.IsEmpty)
					{
						examSettingsSubQuery.AddToFilter(ExamSettingSchema.EXS_ExamVersion, CurrentExamVersion);
					}

					skillGroupSubQueryAccred.AddSubQuery(skillPivotSubQuery, JoinCondition.And);
					skillGroupSubQueryGroup.AddSubQuery(skillPivotSubQuery, JoinCondition.And);

					skillGroupSubQueryAccred.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode,
						GlbAccreditationSchema.Constants.Prefix);

					skillGroupSubQueryGroup.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode,
						GlbAccreditationJobSkillGroupSchema.Constants.Prefix);

					var skillGroupSubQueryAccred2 = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.HJG_ParentID);
					skillGroupSubQueryAccred2.AddSubQuery(skillGroupSubQueryGroup, JoinCondition.And);
					skillGroupSubQueryAccred.AddAsUnionQuery(skillGroupSubQueryAccred2);

					query.AddSubQuery(skillGroupSubQueryAccred, JoinCondition.And);

					relatedAccreditations = new GlbAccreditationCollection(Factory, query);
				}

				return relatedAccreditations;
			}
		}

		#region Test data

#if DEBUG

		public ILearningCentreCampaignItem NewCampaignItemWithRelatedTests(IExamSetting settings)
		{
			LearningCentreCampaignItem campaignItem = CampaignsItemsSent.AddNew();
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "abc@test.de";
			campaignItem.G8_RecipientID = applicant.PK;
			campaignItem.G8_RecipientTableCode = applicant.TablePrefix;
			return campaignItem;
		}

#endif
		#endregion
	}
}
