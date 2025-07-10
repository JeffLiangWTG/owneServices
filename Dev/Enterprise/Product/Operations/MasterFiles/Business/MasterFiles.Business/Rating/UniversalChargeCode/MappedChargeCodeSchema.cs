using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class MappedChargeCodeSchema : Schema, ITableSchema
	{
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static MappedChargeCodeSchema()
		{
			var column = 0;
			Instance = new MappedChargeCodeSchema();
			PK = new SchemaPKColumn(Instance, Constants.PK);
			UCC_Code = new SchemaStringColumn(Instance, Constants.UCC_Code, column++, SqlDbType.VarChar, "", !IsNullable, AccChargeCodeUniversalCodeMapping.Schema.AUP_CodeMaxLength, false, false);
			UCC_Description = new SchemaStringColumn(Instance, Constants.UCC_Description, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
			UCC_GlobalChargeCode = new SchemaStringColumn(Instance, Constants.UCC_GlobalChargeCode, column++, SqlDbType.VarChar, "", !IsNullable, AccChargeCode.Schema.AC_CodeMaxLength, false, false);
			UCC_GlobalChargeCodeDescription = new SchemaStringColumn(Instance, Constants.UCC_GlobalChargeCodeDescription, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
			UCC_LocalChargeCode = new SchemaStringColumn(Instance, Constants.UCC_LocalChargeCode, column++, SqlDbType.VarChar, "", !IsNullable, AccChargeCode.Schema.AC_CodeMaxLength, false, false);
			UCC_LocalChargeCodeDescription = new SchemaStringColumn(Instance, Constants.UCC_LocalChargeCodeDescription, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
			UCC_RateProvider = new SchemaStringColumn(Instance, Constants.UCC_RateProvider, column++, SqlDbType.VarChar, "", !IsNullable, Constants.UCC_RateProviderMaxLength, false, false);
			UCC_Carrier = new SchemaStringColumn(Instance, Constants.UCC_Carrier, column++, SqlDbType.VarChar, "", !IsNullable, Constants.UCC_CarrierMaxLength, false, false);
			UCC_ForeignCode = new SchemaStringColumn(Instance, Constants.UCC_ForeignCode, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
			UCC_ForeignName = new SchemaStringColumn(Instance, Constants.UCC_ForeignName, column++, SqlDbType.VarChar, "", !IsNullable, 2147483647, false, false);
		}

		MappedChargeCodeSchema()
		{
		}

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "UniversalChargeCode";
			public const string Prefix = "UCC";
			public const string PK = "UCC_PK";

			public const string UCC_Code = "UCC_Code";
			public const string UCC_Description = "UCC_Description";
			public const string UCC_GlobalChargeCode = "UCC_GlobalChargeCode";
			public const string UCC_GlobalChargeCodeDescription = "UCC_GlobalChargeCodeDescription";
			public const string UCC_LocalChargeCode = "UCC_LocalChargeCode";
			public const string UCC_LocalChargeCodeDescription = "UCC_LocalChargeCodeDescription";
			public const string UCC_RateProvider = "UCC_RateProvider";
			public const string UCC_Carrier = "UCC_Carrier";
			public const string UCC_ForeignCode = "UCC_ForeignCode";
			public const string UCC_ForeignName = "UCC_ForeignName";

			public const int UCC_RateProviderMaxLength = 4;
			public const int UCC_CarrierMaxLength = 4;
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
					UCC_Code,
					UCC_Description,
					UCC_GlobalChargeCode,
					UCC_GlobalChargeCodeDescription,
					UCC_LocalChargeCode,
					UCC_LocalChargeCodeDescription,
					UCC_RateProvider,
					UCC_Carrier,
					UCC_ForeignCode,
					UCC_ForeignName
			});
		}

		#endregion

		#region PK

		public static readonly SchemaPKColumn PK;

		#endregion

		public static readonly SchemaStringColumn UCC_Code;
		public static readonly SchemaStringColumn UCC_Description;
		public static readonly SchemaStringColumn UCC_GlobalChargeCode;
		public static readonly SchemaStringColumn UCC_GlobalChargeCodeDescription;
		public static readonly SchemaStringColumn UCC_LocalChargeCode;
		public static readonly SchemaStringColumn UCC_LocalChargeCodeDescription;
		public static readonly SchemaStringColumn UCC_RateProvider;
		public static readonly SchemaStringColumn UCC_Carrier;
		public static readonly SchemaStringColumn UCC_ForeignCode;
		public static readonly SchemaStringColumn UCC_ForeignName;

		#region GetSchemaColumn(ColumnName)

		internal static SchemaColumn GetSchemaColumn(string columnName)
		{
			switch (columnName)
			{
				case Constants.PK:
					return PK;

				case Constants.UCC_Code:
					return UCC_Code;
				case Constants.UCC_Description:
					return UCC_Description;
				case Constants.UCC_GlobalChargeCode:
					return UCC_GlobalChargeCode;
				case Constants.UCC_GlobalChargeCodeDescription:
					return UCC_GlobalChargeCodeDescription;
				case Constants.UCC_LocalChargeCode:
					return UCC_LocalChargeCode;
				case Constants.UCC_LocalChargeCodeDescription:
					return UCC_LocalChargeCodeDescription;
				case Constants.UCC_RateProvider:
					return UCC_RateProvider;
				case Constants.UCC_Carrier:
					return UCC_Carrier;
				case Constants.UCC_ForeignCode:
					return UCC_ForeignCode;
				case Constants.UCC_ForeignName:
					return UCC_ForeignName;

				default:
					return null;
			}
		}

		#endregion

		#region ITableSchema

		public static readonly MappedChargeCodeSchema Instance;

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
