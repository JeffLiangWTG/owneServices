using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentContainers))]
	sealed class DocumentContainersTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			ContainerToSelectFromForPrintingCollection collection = new ContainerToSelectFromForPrintingCollection(Factory);
			return new DocumentContainers(collection);
		}

		public void TestContainersToSelectFrom()
		{
			ContainerToSelectFromForPrinting container1 = new ContainerToSelectFromForPrinting(Factory.New<CommonContainer>());
			ContainerToSelectFromForPrinting container2 = new ContainerToSelectFromForPrinting(Factory.New<CommonContainer>());

			ContainerToSelectFromForPrintingCollection commonContainerCollection = new ContainerToSelectFromForPrintingCollection(Factory);
			commonContainerCollection.Add(container1);
			commonContainerCollection.Add(container2);
			Factory.Save();

			DocumentContainers docContainer = new DocumentContainers(commonContainerCollection);
			AssertNotNull("ContainersToSelectFrom is not null", docContainer.ContainersToSelectFrom);
			AssertEquals("ContainersToSelectFrom count", 2, docContainer.ContainersToSelectFrom.Count);
		}

		public void TestContainersToPrint()
		{
			ContainerToSelectFromForPrinting container1 = new ContainerToSelectFromForPrinting(Factory.New<CommonContainer>());
			container1.JC_Calc_PrintDocumentForContainer = ZBool.True;
			ContainerToSelectFromForPrinting container2 = new ContainerToSelectFromForPrinting(Factory.New<CommonContainer>());
			container2.JC_Calc_PrintDocumentForContainer = ZBool.False;

			ContainerToSelectFromForPrintingCollection commonContainerCollection = new ContainerToSelectFromForPrintingCollection(Factory);
			commonContainerCollection.Add(container1);
			commonContainerCollection.Add(container2);
			Factory.Save();

			DocumentContainers docContainer = new DocumentContainers(commonContainerCollection);
			AssertNotNull("ContainersToPrint is not null", docContainer.ContainersToPrint);
			AssertEquals("ContainersToPrint count", 1, docContainer.ContainersToPrint.Count);
		}
	}
}
