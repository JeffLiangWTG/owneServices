using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class CombinedAddInfoSchemaTest : TestCase
	{
		public void TestSqlSchemaName()
		{
			AssertEquals(JobDeclarationSchema.Constants.SqlSchemaName, new CombinedAddInfoSchema(JobDeclarationSchema.Instance, System.Array.Empty<ITableSchema>()).SqlSchemaName);
		}

		public void TestTableName()
		{
			AssertEquals(JobDeclarationSchema.Constants.TableName, new CombinedAddInfoSchema(JobDeclarationSchema.Instance, System.Array.Empty<ITableSchema>()).TableName);
		}

		public void TestPK()
		{
			AssertEquals(JobDeclarationSchema.Constants.PK, new CombinedAddInfoSchema(JobDeclarationSchema.Instance, System.Array.Empty<ITableSchema>()).PK.Name);
		}

		public void TestPkIndexName()
		{
			AssertEquals(((ITableSchema)JobDeclarationSchema.Instance).PkIndexName, new CombinedAddInfoSchema(JobDeclarationSchema.Instance, System.Array.Empty<ITableSchema>()).PkIndexName);
		}

		public void TestAll()
		{
			var addInfoSchemas = new ITableSchema[] { FRJobDeclarationSchema.Instance, EUAddInfoSchema.Instance };
			var combinedAddInfoSchema = new CombinedAddInfoSchema(JobDeclarationSchema.Instance, addInfoSchemas);
			var expected = new SchemaColumnCollection(JobDeclarationSchema.PK, addInfoSchemas.SelectMany(s => s.All).ToList());
			AssertContainsExactElementsInAnyOrder(expected, combinedAddInfoSchema.All);
		}

		public void TestGetSchemaColumn()
		{
			var addInfoSchemas = new ITableSchema[] { FRJobDeclarationSchema.Instance, EUAddInfoSchema.Instance };
			var combinedAddInfoSchema = new CombinedAddInfoSchema(JobDeclarationSchema.Instance, addInfoSchemas);

			CombineAssertions(() =>
			{
				FRJobDeclarationSchema.All.ForEach(c => AssertEquals(c.Name, c.Name, combinedAddInfoSchema.GetSchemaColumn(c.Name).Name));
				EUAddInfoSchema.All.ForEach(c => AssertEquals(c.Name, c.Name, combinedAddInfoSchema.GetSchemaColumn(c.Name).Name));
				JobDeclarationSchema.All.Where(c => !Schema.IsSystemColumn(c.Name) && !c.IsPKColumn).ForEach(c => AssertNull(c.Name, combinedAddInfoSchema.GetSchemaColumn(c.Name)));
			});
		}
	}
}
