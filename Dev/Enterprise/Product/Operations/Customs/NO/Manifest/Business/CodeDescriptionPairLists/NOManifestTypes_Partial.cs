using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.NO.Manifest.Business
{
	public partial class NOManifestTypes
	{
		public IReadOnlyList<IManifestType> All => new[] { DMO };

		IManifestType DMO => dmo ??= new ManifestType(
			Codes.DMO,
			Descriptions.DMO,
			new string[]
			{
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Road,
				Core.Constants.TransportModes.Mail,
				Core.Constants.TransportModes.FixedTransportInstallations,
				Core.Constants.TransportModes.InlandWaterwayTransport,
				Core.Constants.TransportModes.OwnPropulsion
			},
			new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only());

		IManifestType dmo;
	}
}
