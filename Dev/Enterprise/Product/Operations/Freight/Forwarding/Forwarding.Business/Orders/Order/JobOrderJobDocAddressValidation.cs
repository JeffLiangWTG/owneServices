using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderJobDocAddressValidation : AutoJobDocAddressValidation
	{
		readonly Order order;

		public JobOrderJobDocAddressValidation(Order order, AutoJobDocAddress parent)
			: base(parent)
		{
			this.order = order;
		}

		public new JobDocAddress Parent
		{
			get { return (JobDocAddress)base.Parent; }
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (!Parent.E2_AddressOverride && Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && order != null && order.Shipment != null)
			{
				if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(order.Shipment, out var errorMessage))
				{
					Parent.E2_OA_AddressInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();

			if (Parent.E2_AddressOverride && Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && order != null && order.Shipment != null)
			{
				if (!order.CanAttachOrderToShipmentWithNonMatchingControllingCustomer(order.Shipment, out var errorMessage))
				{
					Parent.E2_AddressOverrideInfo.AddError(errorMessage);
				}
			}
		}

		protected void CheckOrganisationPK()
		{
			if (Parent.E2_AddressType == DocAddressTypes.Codes.ControllingCustomer && order != null && order.HasLinkedSupplierBookingInProgress)
			{
				if (!Parent.IsInDatabase && !Parent.IsDeleted)
				{
					Parent.OrganisationPKInfo.AddError(Res.GetString("f657d219-dda5-4cbd-b124-4df43fd0c610", "Controlling customer cannot be added because this order or at least one of its order lines is linked to a supplier booking in progress."));
				}
				else if ((!Parent.E2_AddressOverride && Parent.E2_OA_AddressInfo.HasChanges) || (Parent.E2_AddressOverride && Parent.E2_AddressOverrideInfo.HasChanges))
				{
					if (((ZBool)Parent.E2_AddressOverrideInfo.OriginalValue == ZBool.False && ((ZGuid)Parent.E2_OA_AddressInfo.OriginalValue).IsEmpty))
					{
						Parent.OrganisationPKInfo.AddError(Res.GetString("f657d219-dda5-4cbd-b124-4df43fd0c610", "Controlling customer cannot be added because this order or at least one of its order lines is linked to a supplier booking in progress."));
					}
					else
					{
						Parent.OrganisationPKInfo.AddError(Res.GetString("fa7241c5-5442-4ceb-8dfb-6cd1076c731a", "Controlling customer cannot be updated because this order or at least one of its order lines is linked to a supplier booking in progress."));
					}
				}
			}
		}
	}
}
