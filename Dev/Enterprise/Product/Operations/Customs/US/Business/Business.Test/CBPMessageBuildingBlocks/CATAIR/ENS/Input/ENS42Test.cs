using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS42Test : BIRDLineUpdateTest
	{
		public void TestUpdateSuplierAddressByCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var notifications = new NotificationBuffer();

			var eNS42 = new ENS42() { SupplierIDCode = "PHEVEAPP80SAB" };
			((IBIRDLineRecord)eNS42).Update(invoiceLine, notifications);
			AssertEquals("Matched address found by MID", orgAddress1.PK, invoiceHeader.JZ_OA_SupplierAddress);

			invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
			eNS42 = new ENS42() { SupplierIDCode = "  PHEVEAPP80SAB" };
			((IBIRDLineRecord)eNS42).Update(invoiceLine, notifications);
			AssertEquals("No matched address found because MID is invalid", ZGuid.Empty, invoiceHeader.JZ_OA_SupplierAddress);

			invoiceHeader.JZ_OA_SupplierAddress = ZGuid.Empty;
			eNS42 = new ENS42() { SupplierIDCode = "PHEVEAPP80SCD" };
			((IBIRDLineRecord)eNS42).Update(invoiceLine, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, invoiceHeader.JZ_OA_SupplierAddress);
		}

		public override void TestUpdateLine()
		{
			Assert("There is nothing users can do to cause system to output 42 records. Fields to make declaration electronic invoicing are removed from UI", true);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS42 ens42 = new ENS42();

			ens42.SupplierIDCode = "ECPEFCIA113MAN";
			ens42.InvoiceNumber = "OUYI-89342";

			ens42.BeginningInvoiceLineNumberA = 1;
			ens42.EndingInvoiceLineNumberA = 2;

			ens42.BeginningInvoiceLineNumberB = 4;
			ens42.EndingInvoiceLineNumberB = 5;

			ens42.BeginningInvoiceLineNumberC = 7;
			ens42.EndingInvoiceLineNumberC = 8;

			ens42.BeginningInvoiceLineNumberD = 10;
			ens42.EndingInvoiceLineNumberD = 11;

			ens42.BeginningInvoiceLineNumberE = 13;
			ens42.EndingInvoiceLineNumberE = 14;

			return new IBIRDLineRecord[] { ens42 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			invoiceLine.JI_Tariff = "0";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";

			InvoiceLineGroupingRange lineGrouping = invoiceLine2.LineGroupingRanges.AddNew();
			lineGrouping.US_StartSequenceNo = 3;

			lineGrouping = invoiceLine2.LineGroupingRanges.AddNew();
			lineGrouping.US_StartSequenceNo = 6;

			lineGrouping = invoiceLine2.LineGroupingRanges.AddNew();
			lineGrouping.US_StartSequenceNo = 9;

			lineGrouping = invoiceLine2.LineGroupingRanges.AddNew();
			lineGrouping.US_StartSequenceNo = 12;

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ECPEFCIA113MAN", GlbCompany.CurrentCompany.Country);
			Factory.Save();
		}

		protected override System.Type GetTypeOfMessageBlock() => typeof(ENS42);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
