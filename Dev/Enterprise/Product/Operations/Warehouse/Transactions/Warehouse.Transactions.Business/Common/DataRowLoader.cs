using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class DataRowLoader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public static DataRow[] Load(BusinessObjectFactory factory, string sql, Action<DbCommand> addParameters)
		{
			Argument.NotNull(factory, nameof(factory));

			// This code will involve loading many rows, from my profiling this code below is substantially faster (~4x) than DynamicBusinessObjectCollection or our own Adapters.
			// I have manually added smart parameterisation which is a big benefit we get from using DynamicBusinessObjectCollection.
			using (var command = ((IDbConnected)factory).Connection.Command(sql))
			{
				addParameters(command);

				using (var reader = command.ExecuteReader())
				{
					var table = new DataTable { Locale = CultureInfo.InvariantCulture };

					using (var ds = new DataSet { Locale = CultureInfo.InvariantCulture, EnforceConstraints = false }) // See comment above, avoiding DynamicBusinessObjectCollection here speeds up this performance critical code by 4x
					{
						ds.Tables.Add(table);
						table.BeginLoadData();
						table.Load(reader);
						table.EndLoadData();
						ds.Tables.Remove(table);
					}

					return table.Rows.Cast<DataRow>().ToArray();
				}
			}
		}

		public static Guid? GetNullableGuid(object value) => value == DBNull.Value ? null : (Guid)value;

		public static string GetNullableString(object value) => value == DBNull.Value ? string.Empty : (string)value;

		public static DateTime? GetNullableDateTimeFromDateTimeOffset(object value) => value == DBNull.Value ? null : ((DateTimeOffset)value).DateTime;
	}
}
