using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DocumentCusContainerCollection))]
	sealed class DocumentCusContainerCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<DocumentCusContainerCollection>
	{
		protected override DocumentCusContainerCollection GetCollectionToTest()
		{
			var baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			var baseCusContainerCollection = new BaseCusContainerCollection<BaseCusContainer>(baseJobDeclaration, Factory);
			return new DocumentCusContainerCollection(Factory, baseCusContainerCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocumentCusContainer(Factory);
		}
	}
}
