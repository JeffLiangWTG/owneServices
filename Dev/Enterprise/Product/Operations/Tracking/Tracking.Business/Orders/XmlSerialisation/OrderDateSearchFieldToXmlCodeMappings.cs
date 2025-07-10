using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Mapping between Enterprise codes and codes in XSD for Order status list
	/// </summary>
	[Immutable]
	public class OrderDateSearchFieldToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderDateSearchFieldToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrdersConstants.DateFilterTypes.All, nameof(Xsd.OrderDateFieldsList.ALL));
			yield return new Mapping(OrdersConstants.DateFilterTypes.MostCommon, nameof(Xsd.OrderDateFieldsList.Common));
			yield return new Mapping(OrdersConstants.DateFilterTypes.ReqInStore, nameof(Xsd.OrderDateFieldsList.ReqInStore));
			yield return new Mapping(OrdersConstants.DateFilterTypes.ReqExWorks, nameof(Xsd.OrderDateFieldsList.ReqExWorks));
			yield return new Mapping(OrdersConstants.DateFilterTypes.ConfirmedDate, nameof(Xsd.OrderDateFieldsList.ConfirmedDate));
			yield return new Mapping(OrdersConstants.DateFilterTypes.OrderDate, nameof(Xsd.OrderDateFieldsList.OrderDate));
			yield return new Mapping(OrdersConstants.DateFilterTypes.ShipmentWindowStart, nameof(Xsd.OrderDateFieldsList.ShipmentWindowStart));
			yield return new Mapping(OrdersConstants.DateFilterTypes.ShipmentWindowEnd, nameof(Xsd.OrderDateFieldsList.ShipmentWindowEnd));
		}

		public static readonly OrderDateSearchFieldToXmlCodeMappings Instance = new OrderDateSearchFieldToXmlCodeMappings();

		protected override string Name
		{
			get { return (NoResString)"Order Date field search List"; } // Developer constant
		}

		public new Xsd.OrderNumberFieldsList GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderNumberFieldsList.ALL, errorContext, notify);
		}
	}
}
