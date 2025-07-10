using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsRMAOrderLineValidation : ZValidation
	{
		public WhsRMAOrderLineValidation(WhsRMAOrderLine parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly WhsRMAOrderLine Parent;

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateQuantityToReturn();
			ValidateWhsOverride();
		}

		#endregion

		#region ValidateQuantityToReturn

		public void ValidateQuantityToReturn() => ValidateCalculatedProperty(Parent.QuantityToReturnInfo);

		#endregion

		#region CheckQuantityToReturn

		protected void CheckQuantityToReturn()
		{
			if (Parent.QuantityToReturn < 0m)
			{
				Parent.QuantityToReturnInfo.AddError(Res.GetString("4f11b55c-005a-49c4-96ce-4fd55fae91af", "Cannot be a negative value"));
			}
			else if (Parent.QuantityToReturn > Parent.AvailableQtyToReturn)
			{
				Parent.QuantityToReturnInfo.AddError(Res.GetString("93257a7b-b5ae-415e-b232-19e6144a82ed", "Please enter an amount less than or equal to the Available Quantity"));
			}
			else if (Parent.QuantityToReturn == 0m)
			{
				Parent.QuantityToReturnInfo.AddWarning(Res.GetString("b633cb08-6814-4054-b8ee-817d0688240f", "Will not generate RMA Receive Line for this line when amount equal 0"));
			}
		}

		#endregion

		#region ValidateWhsOverride

		public void ValidateWhsOverride() => ValidateCalculatedProperty(Parent.WhsOverrideInfo);

		protected void CheckWhsOverride()
		{
			ListValidation.ErrorIfInvalidPK(Parent.WhsOverrideInfo);
			if (Parent.WhsOverride == Parent.OriginalWarehousePK)
			{
				Parent.WhsOverrideInfo.AddError(Res.GetString("25a1bc30-c5ef-43cd-a455-636221fc8dff", "Do not enter the same warehouse as on the Order. Leave this blank to generate RMA for the original Warehouse"));
			}
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsRMAOrderLineValidation); }
		}

		#endregion
	}
}
