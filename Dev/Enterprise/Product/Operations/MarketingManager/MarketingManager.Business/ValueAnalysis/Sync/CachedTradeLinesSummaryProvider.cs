using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.ServiceTasks;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class CachedTradeLinesSummaryProvider : TradeLinesSummaryProviderCommon
	{
		public CachedTradeLinesSummaryProvider(DbConnection connection, ZDate from, ZDate to, ILogger serviceLogger = null, bool includeActuals = true, bool includeProspect = true, IEnumerable<Guid> orgPks = null)
			: base(connection, includeActuals, includeProspect)
		{
			this.OrgPks = orgPks;
			CacheFrom = from;
			CacheTo = to;

			this.tradeLineTableCache = new TradeLinesSummaryTemporaryTableCache(this, TradeLineCacheTableName, CreateTradeLinesSummaryCacheQuery_TableDeclaration, "", includeActuals, includeProspect, serviceLogger);
		}

		readonly TemporaryTableCache tradeLineTableCache;

		public readonly ZDate CacheFrom;
		public readonly ZDate CacheTo;
		public readonly IEnumerable<Guid> OrgPks;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int createCacheCommandTimeoutMinutes = 360;

		#region Getting Trade Status

		[SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Part of exception message.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of exception message., Part of SQL expression.")]
		protected override DynamicBusinessObjectCollection GetRawCollection(IOrgHeader org, TradeLinesSynchronizationRange syncRange)
		{
			var from = syncRange.From;
			var to = syncRange.To;

			if (from < CacheFrom)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "'from' ({0}) must be equal to or greater than 'CacheFrom' ({1})", from.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture), CacheFrom.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)));
			}

			if (to > CacheTo)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "'to' ({0}) must be equal to or less than 'CacheTo' ({1})", to.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture), CacheTo.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)));
			}

			var selectSql = new ZStringBuilder(@"
SELECT *
FROM <tablename>
WHERE
	MainOrg = @Org");

			var parameters = new ZSqlParameterCollection
			{
				{ "@Org", org.PK, OrgHeaderSchema.PK }
			};

			if (from != CacheFrom)
			{
				selectSql.AppendLine(" AND (PeriodStart IS NULL OR PeriodStart >= @From)");
				parameters.Add("@From", from, JobShipmentSchema.JS_SystemCreateTimeUtc);
			}

			if (to != CacheTo)
			{
				selectSql.AppendLine(" AND (PeriodStart IS NULL OR PeriodStart <= @To)");
				parameters.Add("@To", to, JobShipmentSchema.JS_SystemCreateTimeUtc);
			}

			return tradeLineTableCache.Load(selectSql.ToString(), parameters);
		}

		#endregion

		#region Cache management

		public void BuildCache()
		{
			tradeLineTableCache.EnsureCacheTableCreated();
		}

		public void ResetFactory()
		{
			this.factory = new BusinessObjectFactory(Connection);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			tradeLineTableCache.Dispose();
			base.Dispose(isDisposing);
		}

		#endregion

		#region Classes

		class TradeLinesSummaryTemporaryTableCache : TemporaryTableCache
		{
			public TradeLinesSummaryTemporaryTableCache(CachedTradeLinesSummaryProvider provider, string temporaryTableName, string createTableQuery, string dropTableQuery, bool includeActuals, bool includeProspect, ILogger serviceLogger = null)
				: base(provider.Connection, temporaryTableName, createTableQuery, dropTableQuery, createCacheCommandTimeoutMinutes)
			{
				this.provider = provider;
				this.includeActuals = includeActuals;
				this.includeProspect = includeProspect;
				this.serviceLogger = serviceLogger;
			}

			readonly CachedTradeLinesSummaryProvider provider;
			readonly bool includeActuals;
			readonly bool includeProspect;
			readonly ILogger serviceLogger;

			[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
			[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			protected override void CreateTableIfNeededCore()
			{
				base.CreateTableIfNeededCore();

				// population of temp table uses parameters so will be executed in sp_executesql and therefore needs to be in a separate call from the table declaration
				var orgParamType = provider.OrgPks == null || !provider.OrgPks.Any() ? OrgParameterType.None : OrgParameterType.TableValue;
				var createIndexSql = string.Format(CultureInfo.InvariantCulture, "CREATE NONCLUSTERED INDEX [TradeLinesSummaryCache_MainOrg] ON [{0}] ([MainOrg])", TradeLineCacheTableName);

				using (var command = connection.Command(createIndexSql, createCacheCommandTimeoutMinutes * 60))
				{
					command.AddParameter("@From", System.Data.SqlDbType.Date, provider.CacheFrom.ToDateTime());
					command.AddParameter("@To", System.Data.SqlDbType.Date, provider.CacheTo.ToDateTime());

					if (orgParamType == OrgParameterType.TableValue)
					{
						command.AddParameter(ZSqlParameter.New(TradeLinesSummaryProviderCommon.OrgPkParamName, provider.OrgPks, CargoWise.Schema.Schema.GenericPkColumn, true));
					}

					var populateTradeLinesSummaryCacheQueries = CreatePopulateTradeLinesSummaryCacheQuery(includeActuals, includeProspect, orgParamType);
					foreach (var query in populateTradeLinesSummaryCacheQueries)
					{
						command.CommandText = query.Item2;
						command.ExecuteNonQuery();
						serviceLogger?.Log(LogType.Debug, FormattableString.Invariant($"{query.Item1} Cache built"));
					}

					command.CommandText = createIndexSql;
					command.ExecuteNonQuery();
				}
			}
		}

		#endregion
	}
}
