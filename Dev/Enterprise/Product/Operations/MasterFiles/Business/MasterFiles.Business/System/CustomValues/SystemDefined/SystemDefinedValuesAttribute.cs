namespace Enterprise.MasterFiles.Business.CustomValues
{
	using System;
	using CargoWise.Common;
	using CargoWise.Common.Collections;
	using CargoWise.EntityFramework;

	/// <summary>
	/// Allows a business object to support SystemDefinedValues (stored in GenAddOnColumn)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class SystemDefinedValuesAttribute : Attribute
	{
		public static bool IsEnabled(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var businessObjectType = businessObject.GetType();

			bool result;

			if (systemDefinedValues == null)
			{
				systemDefinedValues = new LRUCache<Type, bool?>();
			}
			bool? cacheValue = systemDefinedValues[businessObjectType];
			if (cacheValue.HasValue)
			{
				result = cacheValue.Value;
			}
			else
			{
				result = businessObjectType.GetCustomAttributes(typeof(SystemDefinedValuesAttribute), true).Length > 0;
				systemDefinedValues.Add(businessObjectType, result);
			}
			return result;
		}

		[ThreadStatic]
		static LRUCache<Type, bool?> systemDefinedValues;
	}
}

