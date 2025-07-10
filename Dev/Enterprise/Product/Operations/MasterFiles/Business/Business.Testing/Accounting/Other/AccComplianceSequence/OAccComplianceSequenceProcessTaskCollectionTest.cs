using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceProcessTaskCollection))]
	sealed class OAccComplianceSequenceProcessTaskCollectionTest : ProcessTaskCollectionTest<AccComplianceSequenceProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(AccComplianceSequenceProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override AccComplianceSequenceProcessTaskCollection GetCollectionToTestCore()
		{
			return (AccComplianceSequenceProcessTaskCollection)Factory.NewWithValidTestData<AccComplianceSequence>().WorkflowItems;
		}

		#endregion
	}
}
