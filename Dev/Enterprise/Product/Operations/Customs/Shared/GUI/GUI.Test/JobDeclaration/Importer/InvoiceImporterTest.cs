using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceImporterTest : TestCaseWithFactory
	{
		public void TestInitialiseInvoiceImporter()
		{
			importer.ShowInvoiceImporterForm();
			AssertEquals(declaration, importer.Declaration);
			AssertEquals(dataTransferImpl, importer.DataTransferImpl);
		}

		public void TestImporterFormProperty()
		{
			importer.ShowInvoiceImporterForm();
			AssertNotNull(importer.ImporterForm);
			AssertEquals("Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|Excel files (*.xls)|*.xls|Xml files (*.xml)|*.xml|All files (*.*)|*.*", importer.ImporterForm.DialogFilter);
			EventInfo @event = importer.ImporterForm.GetType().GetEvent("StartProcess");
			MethodInfo expectedMethod = importer.GetType().GetMethod("ImporterForm_StartProcess");
			AssertEquals(expectedMethod, @event.GetRaiseMethod());
			@event = importer.ImporterForm.GetType().GetEvent("ProcessCancelled");
			expectedMethod = importer.GetType().GetMethod("ImporterForm_Cancelled");
			AssertEquals(expectedMethod, @event.GetRaiseMethod());
		}

		public void TestImporterForm_StartProcessHandler()
		{
			importer.ShowInvoiceImporterForm();
			Assert(!importer.importerEventsAttached);
			importer.ImporterForm_StartProcess(importer.ImporterForm, new ProcessFileEventArgs("test"));
			EventInfo @event = importer.DataTransferImpl.GetType().GetEvent("Processed");
			MethodInfo expectedMethod = importer.GetType().GetMethod("DataTransferImpl_FileRowProcessed");
			AssertEquals(expectedMethod, @event.GetRaiseMethod());
			@event = importer.DataTransferImpl.GetType().GetEvent("ProcessCompleted");
			expectedMethod = importer.GetType().GetMethod("DataTransferImpl_ProcessCompleted");
			AssertEquals(expectedMethod, @event.GetRaiseMethod());
			@event = importer.DataTransferImpl.GetType().GetEvent("UnknownOrganisationCodeFound");
			expectedMethod = importer.GetType().GetMethod("DataTransferImpl_UnknownOrganisationCodeFound");
			AssertEquals(expectedMethod, @event.GetRaiseMethod());
			Assert(importer.importerEventsAttached);
		}

		public void TestImporterFormDispose()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			importer.ShowInvoiceImporterForm();
			Assert("ImporterForm should be disposed properly after closed", importer.ImporterForm.IsDisposed);
			AssertNoExceptionThrown("only created and disposed in ShowInvoiceImporterForm", () =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var dataTransferImpl = new DataTransferImpl();
				var importer = new InvoiceImporter(declaration, dataTransferImpl);
				importer.DataTransferImpl_FileRowProcessed(null, null);
				importer.DataTransferImpl_ProcessCompleted(null, null);
				importer.ImporterForm_StartProcess(null, null);
				AssertNull(importer.ImporterForm);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			dataTransferImpl = new DataTransferImpl();
			importer = new InvoiceImporter(declaration, dataTransferImpl);
		}

		protected override void TearDown()
		{
			base.TearDown();
			importer.ImporterForm.Dispose();
		}

		BaseJobDeclaration declaration;
		InvoiceImporter importer;
		DataTransferImpl dataTransferImpl;
	}
}
