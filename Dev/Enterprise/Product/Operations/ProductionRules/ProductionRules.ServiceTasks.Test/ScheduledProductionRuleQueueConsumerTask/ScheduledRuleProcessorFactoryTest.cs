using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProductionRules.Integration;
using Moq;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.ServiceTasks.Testing
{
	class ScheduledRuleProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<ScheduledRuleProcessorFactory>(ObjectFactory.Get<IScheduledRuleProcessorFactory>());
		}

		public void TestGetRuleProcessor()
		{
			var ruleProcessorFactory = new ScheduledRuleProcessorFactory();

			var hashTable = new KeyObjectHandleDictionaryObject();

			using (ObjectFactory.Substitute("ScheduledRuleProcessors", hashTable))
			{
				AssertNull(ruleProcessorFactory.GetRuleProcessor(RulesContextType.DummyForTesting));

				var mockRuleProcessor = Mock.Of<IScheduledRuleProcessor>();
				using (ObjectFactory.Substitute("TestProcessor", mockRuleProcessor))
				{
					hashTable.SourceDictionary = new Dictionary<string, string> { { nameof(RulesContextType.DummyForTesting), "TestProcessor" } };

					AssertEquals(mockRuleProcessor, ruleProcessorFactory.GetRuleProcessor(RulesContextType.DummyForTesting));
					AssertNull(ruleProcessorFactory.GetRuleProcessor(RulesContextType.SecondDummyForTesting));
				}
			}
		}

		public void TestGetRuleProcessor_ValidErrorContractGroupRegistryItem()
		{
			var ruleProcessorFactory = new ScheduledRuleProcessorFactory();
			foreach (RulesContextType type in Enum.GetValues(typeof(RulesContextType)))
			{
				TestGetRuleProcessor_ValidErrorContractGroupRegistryItemCore(ruleProcessorFactory, type);
			}
		}

		void TestGetRuleProcessor_ValidErrorContractGroupRegistryItemCore(ScheduledRuleProcessorFactory ruleProcessorFactory, RulesContextType contextType)
		{
			var ruleProcessor = ruleProcessorFactory.GetRuleProcessor(contextType);
			if (ruleProcessor != null)
			{
				AssertNotNull($"{contextType} RuleProcessor", ruleProcessor);
			}
		}

		public void TestGetRuleProcessor_EndToEnd_WaveCreation()
		{
			var ruleProcessorFactory = new ScheduledRuleProcessorFactory();
			var ruleProcessor = ruleProcessorFactory.GetRuleProcessor(RulesContextType.ProductWarehouseWaveCreation);
			AssertNotNull(ruleProcessor);

			var expectedRuleProcessor = ObjectFactory.Get<IScheduledRuleProcessor>("WaveCreationRuleProcessor");
			AssertEquals(ruleProcessor, expectedRuleProcessor);
		}

		public void TestGetRuleProcessor_EndToEnd_CycleCountAutomation()
		{
			var ruleProcessorFactory = new ScheduledRuleProcessorFactory();
			var ruleProcessor = ruleProcessorFactory.GetRuleProcessor(RulesContextType.ProductWarehouseCycleCountTaskCreation);
			AssertNotNull(ruleProcessor);

			var expectedRuleProcessor = ObjectFactory.Get<IScheduledRuleProcessor>("CycleCountTaskCreationRuleProcessor");
			AssertEquals(ruleProcessor, expectedRuleProcessor);
		}
	}
}
