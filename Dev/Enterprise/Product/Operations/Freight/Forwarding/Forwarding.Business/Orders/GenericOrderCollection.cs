using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[ModuleID(ModuleId.Orders)]
	public class GenericOrderCollection : BusinessObjectCollection<BusinessObject>, IBusinessObjectCollection
	{
		public GenericOrderCollection(BusinessObjectFactory factory, ForwardingShipment parent)
			: base(factory)
		{
			Parent = Argument.NotNull(parent, "ForwardingShipment parent");
			WrappedOrderCollection = Parent.AttachedOrders;
			WrappedWarehouseOrdersCollection = Parent.AttachedWarehouseOrders;
			WrappedAttachedOrdersFromSupplierBooking = new Lazy<IActiveBusinessObjectCollection>(() => Parent.AttachedOrdersFromSupplierBooking);
			new OrdersOnShipmentLimitHelper(parent).SetCollectionLimit(this);
		}

		readonly ForwardingShipment Parent;
		readonly OrderCollection WrappedOrderCollection;
		readonly IActiveBusinessObjectCollection WrappedWarehouseOrdersCollection;
		readonly Lazy<IActiveBusinessObjectCollection> WrappedAttachedOrdersFromSupplierBooking;

		#region Binding

		public IBusinessObjectCollection CurrentBindingList
		{
			get
			{
				switch (CurrentModuleID)
				{
					case ModuleId.Orders:
						return OrderCollectionForBinding;
					case ModuleId.WhsOrder:
						return WarehouseOrderCollectionForBinding;
					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Module ID {0} not supported.", CurrentModuleID.ToString()));
				}
			}
		}

		public ModuleId CurrentModuleID { get; set; }

		IBusinessObjectCollection OrderCollectionForBinding
		{
			get { return Parent.PossibleOrdersForAttachment_List; }
		}

		IBusinessObjectCollection WarehouseOrderCollectionForBinding
		{
			get
			{
				var warehouseOrderCollectionForBinding = ObjectFactory.New<IWhsOrderCollection>(Factory);
				var collectionAsProvider = (IFilterBusinessObjectDefaultsProvider)warehouseOrderCollectionForBinding;
				var consigneeOrganisation = Parent.ConsigneeDocumentaryAddress.Organisation;
				if (consigneeOrganisation != null)
				{
					collectionAsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consignee", "Property", consigneeOrganisation.PK));
				}

				var consignorOrganisation = Parent.ConsignorDocumentaryAddress.Organisation;
				if (consignorOrganisation != null)
				{
					collectionAsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", consignorOrganisation.PK));
				}

				collectionAsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Order Status", "Property", new ZString("FIN")));

				return warehouseOrderCollectionForBinding;
			}
		}

		Type IBusinessObjectCollection.GetTypeOfElementsFromPK(ZGuid pk)
		{
			return CurrentBindingList.GetTypeOfElementsFromPK(pk);
		}

		#endregion

		#region Add

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var attachedOrder = bizOAdded as IAttachedOrder;
			if (attachedOrder == null)
			{
				throw new ArgumentException("The BusinessObject '" + bizOAdded.GetType().Name + "' added to GenericOrderCollection must implement IAttachedOrder.");
			}
			else
			{
				attachedOrder.ShouldSkipAllValidations = true;
			}

			base.OnAdded(bizOAdded);

			if (!IsAddingExistingElementsSemaphore.IsSuspended)
			{
				var collection = GetRelevantCollection(bizOAdded);
				if (collection != null)
				{
					collection.Add(bizOAdded);
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (!IsAddingExistingElementsSemaphore.IsSuspended)
			{
				var order = child as Order;
				if (order != null)
				{
					WrappedOrderCollection.OnNewOrderAdded(order);
				}
			}
		}

		#region IsAddingExistingElementsSemaphore

		Semaphore IsAddingExistingElementsSemaphore
		{
			get { return isAddingExistingElementsSemaphore ?? (isAddingExistingElementsSemaphore = new Semaphore()); }
		}

		Semaphore isAddingExistingElementsSemaphore;

		#endregion

		#endregion

		#region Remove

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			((IAttachedOrder)bizO).ShouldSkipAllValidations = false;

			var collection = GetRelevantCollection(bizO);
			if (collection != null)
			{
				collection.RemoveFromRelationship(bizO);
			}
		}

		#endregion

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new NonPersistentBusinessObjectFindBoxListProvider(this); }
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return fetchStrategy ?? (fetchStrategy = new OrderCollectionFetchStrategy(this));
		}

		IBusinessObjectCollectionFetchStrategy fetchStrategy;

		IBusinessObjectCollection GetRelevantCollection(BusinessObject bizO)
		{
			IBusinessObjectCollection result = null;

			if (bizO is Order)
			{
				result = WrappedOrderCollection;
			}
			else if (bizO is IWhsOrder)
			{
				result = WrappedWarehouseOrdersCollection;
			}

			return result;
		}

		public void BuildCollection()
		{
			using (new SemaphoreManager(IsAddingExistingElementsSemaphore))
			{
				var elementsToRemove = this.Where(x => !GetRelevantCollection(x).Contains(x)).ToList();

				RemoveWorkflowLinkBetweenBusinssObjectCollectionAndShipment(elementsToRemove);
				RemoveRange(elementsToRemove);

				AttachWorkflowLinkBetweenWorkflowLinkAndShipment(WrappedWarehouseOrdersCollection);
				AddRange(WrappedOrderCollection);
				AddRange(WrappedWarehouseOrdersCollection);
				AddRange(WrappedAttachedOrdersFromSupplierBooking.Value.Cast<Order>().Where(order => !Contains(order)));
			}
		}

		void RemoveWorkflowLinkBetweenBusinssObjectCollectionAndShipment(IList<BusinessObject> businessObjectCollection)
		{
			var workflowProviders = businessObjectCollection.OfType<IWorkflowProviderCore>().ToArray();

			if (workflowProviders.Any())
			{
				var linkageService = ObjectFactory.Get<IWorkflowProvidersLinkageService>();

				foreach (var workflowProvider in workflowProviders)
				{
					linkageService.WorkflowProvidersUnLinked(workflowProvider, Parent, Factory);
				}
			}
		}

		void AttachWorkflowLinkBetweenWorkflowLinkAndShipment(IActiveBusinessObjectCollection businessObjectCollection)
		{
			var workflowProviders = businessObjectCollection.OfType<IWorkflowProviderCore>().ToArray();

			if (workflowProviders.Any())
			{
				var linkageService = ObjectFactory.Get<IWorkflowProvidersLinkageService>();

				foreach (var workflowProvider in workflowProviders)
				{
					linkageService.WorkflowProvidersLinked(workflowProvider, Parent, Factory);
				}
			}
		}
		public new IAttachedOrder this[int index]
		{
			get { return (IAttachedOrder)Elements[index]; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (child is Order order
				&& order.JD_JS.IsEmpty
				&& !order.IsInDatabase
				&& Parent.IsInDatabase)
			{
				order.JD_JS = Parent.PK;
			}

			base.SetCollectionRelationships(child);
		}

		public override void Load()
		{
			BuildCollection();
		}
	}
}
