using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolDocumentSupporterQueryProviderTest : TestCaseWithFactory
	{
		public void TestConfirmBOLPrinting()
		{
			ICommonConsolDocumentSupporterQueryProvider queryProvider = new CommonConsolDocumentSupporterQueryProvider();
			CommonShipment shipment = Factory.New<CommonShipment>();

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;
			AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
		}

		public void TestGetConsolToPrint()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			DocumentCommonConsol docConsol = new DocumentCommonConsol(consol, Constants.DataContext.LoadListDocument);

			ICommonConsolDocumentSupporterQueryProvider queryProvider = new CommonConsolDocumentSupporterQueryProvider();
			AssertEquals(docConsol, queryProvider.GetConsolToPrint(docConsol));
		}

		public void TestGetContainersToPrint()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);
			ContainerToSelectFromForPrinting containerToSelect1 = new ContainerToSelectFromForPrinting(container1);
			ContainerToSelectFromForPrinting containerToSelect2 = new ContainerToSelectFromForPrinting(container2);
			ContainerToSelectFromForPrinting containerToSelect3 = new ContainerToSelectFromForPrinting(container3);

			containersToSelectFrom.Add(containerToSelect1);
			containersToSelectFrom.Add(containerToSelect2);
			containersToSelectFrom.Add(containerToSelect3);

			ICommonConsolDocumentSupporterQueryProvider queryProvider = new CommonConsolDocumentSupporterQueryProvider();
			ContainersToPrintOptions containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);

			AssertNotNull(containersToPrintOptions);
			AssertContainsExactElementsInAnyOrder(new[] { containerToSelect1.Container, containerToSelect2.Container, containerToSelect3.Container }, containersToPrintOptions.ContainersToPrint);
			AssertEquals(true, containersToPrintOptions.IncludeUnContainerised);
		}
	}
}
