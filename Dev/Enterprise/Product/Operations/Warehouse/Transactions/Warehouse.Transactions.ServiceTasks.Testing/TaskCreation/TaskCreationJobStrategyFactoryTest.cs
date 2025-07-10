using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Moq;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class TaskCreationJobStrategyFactoryTest : WhsTestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<TaskCreationJobStrategyFactory>(ObjectFactory.Get<ITaskCreationJobStrategyFactory>());
		}

		public void TestGetJobStrategy_NullArgument_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskCreationJobStrategyFactory().GetJobStrategy(null));
		}

		public void TestGetJobStrategy_EmptyArgument_Throws()
		{
			AssertExceptionThrown<ArgumentException>(() => new TaskCreationJobStrategyFactory().GetJobStrategy(string.Empty));
		}

		public void TestGetJobStrategy()
		{
			var jobStrategyFactory = new TaskCreationJobStrategyFactory();

			var hashTable = new KeyObjectHandleDictionaryObject();

			using (ObjectFactory.Substitute("WarehouseTaskCreationStrategies", hashTable))
			{
				AssertNull(jobStrategyFactory.GetJobStrategy("TST"));

				var mockStrategy = Mock.Of<ITaskCreationJobStrategy>();
				using (ObjectFactory.Substitute("TestStrategy", mockStrategy))
				{
					hashTable.SourceDictionary = new Dictionary<string, string> { { "TST", "TestStrategy" } };

					AssertEquals(mockStrategy, jobStrategyFactory.GetJobStrategy("TST"));
					AssertNull(jobStrategyFactory.GetJobStrategy("OTH"));
				}
			}
		}
	}
}
