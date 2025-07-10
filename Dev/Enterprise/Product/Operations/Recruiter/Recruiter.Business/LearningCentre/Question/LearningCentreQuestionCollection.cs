using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreQuestionCollection : VoteExamSurveyQuestionCollection, IImportCollectionInfoProvider
	{
		public LearningCentreQuestionCollection(LearningCentreCampaign campaign, bool isActive)
			: base(campaign, isActive)
		{
		}

		public LearningCentreQuestionCollection(LearningCentreCampaign campaign, ZQuery additionalFilter)
			: base(campaign, additionalFilter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(VoteExamSurveyQuestionSchema.HY_AnswerType, SQLComparisonOperator.NotEqual, LearningCentreAnswerTypeList.Codes.ScaleRange);
			return query;
		}

		public new LearningCentreQuestion this[int index]
		{
			get { return (LearningCentreQuestion)base[index]; }
		}

		public new LearningCentreQuestion AddNew()
		{
			return (LearningCentreQuestion)base.AddNew();
		}

		#region IImportCollectionInfoProvider Members

		string IImportCollectionInfoProvider.ContextKey
		{
			get { return null; }
		}

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				if (importCollectionInfo == null)
				{
					importCollectionInfo = new ImportCollectionInfoImpl(new LearningCentreQuestionCollectionForImport((LearningCentreCampaign)campaign));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_QuestionOrder, Res.GetString("7f04cb77-711f-4246-92e1-410d590cace3", "No."));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_SubQuestionOrder, Res.GetString("4f389a2a-8893-4568-8189-6ea7de6c2b1e", "Option No."));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_Question, Res.GetString("89073fbd-30e1-4bc5-a2da-f2c4c60d7987", "Text"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_AnswerType, Res.GetString("133f099c-85d6-4956-8798-898767f5e167", "Type"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_IsRandomisable, Res.GetString("565bf4da-711a-4553-8528-af4a24f6af07", "Randomizable"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_RN_NKCountryCode, Res.GetString("28114684-28a7-41a4-98cd-6168c13291e3", "Country/Region"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_QuestionCategory, Res.GetString("05715781-4fe1-476d-a3ac-e55a8e6cd00c", "Category"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_ExamCorrectAnswer, Res.GetString("f0d2c08d-6895-4f4d-b9ec-6a2b8d42ef4d", "Correct Answer"));
					AddImportPropertyInfo(AutoVoteExamSurveyQuestion.Schema.HY_AnswerWeighting, Res.GetString("01a4fe3c-77dd-4208-8c0a-6e872c87a93d", "Answer Weighting"));
				}
				importCollectionInfo.Completed += (sender, e) =>
					{
						if (e.Success)
						{
							OnImportCompleted();
						}
					};
				return importCollectionInfo;
			}
		}
#if DEBUG
		public
#endif
		class LearningCentreQuestionCollectionForImport : DependentBusinessObjectCollection<LearningCentreQuestion, LearningCentreCampaign>
		{
			public LearningCentreQuestionCollectionForImport(LearningCentreCampaign campaign)
				: base(campaign)
			{
			}

			protected override BusinessObject AddNewCore()
			{
				LearningCentreQuestion newBizO = (LearningCentreQuestion)base.AddNewCore();
				newBizO.SuspendValidation();
				return newBizO;
			}

			public void OnImportCompleted()
			{
				foreach (LearningCentreQuestion question in this)
				{
					question.ResumeValidation();
					question.ResumeSettingSubQuestionsOrder();
				}
				RemoveAllButLeaveRelationshipsIntact();
			}
		}

		void OnImportCompleted()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				LearningCentreQuestionCollectionForImport collection = (LearningCentreQuestionCollectionForImport)((IImportCollectionInfoProvider)this).ImportCollectionInfo.Collection;
				collection.OnImportCompleted();
			}
		}

		void AddImportPropertyInfo(string propertyName, string headerText)
		{
			importCollectionInfo.Add(new ImportPropertyInfoImpl<LearningCentreQuestion>(propertyName)
			{
				HeaderText = headerText
			});
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ImportCollectionInfoImpl importCollectionInfo;

		#endregion
	}
}
