using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class CusEntryHeaderLookups : Declaration.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader entry)
			: base(entry)
		{
		}

		public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<LowValueManifestStatusList>();
	}
}
