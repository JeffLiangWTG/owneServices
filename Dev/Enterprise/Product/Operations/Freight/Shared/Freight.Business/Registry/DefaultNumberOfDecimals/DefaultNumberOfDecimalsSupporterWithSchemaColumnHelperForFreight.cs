using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public static class DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight
	{
		public static ZDecimal GetRoundedValue<T>(T parent, SchemaColumn column, PropertyDescriptor property, ZDecimal value)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporterWithSchemaColumn
		{
			var roundingMode = DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundingMode(parent, property);
			var decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(parent, property);

			var result = DefaultNumberOfDecimals.GetRoundedValue(value, roundingMode, decimals);

			if (column is SchemaDecimalColumn decimalColumn
				&& !result.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale)
				&& value.IsWithinSqlPrecisionAndScale(decimalColumn.Precision, decimalColumn.Scale))
			{
				result = DefaultNumberOfDecimals.GetRoundedValue(value, RoundingModes.Down, decimals);
			}

			return result;
		}
	}
}
