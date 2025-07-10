using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ABLEntryNumLookups : ASYCUDA.Business.ABLEntryNumLookups
	{
		public ABLEntryNumLookups(ABLEntryNum parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CustomsEntryNumberTypes => Factory.GetCachedValue<ZaLRNTypes>();
	}
}
