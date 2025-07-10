using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class UniversalCommodityCodeSchema : Schema, ITableSchema
	{
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static UniversalCommodityCodeSchema()
		{
			var column = 0;
			Instance = new UniversalCommodityCodeSchema();
			PK = new SchemaPKColumn(Instance, Constants.PK);
			RH_UniversalCommodityGroup = new SchemaStringColumn(Instance, Constants.RH_UniversalCommodityGroup, column++, SqlDbType.VarChar, "", !IsNullable, RefCommodityCode.Schema.RH_UniversalCommodityGroupMaxLength, false, false);
			RH_UniversalCommodityGroupDescription = new SchemaStringColumn(Instance, Constants.RH_UniversalCommodityGroupDescription, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
			RH_Code = new SchemaStringColumn(Instance, Constants.RH_Code, column++, SqlDbType.VarChar, "", !IsNullable, RefCommodityCode.Schema.RH_CodeMaxLength, false, false);
			RH_Description = new SchemaStringColumn(Instance, Constants.RH_Description, column++, SqlDbType.VarChar, "", !IsNullable, RefCommodityCode.Schema.RH_DescriptionMaxLength, false, false);
		}

		UniversalCommodityCodeSchema()
		{
		}

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "UniversalCommodityCode";
			public const string PK = "PK";

			public const string RH_UniversalCommodityGroup = "RH_UniversalCommodityGroup";
			public const string RH_UniversalCommodityGroupDescription = "RH_UniversalCommodityGroupDescription";
			public const string RH_Code = "RH_Code";
			public const string RH_Description = "RH_Description";
		}

		#endregion

		#region All

		public static SchemaColumnCollection All
		{
			get { return AllHolder.all; }
		}

		static class AllHolder
		{
			public static readonly SchemaColumnCollection all = new SchemaColumnCollection(PK, new SchemaColumn[]
			{
				RH_UniversalCommodityGroup,
				RH_UniversalCommodityGroupDescription,
				RH_Code,
				RH_Description
			});
		}

		#endregion

		#region PK

		public static readonly SchemaPKColumn PK;

		#endregion

		public static readonly SchemaStringColumn RH_UniversalCommodityGroup;
		public static readonly SchemaStringColumn RH_UniversalCommodityGroupDescription;
		public static readonly SchemaStringColumn RH_Code;
		public static readonly SchemaStringColumn RH_Description;

		#region GetSchemaColumn(ColumnName)

		internal static SchemaColumn GetSchemaColumn(string columnName)
		{
			switch (columnName)
			{
				case Constants.PK:
					return PK;

				case Constants.RH_UniversalCommodityGroup:
					return RH_UniversalCommodityGroup;
				case Constants.RH_UniversalCommodityGroupDescription:
					return RH_UniversalCommodityGroupDescription;
				case Constants.RH_Code:
					return RH_Code;
				case Constants.RH_Description:
					return RH_Description;

				default:
					return null;
			}
		}

		#endregion

		#region ITableSchema

		public static readonly UniversalCommodityCodeSchema Instance;

		string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;

		string ITableSchema.TableName => Constants.TableName;

		SchemaPKColumn ITableSchema.PK => PK;

		string ITableSchema.PkIndexName => "";

		SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
		{
			return GetSchemaColumn(columnName);
		}

		SchemaColumnCollection ITableSchema.All => All;

		#endregion
	}
}