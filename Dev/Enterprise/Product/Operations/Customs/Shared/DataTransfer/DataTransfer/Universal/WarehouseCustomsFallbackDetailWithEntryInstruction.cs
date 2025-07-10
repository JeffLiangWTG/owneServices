using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsFallbackDetailWithEntryInstruction : WarehouseCustomsFallbackDetail
	{
		public Dictionary<ZInt, ZString> EntryInstructionProcedureMap;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZInt, List<EntryHeader>> EntryInstructionEntryHeaderMap;

		public bool IsExport;

		protected override WarehouseCustomsFallbackDetail CloneCore()
		{
			return new WarehouseCustomsFallbackDetailWithEntryInstruction()
			{
				IsExport = this.IsExport,
				IsExWarehouse = this.IsExWarehouse,
				SupplierAddress = this.SupplierAddress,
				InvoiceLineAddInfosApplicableForInwardWarehousing = this.InvoiceLineAddInfosApplicableForInwardWarehousing,
				EntryInstructionProcedureMap = this.EntryInstructionProcedureMap,
				EntryInstructionEntryHeaderMap = this.EntryInstructionEntryHeaderMap,
				FallbackAddInfos = this.FallbackAddInfos
			};
		}
	}
}
