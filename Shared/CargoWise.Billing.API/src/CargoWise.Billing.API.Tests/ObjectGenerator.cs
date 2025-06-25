using System;

namespace CargoWise.Billing.Tests.Common
{
	public static class ObjectGenerator
	{
		public static T CreateWithAllPropertiesSet<T>()
			where T : new()
		{
			var result = new T();
			foreach (var property in typeof(T).GetProperties())
			{
				var propertyName = property.Name;
				var propertyTypeName = property.PropertyType.Name;
				switch (propertyTypeName)
				{
					case "String":
						property.SetValue(result, propertyName);
						break;
					case "Int32":
						property.SetValue(result, propertyName.Length);
						break;
					case "DateTime":
						property.SetValue(result, DateTime.UtcNow);
						break;
					default:
						throw new Exception(string.Format("Unexpected property type [{0}]. Add correspondent case and populate data.", propertyTypeName));
				}
			}
			return result;
		}
	}
}