using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusContainerCollection))]
	sealed class DocCusContainerCollectionTests : DocBaseCusContainerCollectionTest<DocCusContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusContainer = Factory.New<CusContainer>();
			return DocCusContainer.New(cusContainer, Factory);
		}

		protected override DocCusContainerCollection GetCollectionToTest()
		{
			return new DocCusContainerCollection(Factory);
		}
	}
}
