using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderDocumentSupporter : DocumentSupporter
	{
		public OrderDocumentSupporter(Order order)
			: base(order)
		{
		}

		protected Order Order
		{
			get { return (Order)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Order; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Order);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = System.Array.Empty<DocumentWrapper>();

			switch (dataContext)
			{
				case Core.Constants.DataContext.Order:
				case Core.Constants.DataContext.Shipment:
				case Core.Constants.DataContext.PreAlert:
				case Core.Constants.DataContext.RequestForMissingDocuments:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Order, Order) };
					break;

				case Core.Constants.DataContext.LandedCostHeader:

					BusinessObject lcHeader = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.LandedCosting.ILandedCostHeader>(new LandedCostHeaderFilter(Order));

					if (lcHeader != null)
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.LandedCostHeader, lcHeader) };
					}
					break;

				default:
					break;
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.Order,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.Shipment,
				Core.Constants.DataContext.PreAlert,
				Core.Constants.DataContext.RequestForMissingDocuments,
				Core.Constants.DataContext.LandedCostHeader
			};
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.OrderTrackingCustomiseDocuments; }
		}

		public override string TransportMode
		{
			get { return Order.JD_TransportMode; }
		}

		public override bool IsImport
		{
			get { return Order.IsImport(); }
		}

		public override string LocalPort(IContactType contact, DocumentDirection direction)
		{
			string result = ZString.Empty;

			if (contact == ContactType.Consignee)
			{
				result = Order.JD_RL_NKPortOfDischarge;
			}
			else if (contact == ContactType.Consignor)
			{
				result = Order.JD_RL_NKPortOfLoading;
			}

			return result;
		}

		public override string ForeignPort(IContactType contact, DocumentDirection direction)
		{
			string result = ZString.Empty;

			if (contact == ContactType.Consignee)
			{
				result = Order.JD_RL_NKPortOfLoading;
			}
			else if (contact == ContactType.Consignor)
			{
				result = Order.JD_RL_NKPortOfDischarge;
			}

			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (contact == ContactType.Consignee)
			{
				if (Order.Shipment != null && Order.Shipment.Consignee != null)
				{
					result = Order.Shipment.DocumentSupporter.GetContactOrganisation(menuName, ContactType.Consignee, DocumentDirection.ARV);
				}
				else
				{
					if (Order.Buyer != null)
					{
						result = new OrgHeaderContact(Order.Buyer, null);
					}
				}
			}
			else if (contact == ContactType.Consignor)
			{
				if (Order.Shipment != null && Order.Shipment.Consignor != null)
				{
					result = Order.Shipment.DocumentSupporter.GetContactOrganisation(menuName, ContactType.Consignor, DocumentDirection.DEP);
				}
				else
				{
					if (Order.Supplier != null)
					{
						result = new OrgHeaderContact(Order.Supplier, null);
					}
				}
			}
			else if (contact == ContactType.ExportFreightAgent)
			{
				if (Order.IsShipmentAttached)
				{
					if (Order.Shipment.DepartureConsol != null && Order.Shipment.DepartureConsol.SendingForwarder != null)
					{
						result = new OrgHeaderContact(Order.Shipment.DepartureConsol.SendingForwarder, Order.Shipment.DepartureConsol.SendingForwarderAddress);
					}
				}
				else
				{
					if (Order.SendingAgent != null)
					{
						result = new OrgHeaderContact(Order.SendingAgent, null);
					}
				}
			}
			else if (contact == ContactType.ImportFreightAgent)
			{
				if (Order.IsShipmentAttached)
				{
					if (Order.Shipment.ArrivalConsol != null && Order.Shipment.ArrivalConsol.ReceivingForwarder != null)
					{
						result = new OrgHeaderContact(Order.Shipment.ArrivalConsol.ReceivingForwarder, Order.Shipment.ArrivalConsol.ReceivingForwarderAddress);
					}
				}
				else
				{
					if (Order.ReceivingAgent != null)
					{
						result = new OrgHeaderContact(Order.ReceivingAgent, null);
					}
				}
			}
			else if (contact == ContactType.ControllingCustomer)
			{
				OrgHeader org = null;
				OrgAddress address = null;
				if (!Order.ControllingCustomerDocAddress.E2_AddressOverride)
				{
					org = Order.ControllingCustomerDocAddress.Organisation;
					address = Order.ControllingCustomerDocAddress.Address;
				}
				result = new OrgHeaderContact(org, address);
			}
			else if (contact == ContactType.ControllingAgent)
			{
				OrgHeader org = null;
				OrgAddress address = null;

				if (!Order.ControllingAgentDocAddress.E2_AddressOverride)
				{
					org = Order.ControllingAgentDocAddress.Organisation;
					address = Order.ControllingAgentDocAddress.Address;
				}

				result = new OrgHeaderContact(org, address);
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.LandedCostHeader:
					{
						var lcHeader = Factory.LoadTop1<Enterprise.Integration.LandedCosting.ILandedCostHeader>(new LandedCostHeaderFilter(Order)) as BusinessObject;
						if (lcHeader == null)
						{
							if (Order.IsImport())
							{
								return Res.GetString("b832638e-21c7-47c2-ab85-5d79bd420c5e", "This Order does not have any Transport Logistics Costs entered within the Landed Costing tab.");
							}

							return Res.GetString("e9864576-9597-4d46-9b91-75826f7fc0f6", "This Order is not an Import so Landing Costing is not possible.");
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.Order
				&& dataContext != Constants.DataContext.GenericFreightJob
				&& dataContext != Constants.DataContext.Shipment
				&& dataContext != Constants.DataContext.PreAlert
				&& dataContext != Constants.DataContext.RequestForMissingDocuments
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Branding

		public override IOrgHeader GetBrandedOrganisation(IContactType contactType, DocumentDirection direction)
		{
			IOrgHeader result = null;
			ContactType concreteContactType = contactType as ContactType;

			if (concreteContactType != null)
			{
				if (concreteContactType.BrandingType == ContactBrandingType.Agent)
				{
					result = (direction == DocumentDirection.DEP) ? Order.SendingAgent : Order.ReceivingAgent;
				}
				else if (concreteContactType.BrandingType == ContactBrandingType.Client)
				{
					result = (direction == DocumentDirection.DEP) ? Order.Supplier : Order.Buyer;
				}
			}

			return result;
		}

		#endregion
	}
}
