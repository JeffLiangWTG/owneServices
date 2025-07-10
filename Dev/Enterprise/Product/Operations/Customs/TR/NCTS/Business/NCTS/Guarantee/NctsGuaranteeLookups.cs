using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsGuaranteeLookups : EU.NCTS.Business.NctsGuaranteeLookups
	{
		public NctsGuaranteeLookups(NctsGuarantee parent) : base(parent)
		{
		}

		protected new NctsGuarantee Parent => (NctsGuarantee)base.Parent;

		protected override CodeDescriptionPairList BondTypeListCore => Factory.GetCachedValue<GuaranteeTypeList>();
	}
}
