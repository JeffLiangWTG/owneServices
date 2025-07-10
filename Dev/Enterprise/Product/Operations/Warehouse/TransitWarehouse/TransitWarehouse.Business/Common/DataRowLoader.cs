using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class DataRowLoader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public static DataRow[] Load(BusinessObjectFactory factory, string sql, Action<DbCommand> addParameters)
		{
			Argument.NotNull(factory, nameof(factory));

			//Referring to Enterprise\Product\Operations\Warehouse\Transactions\Warehouse.Transactions.Business\Common\DataRowLoader.cs
			using (var command = ((IDbConnected)factory).Connection.Command(sql))
			{
				addParameters(command);

				using (var reader = command.ExecuteReader())
				{
					var table = new DataTable { Locale = CultureInfo.InvariantCulture };

					using (var ds = new DataSet { Locale = CultureInfo.InvariantCulture, EnforceConstraints = false })
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
