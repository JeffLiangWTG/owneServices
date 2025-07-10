using System.Collections.Generic;
using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ConversionsToSKUTableForceSPCPackType : ConversionsToSKUTable
	{
		public ConversionsToSKUTableForceSPCPackType(OrgSupplierPart product)
			: base(product)
		{
		}

		protected override void MarkAdditionalInvalidConversionPaths(IEnumerable<ConversionToSKU> conversions)
		{
			var invalidUOMTypes = new List<string> { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.Pallet };
			foreach (var conversion in conversions.Where(c =>
				c.PackTypesFromConversionPath.Count() > 1 &&
				c.PackTypesFromConversionPath.Any(p => invalidUOMTypes.Contains(c.UOMType)))
			)
			{
				conversion.IsInvalid = true;
			}
		}
	}
}
