using System.Collections;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Need to use this because ADO.NET blows up when you have too many parameters. This generator now caches similar parameters using a hashtable
	/// to reduce the number of total params.
	/// </summary>
	public sealed class SimilarOrgsParameterGenerator
	{
		public SimilarOrgsParameterGenerator(Hashtable tableOfParameters)
		{
			ParameterValues = tableOfParameters ?? new Hashtable();
		}

#if DEBUG
		internal
#endif
 readonly Hashtable ParameterValues;

		public ZSqlParameter GetParameterForValue(SchemaColumn schemaColumn, string columnValue)
		{
			return GetParameterForValue(schemaColumn, columnValue, SQLComparisonOperator.Equal);
		}

		public ZSqlParameter GetParameterForValue(SchemaColumn schemaColumn, string columnValue, SQLComparisonOperator sqlOperator)
		{
			ZSqlParameter result = (ZSqlParameter)ParameterValues[schemaColumn.Name + columnValue];
			if (result == null)
			{
				ComparisonOptions options = ComparisonOptions.Default;
				if (schemaColumn.SqlDbType == SqlDbType.NVarChar || schemaColumn.SqlDbType == SqlDbType.NChar)
				{
					options = ComparisonOptions.NationalLanguage;
				}

				result = ZSqlParameter.New("@" + schemaColumn.Name + "_" + ParameterValues.Count + "_", columnValue, schemaColumn, sqlOperator, options);

				ParameterValues[schemaColumn.Name + columnValue] = result;
			}

			return result;
		}
	}
}
