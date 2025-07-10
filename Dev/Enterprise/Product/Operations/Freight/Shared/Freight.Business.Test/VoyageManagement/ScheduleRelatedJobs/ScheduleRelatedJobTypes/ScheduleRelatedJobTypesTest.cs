using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleRelatedJobTypesTest : TestCaseWithFactory
	{
		public void TestAllClassesThatRequireAScheduleRelatedJobTypeHaveOne()
		{
			AssertAllTransportParentTypesHaveAScheduleRelatedJobType();
			AssertAllTablesLinkedToSailingViaForeignKeyHaveAScheduleRelatedJobType();
		}

		void AssertAllTransportParentTypesHaveAScheduleRelatedJobType()
		{
			var scheduleRelatedJobTypes = ScheduleRelatedJobTypes.New();

			var transportParentTypes = typeof(TransportParentTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
				.Where(x => x.IsLiteral)
				.Select(x => x.GetValue(null))
				.Cast<string>();

			foreach (var transportParentType in transportParentTypes)
			{
				AssertCollectionContains(FormattableString.Invariant($"It looks like you have added a new TransportParentType '{transportParentType}'. You must also add it to ViewSailingRelatedJob and add a ScheduleRelatedJobType for it so it shows up on the Related Jobs tab of the Sailing form."),
					transportParentType, scheduleRelatedJobTypes.GetAllCodes());
			}
		}

		void AssertAllTablesLinkedToSailingViaForeignKeyHaveAScheduleRelatedJobType()
		{
			foreach (var fkToSailingColumn in GetAllForeignKeyToJobSailingColumns()
				.Where(c => !TablesWhichReallyDontNeedAScheduleRelatedJobType.Contains(c.TableName)))
			{
				var tableHasBeenImplemented = false;
				foreach (ScheduleRelatedJobType sailingRelatedJobType in ScheduleRelatedJobTypes.New())
				{
					if (GetTableName(sailingRelatedJobType) == fkToSailingColumn.TableName)
					{
						tableHasBeenImplemented = true;
						break;
					}
				}

				Assert(FormattableString.Invariant($"It looks like you have added a foreign key to JobSailing ({fkToSailingColumn.TableName}.{fkToSailingColumn.Name}). You will also need to add your linked table to ViewSailingRelatedJob and add a SailingRelatedJobType for it so it shows up on the Related Jobs tab of the Sailing form."),
					tableHasBeenImplemented);
			}
		}

		IEnumerable<string> TablesWhichReallyDontNeedAScheduleRelatedJobType
		{
			get
			{
				yield return ViewSailingRelatedJobSchema.Constants.TableName; // This is the view itself
				yield return JobConsolTransportSchema.Constants.TableName; // Transport parents will be shown, not transports themselves
			}
		}

		IEnumerable<SchemaColumn> GetAllForeignKeyToJobSailingColumns()
		{
			var schemaClasses = Assembly.Load("Enterprise.ZArchitecture.Schema")
				.GetTypes()
				.Where(t => t.IsClass
					&& t.Namespace == "Enterprise.ZArchitecture.Schema"
					&& t.GetInterface(typeof(ITableSchema).FullName) != null);
			foreach (var schemaClass in schemaClasses)
			{
				var schemaColumns = (SchemaColumnCollection)schemaClass.GetProperty("All").GetValue(null);
				foreach (var schemaColumn in schemaColumns)
				{
					if (Schema.GetForeignKeyTargetPrefixFromColumnName(schemaColumn.Name) == "JX")
					{
						yield return schemaColumn;
					}
				}
			}
		}

		public void TestAllSailingRelatedJobTypesAreIncludedInViewSailingRelatedJobTypes()
		{
			var sql = @"
SELECT definition
FROM sys.objects o
JOIN sys.sql_modules m on m.object_id = o.object_id
WHERE o.object_id = object_id('dbo.ViewSailingRelatedJob')";

			var viewScript = "";

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				reader.Read();
				viewScript = reader["definition"].ToString();
			}

			foreach (ScheduleRelatedJobType sailingRelatedJobType in ScheduleRelatedJobTypes.New())
			{
				var tableName = GetTableName(sailingRelatedJobType);
				if (tableName == ViewQuotedBookingSchema.Constants.TableName)
				{
					tableName = JobShipmentSchema.Constants.TableName;
				}

				Assert(FormattableString.Invariant($"It looks like you have added a new SailingRelatedJobType {sailingRelatedJobType.Code}. You must also add it to ViewSailingRelatedJob so it shows up on the Related Jobs tab of the Sailing form."),
					viewScript.Contains(tableName));
			}
		}

		#region Implementation

		string GetTableName(ScheduleRelatedJobType sailingRelatedJobType)
		{
			return sailingRelatedJobType.Code == "QSH" ? ViewQuotedBookingSchema.Constants.TableName : Factory.New(sailingRelatedJobType.BizOType).TableName;
		}

		#endregion
	}
}
