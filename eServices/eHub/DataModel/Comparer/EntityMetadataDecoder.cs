using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.DataModel.Comparer.DBInfo;

namespace CargoWise.eHub.DataModel.Comparer
{
	public class EntityMetadataDecoder
	{
		private readonly Assembly _dataClassesAssembly;

		public EntityMetadataDecoder(Assembly dataClassesAssembly)
		{
			if (dataClassesAssembly == null) throw new ArgumentNullException("dataClassesAssembly");
			_dataClassesAssembly = dataClassesAssembly;
		}

		public IList<ITableInfo> DecodeTable(DbContext context, string[] filterTables)
		{
			var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
			var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

			var allEfClasses = metadata
				.GetItems<EntityContainer>(DataSpace.CSpace)
				.Single()
				.EntitySets;

			var result = new List<ITableInfo>();
			Func<string[], EntitySet, bool> filterTableFunction = (tableList, entitySet) =>
			{
				if (tableList != null && tableList.Length != 0)
				{
					return tableList.Contains(entitySet.ElementType.Name);
				}

				return true;
			};

			foreach (var entitySet in allEfClasses.Where(x => filterTableFunction(filterTables, x)))
			{
				var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
					.Single()
					.EntitySetMappings
					.Single(s => s.EntitySet == entitySet);

				var tableEntitySet = mapping
					.EntityTypeMappings.Single()
					.Fragments.Single()
					.StoreEntitySet;

				var tableName = (string)(tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name);
				var tableSchema = tableEntitySet.MetadataProperties["Schema"].Value.ToString();
				var oSpaceEntity = objectItemCollection.Single(x => x.ToString().EndsWith("." + entitySet.ElementType.Name));
				var clrClassType = GetClrClassType(oSpaceEntity.ToString());

				var propDecoder = new EntityPropertiesDecoder(mapping, tableEntitySet);
				var columnInfos = propDecoder.DecodeTableProperties(entitySet, clrClassType);

				result.Add(new TableInfo(tableName, tableSchema, clrClassType, columnInfos));
			}

			return result;
		}

		private Type GetClrClassType(string classFullName)
		{

			var clrClassType = _dataClassesAssembly.GetType(classFullName, false);
			if (clrClassType == null)
				throw new InvalidOperationException(String.Format("Could not find the EF data class {0} in the assembly {1}.",
					classFullName, _dataClassesAssembly.GetName().Name));
			return clrClassType;
		}
	}
}
