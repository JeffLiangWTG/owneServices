using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public class TWManifestLayoutBuilder<T> : ManifestLayoutBuilder<T> where T : AsycudaManifestHeader
	{
		protected override int MaxColumns => 3;
	}
}
