using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class ShipmentSubTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ShipmentSubTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ShipmentSubTypeList.Codes.LowValueEntriesShipments, nameof(Xsd.ISFShipmentSubType.Item01));
			yield return new Mapping(ShipmentSubTypeList.Codes.InformalShipments, nameof(Xsd.ISFShipmentSubType.Item02));
			yield return new Mapping(ShipmentSubTypeList.Codes.GeneralNote3eShipments, nameof(Xsd.ISFShipmentSubType.Item03));
		}

		public static readonly ShipmentSubTypeToXmlCodeMappings Instance = new ShipmentSubTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Shipment Sub Type"; }
		}

		public new Xsd.ISFShipmentSubType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFShipmentSubType.Item01, errorContext, notify);
		}
	}
}
