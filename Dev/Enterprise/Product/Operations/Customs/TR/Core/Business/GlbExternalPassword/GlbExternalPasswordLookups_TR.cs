using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class GlbExternalPasswordLookups_TR : GlbExternalPasswordLookups
	{
		public GlbExternalPasswordLookups_TR(GlbExternalPassword_TR parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CertificateAuthorities
		{
			get { return Factory.GetCachedValue<CertificateAuthorities>(); }
		}

		public CodeDescriptionPairList ChipsetList
		{
			get { return Factory.GetCachedValue<ChipsetList>(); }
		}
	}
}
