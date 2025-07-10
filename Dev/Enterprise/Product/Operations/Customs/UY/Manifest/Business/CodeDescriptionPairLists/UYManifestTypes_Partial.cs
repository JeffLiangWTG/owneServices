using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public partial class UYManifestTypes
	{
		public IManifestType MAN => man ?? (man = new ManifestType(
			Codes.MAN,
			Descriptions.MAN,
			new[] { Core.Constants.TransportModes.Air },
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType man;

		public IReadOnlyList<IManifestType> All => new[] { MAN };
	}
}
