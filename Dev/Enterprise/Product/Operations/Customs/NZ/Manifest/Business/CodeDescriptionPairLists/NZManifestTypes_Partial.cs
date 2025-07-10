using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public partial class NZManifestTypes
	{
		public IManifestType ICR => icr ?? (icr = new ManifestType(
			Codes.ICR,
			Descriptions.ICR,
			new List<string> { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea },
			new[] { ApplicationCodeTypeList.Codes.ShippingLine },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()
		));
		IManifestType icr;

		public IManifestType OCR => ocr ?? (ocr = new ManifestType(
			Codes.OCR,
			Descriptions.OCR,
			new List<string> { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea },
			new[] { ApplicationCodeTypeList.Codes.ShippingLine },
			MessageLevel.Manifest,
			ShipmentTypeList.Export22Only()
		));
		IManifestType ocr;

		public IReadOnlyList<IManifestType> All => new[] { ICR, OCR };
	}
}
