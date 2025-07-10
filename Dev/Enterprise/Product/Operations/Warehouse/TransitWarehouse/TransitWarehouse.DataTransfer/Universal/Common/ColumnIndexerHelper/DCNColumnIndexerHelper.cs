using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class DCNColumnIndexerHelper
	{
		public static ZString GetAdditionalReferenceByType(UniversalObjectFactory factory, IColumnIndexer dcn, ZString type)
		{
			if (dcn != null)
			{
				var additionalReferences = GetAdditionalReferences(factory, dcn);
				return AdditionalReferenceColumnIndexerHelper.GetFirstAdditionalReferenceByType(additionalReferences, type);
			}
			return "";
		}

		public static IColumnIndexer[] GetAdditionalReferences(UniversalObjectFactory factory, IColumnIndexer dcn)
		{
			if (dcn != null)
			{
				var pk = dcn.GetValue(WhsItemDispatchConsignmentSchema.PK);
				return AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(factory, pk);
			}
			return System.Array.Empty<IColumnIndexer>();
		}

		public static ZString GetShipmentNumber(UniversalObjectFactory factory, IColumnIndexer dcn)
		{
			if (dcn != null)
			{
				return GetAdditionalReferenceByType(factory, dcn, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			}
			return "";
		}

		public static ZString GetFormattedReference(UniversalObjectFactory factory, IColumnIndexer dcn)
		{
			if (dcn != null)
			{
				return TransitWarehouseHelper.GetFormattedReferenceString(dcn.GetValue(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID), dcn.GetValue(WhsItemDispatchConsignmentSchema.WDC_JobID));
			}
			return "";
		}
	}
}
