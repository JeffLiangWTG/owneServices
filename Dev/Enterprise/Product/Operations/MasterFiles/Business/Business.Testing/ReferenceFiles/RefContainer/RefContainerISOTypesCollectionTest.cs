using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainerISOTypesCollection))]
	sealed class RefContainerISOTypesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RefContainerISOTypesCollection>
	{
		protected override RefContainerISOTypesCollection GetCollectionToTest()
		{
			return new RefContainerISOTypesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerISOType();
		}
	}
}
