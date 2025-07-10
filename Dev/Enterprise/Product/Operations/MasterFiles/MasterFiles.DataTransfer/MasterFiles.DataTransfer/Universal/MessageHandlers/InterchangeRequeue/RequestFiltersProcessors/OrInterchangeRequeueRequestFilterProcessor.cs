using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	class OrInterchangeRequeueRequestFilterProcessor : IInterchangeRequeueRequestFilterProcessor
	{
		readonly string filterType;
		readonly SchemaColumn column;

		readonly ZDBOnlyQuery primaryQuery;
		readonly ZDBOnlySubQuery subQuery;

		public OrInterchangeRequeueRequestFilterProcessor(SchemaColumn column, string filterType)
		{
			this.column = column;
			this.filterType = filterType;
		}

		public OrInterchangeRequeueRequestFilterProcessor(Type primaryTable, SchemaColumn fkColumnInParentTable, Type relatedTable, SchemaColumn columnToFilterByInRelatedTable, string filterType) : this(columnToFilterByInRelatedTable, filterType)
		{
			primaryQuery = new ZDBOnlyQuery(primaryTable);
			subQuery = new ZDBOnlySubQuery(relatedTable, fkColumnInParentTable);
		}

		public ZQuery Process(IEnumerable<InterchangeRequeueRequestFilter> filters, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (primaryQuery != null && subQuery != null)
			{
				AppendFiltersToZQuery(subQuery, filters);

				if (subQuery.GetOrParts().Any())
				{
					primaryQuery.AddSubQuery(subQuery, JoinCondition.And);
				}

				return primaryQuery;
			}
			else
			{
				var result = new ZQuery();
				AppendFiltersToZQuery(result, filters);
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		void AppendFiltersToZQuery(ZQuery query, IEnumerable<InterchangeRequeueRequestFilter> filters)
		{
			foreach (var value in filters.Where(f => (string)f.Type == filterType).Select(f => f.Value))
			{
				query.AddToFilter(JoinCondition.Or, column, value);
			}
		}
	}
}
