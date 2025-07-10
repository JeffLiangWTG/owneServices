using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ServicesSelectionProviderTest : TestCaseWithFactory
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestNoException()
		{
			IServicesSelectionProvider provider = new ServicesSelectionGuiProvider();
			AssertNoExceptionThrown(() => provider.GetServicesToPrint(null));

			var serviceParent = new Mock<IHaveServices>(MockBehavior.Strict);
			serviceParent.Setup(m => m.Services).Returns((JobServiceDependentCollection)null);
			AssertNoExceptionThrown(() => provider.GetServicesToPrint(serviceParent.Object));
		}

		public void TestRegister()
		{
			IServicesSelectionProvider provider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("prerequisite", provider);

			ServicesSelectionGuiProvider.Register(Factory);

			provider = Factory.GetValue<IServicesSelectionProvider>();

			AssertNotNull(provider);
			Assert(provider is ServicesSelectionGuiProvider);
		}

		public void TestNoServices()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			IServicesSelectionProvider provider = new ServicesSelectionGuiProvider();
			JobService[] services = provider.GetServicesToPrint(shipment.DocsAndCartage);

			AssertNull(services);
			AssertEquals("There are no services to print.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestServices()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobService service1 = shipment.DocsAndCartage.Services.AddNew();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			IServicesSelectionProvider provider = new ServicesSelectionGuiProvider();
			JobService[] services = provider.GetServicesToPrint(shipment.DocsAndCartage);

			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertContainsExactElementsInAnyOrder(new[] { service1 }, services);

			JobService service2 = shipment.DocsAndCartage.Services.AddNew();
			JobService service3 = shipment.DocsAndCartage.Services.AddNew();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
			provider = new ServicesSelectionGuiProvider();
			services = provider.GetServicesToPrint(shipment.DocsAndCartage);

			AssertNull(services);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			provider = new ServicesSelectionGuiProvider();
			services = provider.GetServicesToPrint(shipment.DocsAndCartage);

			Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentServicesForm);
			AssertContainsExactElementsInAnyOrder(new[] { service1, service2, service3 }, services);
		}
	}
}
