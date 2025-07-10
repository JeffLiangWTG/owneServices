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
	public class OrderNumberSearchFieldToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderNumberSearchFieldToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrdersConstants.NumberFilterTypes.All, nameof(Xsd.OrderNumberFieldsList.ALL));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.MostCommon, nameof(Xsd.OrderNumberFieldsList.Common));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.BookingConfRef, nameof(Xsd.OrderNumberFieldsList.ConfirmNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.ContainerNo, nameof(Xsd.OrderNumberFieldsList.ContainerNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.HouseBill, nameof(Xsd.OrderNumberFieldsList.HouseBill));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.InvoiceNumber, nameof(Xsd.OrderNumberFieldsList.InvoiceNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.MasterBill, nameof(Xsd.OrderNumberFieldsList.MasterBillNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.OrderNumber, nameof(Xsd.OrderNumberFieldsList.OrderNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.ProductNo, nameof(Xsd.OrderNumberFieldsList.ProductNumber));
			yield return new Mapping(OrdersConstants.NumberFilterTypes.ShipmentNo, nameof(Xsd.OrderNumberFieldsList.ShipmentNumber));
		}

		public static readonly OrderNumberSearchFieldToXmlCodeMappings Instance = new OrderNumberSearchFieldToXmlCodeMappings();

		protected override string Name
		{
			get { return (NoResString)"Order Number Search Field"; } // Developer constant
		}

		public new Xsd.OrderNumberFieldsList GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderNumberFieldsList.ALL, errorContext, notify);
		}
	}
}
