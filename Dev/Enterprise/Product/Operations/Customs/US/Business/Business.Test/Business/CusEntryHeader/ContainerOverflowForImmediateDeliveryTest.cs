using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ContainerOverflowForImmediateDelivery))]
	sealed class ContainerOverflowForImmediateDeliveryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerString()
		{
			var containerOverflow = new ContainerOverflowForImmediateDelivery();
			containerOverflow.ContainerString = "ABCU1234567";
			AssertEquals("ABCU1234567", containerOverflow.ContainerString);
		}

		protected override BusinessObject GetNewBusinessObject() => new ContainerOverflowForImmediateDelivery();
	}
}
