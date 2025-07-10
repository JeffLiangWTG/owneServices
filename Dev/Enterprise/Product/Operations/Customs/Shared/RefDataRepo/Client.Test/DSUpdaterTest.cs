using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Models;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	abstract class DSUpdaterTest<TServer, TStorage> : TestCase
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		public void TestUpdate()
		{
			var now = DateTime.UtcNow;
			now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertUpdatePreCondition();
			foreach (var type in GetTypesForUpdate())
			{
				var updatedResult = UpdateType(serverData, SharedSQLBuilder.GetTableName(type.Item1));
				proxy = Helper.GetServerProxy(serverData);
				Helper.RunUpdater(GetUpdater(proxy));
				AssertUpdateResult(type, updatedResult, type.Item2);
				AssertSysColumnsWhenUpdate(type.Item1, updatedResult, now);
			}
		}

		public virtual void AssertUpdateResult(Tuple<Type, int> type, Tuple<string, string> updatedResult, int expectedResultCount)
		{
			AssertEquals($@"Type : {type.Item1.Name}", expectedResultCount, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} 
WHERE {updatedResult.Item1} = '{updatedResult.Item2}'"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Windows Compact FrameWorks does not support Enterprise.Core.Globalisation")]
		public virtual void AssertSysColumnsWhenUpdate(Type type, Tuple<string, string> updatedResult, DateTime lastEditTime)
		{
			if (SharedSQLBuilder.HasSystemTimeAndUserColumns(type))
			{
				var tableName = SharedSQLBuilder.GetTableName(type);
				var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(type);
				using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName} WHERE {updatedResult.Item1} = '{updatedResult.Item2}'"))
				{
					if (reader.Read())
					{
						Assert($"{tableName}.SystemLastEditTimeUtc should be updated.", DateTime.Parse(reader[2].ToString()) >= lastEditTime);
						AssertEquals($"{tableName}.SystemLastEditUser", "~BP", reader[3].ToString());
					}
				}
			}
		}

		public virtual void AssertUpdatePreCondition()
		{
			AssertEquals("Pre-condition", 1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName<TStorage>()}"));
		}

		public virtual IEnumerable<Tuple<Type, int>> GetTypesForUpdate()
		{
			return GetTypesAndNoOfRecords();
		}

		protected Tuple<string, string> UpdateType(object serverData, string name)
		{
			Tuple<string, string> result = null;
			var serverDataType = serverData.GetType();
			if (serverDataType.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				result = UpdateData(serverData);
			}
			if (serverDataType.Name == "UNDGSubstance" && name == SharedSQLBuilder.GetTableName<IZZUNDGSubstance>())
			{
				result = UpdateData(serverData);
			}
			foreach (var property in serverDataType.GetProperties().Where(x => x.PropertyType.IsArray))
			{
				var children = property.GetValue(serverData) as IEnumerable;
				if (children != null)
				{
					foreach (var child in children)
					{
						result = UpdateType(child, name) ?? result;
					}
				}
			}
			result = ExtraUpdateTestAction(serverData, name) ?? result;
			return result;
		}

		public virtual Tuple<string, string> ExtraUpdateTestAction(object serverData, string name)
		{
			return null;
		}

		protected abstract Tuple<string, string> UpdateData(object serverData);

		public virtual void TestDeleteDependentRecord()
		{
			foreach (var type in GetTypesAndNoOfRecords().Skip(1))
			{
				var serverData = GetServerData();
				var proxy = Helper.GetServerProxy(serverData);
				Helper.RunUpdater(GetUpdater(proxy));
				AssertGreaterThanOrEqualTo($"Pre-condition for {type.Item1.Name}", (int)conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName<TStorage>()}"), 1);
				RemoveType(serverData, SharedSQLBuilder.GetTableName(type.Item1));
				proxy = Helper.GetServerProxy(serverData);
				Helper.RunUpdater(GetUpdater(proxy));
				AssertEquals($"Type : {type.Item1.Name}", 0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)}"));
			}
		}

		static void RemoveType(object serverData, string name)
		{
			foreach (var property in serverData.GetType().GetProperties().Where(x => x.PropertyType.IsArray))
			{
				if (property.PropertyType.GetElementType().Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					property.SetValue(serverData, null);
				}
				var children = property.GetValue(serverData) as IEnumerable;
				if (children != null)
				{
					foreach (var child in children)
					{
						RemoveType(child, name);
					}
				}
			}
		}

		public void TestDeleteDataSet()
		{
			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertDeletePreCondition();
			serverData.Deleted = true;
			proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertDeleteDataSet();
		}

		public virtual void AssertDeletePreCondition()
		{
			AssertEquals("Pre-condition", 1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName<TStorage>()}"));
		}

		public virtual void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName<TStorage>()}"));
		}

		public void TestInsert()
		{
			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			foreach (var type in GetTypesAndNoOfRecords())
			{
				AssertInsert(type);
				AssertSysColumnsWhenInsert(type.Item1);
			}
		}

		public virtual void AssertInsert(Tuple<Type, int> type)
		{
			AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)}"));
			AssertRecords(type.Item1);
		}

		protected virtual void AssertRecords(Type type) { }

		protected virtual void AssertSysColumnsWhenInsert(Type type)
		{
			if (SharedSQLBuilder.HasSystemTimeAndUserColumns(type))
			{
				var tableName = SharedSQLBuilder.GetTableName(type);
				var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(type);
				using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName}"))
				{
					if (reader.Read())
					{
						AssertNotNullOrEmpty($"{tableName}.SystemCreateTimeUtc", reader[0].ToString());
						AssertNotNullOrEmpty($"{tableName}.SystemLastEditTimeUtc", reader[2].ToString());
						AssertEquals($"{tableName}.SystemCreateUser", "~BP", reader[1].ToString());
						AssertEquals($"{tableName}.SystemLastEditUser", "~BP", reader[3].ToString());
					}
				}
			}
		}

		protected abstract TServer GetServerData();
		protected abstract IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords();
		protected abstract void PrepareDatabase();
		protected abstract IDataSetUpdater GetUpdater(IServerProxy proxy);

		protected IDbConnection conn;
		protected IDBHelper dbHelper;
		protected IRefVersionControlManager versionControlManager;

		protected override void SetUp()
		{
			base.SetUp();
			dbConnection = Db.NewAdminConnection();
			conn = ((IDbConnectionInternals)dbConnection).ADOConnection;
			dbHelper = new DBHelper(conn);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
			PrepareDatabase();
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(SharedSQLBuilder.GetTableName<TStorage>());
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(SharedSQLBuilder.GetTableName<TStorage>(), DateTime.UtcNow.AddDays(-1), string.Empty, 1), oldRefDbVersionControl);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (conn != null)
			{
				conn.Dispose();
			}
			dbConnection?.Dispose();
		}

		DbConnection dbConnection;
	}
}
