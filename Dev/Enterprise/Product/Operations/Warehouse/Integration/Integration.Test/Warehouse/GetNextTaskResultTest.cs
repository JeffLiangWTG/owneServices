using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.Testing
{
	class GetNextTaskResultTest : TestCase
	{
		public void TestConstructors()
		{
			var taskPK = Guid.NewGuid();
			var result1 = new GetNextTaskResult(taskPK, "WUL");
			AssertEquals(taskPK, result1.TaskPK);
			AssertEquals("WUL", result1.TaskFormFlowType);
			AssertEquals(null, result1.ErrorMessage);

			var result2 = new GetNextTaskResult("The process blew up!");
			AssertEquals(Guid.Empty, result2.TaskPK);
			AssertEquals(null, result2.TaskFormFlowType);
			AssertEquals("The process blew up!", result2.ErrorMessage);
		}
	}
}
