using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class ShipmentTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ShipmentTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ShipmentTypeList.Codes.StandardOrRegularFilings, nameof(Xsd.ISFShipmentType.Item01));
			yield return new Mapping(ShipmentTypeList.Codes.ToOrderShipments, nameof(Xsd.ISFShipmentType.Item02));
			yield return new Mapping(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, nameof(Xsd.ISFShipmentType.Item03));
			yield return new Mapping(ShipmentTypeList.Codes.MilitaryAndGovernment, nameof(Xsd.ISFShipmentType.Item04));
			yield return new Mapping(ShipmentTypeList.Codes.DiplomaticShipment, nameof(Xsd.ISFShipmentType.Item05));
			yield return new Mapping(ShipmentTypeList.Codes.Carnet, nameof(Xsd.ISFShipmentType.Item06));
			yield return new Mapping(ShipmentTypeList.Codes.USReturnGoods, nameof(Xsd.ISFShipmentType.Item07));
			yield return new Mapping(ShipmentTypeList.Codes.FTZShipments, nameof(Xsd.ISFShipmentType.Item08));
			yield return new Mapping(ShipmentTypeList.Codes.InternationalMailShipments, nameof(Xsd.ISFShipmentType.Item09));
			yield return new Mapping(ShipmentTypeList.Codes.OuterContinentalShelfShipments, nameof(Xsd.ISFShipmentType.Item10));
			yield return new Mapping(ShipmentTypeList.Codes.Informal, nameof(Xsd.ISFShipmentType.Item11));
		}

		public static readonly ShipmentTypeToXmlCodeMappings Instance = new ShipmentTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF Shipment Type"; }
		}

		public new Xsd.ISFShipmentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFShipmentType.Item01, errorContext, notify);
		}
	}
}
