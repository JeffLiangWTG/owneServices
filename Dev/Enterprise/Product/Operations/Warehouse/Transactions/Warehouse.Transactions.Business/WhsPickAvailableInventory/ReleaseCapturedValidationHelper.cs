using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	static class ReleaseCapturedValidationHelper
	{
		public static void CheckPickedPickLinesAreFullyReleaseCaptured(ZPropertyInfo pickedDateInfo, WhsPickOrderedInventory orderedInventory, WhsProduct product, OrgHeader client, IEnumerable<WhsPickLine> pickLines)
		{
			if (!pickedDateInfo.Value.IsEmpty && orderedInventory != null && product != null && orderedInventory.Owners.Any(o => o.WE_WE_ParentDocketLine.IsEmpty) && pickLines.Any(p => hasReleaseCapturedMandatoryAttributeUncaptured(p)))
			{
				pickedDateInfo.AddError(Res.GetString("8ec6019b-6f18-4423-a9d0-b2fd1232d1f5", "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked."));
			}

			bool hasReleaseCapturedMandatoryAttributeUncaptured(WhsPickLine pickLine) =>
				product.IsMandatoryPartAttribReleaseCaptured(client, 1) && pickLine.WZ_ReleaseCapturedPartAttrib1.IsEmpty
				|| product.IsMandatoryPartAttribReleaseCaptured(client, 2) && pickLine.WZ_ReleaseCapturedPartAttrib2.IsEmpty
				|| product.IsMandatoryPartAttribReleaseCaptured(client, 3) && pickLine.WZ_ReleaseCapturedPartAttrib3.IsEmpty
				|| product.IsSerialNumberReleaseCaptured(client) && pickLine.WZ_ReleaseCapturedSerialNumber.IsEmpty;
		}
	}
}
