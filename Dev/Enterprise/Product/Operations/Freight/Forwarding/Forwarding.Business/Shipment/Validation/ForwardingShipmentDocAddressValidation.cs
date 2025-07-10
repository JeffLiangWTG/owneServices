using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentDocAddressValidation : ShipmentDocAddressValidation
	{
		public ForwardingShipmentDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.ConsigneeDocumentaryAddress
				&& Shipment.IsImport()
				&& RequireOrders
				&& !HasOrders)
			{
				string message = Res.GetString("a30c7782-bcf3-4bac-8859-06dc67d8b726", "This Consignee requires Order References to be entered.\r\nPlease attach an order or enter references in the Order Refs field.");

				if (ShipmentDomainService.GetInstance(Parent.Factory).DocAddressOrganisationValidationWarnOnly)
				{
					Parent.OrganisationPKInfo.AddWarning(message);
				}
				else
				{
					Parent.OrganisationPKInfo.AddError(message);
				}
			}

			if (!Parent.E2_AddressOverride)
			{
				CheckOrganisationDocumentDeliveryAddress(Parent.OrganisationPKInfo);
			}
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();

			if (Parent.E2_AddressOverride)
			{
				CheckOrganisationDocumentDeliveryAddress(Parent.E2_CompanyNameInfo);
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && !Parent.E2_AddressOverride && Shipment != null)
			{
				foreach (var order in Shipment.GenericOrders.OfType<Order>())
				{
					if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(Shipment, out var errorMessage))
					{
						Parent.E2_OA_AddressInfo.AddError(errorMessage);
					}

					break;
				}
			}
		}

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();

			if (Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && Parent.E2_AddressOverride && Shipment != null)
			{
				foreach (var order in Shipment.GenericOrders.OfType<Order>())
				{
					if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(Shipment, out var errorMessage))
					{
						Parent.E2_AddressOverrideInfo.AddError(errorMessage);
					}

					break;
				}
			}
		}

		void CheckOrganisationDocumentDeliveryAddress(ZPropertyInfo infoToValidate)
		{
			if (Parent.E2_AddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress
				&& Shipment.DeliveryAddressHasBeenChangedByShipmentUser
				&& Shipment.ShipmentAnnouncer != null
				&& Shipment.ShipmentAnnouncer.IsNotificationRequiredIfDeliveryAddressChangedByFreight(Shipment))
			{
				string message = Res.GetString("9428c43b-6ddf-4d0d-9bf8-20f02640e474", "The delivery address of this shipment has been changed and there is at least one attached declaration where your Customs Brokerage Department should be made aware of this change. {0}", Shipment.ShipmentAnnouncer.AdditionalNotificationText);
				infoToValidate.AddWarning(message);
			}
		}

		bool HasOrders
		{
			get { return Shipment.GenericOrders.Count != 0 || Shipment.AttachedOrders.Count != 0 || Shipment.DocsAndCartage.OrderItems.Count != 0; }
		}

		bool RequireOrders
		{
			get { return Parent.Organisation != null && Parent.Organisation.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs; }
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)Parent.Parent; }
		}
	}
}
