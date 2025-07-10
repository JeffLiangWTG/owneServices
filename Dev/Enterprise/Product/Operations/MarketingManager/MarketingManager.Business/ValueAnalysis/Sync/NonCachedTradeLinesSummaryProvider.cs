using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class NonCachedTradeLinesSummaryProvider : TradeLinesSummaryProviderCommon
	{
		public NonCachedTradeLinesSummaryProvider(DbConnection connection, bool includeActuals = true, bool includeProspect = true)
			: base(connection, includeActuals, includeProspect)
		{
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override DynamicBusinessObjectCollection GetRawCollection(IOrgHeader org, TradeLinesSynchronizationRange syncRange)
		{
			var parameters = new ZSqlParameterCollection
			{
				{ FromParamName, syncRange.From, Schema.GenericDateTimeColumn },
				{ ToParamName, syncRange.To, Schema.GenericDateTimeColumn },
				{ OrgPkParamName, org.PK, Schema.GenericPkColumn }
			};

			using (var command = Connection.Command(CreateTradeLinesSummaryCacheQuery_TableDeclaration))
			{
				command.ExecuteNonQuery();

				command.AddParameters(parameters);

				var populateTradeLinesSummaryCacheQueries = CreatePopulateTradeLinesSummaryCacheQuery(syncRange.IncludeActuals, syncRange.IncludeProspect, OrgParameterType.SingleValue);
				foreach (var query in populateTradeLinesSummaryCacheQueries)
				{
					command.CommandText = query.Item2;
					command.ExecuteNonQuery();
				}
			}

			var sql = FormattableString.Invariant($@"SELECT * FROM {TradeLinesSummaryProviderCommon.TradeLineCacheTableName} WHERE MainOrg = '{org.PK}'");
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory(Connection));
			collection.Load(sql, parameters);

			var dropTableSql = string.Format(CultureInfo.InvariantCulture, "DROP TABLE {0};", TradeLinesSummaryProviderCommon.TradeLineCacheTableName);

			using (var command = Connection.Command(dropTableSql))
			{
				command.ExecuteNonQuery();
			}

			return collection;
		}
	}
}
