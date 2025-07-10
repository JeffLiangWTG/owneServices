using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public partial class PEManifestTypes
	{
		public IManifestType MAN => man ?? (man = new ManifestType(
			Codes.MAN,
			Descriptions.MAN,
			new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea },
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest));
		IManifestType man;

		public IReadOnlyList<IManifestType> All => new[] { MAN };
	}
}
