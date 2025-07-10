using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ATFCollection))]
	public class ATFCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.ATFLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.ATFLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
			invoiceLine.SetReadOnlyIncludingChildren(true);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CopyLastPGADetailsToNewLine = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.ATFLines.AllowAddNewPGALines = true;
			invoiceLine.JI_CustomsQuantity = 888.88;
			var atfLine1 = invoiceLine.ATFLines.AddNew();
			Assert("Expected: the correct default value is 0", atfLine1.US_Quantity == 0);

			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 888.88;
			var atfLine2 = invoiceLine.ATFLines.AddNew();
			Assert("Expected: the correct default value is 888.88", atfLine2.US_Quantity == 888.88);

			atfLine2.US_Quantity = 333.33;
			var atfLine3 = invoiceLine.ATFLines.AddNew();
			Assert("Expected: the correct default value is 555.55", atfLine3.US_Quantity == 555.55);

			var atfLine4 = invoiceLine.ATFLines.AddNew();
			Assert("Expected: the correct default value is 0", atfLine4.US_Quantity == 0);
		}

		public void TestInitiateMunitionsListCategoriWhenCreated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_FDAADTA = ZDateTime.BrettsBirthday;
			declaration.US_SchDEntry = "1101";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CN";

			var atf1 = invoiceLine.ATFLines.AddNew();
			AssertEquals("I (a)-(e)", atf1.US_MunitionsListCategory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ATFCollection(InvoiceLine);
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
