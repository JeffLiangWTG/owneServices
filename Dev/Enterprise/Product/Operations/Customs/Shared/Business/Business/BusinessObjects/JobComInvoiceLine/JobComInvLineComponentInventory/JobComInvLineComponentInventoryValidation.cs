//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvLineComponentInventoryValidation
//
//    This class should be used for overriding validation in AutoJobComInvLineComponentInventoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class JobComInvLineComponentInventoryValidation : AutoJobComInvLineComponentInventoryValidation
	{
		public JobComInvLineComponentInventoryValidation(AutoJobComInvLineComponentInventory parent) : base(parent)
		{
		}

		public new JobComInvLineComponentInventory Parent => (JobComInvLineComponentInventory)base.Parent;

		protected override void CheckJIV_QuantityToDraw()
		{
			base.CheckJIV_QuantityToDraw();

			var declaration = Parent.InvoiceLine?.Declaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive && declaration.IsAllocatedQuantityRequiredForBondedWarehouse && !declaration.IsBondedWarehousingDisabled && declaration.SupportInwardProcessing && (Parent.JIV_QuantityToDraw <= 0 || Parent.JIV_QuantityToDraw > Parent.QuantityOnHand))
			{
				if (IsOutwardBondedWarehousingEnabled(declaration))
				{
					if (Parent.InvoiceLine.Part != null && declaration.BondedWarehousingHelper.IsMarkedForBondedWarehousing(Parent.InvoiceLine))
					{
						Parent.JIV_QuantityToDrawInfo.AddMessageError(AllocatedQuantityIsRequiredForWarehouse(declaration.TermNameForBondedWarehouse));
					}
				}
			}
		}

		bool IsOutwardBondedWarehousingEnabled(BaseJobDeclaration declaration)
		{
			return declaration.SupportMultipleWarehouseEntry ? (Parent.InvoiceLine.CusEntryLine?.Header?.IsOutwardBondedWarehousingEnabled ?? false) : declaration.IsOutwardBondedWarehousingEnabled;
		}

		public static string AllocatedQuantityIsRequiredForWarehouse(string term)
		{
			return Res.GetString("842D1145-0C2A-46CA-99D4-FEFE83C15FB5", "Please enter a Quantity to Draw that is greater than 0 and does not exceed the available Quantity on Hand for {0} integration.", term);
		}
	}
}
