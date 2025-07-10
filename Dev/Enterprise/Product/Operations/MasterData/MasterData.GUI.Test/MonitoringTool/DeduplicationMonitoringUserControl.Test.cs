using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationMonitoringUserControlTest : TestCase
	{
		public void TestItemsAreAddedToTreeView()
		{
			var guid = Guid.NewGuid();
			var monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			var dummyObject = new DummyResult
			{
				Description = "A Dummy",
				Org = "AUTELEBNY",
				Target = "Auto Electrical " + guid.ToString()
			};

			monitoringObjects.TryAdd("MethodName_1", new MonitoringObjectValue { ExecutionTimeInMilliseconds = 4000, Value = new List<DummyResult> { dummyObject } });

			using (var control = new DeduplicationMonitoringUserControl())
			{
				control.SetDataContext(monitoringObjects);
				control.treeView.ExpandAll();

				var nodes = control.treeView.AllNodes;

				AssertEquals("Total Number of Nodes", 5, nodes.Count());
			}
		}

		#region Implementation

		class DummyResult
		{
			public string Target { get; set; }
			public string Org { get; set; }
			public string Description { get; set; }
		}

		#endregion
	}
}
