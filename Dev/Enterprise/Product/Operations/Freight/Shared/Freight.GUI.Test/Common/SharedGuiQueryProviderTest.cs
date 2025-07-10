using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	class SharedGuiQueryProviderTest : TestCaseWithFactory
	{
		public void TestConfirmBOLPrinting()
		{
			SharedGuiQueryProvider queryProvider = new SharedGuiQueryProvider();

			CommonShipment shipment = Factory.New<CommonShipment>();

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;

			AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertContains("You do not have the appropriate security rights to continue running this document", UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertContains("Are you sure you want to run this document", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertContains("Are you sure you want to run this document", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetContainersToPrint()
		{
			ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);

			SharedGuiQueryProvider queryProvider = new SharedGuiQueryProvider();
			ContainersToPrintOptions containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);

			AssertNull(containersToPrintOptions);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertContains("There are no containers", UnitTestUserNotification.Instance.LastMessage.Text);

			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();

			ContainerToSelectFromForPrinting containerToSelect1 = new ContainerToSelectFromForPrinting(container1);
			containerToSelect1.JC_Calc_PrintDocumentForContainer = true;

			containersToSelectFrom.Add(containerToSelect1);

			containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, false);
			AssertContainsExactElementsInAnyOrder(new[] { container1 }, containersToPrintOptions.ContainersToPrint);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);
			AssertContainsExactElementsInAnyOrder(new[] { container1 }, containersToPrintOptions.ContainersToPrint);
			Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentContainersForm);

			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			ContainerToSelectFromForPrinting containerToSelect2 = new ContainerToSelectFromForPrinting(container2);
			containerToSelect2.JC_Calc_PrintDocumentForContainer = true;
			ContainerToSelectFromForPrinting containerToSelect3 = new ContainerToSelectFromForPrinting(container3);
			containerToSelect3.JC_Calc_PrintDocumentForContainer = false;

			containersToSelectFrom.Add(containerToSelect2);
			containersToSelectFrom.Add(containerToSelect3);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);

			AssertNotNull(containersToPrintOptions);
			AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, containersToPrintOptions.ContainersToPrint);
			AssertEquals(false, containersToPrintOptions.IncludeUnContainerised);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);
			AssertNull(containersToPrintOptions);
		}
	}
}
