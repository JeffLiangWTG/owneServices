using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTaskCollectionSortTests : TemplateApplicationTestCase
	{
		public void TestDefaultCollectionSort()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var t1 = MakeTask(dummy, description: "X");
			var t2 = MakeTask(dummy, description: "Y");
			var t3 = MakeTask(dummy, description: "C");
			var t6 = MakeTask(dummy, description: "C");
			var t5 = MakeTask(dummy, description: "C");
			var t4 = MakeTask(dummy, description: "C");

			t1.P9_Sequence = 1;
			t2.P9_Sequence = 1;
			t3.P9_Sequence = 2;
			t4.P9_Sequence = 2;
			t5.P9_Sequence = 2;
			t6.P9_Sequence = 2;

			t3.P9_GS_NKAssignedStaffMember = "AAA";
			t4.P9_GS_NKAssignedStaffMember = "BAA";
			t5.P9_GS_NKAssignedStaffMember = "CAA";
			t6.P9_GS_NKAssignedStaffMember = "DAA";

			var comparer1 = ((IWorkflowProviderCollection)dummy.WorkflowItems).GetDefaultOrderComparer();
			var comparer2 = ((IWorkflowProviderCollection)dummy.WorkflowItems.Tasks).GetDefaultOrderComparer();

			var sorted1 = dummy.WorkflowItems.Cast<ProcessTask>()
				.OrderBy(t => t, Comparer<ProcessTask>.Create((a, b) => comparer1.Compare(a, b)))
				.ToArray();
			var sorted2 = dummy.WorkflowItems.Cast<ProcessTask>()
				.OrderBy(t => t, Comparer<ProcessTask>.Create((a, b) => comparer2.Compare(a, b)))
				.ToArray();

			AssertArrayEqualsByElements(new[] { t1, t2, t3, t4, t5, t6 }, sorted1);
			AssertArrayEqualsByElements(new[] { t1, t2, t3, t4, t5, t6 }, sorted2);
		}
	}
}
