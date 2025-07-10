using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public partial class TRManifestTypes
	{
		public static IReadOnlyList<ManifestType> All => new[] { ATAIHR, ATAITH, CIKONC, DEMIHR, DEMITH, DENIHR, DENITH, DIGIHR, DIGITH, EMANIF, GRUPAJ, HAVIHR, HAVITH, TESLIM, TIRIHR, TIRITH, VARONC };

		public static bool IsManifestTypesRelatedToSea(ZString transportMode, ZString manifestType)
		{
			return transportMode == Core.Constants.TransportModes.Sea
				&& (manifestType == Codes.CIKONC
				|| manifestType == Codes.DENIHR
				|| manifestType == Codes.DENITH
				|| manifestType == Codes.HAVIHR
				|| manifestType == Codes.HAVITH
				|| manifestType == Codes.VARONC
				|| manifestType == Codes.EMANIF);
		}

		public static bool IsManifestTypesRelatedToAir(ZString transportMode, ZString manifestType) => transportMode == Core.Constants.TransportModes.Air && (manifestType == Codes.HAVITH || manifestType == Codes.VARONC || manifestType == Codes.GRUPAJ);

		public static bool IsNeedToDefaultDateAtCustomsOffice(string transportMode, string manifestType)
		{
			return (transportMode == Core.Constants.TransportModes.Sea || transportMode == Core.Constants.TransportModes.Air)
				&& (manifestType == Codes.CIKONC || manifestType == Codes.VARONC);
		}

		static ManifestType ATAITH => new ManifestType(
										Codes.ATAITH,
										Descriptions.ATAITH,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType ATAIHR => new ManifestType(
										Codes.ATAIHR,
										Descriptions.ATAIHR,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType CIKONC => new ManifestType(
										Codes.CIKONC,
										Descriptions.CIKONC,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType DEMITH => new ManifestType(
										Codes.DEMITH,
										Descriptions.DEMITH,
										Core.Constants.TransportModes.Rail,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType DEMIHR => new ManifestType(
										Codes.DEMIHR,
										Descriptions.DEMIHR,
										Core.Constants.TransportModes.Rail,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType DENITH => new ManifestType(Codes.DENITH,
										Descriptions.DENITH,
										Core.Constants.TransportModes.Sea,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType DENIHR => new ManifestType(
										Codes.DENIHR,
										Descriptions.DENIHR,
										Core.Constants.TransportModes.Sea,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType DIGITH => new ManifestType(
										Codes.DIGITH,
										Descriptions.DIGITH,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType DIGIHR => new ManifestType(
										Codes.DIGIHR,
										Descriptions.DIGIHR,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType GRUPAJ => new ManifestType(
										Codes.GRUPAJ,
										Descriptions.GRUPAJ,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22AndImport23());

		static ManifestType HAVIHR => new ManifestType(
										Codes.HAVIHR,
										Descriptions.HAVIHR,
										Core.Constants.TransportModes.Air,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType HAVITH => new ManifestType(
										Codes.HAVITH,
										Descriptions.HAVITH,
										Core.Constants.TransportModes.Air,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType TESLIM => new ManifestType(
										Codes.TESLIM,
										Descriptions.TESLIM,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22AndImport23());

		static ManifestType TIRIHR => new ManifestType(
										Codes.TIRIHR,
										Descriptions.TIRIHR,
										Core.Constants.TransportModes.Road,
										new[] { ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22Only());

		static ManifestType TIRITH => new ManifestType(
										Codes.TIRITH,
										Descriptions.TIRITH,
										Core.Constants.TransportModes.Road,
										new[] { ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType VARONC => new ManifestType(
										Codes.VARONC,
										Descriptions.VARONC,
										transportModes,
										new[] { ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Import23Only());

		static ManifestType EMANIF => new ManifestType(
										Codes.EMANIF,
										Descriptions.EMANIF,
										Core.Constants.TransportModes.Sea,
										new[] { ApplicationCodeTypeList.Codes.ShippingLine },
										MessageLevel.Manifest,
										ShipmentTypeList.Export22AndImport23());

		static readonly IEnumerable<string> transportModes = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.InlandWaterwayTransport, Core.Constants.TransportModes.OwnPropulsion };
	}
}
