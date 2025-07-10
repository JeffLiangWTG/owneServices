using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonLanguageValidation : AutoGlbPersonLanguageValidation
	{
		public GlbPersonLanguageValidation(AutoGlbPersonLanguage parent) : base(parent)
		{
		}

		#region G7_Language

		protected override void CheckG7_Language()
		{
			base.CheckG7_Language();
			MandatoryValidation.CheckEntered(Parent.G7_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G7_LanguageInfo);
		}

		#endregion

		#region G7_SkillLevel

		protected override void CheckG7_SkillLevel()
		{
			base.CheckG7_SkillLevel();
			MandatoryValidation.CheckEntered(Parent.G7_SkillLevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G7_SkillLevelInfo);
		}

		#endregion
	}
}
