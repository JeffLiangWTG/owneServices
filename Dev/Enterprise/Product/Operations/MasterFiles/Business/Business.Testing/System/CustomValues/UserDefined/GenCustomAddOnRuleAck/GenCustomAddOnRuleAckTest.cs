using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleAck))]
	sealed class GenCustomAddOnRuleAckTest : EnterpriseBusinessObjectTestCase
	{
		public void TestXK_ParentTableColumn()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();
			AssertEquals(string.Empty, ack.XK_ParentTableColumn);

			ack.XK_ParentTableColumn = "Z0_AAA";
			AssertEquals("Z0_AAA", ack.XK_ParentTableColumn);

			ack.XK_Warning = "Z0_BBB";
			AssertEquals("Z0_BBB", ack.XK_ParentTableColumn);
		}

		public void TestXK_Warning()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();
			AssertEquals(string.Empty, ack.XK_Warning);

			ack.XK_ParentTableColumn = "Z0_AAA";
			AssertEquals("Z0_AAA", ack.XK_Warning);

			ack.XK_Warning = "Z0_BBB";
			AssertEquals("Z0_BBB", ack.XK_Warning);
		}

		public void TestHumanReadableNameCore()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();
			ack.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.TestGuidForWarningAcknowledgment;

			var dummyBizO = Factory.New<DummyBizOWithAutoLogs>();
			ack.XK_ParentID = dummyBizO.PK;
			ack.XK_ParentTableCode = dummyBizO.TablePrefix;

			AssertEquals("Test Validation - DummyBizo", ack.HumanReadableName);
		}

		public void TestXK_ParentIDHumanReadableName()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();

			var dummyBizO = Factory.New<DummyBizOWithAutoLogs>();
			ack.XK_ParentID = dummyBizO.PK;
			ack.XK_ParentTableCode = dummyBizO.TablePrefix;

			AssertEquals("DummyBizo", ack.XK_ParentIDHumanReadableName);
		}

		public void TestXK_RuleIDHumanReadableName()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();
			ack.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.TestGuidForWarningAcknowledgment;

			AssertEquals("Test Validation", ack.XK_RuleIDHumanReadableName);
		}

		public void TestLogs()
		{
			var ack = Factory.New<GenCustomAddOnRuleAck>();
			AssertEquals("Pre-condition", 0, ack.Logs.GetAllLogs().Count);
			Factory.Save();
			ack.XK_IsCancelled = true;
			Factory.Save();
			AssertEquals("Log shouldbve been created", 1, ack.Logs.GetAllLogs().Count);
		}
	}
}
