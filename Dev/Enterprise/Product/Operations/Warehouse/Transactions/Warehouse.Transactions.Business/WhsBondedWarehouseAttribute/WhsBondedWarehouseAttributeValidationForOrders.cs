namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeValidationForOrders : WhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidationForOrders(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		#region ShouldCheckIfEntryNumberIsEntered

		protected override bool ShouldCheckIfEntryNumberIsEntered
		{
			get
			{
				var parent = Parent;
				var order = (WhsOrder)parent.Parent.Docket;
				var pick = order.Pick;

				var relaxValidation = WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(order);
				var shouldcheck = !order.IsFTZCustomsOrderWithPermit || (!parent.IsZoneStatusDomestic && order.IsAttachedToPick && pick.IsInDatabase);

				return !relaxValidation && shouldcheck;
			}
		}

		#endregion
	}
}
