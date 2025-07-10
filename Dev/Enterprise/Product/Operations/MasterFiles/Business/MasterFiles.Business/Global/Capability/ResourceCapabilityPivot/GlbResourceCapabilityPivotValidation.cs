
namespace Enterprise.MasterFiles.Business
{
	public class GlbResourceCapabilityPivotValidation : AutoGlbResourceCapabilityPivotValidation
	{
		public GlbResourceCapabilityPivotValidation(AutoGlbResourceCapabilityPivot parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckSkillLevel();
		}

		void CheckSkillLevel()
		{
			var pivot = (GlbResourceCapabilityPivot)Parent;
			pivot.SkillLevelInfo.ClearAllNotifications();
			if (Parent.G5_SkillLevel == 0)
			{
				pivot.SkillLevelInfo.AddError(Res.GetString("a1a1ace8-bde8-4172-8c91-aa68ea2769cf", "Please select a value."));
			}
		}
	}
}
