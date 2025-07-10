using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class LoadListColumnIndexerHelper
	{
		public static ZString GetAdditionalReferenceByType(UniversalObjectFactory factory, IColumnIndexer ddl, ZString type)
		{
			if (ddl != null)
			{
				var additionalReferences = GetAdditionalReferences(factory, ddl);
				return AdditionalReferenceColumnIndexerHelper.GetFirstAdditionalReferenceByType(additionalReferences, type);
			}
			return "";
		}

		public static IColumnIndexer[] GetAdditionalReferences(UniversalObjectFactory factory, IColumnIndexer ddl)
		{
			if (ddl != null)
			{
				var pk = ddl.GetValue(WhsItemDispatchLoadListSchema.PK);
				return AdditionalReferenceColumnIndexerHelper.GetAdditionalReferencesByParent(factory, pk);
			}
			return System.Array.Empty<IColumnIndexer>();
		}

		public static ZString GetMasterBill(UniversalObjectFactory factory, IColumnIndexer ddl)
		{
			if (ddl != null)
			{
				return GetAdditionalReferenceByType(factory, ddl, WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			}
			return "";
		}

		public static ZString GetFormattedReference(UniversalObjectFactory factory, IColumnIndexer ddl)
		{
			if (ddl != null)
			{
				return TransitWarehouseHelper.GetFormattedReferenceString(GetMasterBill(factory, ddl), ddl.GetValue(WhsItemDispatchLoadListSchema.WDL_JobID));
			}
			return "";
		}
	}
}
