using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class AllocatePackageLabelsHelper
	{
		public static void FillPackagePropertiesFromProduct(PkgPackage package, OrgSupplierPart product, ZString targetPackType)
		{
			if (!product.OP_MeasureUQ.IsEmpty)
			{
				package.KP_DimensionUQ = product.OP_MeasureUQ;
			}

			if (!product.OP_CubicUQ.IsEmpty)
			{
				package.KP_VolumeUQ = product.OP_CubicUQ;
			}

			var isPackTypeSKU = product.OP_StockKeepingUnit.EqualsIgnoringCase(targetPackType);
			var partUnits = !isPackTypeSKU ? product.PartUnits.Cast<OrgPartUnit>().Where(x => x.OF_ParentPackType == targetPackType).ToArray() : Array.Empty<OrgPartUnit>();
			if (partUnits.Length == 1 && !partUnits.Single().HasErrors())
			{
				var partUnit = partUnits.Single();
				package.KP_Length = partUnit.OF_Depth;
				package.KP_Width = partUnit.OF_Width;
				package.KP_Height = partUnit.OF_Height;
				package.KP_Volume = partUnit.OF_Cubic;

				if (!product.OP_WeightUQ.IsEmpty && partUnit.OF_Weight > 0)
				{
					package.KP_WeightUQ = product.OP_WeightUQ;
					package.KP_TareWeight = 0m;
					package.KP_Weight = partUnit.OF_Weight;
				}
			}
			// if there's no valid unit conversion to the Pack Type then we ignore the values from the RefPackType
			else
			{
				package.KP_Volume = 0;
				package.KP_Length = 0;
				package.KP_Width = 0;
				package.KP_Height = 0;
				package.KP_TareWeight = 0;
				package.KP_Weight = 0;

				// checking if the SKU is the same as the Pack Type, this will always be mutually exclusive with having a valid Unit Conversion.
				if (isPackTypeSKU)
				{
					if (!product.OP_WeightUQ.IsEmpty)
					{
						package.KP_WeightUQ = product.OP_WeightUQ;
						package.KP_Weight = product.OP_Weight;
					}

					if (!product.OP_MeasureUQ.IsEmpty)
					{
						package.KP_Length = product.OP_Depth;
						package.KP_Width = product.OP_Width;
						package.KP_Height = product.OP_Height;
					}

					if (!product.OP_CubicUQ.IsEmpty)
					{
						package.KP_Volume = product.OP_Cubic;
					}
				}
			}
		}
	}
}
