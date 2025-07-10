using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class ZaManifestTypes
	{
		public IManifestType ALH => alh ?? (alh = new ManifestType(
			Codes.ALH,
			Descriptions.ALH,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Bill
		));
		IManifestType alh;

		public IManifestType ALM => alm ?? (alm = new ManifestType(
			Codes.ALM,
			Descriptions.ALM,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType alm;

		public IManifestType AQM => aqm ?? (aqm = new ManifestType(
			Codes.AQM,
			Descriptions.AQM,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType aqm;

		public IManifestType BBB => bbb ?? (bbb = new ManifestType(
			Codes.BBB,
			Descriptions.BBB,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType bbb;

		public IManifestType COH => coh ?? (coh = new ManifestType(
			Codes.COH,
			Descriptions.COH,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Bill
		));
		IManifestType coh;

		public IManifestType COM => com ?? (com = new ManifestType(
			Codes.COM,
			Descriptions.COM,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType com;

		public IManifestType ECL => ecl ?? (ecl = new ManifestType(
			Codes.ECL,
			Descriptions.ECL,
			Core.Constants.TransportModes.Sea,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType ecl;

		public IManifestType FFM => ffm ?? (ffm = new ManifestType(
			Codes.FFM,
			Descriptions.FFM,
			Core.Constants.TransportModes.Air,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType ffm;

		public IManifestType FWB => fwb ?? (fwb = new ManifestType(
			Codes.FWB,
			Descriptions.FWB,
			Core.Constants.TransportModes.Air,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType fwb;

		public IManifestType HAB => hab ?? (hab = new ManifestType(
			Codes.HAB,
			Descriptions.HAB,
			Core.Constants.TransportModes.Air,
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Bill
		));
		IManifestType hab;

		public IManifestType RFM => rfm ?? (rfm = new ManifestType(
			Codes.RFM,
			Descriptions.RFM,
			Core.Constants.TransportModes.Road,
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Manifest
		));
		IManifestType rfm;

		public IManifestType RMA => rma ?? (rma = new ManifestType(
			Codes.RMA,
			Descriptions.RMA,
			Core.Constants.TransportModes.Rail,
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType rma;

		public IReadOnlyList<IManifestType> All => new[] { ALH, ALM, AQM, BBB, COH, COM, ECL, FFM, FWB, HAB, RFM, RMA };
	}
}
