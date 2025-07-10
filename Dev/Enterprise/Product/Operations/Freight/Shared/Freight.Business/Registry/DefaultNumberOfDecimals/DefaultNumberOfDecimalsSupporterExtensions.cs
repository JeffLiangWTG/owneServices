using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class DefaultNumberOfDecimalsSupporterExtensions
	{
		public static ZDecimal GetRoundedValue(this IDefaultNumberOfDecimalsSupporterWithSchemaColumn supporter, SchemaColumn column, ZPropertyInfo propertyInfo, ZDecimal value)
		{
			return supporter.GetRoundedValue(column, propertyInfo.PropertyDescriptor, value);
		}

		public static void SetRoundedValue(this IDefaultNumberOfDecimalsSupporterWithSchemaColumn supporter, SchemaColumn column, ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = supporter.GetRoundedValue(column, propertyInfo.PropertyDescriptor, (ZDecimal)propertyInfo.Value);
		}

		public static ZDecimal GetRoundedValue(this IDefaultNumberOfDecimalsSupporter supporter, ZPropertyInfo propertyInfo, ZDecimal value)
		{
			return supporter.GetRoundedValue(propertyInfo.PropertyDescriptor, value);
		}

		public static void SetRoundedValue(this IDefaultNumberOfDecimalsSupporter supporter, ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = supporter.GetRoundedValue(propertyInfo.PropertyDescriptor, (ZDecimal)propertyInfo.Value);
		}
	}
}
