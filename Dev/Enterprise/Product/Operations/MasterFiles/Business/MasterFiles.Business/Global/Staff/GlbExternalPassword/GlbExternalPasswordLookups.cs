using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordLookups : AutoGlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups(AutoGlbExternalPassword parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList PasswordStatusList => Enterprise.MasterFiles.Business.PasswordStatusList.GetFullList(Factory);
		public virtual CodeDescriptionPairList PasswordTypeList => Factory.GetCachedValue<PasswordTypesList>();

		protected new GlbExternalPassword Parent => (GlbExternalPassword)base.Parent;
	}
}
