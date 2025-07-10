using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreCampaignValidation : GlbCompanyCampaignValidation
	{
		public LearningCentreCampaignValidation(LearningCentreCampaign parent)
			: base(parent)
		{
		}

		public new LearningCentreCampaign Parent
		{
			get { return (LearningCentreCampaign)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSettingsCollection();
		}

		void ValidateSettingsCollection()
		{
			var errorAddOne = Res.GetString("B7E6DD5A-A099-4A65-8B09-6C8F0A8A5D05", "Please add at least one Exam Setting.");
			if (Parent.ExamSettingCollection.Count == 0)
			{
				Parent.AddRowError(errorAddOne);
			}
			else
			{
				Parent.RemoveRowError(errorAddOne);
			}

			bool hasDefault = false;
			foreach (var setting in Parent.ExamSettingCollection)
			{
				if (setting.EXS_IsDefault)
				{
					hasDefault = true;
					break;
				}
			}

			var errorNoDefault = Res.GetString("DEFEBF23-B366-4AC7-BBA3-B609544DB052", "Please set at least one Exam Settings as default.");
			if (!hasDefault)
			{
				Parent.AddRowError(errorNoDefault);
			}
			else
			{
				Parent.RemoveRowError(errorNoDefault);
			}
		}

		#region Disabled base validations

		protected override void CheckCampaignID()
		{
		}

		protected override void CheckG0_EstimatedStartedDate()
		{
		}

		protected override void CheckG0_Category()
		{
		}

		protected override void CheckG0_Type()
		{
		}

		protected override void CheckG0_GS_NKCampaignManager()
		{
		}

		protected override bool ShouldCheckForCampaignCoordinatorEmailAddress
		{
			get { return false; }
		}

		#endregion
	}
}
