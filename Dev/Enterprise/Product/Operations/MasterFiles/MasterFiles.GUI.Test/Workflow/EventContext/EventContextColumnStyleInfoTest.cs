using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EventContextColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			AssertEquals(typeof(EventContextColumnStyle), new EventContextColumnStyleInfo().ColumnStyleType);
		}

		public void TestWorkflowDescriptor()
		{
			var columnStyleInfo = new EventContextColumnStyleInfo();
			AssertNull(columnStyleInfo.WorkflowDescriptor);

			columnStyleInfo.WorkflowDescriptor = DummyWorkflowDescriptor.Instance;
			AssertSame(DummyWorkflowDescriptor.Instance, columnStyleInfo.WorkflowDescriptor);
		}
	}
}
