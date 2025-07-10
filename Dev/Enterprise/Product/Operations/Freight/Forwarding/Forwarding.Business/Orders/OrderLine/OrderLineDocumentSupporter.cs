using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineDocumentSupporter : DocumentSupporter
	{
		public OrderLineDocumentSupporter(OrderLine orderLine)
			: base(orderLine)
		{
		}

		protected OrderLine OrderLine
		{
			get { return (OrderLine)BusinessObject; }
		}

		public override BusinessContext BusinessContext => BusinessContext.Order;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.OrderLineTrackingCustomiseDocuments;

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.Order
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = System.Array.Empty<DocumentWrapper>();

			if (OrderLine.Order != null && dataContext == Core.Constants.DataContext.Order)
			{
				result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Order, OrderLine.Order) };
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.Order
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (OrderLine.Order != null)
			{
				if (contact == ContactType.Consignee && OrderLine.Order.Buyer != null)
				{
					result = new OrgHeaderContact(OrderLine.Order.Buyer, null);
				}
				else if (contact == ContactType.Consignor && OrderLine.Order.Supplier != null)
				{
					result = new OrgHeaderContact(OrderLine.Order.Supplier, null);
				}
				else if (contact == ContactType.ControllingCustomer)
				{
					OrgHeader org = null;
					OrgAddress address = null;
					if (!OrderLine.Order.ControllingCustomerDocAddress.E2_AddressOverride)
					{
						org = OrderLine.Order.ControllingCustomerDocAddress.Organisation;
						address = OrderLine.Order.ControllingCustomerDocAddress.Address;
					}
					result = new OrgHeaderContact(org, address);
				}
			}

			return result;
		}
	}
}
