using System;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class NumericalCheckHelper
	{
		public static (Decimal result, ZString message) GetValidDecimalValue(SchemaDecimalColumn column, ZDecimal value)
		{
			/// <summary>
			/// Check if the number exceeds the maximum value limit in the database. If so, return the maximum value allowed for the field in the database.
			/// </summary>
			/// <returns>
			/// - result: Valid result.
			/// - message: If the number exceeds the maximum value limit in the database, the message will be generated.
			/// </returns>
			var message = ZString.Empty;
			if (!value.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale))
			{
				var maxValue = (decimal)Math.Pow(10, column.Precision - column.Scale) - 1;
				message = Res.GetString("ce93aee5-ea6a-4a2e-b32b-9088c106e871",
					"Attempted to insert '{0}' into Field [{1}] which has a maximum numeric value of '{2}'. Field was truncated to the max value.",
					value.ToString(), column.Name, maxValue);
				value = maxValue;
			}
			return (value, message);
		}
	}
}
