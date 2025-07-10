using System.Collections.Generic;
using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ConversionsToSKUTableForceCASPackType : ConversionsToSKUTable
	{
		public ConversionsToSKUTableForceCASPackType(OrgSupplierPart product)
			: base(product)
		{
		}

		protected override void MarkAdditionalInvalidConversionPaths(IEnumerable<ConversionToSKU> conversions)
		{
			var packTypes = Product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			if (conversions.Any(c => c.UOMType == UOMPackTypesList.Codes.Case))
			{
				foreach (var conversion in conversions.Where(c => c.PackTypesFromConversionPath.Any(p => packTypes.TryGetValue(p, out var packType) && packType.F3_UOMType == UOMPackTypesList.Codes.Pallet)))
				{
					conversion.IsInvalid = true;
				}
			}
		}
	}
}
