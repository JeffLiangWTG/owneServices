using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestDocManagerInfo : DocManagerInfo
	{
		public DtbLinehaulManifestDocManagerInfo(DtbLinehaulManifest manifest)
			: base(manifest, Constants.DocManagerCodes.DomesticTransportLinehaulManifest)
		{
		}
	}
}
