using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class ConversionsToSKUTableForAllocation : ConversionsToSKUTable
	{
		public ConversionsToSKUTableForAllocation(OrgSupplierPart product)
			: base(product)
		{
		}

		protected override ZString GetUOMType(string packType, decimal qtySku, IReadOnlyDictionary<ZString, ZString> uomTypes)
		{
			packType = packType.ToUpper();
			var uomType = base.GetUOMType(packType, qtySku, uomTypes);

			if (packType == PkgUnit.Pallet)
			{
				uomType = UOMPackTypesList.Codes.Pallet;
			}
			else if (uomType.IsEmpty)
			{
				if (packType == Product.OP_StockKeepingUnit.ToUpper())
				{
					uomType = UOMPackTypesList.Codes.SplitCase;
				}
				else if (qtySku == 1m)
				{
					uomType = GetUOMType(Product.OP_StockKeepingUnit, 1m, uomTypes);
				}
				else
				{
					uomType = UOMPackTypesList.Codes.Case;
				}
			}

			return uomType;
		}
	}
}
