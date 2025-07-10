using Enterprise.Customs.Common.NZ;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				if (Parent.AMA_ManifestType == NZManifestTypes.Codes.ICR)
				{
					return Factory.GetCachedValue<NZMessageStatusList>();
				}
				else
				{
					return base.MessageStatusList;
				}
			}
		}
	}
}
