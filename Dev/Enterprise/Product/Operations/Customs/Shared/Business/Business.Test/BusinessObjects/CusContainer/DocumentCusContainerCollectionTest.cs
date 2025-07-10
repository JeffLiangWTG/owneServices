using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DocumentCusContainerCollectionTest : TestCaseWithFactory
	{
		public void TestDocumentCusContainerCollection()
		{
			var baseJobDeclaration = Factory.New<BaseJobDeclaration>();

			var baseCusContainerCollection = new BaseCusContainerCollection<BaseCusContainer>(baseJobDeclaration, Factory);
			var cusContainer1 = baseCusContainerCollection.AddNew();
			var cusContainer2 = baseCusContainerCollection.AddNew();

			var documentCusContainerCollection = new DocumentCusContainerCollection(Factory, baseCusContainerCollection);

			cusContainer1.CO_ContainerNumber = "COOO1111111";
			cusContainer2.CO_ContainerNumber = "COOO2222222";

			AssertEquals("2 containers in collection", 2, documentCusContainerCollection.Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			hashtable.Add(cusContainer1, cusContainer1);
			hashtable.Add(cusContainer2, cusContainer2);
			foreach (DocumentCusContainer wrappedCusContainer in documentCusContainerCollection)
			{
				Assert("Should be in Collection", hashtable.Contains(wrappedCusContainer.Container));
			}
		}
	}
}
