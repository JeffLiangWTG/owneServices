using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class WhsTransitUniversalEventHelper
	{
		public static IColumnIndexer[] GetBizoColumnIndexers(UniversalObjectFactory factory, BusinessObject[] bizos)
		{
			var result = Array.Empty<IColumnIndexer>();

			if (bizos.Any())
			{
				var bizosType = bizos[0].GetType();
				var bizosSchemaColumn = bizos[0].PKSchemaColumn;
				var bizosTableName = bizos[0].TableName;

				if (bizos.All(b => b.GetType().Equals(bizosType)))
				{
					var columnIndexerQuery = new ZQuery(bizosSchemaColumn, bizos.Select(b => b.PK));
					result = Array.ConvertAll(factory.RowFactory.Load(bizosTableName, columnIndexerQuery), DataObjectReader.GetColumnIndexerFromRow);
				}
				else
				{
					throw new ArgumentException("Bizo contains multiple types.");
				}
			}

			return result;
		}
	}
}
