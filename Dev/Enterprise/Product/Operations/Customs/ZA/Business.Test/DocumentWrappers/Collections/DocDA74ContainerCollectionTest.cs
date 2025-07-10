using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDA74ContainerCollection))]
	sealed class DocDA74ContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocDA74ContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocDA74Container("", "", Factory);
		}

		protected override DocDA74ContainerCollection GetCollectionToTest()
		{
			return new DocDA74ContainerCollection(Factory);
		}
	}
}
