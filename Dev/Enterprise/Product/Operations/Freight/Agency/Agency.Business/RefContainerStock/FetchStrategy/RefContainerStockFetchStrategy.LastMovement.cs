using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	partial class RefContainerStockFetchStrategy
	{
		sealed class LastMovementFetchHint : IFetchHint
		{
			public LastMovementFetchHint(ZGuid stockPK)
			{
				this.stockPK = stockPK;
			}

			public string BuilderKey
			{
				get { return "LastMovementFetchHint"; }
			}

			public void GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					builder.Init(directSQL, "@E9_R6", JobContainerMoveSchema.E9_R6);
				}

				builder.AddValue(stockPK);
			}

			public IQueryHashKey GetHashKeyObject()
			{
				return new FetchHint.EnumerableHashObject { "LAST:", stockPK }; // hard-coded constant
			}

			public ZQuery GetQuery()
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, null);
				filter.AddToFilter(JobContainerMoveSchema.E9_R6, stockPK);
				filter.OrderBy = JobContainerMoveSchema.Constants.E9_MovementDate + " DESC";
				filter.MaximumRows = 1;

				return filter;
			}

			public bool IsDataHintLoaded { get; set; }

			public bool IsNeeded(QueryHistoryProvider historyProvider)
			{
				return !historyProvider.IsQueryCached(TableName, GetQuery());
			}

			public IEnumerable<SchemaColumn> LoadWithBlobs
			{
				get { return System.Array.Empty<SchemaColumn>(); }
			}

			public string TableName
			{
				get { return JobContainerMoveSchema.Constants.TableName; }
			}

			#region SQL

			readonly string directSQL = string.Format(CultureInfo.InvariantCulture,
				"{0} in " +
				"( " +
					"select {0} " +
					"from " +
					"( " +
						"select {0}, ROW_NUMBER() over (partition by {1} order by {1}, {2} desc) as [index] " +
						"from {3} " +
						"where {2} is not null and {1} in (SELECT Value FROM @E9_R6) " +
					") bob " +
					"where [index] = 1 " +
				") " +
				"", JobContainerMoveSchema.Constants.PK, JobContainerMoveSchema.Constants.E9_R6, JobContainerMoveSchema.Constants.E9_MovementDate, JobContainerMoveSchema.Constants.TableName);

			#endregion

			readonly ZGuid stockPK;
		}
	}
}
