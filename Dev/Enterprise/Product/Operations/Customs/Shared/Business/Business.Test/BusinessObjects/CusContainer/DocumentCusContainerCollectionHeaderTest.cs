using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DocumentCusContainerCollectionHeader))]
	sealed class DocumentCusContainerCollectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			var baseCusContainerCollection = new BaseCusContainerCollection<BaseCusContainer>(baseJobDeclaration, Factory);
			var documentCusContainerCollection = new DocumentCusContainerCollection(Factory, baseCusContainerCollection);
			return new DocumentCusContainerCollectionHeader(documentCusContainerCollection);
		}

		public void TestDocumentCusContainerCollection()
		{
			BaseJobDeclaration baseJobDeclaration = Factory.New<BaseJobDeclaration>();

			var baseCusContainerCollection = new BaseCusContainerCollection<BaseCusContainer>(baseJobDeclaration, Factory);
			var cusContainer1 = baseCusContainerCollection.AddNew();
			var cusContainer2 = baseCusContainerCollection.AddNew();

			var documentCusContainerCollection = new DocumentCusContainerCollection(Factory, baseCusContainerCollection);
			var documentCusContainerCollectionHeader = new DocumentCusContainerCollectionHeader(documentCusContainerCollection);

			cusContainer1.CO_ContainerNumber = "COOO1111111";
			cusContainer2.CO_ContainerNumber = "COOO2222222";

			AssertEquals("2 containers in collection", 2, documentCusContainerCollection.Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			hashtable.Add(cusContainer1, cusContainer1);
			hashtable.Add(cusContainer2, cusContainer2);
			foreach (DocumentCusContainer wrappedCusContainer in documentCusContainerCollectionHeader.DocumentCusContainerCollection)
			{
				Assert("Should be in Collection", hashtable.Contains(wrappedCusContainer.Container));
			}
		}

		public void TestGridColumnProperties()
		{
			BaseJobDeclaration baseJobDeclaration = Factory.New<BaseJobDeclaration>();

			var baseCusContainerCollection = new BaseCusContainerCollection<BaseCusContainer>(baseJobDeclaration, Factory);
			var cusContainer = baseCusContainerCollection.AddNew();
			cusContainer.CO_ContainerNumber = "COOO1234567";
			cusContainer.CO_FCL_LCL_AIR = "FCL";
			cusContainer.CO_Seal = "222";
			cusContainer.CO_Weight = ZDecimal.Parse("2.4");
			cusContainer.CO_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE")).PK;

			var documentCusContainerCollection = new DocumentCusContainerCollection(Factory, baseCusContainerCollection);
			var documentCusContainerCollectionHeader = new DocumentCusContainerCollectionHeader(documentCusContainerCollection);
			AssertEquals("Print Container default", true, documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].PrintContainer);
			AssertEquals("Container number", "COOO1234567", documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].Container.CO_ContainerNumber);
			AssertEquals("Container fcl/lcl", "FCL", documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].Container.CO_FCL_LCL_AIR);
			AssertEquals("Container seal", "222", documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].Container.CO_Seal);
			AssertEquals("Container weight", ZDecimal.Parse("2.4"), documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].Container.CO_Weight);
			AssertEquals("Container code", "20RE", documentCusContainerCollectionHeader.DocumentCusContainerCollection[0].Container.Container.RC_Code);
		}
	}
}
