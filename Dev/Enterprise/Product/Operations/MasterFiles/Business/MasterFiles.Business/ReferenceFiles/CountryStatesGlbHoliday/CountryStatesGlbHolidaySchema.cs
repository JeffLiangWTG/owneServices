using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class CountryStatesGlbHolidaySchema : Schema, ITableSchema
	{
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static CountryStatesGlbHolidaySchema()
		{
			var column = 0;
			Instance = new CountryStatesGlbHolidaySchema();
			PK = new SchemaPKColumn(Instance, Constants.PK);
			GHC_HolidayName = new SchemaStringColumn(Instance, Constants.GHC_HolidayName, column++, SqlDbType.VarChar, "", !IsNullable, 35, false, false);
			GHC_ParentTableCode = new SchemaStringColumn(Instance, Constants.GHC_ParentTableCode, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false);
			GHC_RecurrDay = new SchemaStringColumn(Instance, Constants.GHC_RecurrDay, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false);
			GHC_Recurring = new SchemaBoolColumn(Instance, Constants.GHC_Recurring, column++, false, true, false);
			GHC_RecurrType = new SchemaStringColumn(Instance, Constants.GHC_RecurrType, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false);
			GHC_Date = new SchemaDateColumn(Instance, Constants.GHC_Date, column++, SqlDbType.Date, DBNull.Value, IsNullable, false);
			GHC_IsActive = new SchemaBoolColumn(Instance, Constants.GHC_IsActive, column++, false, true, false);
			GHC_IsWorkingDay = new SchemaBoolColumn(Instance, Constants.GHC_IsWorkingDay, column++, false, true, false);
			GHC_ParentId = new SchemaGuidColumn(Instance, Constants.GHC_ParentId, column++, DBNull.Value, !IsNullable, false);
			GHC_CountryCode = new SchemaStringColumn(Instance, Constants.GHC_CountryCode, column++, SqlDbType.VarChar, "", IsNullable, 2, false, false);
		}

		CountryStatesGlbHolidaySchema()
		{
		}

		#region Constants

		public static class Constants
		{
			public const string SqlSchemaName = "dbo";
			public const string TableName = "CountryStatesGlbHoliday";
			public const string PK = "PK";

			public const string GHC_HolidayName = "GHC_HolidayName";
			public const string GHC_RecurrType = "GHC_RecurrType";
			public const string GHC_Recurring = "GHC_Recurring";
			public const string GHC_IsActive = "GHC_IsActive";
			public const string GHC_IsWorkingDay = "GHC_IsWorkingDay";
			public const string GHC_RecurrDay = "GHC_RecurrDay";
			public const string GHC_Date = "GHC_Date";
			public const string GHC_ParentTableCode = "GHC_ParentTableCode";
			public const string GHC_ParentId = "GHC_ParentId";
			public const string GHC_CountryCode = "GHC_CountryCode";
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
				GHC_HolidayName,
				GHC_RecurrType,
				GHC_Recurring,
				GHC_IsActive,
				GHC_IsWorkingDay,
				GHC_RecurrDay,
				GHC_Date,
				GHC_ParentTableCode,
				GHC_ParentId,
				GHC_CountryCode
			});
		}

		#endregion

		#region PK

		public static readonly SchemaPKColumn PK;

		#endregion

		public static readonly SchemaStringColumn GHC_HolidayName;
		public static readonly SchemaStringColumn GHC_RecurrType;
		public static readonly SchemaBoolColumn GHC_Recurring;
		public static readonly SchemaBoolColumn GHC_IsActive;
		public static readonly SchemaBoolColumn GHC_IsWorkingDay;
		public static readonly SchemaStringColumn GHC_RecurrDay;
		public static readonly SchemaDateColumn GHC_Date;
		public static readonly SchemaStringColumn GHC_ParentTableCode;
		public static readonly SchemaGuidColumn GHC_ParentId;
		public static readonly SchemaStringColumn GHC_CountryCode;

		#region GetSchemaColumn(ColumnName)

		internal static SchemaColumn GetSchemaColumn(string columnName)
		{
			switch (columnName)
			{
				case Constants.PK:
					return PK;

				case Constants.GHC_HolidayName:
					return GHC_HolidayName;
				case Constants.GHC_RecurrType:
					return GHC_RecurrType;
				case Constants.GHC_Recurring:
					return GHC_Recurring;
				case Constants.GHC_IsActive:
					return GHC_IsActive;
				case Constants.GHC_IsWorkingDay:
					return GHC_IsWorkingDay;
				case Constants.GHC_RecurrDay:
					return GHC_RecurrDay;
				case Constants.GHC_Date:
					return GHC_Date;
				case Constants.GHC_ParentTableCode:
					return GHC_ParentTableCode;
				case Constants.GHC_ParentId:
					return GHC_ParentId;
				case Constants.GHC_CountryCode:
					return GHC_CountryCode;
				default:
					return null;
			}
		}

		#endregion

		#region ITableSchema

		public static readonly CountryStatesGlbHolidaySchema Instance;

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
