using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class ForwardingJobInvoicesExporterTest : TestCaseWithFactory
	{
		public void TestExportInvoices()
		{
			ForwardingJobInvoicesExporter exporter = new ForwardingJobInvoicesExporter(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			Xsd.TxnHeaderCollection invoiceValues = exporter.PopulateInvoicesToXSD(Shipment.JobNumber, new ValueObjectExportContext(buffer), false);

			AssertEquals("invoiceValue count", 1, invoiceValues.Count);
			Xsd.TxnHeader invoiceValue = invoiceValues[0];
			AssertEquals("Invoice in the collection", ARInvoice[AccTransactionHeaderSchema.AH_TransactionNum], invoiceValue.TxnNumber);
		}

		void SetupShipmentWithInvoices()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();

			JobHeader jobheader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobheader.JH_ParentID = Shipment.PK;
			jobheader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			ARInvoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoice)));
			ARInvoice[AccTransactionHeaderSchema.AH_JH] = jobheader.PK;
			ARInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = Shipment.JobNumber;

			BusinessObject aRInvoiceLine = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IARInvoiceLine)));
			aRInvoiceLine[AccTransactionLinesSchema.AL_AH] = ARInvoice.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_JH] = jobheader.PK;
			aRInvoiceLine[AccTransactionLinesSchema.AL_AG] = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = aRInvoiceLine.PK;
			charge.JR_JH = jobheader.PK;

			Factory.Save();
		}

		ForwardingShipment Shipment;
		BusinessObject ARInvoice;

		protected override void SetUp()
		{
			base.SetUp();
			SetupShipmentWithInvoices();
		}
	}
}
