using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	[Immutable]
	public sealed class CombinedAddInfoSchema : ITableSchema
	{
		public CombinedAddInfoSchema(ITableSchema bizoSchema, IEnumerable<ITableSchema> addInfoSchemas)
		{
			this.bizoSchema = bizoSchema;
			this.addInfoSchemas = addInfoSchemas.ToArray();
		}
		readonly ITableSchema bizoSchema;
		readonly ITableSchema[] addInfoSchemas;

		public string SqlSchemaName => bizoSchema.SqlSchemaName;

		public string TableName => bizoSchema.TableName;

		public SchemaPKColumn PK => bizoSchema.PK;

		public string PkIndexName => bizoSchema.PkIndexName;

		public SchemaColumnCollection All => new SchemaColumnCollection(PK, addInfoSchemas.SelectMany(s => s.All).ToList());

		public SchemaColumn GetSchemaColumn(string columnName)
			=> addInfoSchemas.Select(s => s.GetSchemaColumn(columnName)).WhereNotNull().FirstOrDefault();
	}
}
