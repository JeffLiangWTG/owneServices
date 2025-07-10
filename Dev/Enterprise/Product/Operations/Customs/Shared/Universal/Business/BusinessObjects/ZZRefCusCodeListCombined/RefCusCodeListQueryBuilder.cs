using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	sealed class RefCusCodeListQueryBuilder : IRefCusCodeListSqlQueryBuilder
	{
		public RefCusCodeListQueryBuilder(string languageCode, bool onlyCurrentLanguageCode)
		{
			this.languageCode = Argument.NotNull(languageCode, nameof(languageCode));
			this.onlyCurrentLanguageCode = onlyCurrentLanguageCode;
		}

		readonly string languageCode;
		readonly bool onlyCurrentLanguageCode;

		ZJoinQuery IRefCusCodeListSqlQueryBuilder.Build(ZQuery query)
		{
			var selectList = new[]
			{
				FormattableString.Invariant($"{RefCusCodeList.Schema.ZZD_Code} as {RefCusCodeListTypes.CodeFieldName}"),
				FormattableString.Invariant($"ISNULL({RefCusCodeListLanguage.Schema.ZXA_Description}, {RefCusCodeList.Schema.ZZD_Description}) as {RefCusCodeListTypes.DescriptionFieldName}")
			};

			var refCusCodeListLanguageTableName = RefCusCodeListLanguageSchema.Constants.TableName;
			var refCusCodeListSqlQuery = new ZJoinQuery($"{ZZRefCusCodeListCombinedSchema.Constants.SqlSchemaName}.{ZZRefCusCodeListCombinedSchema.Constants.TableName}", selectList);
			var cusCodeListLanguageJoinType = onlyCurrentLanguageCode ? "INNER" : "LEFT";
			refCusCodeListSqlQuery.AddJoinPart(FormattableString.Invariant($"{cusCodeListLanguageJoinType} JOIN {refCusCodeListLanguageTableName} ON ZXA_ZZD_CodeList = ZZD_PK AND ZXA_ZX6_NKLanguage = @Language"),
				new[] { ZSqlParameter.New("@Language", languageCode, RefCusCodeListLanguageSchema.ZXA_ZX6_NKLanguage) });
			if (query != null)
			{
				refCusCodeListSqlQuery.AddToFilter(query);
			}

			return refCusCodeListSqlQuery;
		}
	}
}
