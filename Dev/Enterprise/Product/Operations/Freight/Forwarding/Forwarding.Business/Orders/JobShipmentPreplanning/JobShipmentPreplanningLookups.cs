using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobShipmentPreplanningLookups : AutoJobShipmentPreplanningLookups
	{
		public JobShipmentPreplanningLookups(AutoJobShipmentPreplanning parent)
			: base(parent)
		{
		}

		public new JobShipmentPreplanning Parent
		{
			get { return (JobShipmentPreplanning)base.Parent; }
		}

		#region Orders

		public OrderCollection Orders
		{
			get
			{
				PreadviceOrderCollection result = new PreadviceOrderCollection(Factory, Parent);

				ZQuery additionalFilter = new ZQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, Parent.EF_OA_BuyerAddress);
				additionalFilter.AddToFilter(JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning, null);

				ZQuery shipmentSubQuery = new ZQuery(JobOrderHeaderSchema.JD_JS, null);
				if (Parent.EF_JS.IsValid)
				{
					shipmentSubQuery.AddToFilter(JoinCondition.Or, JobOrderHeaderSchema.JD_JS, Parent.EF_JS);
				}
				additionalFilter.AddToFilter(shipmentSubQuery);
				result.AdditionalFilter = additionalFilter;

				OrdersFilterProvider defaultsFilterProvider = new OrdersFilterProvider();
				defaultsFilterProvider.Buyer = Parent.BuyerPK;
				defaultsFilterProvider.ShowUnAttatchedOrders = true;
				defaultsFilterProvider.SetDefaultFilters(result);

				return result;
			}
		}
#if DEBUG
		public
#else
		internal 
#endif
		class PreadviceOrderCollection : OrderCollection
		{
			public PreadviceOrderCollection(BusinessObjectFactory factory, JobShipmentPreplanning parent)
				: base(factory)
			{
				this.parent = parent;
			}

			readonly JobShipmentPreplanning parent;

			protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				Order order = (Order)selectedBusinessObject;

				if (order.BuyerPK != parent.BuyerPK)
				{
					errors.Add(Res.GetString("029a895d-a79e-4932-9ebf-735330fd8092", "You can only choose Orders that are for the same buyer as the Pre Advice."));
				}

				if ((!order.JD_JS.IsEmpty && order.JD_JS != parent.EF_JS)
					|| (!order.JD_EF_ShipmentPrePlanning.IsEmpty && order.JD_EF_ShipmentPrePlanning != parent.PK))
				{
					errors.Add(Res.GetString("9e1dd90f-817b-40f6-953b-d7079fd2e085", "You can only choose Orders that are not already linked to a Shipment or Shipment Pre Advice."));
				}
			}
		}

		#endregion

		#region Carriers

		public override OrgHeaderCollection Carriers
		{
			get { return carriers ?? (carriers = new ShippingProviderCollection(Factory)); }
		}
		OrgHeaderCollection carriers;

		#endregion

		#region Buyers

		public OrgHeaderCollection Buyers
		{
			get { return buyers ?? (buyers = new ConsigneeCollection(Factory)); }
		}
		OrgHeaderCollection buyers;

		#endregion

		#region Sending Agents

		public override OrgHeaderCollection SendingAgents
		{
			get { return sendingAgents ?? (sendingAgents = new ForwarderCollection(Factory)); }
		}
		OrgHeaderCollection sendingAgents;

		#endregion

		#region Receiving Agents

		public override OrgHeaderCollection ReceivingAgents
		{
			get { return receivingAgents ?? (receivingAgents = new ForwarderCollection(Factory)); }
		}
		OrgHeaderCollection receivingAgents;

		#endregion

		#region Shipments

		public ForwardingShipmentCollection Shipments
		{
			get { return shipments ?? (shipments = new ForwardingShipmentCollection(Factory)); }
		}
		ForwardingShipmentCollection shipments;

		#endregion

		#region Declarations

		public BusinessObjectCollection Declarations
		{
			get { return declarations ?? (declarations = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclarationCollection>(), Factory)); }
		}
		BusinessObjectCollection declarations;

		#endregion

		#region Consolidations

		public ForwardingConsolCollection Consolidations
		{
			get
			{
				var consolidations = new ForwardingConsolCollection(Factory);

				var filterProvider = new ForwardingConsolDefaultFilterProvider
				{
					Carrier = Parent.EF_OH_Carrier,
					SendingAgent = Parent.EF_OH_SendingAgent,
					ReceivingAgent = Parent.EF_OH_ReceivingAgent,
					MasterBill = Parent.EF_MasterBill,
					LoadPort = Parent.EF_RL_NKPortLoad,
					DischargePort = Parent.EF_RL_NKPortDisch,
					TransportMode = Parent.MainTransportMode
				};

				filterProvider.SetDefaultFilters(consolidations);

				return consolidations;
			}
		}

		#endregion

		#region Units

		public CodeDescriptionPairList UnitPackList
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		public CodeDescriptionPairList UnitWeightList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList UnitVolumeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion
	}
}
