using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class BondedWarehouseTransactionLineTestCase : TestCaseWithFactory
	{
		public void TestWarehouse()
		{
			var line = new BondedWarehouseTransactionLine(entryLine);
			AssertNull(line.Warehouse);
		}

		public void TestEntryKey()
		{
			declaration.JE_CustomsOffice = "JSA";
			entryHeader.EntryNumber = "123A";
			entryHeader.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2001, 2, 3));
			var line = new BondedWarehouseTransactionLine(entryLine);
			AssertEquals("123A/JSA/02.01", line.EntryKey);
		}

		public void TestEntryKey_CustomsOfficeOverride()
		{
			declaration.JE_CustomsOffice = "JSA";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CustomsOfficeOverride = "CNT";
			entryHeader.EntryNumber = "123A";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2001, 2, 3));

			CombineAssertions(() =>
			{
				var line = new BondedWarehouseTransactionLine(entryLine);
				AssertEquals("Use CustomsOfficeOverride", "123A/CNT/02.01", line.EntryKey);

				instruction.CEI_CustomsOfficeOverride = "";
				line = new BondedWarehouseTransactionLine(entryLine);
				AssertEquals("Use JE_CustomsOffice when CustomsOfficeOverride is empty", "123A/JSA/02.01", line.EntryKey);
			});
		}

		public void TestGetEntryLineNumberFromInvoiceLine()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			var line = new BondedWarehouseTransactionLine(entryLine);
			invoiceLine.JI_PreviousEntryLineNumber = 4;
			AssertEquals((ZShort)4, line.EntryLineNumber);
		}

		public void TestAddInfo()
		{
			invoiceLine.JI_PermitNumber = "@@";
			var line = new BondedWarehouseTransactionLine(entryLine);
			AssertEquals("Addinfo string returned", entryLine.RandomLine.GetAddInfoString(), ((IWhsBondedWarehouseTransactionLine)line).AddInfo);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
	}
}
