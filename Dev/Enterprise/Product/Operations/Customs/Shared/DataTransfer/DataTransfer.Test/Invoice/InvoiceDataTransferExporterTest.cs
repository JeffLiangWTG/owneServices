using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class InvoiceDataTransferExporterTest : TestCaseWithFactory
	{
		public void TestExport()
		{
			using (TempFile file = TempFile.New())
			{
				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_InvoiceNumber = "ywre798453h";

				var importer = Factory.New<OrgHeader>();
				importer.FillWithValidTestData();
				invoice.JZ_OH_Buyer = importer.PK;

				var supplier = Factory.New<OrgHeader>();
				supplier.FillWithValidTestData();
				invoice.JZ_OH_Supplier = supplier.PK;

				Factory.Save();

				var notify = new NotificationBuffer();

				var director = new TestInvoiceDataTransferExporter(StandAloneInvoiceValueObjectDataAdapter.New(), false);

				using (Stream writer = new FileStream(file.Filename, FileMode.Append))
				{
					AssertNoExceptionThrown(() => director.DoExport(writer, new BusinessObject[] { invoice }, notify));
				}

				director.PromptUserAndExport(new BusinessObject[] { invoice });
				AssertEquals("DefaultExportFileName", true, director.DefaultFileName.Contains(invoice.JZ_InvoiceNumber));
				AssertEquals("DefaultExportFileName", true, director.DefaultFileName.Contains(supplier.OH_Code));
			}
		}

		sealed class TestInvoiceDataTransferExporter : InvoiceDataTransferExporter
		{
			public TestInvoiceDataTransferExporter(IValueObjectDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public new void DoExport(Stream file, IList selectedBusinessObjects, INotifications notify) => base.DoExport(file, selectedBusinessObjects, notify);
		}
	}
}
