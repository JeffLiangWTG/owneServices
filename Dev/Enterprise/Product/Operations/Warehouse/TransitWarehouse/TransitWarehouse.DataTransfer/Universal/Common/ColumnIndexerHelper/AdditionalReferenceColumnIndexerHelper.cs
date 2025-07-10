using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class AdditionalReferenceColumnIndexerHelper
	{
		public static ZString GetFirstAdditionalReferenceByType(IColumnIndexer[] additionalReferences, ZString type)
		{
			if (additionalReferences != null)
			{
				var additionalReference = additionalReferences.FirstOrDefault(a => a.GetValue(CusEntryNumSchema.CE_EntryType) == type);
				if (additionalReference != null)
				{
					return additionalReference.GetValue(CusEntryNumSchema.CE_EntryNum);
				}
			}
			return "";
		}

		public static IColumnIndexer[] GetAdditionalReferencesByParent(UniversalObjectFactory factory, ZGuid parentPK)
		{
			if (!parentPK.IsEmpty)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parentPK);
				return factory.RowFactory.Load(CusEntryNumSchema.Constants.TableName, query).Select(r => DataObjectReader.GetColumnIndexerFromRow(r)).ToArray();
			}
			return System.Array.Empty<IColumnIndexer>();
		}
	}
}
