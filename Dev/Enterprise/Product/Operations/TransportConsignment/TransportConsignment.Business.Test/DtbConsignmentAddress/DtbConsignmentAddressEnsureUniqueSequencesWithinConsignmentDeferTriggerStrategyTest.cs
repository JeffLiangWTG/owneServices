using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategyTest : TestCaseWithFactory
	{
		public void TestRunType()
		{
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTriggerWhenInsert()
		{
			var data = CreateTestConsignmentAddress();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy));
			AssertEquals("Trigger should be deferred for insert.", true, strategy.ShouldDeferTrigger(data));
		}

		public void TestShouldDeferTriggerWhenUpdateWithoutChanges()
		{
			var data = CreateTestConsignmentAddress();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy));
			AssertEquals("Trigger should be deferred at this stage of setup.", true, strategy.ShouldDeferTrigger(data));
			Factory.Save();

			AssertEquals("Trigger should not be deferred for update without changes.", false, strategy.ShouldDeferTrigger(data));
		}

		public void TestShouldDeferTriggerWhenUpdateWithChanges()
		{
			var data = CreateTestConsignmentAddress();
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IDtbConsignmentAddressEnsureUniqueSequencesWithinConsignmentDeferTriggerStrategy));
			AssertEquals("Trigger should be deferred at this stage of setup.", true, strategy.ShouldDeferTrigger(data));
			Factory.Save();

			data.LTS_Sequence = 2;
			AssertEquals("Trigger should be deferred for update with changes.", true, strategy.ShouldDeferTrigger(data));
		}

		DtbConsignmentAddress CreateTestConsignmentAddress()
		{
			var data = Helper.CreateConsignmentAddress();
			data.LTS_InstructionType = "PIC";
			data.LTS_Sequence = 1;
			data.LTS_Status = "INC";
			return data;
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;
	}
}
