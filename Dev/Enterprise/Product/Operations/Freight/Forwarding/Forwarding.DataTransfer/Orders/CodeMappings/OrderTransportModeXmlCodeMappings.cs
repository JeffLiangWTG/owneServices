using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Immutable]
	public class OrderTransportModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderTransportModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.TransportModes.Sea, nameof(Xsd.OrderTransportMode.SEA));
			yield return new Mapping(Core.Constants.TransportModes.Air, nameof(Xsd.OrderTransportMode.AIR));
			yield return new Mapping(Core.Constants.TransportModes.Rail, nameof(Xsd.OrderTransportMode.RAI));
			yield return new Mapping(Core.Constants.TransportModes.Road, nameof(Xsd.OrderTransportMode.ROA));
			yield return new Mapping(Core.Constants.TransportModes.Courier, nameof(Xsd.OrderTransportMode.COU));
			yield return new Mapping(Core.Constants.TransportModes.Mail, nameof(Xsd.OrderTransportMode.MAI));
			yield return new Mapping(Core.Constants.TransportModes.Unknown, nameof(Xsd.OrderTransportMode.UNK));
			yield return new Mapping(Core.Constants.TransportModes.RollOnRollOff, nameof(Xsd.OrderTransportMode.ROR));
		}

		public static readonly OrderTransportModeToXmlCodeMappings Instance = new OrderTransportModeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("e1fb0da1-9c19-44b4-beee-546ed9ab1843", "Order Transport Mode"); }
		}

		public new Xsd.OrderTransportMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderTransportMode.SEA, errorContext, notify);
		}
	}
}
