using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.VN.Manifest.Business
{
	public partial class VNManifestTypes
	{
		public IManifestType VSW => vsw ?? (vsw = new ManifestType(
			Codes.VSW,
			Descriptions.VSW,
			GetTransportModes(),
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()
			));

		IManifestType vsw;

		public IReadOnlyList<IManifestType> All => new[] { VSW };

		IEnumerable<string> GetTransportModes()
		{
			return new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air };
		}
	}
}
