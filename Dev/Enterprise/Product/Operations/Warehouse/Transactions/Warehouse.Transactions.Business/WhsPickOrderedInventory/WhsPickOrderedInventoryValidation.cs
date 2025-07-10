using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickOrderedInventoryValidation : ZValidation
	{
		public WhsPickOrderedInventoryValidation(WhsPickOrderedInventory parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateQuantityOrdered

		public void ValidateQuantityOrdered()
		{
			ValidateCalculatedProperty(Parent.QuantityOrderedInfo);
		}

		protected void CheckQuantityOrdered()
		{
			if (Parent.QuantityOrdered < 0m)
			{
				Parent.QuantityOrderedInfo.AddError(Res.GetString("3fb89ae6-c7e8-4ab8-9b7f-2d741839210b", "Quantity Ordered must be greater than or equal to zero"));
			}
		}

		#endregion

		#region ValidatePickLineQuantity

		public void ValidatePickLineQuantity()
		{
			ValidateCalculatedProperty(Parent.PickLineQuantityInfo);
		}

		protected void CheckPickLineQuantity()
		{
			if (Parent.Pick != null && !Parent.Pick.IsFinalisedOrCancelled)
			{
				var pickLineQuantity = Parent.PickLineQuantity;
				if (pickLineQuantity < 0m)
				{
					Parent.PickLineQuantityInfo.AddError(Res.GetString("f597e4c9-2a0c-448b-b7fa-e61e1f0ce069", "Quantity Allocated must be greater than or equal to zero"));
				}
				else if (pickLineQuantity > Parent.QuantityOrdered)
				{
					Parent.PickLineQuantityInfo.AddError(Res.GetString("fd6bff03-4d95-4309-96d9-0233c9ac0d58", "Quantity Allocated can not be greater than Units Ordered. Deallocate some stock in the Inventory grid to reduce this value."));
				}
			}
		}

		#endregion

		#region ValidateQuantityShort

		public void ValidateQuantityShort()
		{
			ValidateCalculatedProperty(Parent.QuantityShortInfo);
		}

		protected void CheckQuantityShort()
		{
			if (Parent.Pick != null && !Parent.Pick.IsCancelled)
			{
				var quantityShort = Parent.QuantityShort;
				if (quantityShort > 0m && Parent.Product != null)
				{
					Parent.QuantityShortInfo.AddWarning(
						Res.GetString("3e71b543-6be4-460e-9176-82ef8e1c3b3c", "This item is short by {0}. Allocate more stock in the Inventory grid to bring this value to zero.",
						Parent.Product.FormattedQtyAndUnit(quantityShort)));
				}
			}

			if (!Parent.ValidationQuantityShortWarningMessage.IsEmpty)
			{
				Parent.QuantityShortInfo.AddWarning(Parent.ValidationQuantityShortWarningMessage);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateQuantityOrdered();
			ValidatePickLineQuantity();
			ValidateQuantityShort();
		}

		#endregion

		public override Type AutoValidationType
		{
			get { return typeof(WhsPickOrderedInventoryValidation); }
		}

		readonly WhsPickOrderedInventory Parent;
	}
}
