using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ContainerOverflowForImmediateDeliveryCollection))]
	sealed class ContainerOverflowForImmediateDeliveryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ContainerOverflowForImmediateDeliveryCollection>
	{
		protected override ContainerOverflowForImmediateDeliveryCollection GetCollectionToTest() => new ContainerOverflowForImmediateDeliveryCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ContainerOverflowForImmediateDelivery();
	}
}
