using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using static System.FormattableString;

namespace Enterprise.Customs.Business
{
	public class DeclarationEntryInstructionInventorySelectionHeader : DeclarationInventorySelectionHeader
	{
		public DeclarationEntryInstructionInventorySelectionHeader(BaseJobDeclaration declaration)
			: this(declaration, () => declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions?.OfType<CusEntryInstruction>().OrderBy(x => Invariant($"{x.CEI_Style}|{x.CEI_Description}")).FirstOrDefault())
		{
		}

		public DeclarationEntryInstructionInventorySelectionHeader(CusEntryInstruction entryInstruction)
			: this(entryInstruction.JobDeclaration, () => entryInstruction)
		{
		}

		DeclarationEntryInstructionInventorySelectionHeader(BaseJobDeclaration declaration, Func<CusEntryInstruction> getFirstEntryInstruction)
			: base(declaration)
		{
			if (getFirstEntryInstruction != null)
			{
				entryInstruction = getFirstEntryInstruction();
			}
		}

		readonly CusEntryInstruction entryInstruction;

		protected override BaseJobComInvoiceHeader GetFirstOrCreateNewInvoiceHeader(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var invoice = entryInstruction?.InvoiceLines?.FirstOrDefault()?.InvoiceHeader;
			return invoice ?? base.GetFirstOrCreateNewInvoiceHeader(whsBondedWarehouseAttribute);
		}

		protected override OrgAddress GetWarehouseAddress()
		{
			return entryInstruction?.Warehouse;
		}

		protected override void UpdateParentData()
		{
			base.UpdateParentData();
			if (entryInstruction != null && entryInstruction.CEI_OA_Warehouse.IsEmpty)
			{
				var warehouseAddress = SelectionLines.GetFirstWarehouseAddress();
				if (warehouseAddress != null)
				{
					entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
				}
			}
		}

		protected override void SetHeaderData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var zaInvoiceLine = invoiceLine;
			if (entryInstruction != null)
			{
				zaInvoiceLine.JI_CEI = entryInstruction.PK;
			}
		}
	}
}
