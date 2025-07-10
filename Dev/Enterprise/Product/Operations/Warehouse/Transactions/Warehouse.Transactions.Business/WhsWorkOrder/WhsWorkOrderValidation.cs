using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderValidation : WhsComponentOrderValidation
	{
		public WhsWorkOrderValidation(WhsWorkOrder parent)
			: base(parent)
		{
		}

		#region CheckWD_ExternalReferenceForDuplicates

		protected override bool ShouldCheckForDuplicateExternalReference() => !Parent.BOM.IsAutoCreatingWorkOrders;

		#endregion

		protected override void CheckWD_TotalUnits()
		{
			base.CheckWD_TotalUnits();
			if (Parent.IsFinalising && WarehouseDataRegistry.Instance.TotalUnitsValidation.Value)
			{
				var isAssembly = Parent.IsAssembly;
				var lines = Parent.AssemblyLinesForReceive.Cast<WhsWorkOrderLine>();
				var totalLineUnits = isAssembly
					? lines.Sum(line => line.QuantityAssembledIncludingSecondaryProductsAndOverPickedComponents)
					: Parent.GetDisassemblyQuantity();
				if (Parent.WD_TotalUnits != totalLineUnits)
				{
					string errorMsg;
					if (isAssembly)
					{
						var hasOverPickedComponent = lines.Any(l => l.HaveOverPickedComponent);
						var secondaryAndOverPickedProductMsg = GetSecondaryAndOverPickedProductMessage(Parent.WD_IsInwardsProcessingJob, hasOverPickedComponent);
						errorMsg = Res.GetString("09995cd3-2b63-4054-a8d6-20783d2cc17f", "Total Units {0} does not equal the total of all assembly line units {1}{2}.", Parent.WD_TotalUnits, secondaryAndOverPickedProductMsg, totalLineUnits);
					}
					else
					{
						errorMsg = Res.GetString("7dd4b418-4729-46e0-a671-f9dbfdd1d2b2", "Total Units {0} does not equal the total of all disassembly line units {1}.", Parent.WD_TotalUnits, totalLineUnits);
					}
					Parent.WD_TotalUnitsInfo.AddError(errorMsg);
				}
			}
		}

		string GetSecondaryAndOverPickedProductMessage(bool isInwardsProcessingJob, bool hasOverPickedComponent)
		{
			var result = string.Empty;

			if (isInwardsProcessingJob && hasOverPickedComponent)
			{
				result = Res.GetString("3ddf63a8-4667-464c-ade4-d6487bc39ab2", "(including secondary products and over-picked components) ");
			}
			else if (isInwardsProcessingJob)
			{
				result = Res.GetString("41ee620c-f2bf-4a56-9ad6-a26ef369a68e", "(including secondary products) ");
			}
			else if (hasOverPickedComponent)
			{
				result = Res.GetString("9e0c13a3-4b27-475d-bd5b-1a15e43d1885", "(including over-picked components) ");
			}

			return result;
		}

		#region ValidateWD_AutoFinaliseBOMIntoInventory

		protected override void CheckWD_AutoFinaliseBOMIntoInventory()
		{
			base.CheckWD_AutoFinaliseBOMIntoInventory();

			if (Parent.WD_AutoFinaliseBOMIntoInventory
				&& !Parent.WD_AutoFinaliseBOMIntoInventoryInfo.HasErrors()
				&& !(Parent.Warehouse?.WW_IsVirtualWarehouse ?? false))
			{
				var topLevelBomProducts = Parent.DisassemblyLinesForPick;
				foreach (WhsWorkOrderLine line in topLevelBomProducts)
				{
					if (line.StagingLocationBOM == null)
					{
						Parent.WD_AutoFinaliseBOMIntoInventoryInfo.AddError(Res.GetString("0C9F4960-D29C-4A29-9980-465E21F49FB0",
							"Auto-Finalizing into Inventory can only be enabled if all Lines have a BOM Staging Location."));
						break;
					}
				}
			}
		}

		#endregion

		protected new WhsWorkOrder Parent => (WhsWorkOrder)base.Parent;

		protected override ZString TypeInMsg => Res.GetString("8554c7b0-9ad6-426b-b1f9-a2e60288233b", "Work Order");

		public override void ValidateAll()
		{
			base.ValidateAll();
			Validate_FinalizePickWhenWorkOrderIsFinalized();
		}
	}
}
