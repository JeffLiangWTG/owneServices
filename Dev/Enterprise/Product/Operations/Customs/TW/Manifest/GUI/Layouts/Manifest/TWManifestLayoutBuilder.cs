using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public class TWManifestLayoutBuilder : ManifestLayoutBuilder<Business.AsycudaManifestHeader>
	{
		public TWManifestControlBag TWManiFestControlBag => TWManifestControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
