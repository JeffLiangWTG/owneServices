using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaManifestHeaderDocManagerInfo : DocManagerInfo
	{
		public AsycudaManifestHeaderDocManagerInfo(AsycudaManifestHeader parent) : base(parent, Core.Constants.DocManagerCodes.OutturnGateInOut)
		{
		}
	}
}
