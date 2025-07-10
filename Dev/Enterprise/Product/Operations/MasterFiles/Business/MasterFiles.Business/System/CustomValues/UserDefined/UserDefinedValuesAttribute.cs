namespace Enterprise.MasterFiles.Business.CustomValues
{
	using System;
	using CargoWise.Common;
	using CargoWise.Common.Collections;
	using CargoWise.EntityFramework;

	/// <summary>
	/// Allows a business object to support UserDefinedValues (stored in GenCustomAddOnValue)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class UserDefinedValuesAttribute : Attribute
	{
		public static bool IsEnabled(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");

			var businessObjectType = businessObject.GetType();

			if (userDefinedValues == null)
			{
				userDefinedValues = new LRUCache<Type, bool?>();
			}

			bool result;
			bool? cacheValue = userDefinedValues[businessObjectType];
			if (cacheValue.HasValue)
			{
				result = cacheValue.Value;
			}
			else
			{
				result = businessObjectType.GetCustomAttributes(typeof(UserDefinedValuesAttribute), true).Length > 0;
				userDefinedValues.Add(businessObjectType, result);
			}
			return result;
		}

		[ThreadStatic]
		static LRUCache<Type, bool?> userDefinedValues;
	}
}


