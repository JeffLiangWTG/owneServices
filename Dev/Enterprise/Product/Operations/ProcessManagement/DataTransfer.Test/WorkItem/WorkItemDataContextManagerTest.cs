using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	[TestedType(typeof(WorkItemDataContextManager))]
	class WorkItemDataContextManagerTest : ActivityDataContextManagerTestCase<WorkItemDataContextManager, WorkItem>
	{
		protected override DataContextType ExpectedDataContextType => DataContextType.WorkItem;
		protected override Type ExpectedDataObjectWriterType => typeof(WorkItemDataObjectWriter);
	}
}
