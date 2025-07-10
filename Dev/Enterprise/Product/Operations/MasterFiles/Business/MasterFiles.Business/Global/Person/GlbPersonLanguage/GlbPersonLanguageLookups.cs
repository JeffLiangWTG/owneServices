using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonLanguageLookups : AutoGlbPersonLanguageLookups
	{
		public GlbPersonLanguageLookups(AutoGlbPersonLanguage parent) : base(parent)
		{
		}

		#region Languages

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region Skill Levels

		public ReadOnlyCodeDescriptionPairList SkillLevels => Env.Registry.LanguageSkillLevelList;

		#endregion
	}
}
