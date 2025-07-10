using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Mapping between Enterprise codes and codes in XSD for Shipment status list
	/// </summary>
	[Immutable]
	public class ShipmentStatusListToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ShipmentStatusListToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Shipments.ShipmentStatus.Codes.All, nameof(Xsd.ShipmentStatusList.ALL));
			yield return new Mapping(Shipments.ShipmentStatus.Codes.Delivered, nameof(Xsd.ShipmentStatusList.Delivered));
			yield return new Mapping(Shipments.ShipmentStatus.Codes.Undelivered, nameof(Xsd.ShipmentStatusList.Undelivered));
		}

		public static readonly ShipmentStatusListToXmlCodeMappings Instance = new ShipmentStatusListToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("67af584a-5c68-44a7-9876-d9cf0ca83c0e", "Shipment Status List"); }
		}

		public new Xsd.ShipmentNumberFieldsList GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ShipmentNumberFieldsList.ALL, errorContext, notify);
		}
	}
}
