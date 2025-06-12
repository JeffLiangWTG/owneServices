using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;
using CargoWise.eHub.DataModel.Comparer.DBInfo;

namespace CargoWise.eHub.DataModel.Comparer
{
	internal class EntityPropertiesDecoder
	{
		private readonly EntitySetMapping _mapping;
		private readonly EntitySet _tableEntitySet;

		public EntityPropertiesDecoder(EntitySetMapping mapping, EntitySet tableEntitySet)
		{
			_mapping = mapping;
			_tableEntitySet = tableEntitySet;
		}

		public List<IColumnInfo> DecodeTableProperties(EntitySet entitySet, Type clrClassType)
		{
			var columnInfos = new List<IColumnInfo>();
			foreach (var edmProperty in entitySet.ElementType.DeclaredProperties)
			{
				// Please read https://stackoverflow.com/questions/37806159/what-is-a-complex-type-in-entity-framework-and-when-to-use-it if you want to use ComplexType.
				if (edmProperty.IsComplexType)
				{
					var complexColumn = _mapping.EntityTypeMappings.Single()
						.Fragments.Single()
						.PropertyMappings.OfType<ComplexPropertyMapping>().Single(m => m.Property == edmProperty);
					columnInfos.AddRange(DecodeComplexTypes(complexColumn, clrClassType));
				}
				else
				{
					var columnName = _mapping.EntityTypeMappings.Single()
						.Fragments.Single()
						.PropertyMappings.OfType<ScalarPropertyMapping>()
						.Single(m => m.Property == edmProperty)
						.Column.Name;
					var sqlTypeName = _tableEntitySet.ElementType.DeclaredMembers
						.Single(x => x.Name == columnName).TypeUsage.EdmType.Name;
					var clrProperty = GetPublicAndPrivatePropertyByName(clrClassType, edmProperty.Name);

					string referencedTableName = null;
					foreach (var referencedTable in entitySet.ElementType.NavigationProperties.Where(x => x.GetDependentProperties().Any(p => p.Name == columnName)))
					{
						referencedTableName = referencedTable.TypeUsage.EdmType.Name;
					}
					columnInfos.Add(new ColumnInfo(columnName, sqlTypeName, edmProperty.Nullable, clrProperty, referencedTableName));
				}
			}

			return columnInfos;

		}

		private IEnumerable<ColumnInfo> DecodeComplexTypes(ComplexPropertyMapping complexMapping, Type parentClass)
		{
			//var complexCols = new List<ColumnInfo>();
			//foreach (var property in complexMapping.TypeMappings.SelectMany(x => x.PropertyMappings))
			//{
			//	var complexClrType = GetPublicAndPrivatePropertyByName(parentClass, complexMapping.Property.Name)
			//			.PropertyType;
			//	if (property.Property.IsComplexType)
			//	{
			//		complexCols.AddRange(DecodeComplexTypes((ComplexPropertyMapping)property, complexClrType));
			//	}
			//	else
			//	{
			//		var columnName = ((ScalarPropertyMapping)property).Column.Name;
			//		var sqlTypeName = _tableEntitySet.ElementType.DeclaredMembers
			//			.Single(x => x.Name == columnName).TypeUsage.EdmType.Name;
			//		var clrProperty = GetPublicAndPrivatePropertyByName(complexClrType, property.Property.Name);
			//		complexCols.Add(new ColumnInfo(columnName, sqlTypeName, property.Property.Nullable, clrProperty));
			//	}
			//}
			//return complexCols;
			throw new NotImplementedException("This version does not support comparing complex type at moment. Please implement this if you want to compare it.");
		}

		PropertyInfo GetPublicAndPrivatePropertyByName(Type classToScan, string propertyName)
		{
			var foundProperty = classToScan.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.SingleOrDefault(x => x.Name == propertyName);
			if (foundProperty == null)
				throw new InvalidOperationException(string.Format("Failed to find property called {0} in class {1}.",
					propertyName, classToScan.Name));

			return foundProperty;
		}
	}
}
