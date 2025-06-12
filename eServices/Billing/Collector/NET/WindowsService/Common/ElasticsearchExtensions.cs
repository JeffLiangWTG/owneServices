using Elastic.Clients.Elasticsearch.Sql;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common
{
	public static class ElasticsearchExtensions
	{
		public static T GetValue<T>(this QueryResponse response, string name, SqlRow row)
		{
			(Column Column, int Index) columnInfo = response.Columns.GetColumnInfo(name);
			return row[columnInfo.Index].As<T>();
		}

		public static T GetValue<T>(this QueryResponse response, string name, int rowIndex)
		{
			return response.GetValue<T>(name, response.Rows.ElementAt(rowIndex));
		}

		static (Column Column, int Index) GetColumnInfo(this IReadOnlyCollection<Column> columns, string name)
		{
			return columns.Select((c, i) => ((Column Column, int Index))new (c, i)).Single(x => x.Column.Name == name);
		}
	}
}
