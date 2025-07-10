using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	class ForwardingShipmentTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ForwardingShipmentTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ShipmentTypes.AssemblyMaster, nameof(Xsd.ForwardingShipmentType.ASM));
			yield return new Mapping(Core.Constants.ShipmentTypes.BuyersConsolLead, nameof(Xsd.ForwardingShipmentType.BCN));
			yield return new Mapping(Core.Constants.ShipmentTypes.CoLoadMaster, nameof(Xsd.ForwardingShipmentType.CLD));
			yield return new Mapping(Core.Constants.ShipmentTypes.ShippersConsolLead, nameof(Xsd.ForwardingShipmentType.SCN));
			yield return new Mapping(Core.Constants.ShipmentTypes.StandardHouse, nameof(Xsd.ForwardingShipmentType.STD));
			yield return new Mapping(Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy, nameof(Xsd.ForwardingShipmentType.HLS));
			yield return new Mapping(Core.Constants.ShipmentTypes.BlindCoLoadMaster, nameof(Xsd.ForwardingShipmentType.CLB));
			yield return new Mapping(Core.Constants.ShipmentTypes.HighVolumeLowValue, nameof(Xsd.ForwardingShipmentType.HVL));
			yield return new Mapping(Core.Constants.ShipmentTypes.HighVolumeLowValueMaster, nameof(Xsd.ForwardingShipmentType.HVM));
			yield return new Mapping(Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse, nameof(Xsd.ForwardingShipmentType.Item3PT));
		}

		public static readonly ForwardingShipmentTypeToXmlCodeMappings Instance = new ForwardingShipmentTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("5A0858DA-3263-44D5-9659-E3B9EFC59F54", "Forwarding Shipment Type"); }
		}

		public new Xsd.ForwardingShipmentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ForwardingShipmentType.STD, errorContext, notify);
		}
	}
}
