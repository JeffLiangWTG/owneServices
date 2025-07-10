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
	public class OrderOrganisationSearchFieldToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderOrganisationSearchFieldToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OrdersConstants.OrgFilterTypes.All, nameof(Xsd.OrderOrganisationFieldsList.ALL));
			yield return new Mapping(OrdersConstants.OrgFilterTypes.BuyerSupplier, nameof(Xsd.OrderOrganisationFieldsList.BuyerSupplier));
			yield return new Mapping(OrdersConstants.OrgFilterTypes.SendingRecvAgent, nameof(Xsd.OrderOrganisationFieldsList.SendingReceivingAgent));
		}

		public static readonly OrderOrganisationSearchFieldToXmlCodeMappings Instance = new OrderOrganisationSearchFieldToXmlCodeMappings();

		protected override string Name
		{
			get { return (NoResString)"Order Organization search field List"; } // Developer constant
		}

		public new Xsd.OrderNumberFieldsList GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderNumberFieldsList.ALL, errorContext, notify);
		}
	}
}
