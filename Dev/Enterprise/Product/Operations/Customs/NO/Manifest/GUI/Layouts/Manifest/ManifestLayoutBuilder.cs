using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class ManifestLayoutBuilder : ASYCUDA.GUI.ManifestLayoutBuilder<AsycudaManifestHeader>
{
	protected override int MaxColumns => 3;
}
