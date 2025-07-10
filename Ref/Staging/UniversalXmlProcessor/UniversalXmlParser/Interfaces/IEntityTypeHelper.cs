using System;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Interfaces
{
	public interface IEntityTypeHelper
	{
		string GetTableCode(Type entityType);
		PropertyInfo GetPrimaryKeyProperty(Type entityType);
		List<PropertyInfo> GetProperties(Type entityType);
		PropertyInfo GetProperty(Type entityType, string propertyName);
		PropertyInfo GetForeignKeyProperty(Type entityType, string childTableCode, string parentTableCode);
	}
}
