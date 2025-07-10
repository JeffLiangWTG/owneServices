using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	[Immutable]
	public class OrderLocationSearchFieldToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderLocationSearchFieldToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrdersConstants.PortFilterTypes.All, nameof(Xsd.OrderLocationFieldsList.ALL));
			yield return new Mapping(OrdersConstants.PortFilterTypes.LoadDischargeCode, nameof(Xsd.OrderLocationFieldsList.ConsolLoadDischarge));
			yield return new Mapping(OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode, nameof(Xsd.OrderLocationFieldsList.ShipmentOriginDestination));
		}

		public static readonly OrderLocationSearchFieldToXmlCodeMappings Instance = new OrderLocationSearchFieldToXmlCodeMappings();

		protected override string Name
		{
			get { return (NoResString)"Order Location Search Field"; } // Developer constant
		}

		public new Xsd.OrderLocationFieldsList GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderLocationFieldsList.ALL, errorContext, notify);
		}
	}
}
