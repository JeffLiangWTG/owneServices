using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy

	interface IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy { }

	class WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy
	{
		WhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				OrgSupplierPartBarcodeSchema.PH_Barcode,
				OrgSupplierPartBarcodeSchema.PH_OP
			};
	}

	#endregion
}
