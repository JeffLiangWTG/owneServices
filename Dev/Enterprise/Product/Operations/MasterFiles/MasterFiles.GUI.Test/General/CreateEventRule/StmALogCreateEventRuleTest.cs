using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.General.Testing
{
	[TestedType(typeof(StmALogCreateEventRule))]
	sealed class StmALogCreateEventRuleTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			// You are not allowed to delete StmALog.
		}

		public override void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			Assert("This should not be run on StmALog as it tries to set fields it can't on this type of object.", true);
		}
	}
}
