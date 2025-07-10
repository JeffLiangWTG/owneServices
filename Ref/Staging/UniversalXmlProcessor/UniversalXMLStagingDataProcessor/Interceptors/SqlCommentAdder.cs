using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class SqlCommentAdder
	{
		public const string PrefixCommentTemplate = "Measure Performance metrics : {0} --";

		// '--' is the symbol that EF Core uses to add comments to SQL queries, see https://learn.microsoft.com/en-us/ef/core/querying/tags
		public const string PrefixCommentFlag = "-- Measure Performance metrics";

		SqlCommentAdder(SqlCommentAdderBuilder builder)
		{
			sql = builder.Sql;
			id = builder.Id;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, $"-- {PrefixCommentTemplate}{Environment.NewLine}{sql}", id);
		}

		readonly string sql;
		readonly string id;

		public static SqlCommentAdderBuilder Create()
		{
			return new SqlCommentAdderBuilder();
		}

		public class SqlCommentAdderBuilder
		{
			internal string Sql;
			internal string Id;

			public SqlCommentAdderBuilder WithSql(string sql)
			{
				Sql = sql;
				return this;
			}

			public SqlCommentAdderBuilder WithId(string id)
			{
				Id = id;
				return this;
			}

			public SqlCommentAdder Build()
			{
				return new SqlCommentAdder(this);
			}
		}
	}
}
