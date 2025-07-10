using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AttachRelatedOrderRequestEventArgs : EventArgs
	{
		public AttachRelatedOrderRequestEventArgs(CommonShipment shipment)
		{
			Shipment = shipment;
		}

		public CommonShipment Shipment { get; private set; }
	}

	public sealed class ShipmentDomainService : IService
	{
		ShipmentDomainService(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static ShipmentDomainService GetInstance(BusinessObjectFactory factory)
		{
			ShipmentDomainService result = factory.ServiceContainer.GetService<ShipmentDomainService>();
			if (result == null)
			{
				result = new ShipmentDomainService(factory);
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		public bool DocAddressOrganisationValidationWarnOnly { get; set; }

		public ForwardingModuleShipmentCollection ModuleShipmentCollection
		{
			get { return moduleShipmentCollection ?? (moduleShipmentCollection = new ForwardingModuleShipmentCollection(factory)); }
			set
			{
				if (value != null && moduleShipmentCollection != null)
				{
					throw new InvalidOperationException("You can only create one ForwardingModuleShipmentCollection per factory");
				}
				moduleShipmentCollection = value;
			}
		}
		ForwardingModuleShipmentCollection moduleShipmentCollection;

		#region AttachRelatedOrderRequest event

		public event EventHandler<AttachRelatedOrderRequestEventArgs> AttachRelatedOrderRequest;

		public void RequestAttachRelatedOrder(CommonShipment shipment)
		{
			AttachRelatedOrderRequest?.Invoke(this, new AttachRelatedOrderRequestEventArgs(shipment));
		}

		#endregion

		#region NewSupplierBuyerLink event

		public event EventHandler<NewSupplierBuyerLinkEventArgs> NewSupplierBuyerLink;

		public ZDialogResult QueryNewSupplierBuyerLink()
		{
			var eventArgs = new NewSupplierBuyerLinkEventArgs(ZDialogResult.Cancel);
			NewSupplierBuyerLink?.Invoke(this, eventArgs);
			return eventArgs.Result;
		}

		#endregion

		readonly BusinessObjectFactory factory;
	}

	public sealed class NewSupplierBuyerLinkEventArgs : EventArgs
	{
		public NewSupplierBuyerLinkEventArgs(ZDialogResult result)
		{
			Result = result;
		}

		public ZDialogResult Result { get; set; }
	}
}
