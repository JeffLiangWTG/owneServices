using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class QuestionCategoryCollection : XmlSerialisableCollection<QuestionCategory, LearningCentreCampaign>
	{
		public QuestionCategoryCollection(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		public override void Load()
		{
			base.Load();

			if (Count == 0)
			{
				QuestionCategory defaultCategory = AddNew();
				using (defaultCategory.SuspendSettingHasChanges())
				{
					defaultCategory.Code = QuestionCategory.DefaultCode;
					defaultCategory.Description = QuestionCategory.DefaultDescription;
				}
			}
		}

		public QuestionCategory AddNew(string code, string description)
		{
			QuestionCategory result = AddNew();
			result.Code = code;
			result.Description = description;
			return result;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QuestionCategory(parentBizO);
		}

		#endregion
	}
}
