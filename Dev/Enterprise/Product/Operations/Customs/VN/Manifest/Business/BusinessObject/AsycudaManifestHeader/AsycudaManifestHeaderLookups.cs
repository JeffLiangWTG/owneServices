using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.VN.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				return Factory.GetCachedValue("VNMessageStatusCodeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(MessageStatusCodeList.Codes.NotSent, MessageStatusCodeList.Descriptions.NotSent);
					result.AddPair(MessageStatusCodeList.Codes.Sent, MessageStatusCodeList.Descriptions.Sent);
					result.AddPair(MessageStatusCodeList.Codes.Updated, MessageStatusCodeList.Descriptions.Updated);
					result.AddPair(MessageStatusCodeList.Codes.Cancel, MessageStatusCodeList.Descriptions.Cancel);
					return result;
				});
			}
		}
	}
}
