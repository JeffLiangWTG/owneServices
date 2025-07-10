using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForShipping
	{
		public static ZDecimal GetRoundedValue<T>(T parent, SchemaColumn column, PropertyDescriptor property, ZDecimal value)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporterWithSchemaColumn
		{
			var roundingMode = DefaultNumberOfDecimalsSupporterHelperForShipping.GetRoundingMode(parent, property);
			var decimals = DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(parent, property);

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
