using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Packing.Business
{
	public interface IPackableItemParent
	{
		bool IsDeleted { get; }

		ZString Code { get; }

		ZString Description { get; }
		ZString DescriptionSupplement { get; }
		ZString DescriptionSupplementSeparator { get; }

		ZDecimal TotalQty { get; }
		ZString TotalQtyUQ { get; }
		Money UnitPrice { get; }

		ZDecimal WeightPerUnit { get; }
		ZString WeightUQ { get; }

		ZDecimal AutoPackQtyPerPackage { get; }
		ZString AutoPackPackageType { get; }

		BarcodeMatch IsMatch(string barcode);

		ICustomPropertyContainer AdditionalProperties { get; } // so that consumers can define their own properties (e.g. Warehouse wants to show Product Attributes)

		/// <summary>
		/// These are the actual Items that will be Packed via FK on the PkgPackageItemDivot in the DB.
		/// </summary>
		IEnumerable<IPackableItem> PackableItems { get; }

		void RefreshPackableItems();
	}
}
