using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	sealed class RefCusCodeListTransportModeQueryBuilder : IRefCusCodeListSqlQueryBuilder
	{
		public RefCusCodeListTransportModeQueryBuilder(IRefCusCodeListSqlQueryBuilder queryBuilder, string transportMode)
		{
			this.queryBuilder = Argument.NotNull(queryBuilder, nameof(queryBuilder));
			this.transportMode = Argument.NotNullOrEmpty(transportMode, nameof(transportMode));
		}
		readonly IRefCusCodeListSqlQueryBuilder queryBuilder;
		readonly string transportMode;

		ZJoinQuery IRefCusCodeListSqlQueryBuilder.Build(ZQuery query)
		{
			var tableName = RefCusCodeOrAttributeTransportModeSchema.Constants.TableName;
			var refCusCodeListSqlQuery = queryBuilder.Build(query);
			var joinParameters = new[] { ZSqlParameter.New("@TransportMode", transportMode, RefCusCodeOrAttributeTransportModeSchema.ZZU_TransportMode) };
			refCusCodeListSqlQuery.AddJoinPart(FormattableString.Invariant($"LEFT JOIN {tableName} SpecificTransportMode ON SpecificTransportMode.ZZU_ZZD_CodeList = ZZD_PK AND SpecificTransportMode.ZZU_TransportMode = @TransportMode"), joinParameters);
			refCusCodeListSqlQuery.AddJoinPart(FormattableString.Invariant($"OUTER APPLY (SELECT TOP 1 ZZU_ZZD_CodeList FROM {tableName} WHERE ZZU_ZZD_CodeList = ZZD_PK) AnyTransportMode"));
			refCusCodeListSqlQuery.AddFilterAndZSQLParameterCollection("SpecificTransportMode.ZZU_TransportMode IS NOT NULL OR AnyTransportMode.ZZU_ZZD_CodeList IS NULL", null, JoinCondition.And);
			return refCusCodeListSqlQuery;
		}
	}
}
