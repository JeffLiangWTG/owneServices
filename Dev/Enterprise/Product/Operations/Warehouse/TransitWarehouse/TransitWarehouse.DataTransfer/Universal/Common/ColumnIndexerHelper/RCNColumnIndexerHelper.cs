using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class RCNColumnIndexerHelper
	{
		public static ZString GetAdditionalReferenceByType(UniversalObjectFactory factory, IColumnIndexer rcn, ZString type)
		{
			if (rcn != null)
			{
				var additionalReferences = GetAdditionalReferences(factory, rcn);
				return AdditionalReferenceColumnIndexerHelper.GetFirstAdditionalReferenceByType(additionalReferences, type);
			}
			return "";
		}

		public static IColumnIndexer[] GetAdditionalReferences(UniversalObjectFactory factory, IColumnIndexer rcn)
		{
			if (rcn != null)
			{
				var pk = rcn.GetValue(WhsItemReceiveConsignmentSchema.PK);
				return AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(factory, pk);
			}
			return System.Array.Empty<IColumnIndexer>();
		}

		public static ZString GetShipmentNumber(UniversalObjectFactory factory, IColumnIndexer rcn)
		{
			if (rcn != null)
			{
				return GetAdditionalReferenceByType(factory, rcn, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			}
			return "";
		}

		public static ZString GetFormattedReference(UniversalObjectFactory factory, IColumnIndexer rcn)
		{
			if (rcn != null)
			{
				return TransitWarehouseHelper.GetFormattedReferenceString(rcn.GetValue(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID), rcn.GetValue(WhsItemReceiveConsignmentSchema.WRC_JobID));
			}
			return "";
		}
	}
}
