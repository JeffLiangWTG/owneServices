using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList Natures => Parent.IsAir ? ShipmentTypeList.Export22Only() : ShipmentTypeList.Export22AndImport23();
	}
}
