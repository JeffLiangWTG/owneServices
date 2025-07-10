using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	public class CostsComparerEntryCollection : NonPersistentBusinessObjectCollection<CostsComparerEntry>
	{
		public CostsComparerEntryCollection(CostsComparer parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		public readonly CostsComparer Parent;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CostsComparerEntry(Parent, DummyEntry, new List<RateLine>());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public override void Load(ZQuery filter)
		{
			var allEntryPks = new List<Guid>(1000);
			var allRelatedPks = new Dictionary<Guid, Guid[]>(1000);
			var sql = BuildBulkQuery(filter);
			using (var cmd = CargoWise.Data.Db.Connection.Command(sql)) // using a "bulky query"
			{
				if (!Parent.ValidFromDate.IsEmpty)
				{
					cmd.AddParameter("@ValidFromDate", SqlDbType.DateTime, Parent.ValidFromDate.ToDateTime());
				}

				if (!Parent.ValidToDate.IsEmpty)
				{
					cmd.AddParameter("@ValidToDate", SqlDbType.DateTime, Parent.ValidToDate.ToDateTime());
				}

				cmd.AddParameters(filter.ParameterisedText.Parameters);

				cmd.CommandTimeout = 60 * 60; // allow 60 minutes - comparing lots of rates will still take a while
				using (var reader = cmd.ExecuteReader())
				{
					// Process the list of (entry PK, related PK)
					// It is ordered by entryPK so we can process one entryPk at a time.
					// When a new entryPk value is found we add all the relate PKs that have been accumulated for the last entryPK to allRelatedPks.
					var currentRelatedPks = new List<Guid>(10);
					var relatedPk = Guid.Empty;
					var lastEntryPk = Guid.Empty;
					while (reader.Read())
					{
						var entryPk = reader.GetGuid(0);
						if (!Parent.SingleChargeCodeComparisonOnly)
						{
							var relatedValue = reader.GetValue(1);
							relatedPk = relatedValue is Guid ? (Guid)relatedValue : Guid.Empty;
						}

						if (entryPk != lastEntryPk)
						{
							// Found a new entryPK.
							// Process the last entry PK now we have accumulated all related PKs.
							if (lastEntryPk != Guid.Empty)
							{
								AddEntryAndRelatedPks(allEntryPks, allRelatedPks, lastEntryPk, currentRelatedPks);
							}
							lastEntryPk = entryPk;
						}

						if (relatedPk != Guid.Empty)
						{
							// Accumulate current set of related PKs
							currentRelatedPks.Add(relatedPk);
							Factory.AddFetchHint(RateEntrySchema.Constants.TableName, relatedPk);
						}
					}

					// Process the very last entryPk
					if (lastEntryPk != Guid.Empty)
					{
						AddEntryAndRelatedPks(allEntryPks, allRelatedPks, lastEntryPk, currentRelatedPks);
					}
				}
			}

			foreach (var pk in allEntryPks)
			{
				var entry = Factory.Load<RateEntry>(pk);
				Guid[] relatedPks = null;
				if (!Parent.SingleChargeCodeComparisonOnly)
				{
					allRelatedPks.TryGetValue(entry.PK.ToGuid(), out relatedPks);
				}

				var rateLines = GetEntryRateLinesInclRelated(entry, relatedPks);
				Add(new CostsComparerEntry(Parent, entry, rateLines));
			}
		}

		void AddEntryAndRelatedPks(List<Guid> allEntryPks, Dictionary<Guid, Guid[]> allRelatedPks, Guid entryPk, List<Guid> relatedPks)
		{
			allEntryPks.Add(entryPk);
			Factory.AddFetchHint(RateEntrySchema.Constants.TableName, entryPk);
			if (relatedPks.Count > 0)
			{
				allRelatedPks.Add(entryPk, relatedPks.ToArray());
				relatedPks.Clear();
			}
		}

		#region SuppressResourceStringsCheckRegion

		/// <summary>
		/// Returns a query to load all pairs of entry PK, related PK in one command.
		/// </summary>
		string BuildBulkQuery(ZQuery filter)
		{
			var selectTopString = filter.MaximumRows.HasValue
				? string.Format(CultureInfo.InvariantCulture, "TOP {0} ", filter.MaximumRows)
				: string.Empty;

			var sql = new StringBuilder();
			sql.AppendLine(Invariant($@"CREATE TABLE #CostsComparerTable
(
	MainEntryPK UNIQUEIDENTIFIER,
	MainHeaderPK UNIQUEIDENTIFIER,
	MainOrigin VARCHAR(5) COLLATE DATABASE_DEFAULT,
	MainDestination VARCHAR(5) COLLATE DATABASE_DEFAULT
)
INSERT INTO #CostsComparerTable
SELECT {selectTopString}
	{RateEntrySchema.Constants.PK} AS MainEntryPK,
	{RateEntrySchema.Constants.TI_TH} AS MainHeaderPK,
	{RateEntrySchema.Constants.TI_OriginLRC} AS MainOrigin,
	{RateEntrySchema.Constants.TI_DestinationLRC} AS MainDestination
FROM
{RateEntrySchema.Constants.SqlSchemaName}.{RateEntrySchema.Constants.TableName}
WHERE
{filter.ParameterisedText.ParameterisedQueryText}"));

			if (!Parent.SingleChargeCodeComparisonOnly)
			{
				sql.AppendLine(Invariant($@"SELECT MainEntryPK, relatedEntry.{RateEntrySchema.Constants.PK}
FROM #CostsComparerTable
JOIN {RateEntrySchema.Constants.SqlSchemaName}.{RateEntrySchema.Constants.TableName} mainEntry ON MainEntryPK = mainEntry.{RateEntrySchema.Constants.PK}"));

				AddRelatedLocationJoin(sql, "MainOrigin", "RelatedOrigin", Parent.Origin);
				AddRelatedLocationJoin(sql, "MainDestination", "RelatedDestination", Parent.Destination);

				sql.AppendLine(Invariant($@"LEFT JOIN {RateEntrySchema.Constants.SqlSchemaName}.{RateEntrySchema.Constants.TableName} relatedEntry ON MainHeaderPK = relatedEntry.{RateEntrySchema.Constants.TI_TH} AND MainEntryPK != relatedEntry.{RateEntrySchema.Constants.PK}
	AND RelatedOrigin = relatedEntry.{RateEntrySchema.Constants.TI_OriginLRC}
	AND RelatedDestination = relatedEntry.{RateEntrySchema.Constants.TI_DestinationLRC}"));

				AddModeSql(sql);
				AddDateFilter(sql);
				AddColumnFilter(sql, RateEntrySchema.TI_ViaLRC);
				BuildContainerSql(sql);
				AddColumnFilter(sql, RateEntrySchema.TI_OH_TransportProvider);
				AddColumnFilter(sql, RateEntrySchema.TI_PL_NKCarrierServiceLevel);
				AddColumnFilter(sql, RateEntrySchema.TI_RH_NKCommodityCode);
				AddColumnFilter(sql, RateEntrySchema.TI_ContractNumber);
				AddColumnFilter(sql, RateEntrySchema.TI_OH_Consignee);
				AddColumnFilter(sql, RateEntrySchema.TI_OH_Consignor);
				AddColumnFilter(sql, RateEntrySchema.TI_OH_ControllingCustomer);
				sql.AppendLine($@"GROUP BY MainEntryPK, relatedEntry.{RateEntrySchema.Constants.PK}
ORDER BY MainEntryPK");
			}
			else
			{
				sql.AppendLine("SELECT MainEntryPK FROM #CostsComparerTable");
			}

			sql.AppendLine("\r\nDROP TABLE #CostsComparerTable");

			return sql.ToString();
		}

		void AddRelatedLocationJoin(StringBuilder sql, string mainLocationName, string relatedLocationName, ZString userLocation)
		{
			// This is the core of the opitmization.
			// Join on a result set of all related locations that can match the main entry location.
			// I.e. (location, user location, location country, location zones)
			// This set is expected to be small.
			// Then we can join this set very efficiently on the related location.

			var unionString = !userLocation.IsEmpty
				? Invariant($@" UNION ALL SELECT MainEntryPK AS pk, '{userLocation}' AS {relatedLocationName} FROM #CostsComparerTable WHERE {mainLocationName} != '{userLocation}'")
				: string.Empty;

			sql.AppendLine(Invariant($@"JOIN
(
	SELECT MainEntryPK AS pk, {mainLocationName} AS {relatedLocationName} FROM #CostsComparerTable
	UNION ALL
	SELECT MainEntryPK AS pk, FZ_Code AS {relatedLocationName} FROM #CostsComparerTable JOIN dbo.vw_ZoneUNLOCO ON {mainLocationName} = RL_Code
	UNION ALL
	SELECT MainEntryPK AS pk, FZ_Code AS {relatedLocationName} FROM #CostsComparerTable JOIN dbo.vw_ZoneCountry ON {mainLocationName} = RN_Code
	UNION ALL
	SELECT MainEntryPK AS pk, LEFT({mainLocationName}, 2) AS {relatedLocationName} FROM #CostsComparerTable WHERE LEN({mainLocationName}) = 5
	UNION ALL
	SELECT MainEntryPK AS pk, '' AS {relatedLocationName} FROM #CostsComparerTable WHERE {mainLocationName} != ''
	{unionString}
	) {relatedLocationName}s ON MainEntryPK = {relatedLocationName}s.pk"));
		}

		void AddModeSql(StringBuilder sql)
		{
			if (!Parent.Mode.IsEmpty)
			{
				var freightMode = (FreightMode)Enum.Parse(typeof(FreightMode), Parent.Mode);
				var filter = BuildModeSql(freightMode);
				sql.AppendLine(Invariant($@" AND({filter.LiteralTextSqlFormatted.Replace($"{RateEntrySchema.Constants.Prefix}_", $"relatedEntry.{RateEntrySchema.Constants.Prefix}_").Replace("\n", "\n\t")})"));
			}
		}

		void AddColumnFilter(StringBuilder sql, SchemaColumn column)
		{
			var isNullOrEmptyString = column.IsNullable
				? "is null"
				: " = ''";
			sql.AppendLine(Invariant($" AND (relatedEntry.{column.Name} {isNullOrEmptyString} or relatedEntry.{column.Name} = mainEntry.{column.Name})"));
		}

		void BuildContainerSql(StringBuilder sql)
		{
			var parentContainerPK = Parent.Container?.PK ?? ZGuid.Empty;

			sql.AppendLine(Invariant($@"
	AND
	(
		relatedEntry.{RateEntrySchema.TI_RC.Name} is null OR
		relatedEntry.{RateEntrySchema.TI_RC.Name} = '{parentContainerPK}' OR
		('{parentContainerPK}' = '{ZGuid.Empty}' AND relatedEntry.{RateEntrySchema.TI_RC.Name} = mainEntry.{RateEntrySchema.TI_RC.Name}) OR
		(
			relatedEntry.{RateEntrySchema.TI_MatchContainerRateClass.Name} = 1 AND
			(
				relatedEntry.{RateEntrySchema.TI_RateCategory.Name} IN ('ORG', 'DST') AND
				relatedEntry.{RateEntrySchema.TI_RC.Name} IN
				(
					SELECT RC_PK FROM {RefContainerSchema.Constants.SqlSchemaName}.{RefContainerSchema.Constants.TableName}
					WHERE
						{RefContainerSchema.RC_HandlingRateClass.Name} != '' AND
						{RefContainerSchema.RC_HandlingRateClass.Name} = (
							SELECT TOP 1 {RefContainerSchema.RC_HandlingRateClass.Name} FROM {RefContainerSchema.Constants.SqlSchemaName}.{RefContainerSchema.Constants.TableName}
							WHERE
								RC_PK = 
								case 
									when
										'{parentContainerPK}' = '{ZGuid.Empty}' then (SELECT mainEntry.TI_RC)
									else
										(SELECT '{parentContainerPK}')
								end
						)
				)
			)
		)
	)"));
		}

		#endregion

		#region SummaryItemCount

		List<RateLine> GetEntryRateLinesInclRelated(RateEntry entry, Guid[] relatedEntryPks)
		{
			var result = new List<RateLine>();

			foreach (RateLine currentRateLine in entry.RateLines)
			{
				result.Add(currentRateLine);
			}

			if (!this.Parent.SingleChargeCodeComparisonOnly && relatedEntryPks != null)
			{
				foreach (var pk in relatedEntryPks)
				{
					var relatedEntry = Factory.Load<RateEntry>(pk);
					foreach (RateLine relatedRateLine in relatedEntry.RateLines)
					{
						if (!(relatedRateLine.Uses(CalculatorType.Cartage) || relatedRateLine.Uses(CalculatorType.CartageZoneDistance)))
						{
							result.Add(relatedRateLine);
						}
					}
				}
				new Remover(entry).Remove(result);
			}
			return result;
		}

		#endregion

		#region Filter

		#region Rate Entry Mode

		ZQuery BuildModeSql(FreightMode freightMode)
		{
			var result = new ZQuery();
			if (Parent.ShowAllCharges)
			{
				result.AddToFilter(RatingHelper.GetFreightModeAndFCL_LCLExclusiveFilter(freightMode));
				if (Parent.ShowOriginDestination)
				{
					var odFilter = new ZQuery();
					odFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, RatingConstants.RateCategory.ORG);
					odFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, RatingConstants.RateCategory.DST);

					result.AddToFilter(new ZQuery(odFilter, RatingHelper.GetOriginDestinationModeExclusiveFilter(freightMode)), JoinCondition.Or);
				}
			}
			else if (Parent.ShowOriginChargesOnly || Parent.ShowDestinationChargesOnly)
			{
				result.AddToFilter(RateEntrySchema.TI_RateCategory, Parent.ShowOriginChargesOnly ? RatingConstants.RateCategory.ORG : RatingConstants.RateCategory.DST);
				result.AddToFilter(RatingHelper.GetOriginDestinationModeExclusiveFilter(freightMode));
			}
			else
			{
				throw new NotSupportedException("Either All Charges or only Origin/Dest can be shown");
			}

			return result;
		}

		#endregion

		#region Date

		#region SuppressResourceStringsCheckRegion

		const string MainEntrySqlPrefix = "mainEntry.";
		const string RelatedEntrySqlPrefix = "relatedEntry.";

		void AddDateFilter(StringBuilder sql)
		{
			var startDateField = RelatedEntrySqlPrefix + RateEntrySchema.Constants.TI_RateStartDate;
			var endDateField = RelatedEntrySqlPrefix + RateEntrySchema.Constants.TI_RateEndDate;

			var mainStartDate = MainEntrySqlPrefix + RateEntrySchema.Constants.TI_RateStartDate;
			var mainEndDate = MainEntrySqlPrefix + RateEntrySchema.Constants.TI_RateEndDate;

			var validFromDateString = !Parent.ValidFromDate.IsEmpty
				? Invariant($" ({endDateField} >= @ValidFromDate OR {endDateField} IS NULL) AND ")
				: string.Empty;

			var validToDateString = !Parent.ValidToDate.IsEmpty
				? Invariant($" ({startDateField} < @ValidToDate OR {startDateField} IS NULL) AND ")
				: string.Empty;

			sql.AppendLine(Invariant($@"	AND
	(
		{validFromDateString}
		{validToDateString}
		({mainEndDate} Is NULL OR {startDateField} IS NULL OR {mainEndDate} >= {startDateField})
		AND
		({endDateField} IS NULL OR {mainStartDate} IS NULL OR {endDateField} >= {mainStartDate})
	)"));
		}

		#endregion

		#endregion

		#endregion

		#region RemoveOverriddenRateLines

		class Remover : SimpleOverriddenRateLinesRemover
		{
			public Remover(RateEntry parent)
				: base(null)
			{
				this.parent = parent;
			}

			readonly RateEntry parent;

			internal override LinkedList<BaseRateLineComparer> GetComparers()
			{
				var result = base.GetComparers();
				result.AddLast(new TransitTimeOrFrequencyComparer(parent));

				return result;
			}
		}

		#endregion

		#region DummyEntry

		RateEntry DummyEntry
		{
			get
			{
				if (fDummyEntry == null)
				{
					var dummyRate = (new BusinessObjectFactory()).New<ClientRate>();
					fDummyEntry = dummyRate.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.AddNew();
				}

				return fDummyEntry;
			}
		}

		RateEntry fDummyEntry;

		#endregion

		#region Test Helpers

#if DEBUG
		public bool Contains(RateEntry entry)
		{
			foreach (CostsComparerEntry comparerEntry in this)
			{
				if (comparerEntry.Entry.PK == entry.PK)
				{
					return true;
				}
			}

			return false;
		}
#endif

		#endregion
	}
}
