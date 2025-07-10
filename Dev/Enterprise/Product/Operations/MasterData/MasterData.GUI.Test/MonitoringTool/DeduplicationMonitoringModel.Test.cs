using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Aga.Business.Tree;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationMonitoringModelTest : TestCaseWithFactory
	{
		public void TestModelContainsElement()
		{
			var guid = Guid.NewGuid();
			var monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			var model = new DeduplicationMonitoringModel(monitoringObjects);
			var path = new TreePath("FindDuplicates_1");

			AssertEquals(0, model.GetChildren(path).Cast<DeduplicationMonitoringBaseItem>().Count());

			var dummyObject = new DummyResult
			{
				Description = "A Dummy",
				Org = "AUTELEBNY",
				Target = "Auto Electrical " + guid.ToString()
			};

			monitoringObjects.TryAdd("MethodName_1", new MonitoringObjectValue { ExecutionTimeInMilliseconds = 4000, Value = new List<DummyResult> { dummyObject } });

			model = new DeduplicationMonitoringModel(monitoringObjects);
			path = new TreePath();

			var child1 = model.GetChildren(path);
			AssertEquals(1, child1.Cast<DeduplicationMonitoringBaseItem>().Count());

			var methodNamePath = new DeduplicationMonitoringMethodNameItem("MethodName_1", "2000");
			var secondPath = new TreePath(path, methodNamePath);
			var child2 = model.GetChildren(secondPath);
			AssertEquals(1, child2.Cast<DeduplicationMonitoringBaseItem>().Count());

			var thirdPath = new TreePath(secondPath, new DeduplicationMonitoringTypeItem(nameof(DummyResult), "3", 1, methodNamePath));
			var child3 = model.GetChildren(thirdPath);
			AssertEquals(3, child3.Cast<DeduplicationMonitoringBaseItem>().Count());

			monitoringObjects.Clear();

			monitoringObjects.TryAdd("DebugLog", new MonitoringObjectValue { ExecutionTimeInMilliseconds = 4000, Value = "DebugLogStr" });
			model = new DeduplicationMonitoringModel(monitoringObjects);
			var debugLogChild = model.GetChildren(path).Cast<DeduplicationMonitoringMethodNameItem>();
			AssertEquals(1, debugLogChild.Count());
			AssertEquals("DebugLogStr", debugLogChild.First().DebugLog);
		}

		public void TestSetIconsForOrgAndPerson()
		{
			var monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();

			var dummyObject = new DummyResult
			{
				Description = "A Dummy",
				Org = "AUTELEBNY",
				Target = "Auto Electrical "
			};

			monitoringObjects.TryAdd(DeduplicationDebuggerParticipant.OrganizationPrefix + "MethodName_1", new MonitoringObjectValue { Value = new List<DummyResult> { dummyObject } });
			monitoringObjects.TryAdd(DeduplicationDebuggerParticipant.PersonPrefix + "MethodName_2", new MonitoringObjectValue { Value = new List<DummyResult> { dummyObject } });
			monitoringObjects.TryAdd("A" + DeduplicationDebuggerParticipant.PersonPrefix + "_3", new MonitoringObjectValue { Value = new List<DummyResult> { dummyObject } });

			var model = new DeduplicationMonitoringModel(monitoringObjects);
			var path = new TreePath();

			var childs = model.GetChildren(path).Cast<DeduplicationMonitoringMethodNameItem>().ToList();
			AssertEquals(3, childs.Count);
			AssertNotNull(childs[0].Icon);
			AssertNotNull(childs[1].Icon);
			AssertNull(childs[2].Icon);
		}

		public void TestGenerateQuery()
		{
			var monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			monitoringObjects.TryAdd("MethodName_1", new MonitoringObjectValue { ExecutionTimeInMilliseconds = 4000, Value = new List<DeduplicationDebuggerMaster> { new DeduplicationDebuggerMaster() } });

			var model = new DeduplicationMonitoringModel(monitoringObjects);
			var path = new TreePath();
			var child1 = model.GetChildren(path);
			var itemList = child1.Cast<DeduplicationMonitoringMethodNameItem>();
			var item = itemList.FirstOrDefault();

			AssertEquals(1, itemList.Count());
			AssertEquals("Click here to show the queries used to find pattern matches", item.QueryTitle);
			AssertNotNullOrEmpty("The query should not be null'", item.DBCommandText);
			AssertNoExceptionThrown("No exception thrown on the query.", () => Db.Connection.ExecuteNonQuery(item.DBCommandText));
		}

		public void TestShouldNotThrowSqlException_NotMatchTableDefinition()
		{
			var decider = new MockPatternTableDecider();
			var monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			monitoringObjects.TryAdd("MethodName_1", new MonitoringObjectValue { ExecutionTimeInMilliseconds = 4000, Value = new List<DeduplicationDebuggerMaster> { new DeduplicationDebuggerMaster() { Reporter = decider } } });

			var model = new DeduplicationMonitoringModel(monitoringObjects);
			var path = new TreePath();
			var child1 = model.GetChildren(path);
			var itemList = child1.Cast<DeduplicationMonitoringMethodNameItem>();
			var item = itemList.FirstOrDefault();

			AssertNoExceptionThrown("No exception thrown on the query.", () =>
			{
				var command = Db.Connection.Command(item.DBCommandText);
				foreach (var pair in item.Tables)
				{
					command.AddTableValuedParameter("@hashedValueTable" + pair.Key, "dbo.TVP_int", pair.Value);
				}

				command.ExecuteNonQuery();
			});
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownOnConstructor()
		{
			DeduplicationMonitoringModel model = new DeduplicationMonitoringModel(null);

			var path = new TreePath("FindDuplicates_1");

			AssertEquals(0, model.GetChildren(path).Cast<DeduplicationMonitoringBaseItem>().Count());
		}

		#region Implementation

		class DummyResult
		{
			public string Target { get; set; }
			public string Org { get; set; }
			public string Description { get; set; }
		}

		class MockPatternTableDecider : IPatternTableDecider, IDebugReporter
		{
			public void ComputeValuesToSearch<T>(T bizO, Type lookupType, IHashingFields hashingFieldsRepository, ICollection<Tuple<string, string>> ignoredFieldMapping)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<PatternMatchingResultModel> FindPotentialTargets(IDataRetriever dataRetriever, IDbConnection dbConnection, Guid masterPk,
				int repeatedValueThreshold)
			{
				throw new NotImplementedException();
			}

			public Collection<Tuple<int, string>> FieldsToSearch { get; } = new Collection<Tuple<int, string>>() { new Tuple<int, string>(12345, "PMN") };
			public string DebuggingReport { get; }
		}

		#endregion
	}
}
