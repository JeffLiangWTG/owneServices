using System;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	sealed class UCMPBusinessObjectBindingTest : TestCase
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Table is null", () => new UCMPBusinessObjectBinding(null, "TSK", "QUE", Array.Empty<string>()));
				AssertExceptionThrown<ArgumentException>("Table is empty", () => new UCMPBusinessObjectBinding("", "TSK", "QUE", Array.Empty<string>()));

				AssertExceptionThrown<ArgumentNullException>("Service Task Code is null", () => new UCMPBusinessObjectBinding("TBL", null, "QUE", Array.Empty<string>()));
				AssertExceptionThrown<ArgumentException>("Service Task Code is empty", () => new UCMPBusinessObjectBinding("TBL", "", "QUE", Array.Empty<string>()));

				AssertExceptionThrown<ArgumentNullException>("Queue Name is null", () => new UCMPBusinessObjectBinding("TBL", "TSK", null, Array.Empty<string>()));
				AssertExceptionThrown<ArgumentException>("Queue name is empty", () => new UCMPBusinessObjectBinding("TBL", "TSK", "", Array.Empty<string>()));

				AssertExceptionThrown<ArgumentNullException>("Predicates is null", () => new UCMPBusinessObjectBinding("TBL", "TSK", "QUE", null));
			});
		}

		public void TestTable() => AssertEquals("TBL", businessObjectBinding.Table);

		public void TestServiceTaskCode() => AssertEquals("TSK", businessObjectBinding.ServiceTaskCode);

		public void TestQueueName() => AssertEquals("QUE", businessObjectBinding.QueueName);

		public void TestPredicates() => AssertContainsExactElementsInAnyOrder(new[] { "EM_IsActive=1" }, businessObjectBinding.Predicates);

		protected override void SetUp()
		{
			base.SetUp();
			businessObjectBinding = new UCMPBusinessObjectBinding("TBL", "TSK", "QUE", new[] { "EM_IsActive=1" });
		}

		IHostedServiceBusinessObjectBinding businessObjectBinding;
	}
}
