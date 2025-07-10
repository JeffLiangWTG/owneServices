using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public partial class TWManifestTypes
	{
		public IManifestType MAN => man ?? (man = new ManifestType(
			Codes.MAN,
			Descriptions.MAN,
			GetTransportModes(),
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Export22AndImport23()));
		IManifestType man;

		public IReadOnlyList<IManifestType> All => new[] { MAN };

		IEnumerable<string> GetTransportModes()
		{
			return new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air };
		}
	}
}
