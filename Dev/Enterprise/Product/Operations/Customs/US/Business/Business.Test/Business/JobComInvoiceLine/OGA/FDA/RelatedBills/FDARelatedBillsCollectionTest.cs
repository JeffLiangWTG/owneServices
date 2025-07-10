using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedBillsCollection))]
	public class FDARelatedBillsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FDARelatedBillsCollection>
	{
		public void TestDoNotDefaultValuesWhenConstructed()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			shipment.RegisterEditableChildObject(declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "M9432890";
			declaration.JE_HouseBill = "H9432890";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var fda = invoiceLine.FDAs.AddNew();

			var collectionAccessed = fda.BillsForFDALine;

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		public void TestDefaultFDARelatedBillsDoesNotThrowExceptionWhenFDAIsDeleted()
		{
			TestDefaultFDARelatedBillIsSet();

			JobDeclaration declaration = FDA.Declaration;
			FDA.Delete();
			AssertNoExceptionThrown(delegate
			{ declaration.Bills.RemoveAndDeleteAll(); });
		}

		public void TestDefaultFDARelatedBillIsSet()
		{
			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("House", BillTypeList.Codes.HouseBill));
			var billsAvailable = FDA.BillsAvailable;
			AssertEquals(1, billsAvailable.Count);
			AssertEquals("HB:House", billsAvailable[0].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", true, billsAvailable[0].IsForFDALine);

			RemoveAndDeleteAllTestObjects();

			FDA.Declaration.Bills.Add(GetBill("", BillTypeList.Codes.HouseBill));
			billsAvailable = FDA.BillsAvailable;
			AssertEquals(0, billsAvailable.Count);

			RemoveAndDeleteAllTestObjects();

			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("Master", BillTypeList.Codes.MasterBill));
			billsAvailable = FDA.BillsAvailable;
			AssertEquals(1, billsAvailable.Count);
			AssertEquals("MB:Master", billsAvailable[0].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", true, billsAvailable[0].IsForFDALine);
		}

		public void TestDefaultFDARelatedBillIsHouseBillWhenThereIsOneOtherMasterBill()
		{
			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("House", BillTypeList.Codes.HouseBill));
			FDA.Declaration.Bills.Add(GetBill("Master", BillTypeList.Codes.MasterBill));

			var billsAvailable = FDA.BillsAvailable;
			AssertEquals(2, billsAvailable.Count);
			AssertEquals("HB:House", billsAvailable[0].BillNumber);
			AssertEquals("MB:Master", billsAvailable[1].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", true, billsAvailable[0].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[1].IsForFDALine", false, billsAvailable[1].IsForFDALine);

			RemoveAndDeleteAllTestObjects();

			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("Master", BillTypeList.Codes.MasterBill));
			FDA.Declaration.Bills.Add(GetBill("House", BillTypeList.Codes.HouseBill));
			FDA.BillsForFDALine.RemoveAndDeleteAll();

			var fda = InvoiceLine.FDAs.AddNew();
			var coll = fda.BillsAvailable;
			AssertEquals(2, coll.Count);
			AssertEquals("MB:Master", coll[0].BillNumber);
			AssertEquals("HB:House", coll[1].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", false, coll[0].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[1].IsForFDALine", true, coll[1].IsForFDALine);
		}

		public void TestFDARelatedBillsIsNotChangedAfterAddingBills()
		{
			AssertEquals(0, FDA.Declaration.Bills.Count);

			var masterBill = GetBill("Master", BillTypeList.Codes.MasterBill);
			FDA.Declaration.Bills.Add(masterBill);
			FDA.BillsForFDALine.AddPivotFor(masterBill);

			FDA.Declaration.Bills.Add(GetBill("House", BillTypeList.Codes.HouseBill));
			var billsAvailable = FDA.BillsAvailable;
			AssertEquals(2, billsAvailable.Count);
			AssertEquals("MB:Master", billsAvailable[0].BillNumber);
			AssertEquals("HB:House", billsAvailable[1].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", true, billsAvailable[0].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[1].IsForFDALine", false, billsAvailable[1].IsForFDALine);
		}

		public void TestDefaultFDARelatedBillIsNotSetWhenMultipleBillsOfSameType()
		{
			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("Master1", BillTypeList.Codes.MasterBill));
			FDA.Declaration.Bills.Add(GetBill("House1", BillTypeList.Codes.HouseBill));
			FDA.Declaration.Bills.Add(GetBill("Master2", BillTypeList.Codes.MasterBill));

			FDA.BillsForFDALine.RemoveAndDeleteAll();
			var billsAvailable = FDA.BillsAvailable;
			AssertEquals(3, billsAvailable.Count);
			AssertEquals("MB:Master1", billsAvailable[0].BillNumber);
			AssertEquals("HB:House1", billsAvailable[1].BillNumber);
			AssertEquals("MB:Master2", billsAvailable[2].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", false, billsAvailable[0].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[1].IsForFDALine", false, billsAvailable[1].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[2].IsForFDALine", false, billsAvailable[2].IsForFDALine);

			RemoveAndDeleteAllTestObjects();
			AssertEquals(0, FDA.Declaration.Bills.Count);

			FDA.Declaration.Bills.Add(GetBill("House1", BillTypeList.Codes.HouseBill));
			FDA.Declaration.Bills.Add(GetBill("Master1", BillTypeList.Codes.MasterBill));
			FDA.BillsForFDALine.RemoveAndDeleteAll();
			FDA.Declaration.Bills.Add(GetBill("House2", BillTypeList.Codes.HouseBill));

			billsAvailable = FDA.BillsAvailable;
			AssertEquals(3, billsAvailable.Count);
			AssertEquals("HB:House1", billsAvailable[0].BillNumber);
			AssertEquals("MB:Master1", billsAvailable[1].BillNumber);
			AssertEquals("HB:House2", billsAvailable[2].BillNumber);
			AssertEquals("FDARelatedBillsCollection[0].IsForFDALine", false, billsAvailable[0].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[1].IsForFDALine", false, billsAvailable[1].IsForFDALine);
			AssertEquals("FDARelatedBillsCollection[2].IsForFDALine", false, billsAvailable[2].IsForFDALine);
		}

		public void TestFDARelatedBillAddNew()
		{
			var billsAvailable = FDA.BillsAvailable;
			billsAvailable.AddNew(GetBill("99999"));
			AssertEquals(":99999", billsAvailable[0].BillNumber);
		}

		public void TestDeleteBillFromDeclRebuildCollection()
		{
			AddBill("9999");
			AddBill("8888");
			AddBill("7777");

			AssertEquals(3, FDA.BillsAvailable.Count);

			InvoiceLine.Declaration.Bills[1].Delete();
			AssertEquals(2, FDA.BillsAvailable.Count);
		}

		public void TestFindByBillNumber()
		{
			var billsAvailable = FDA.BillsAvailable;
			billsAvailable.AddNew(GetBill("99999"));
			billsAvailable.AddNew(GetBill("12345"));

			AssertNull(billsAvailable.FindByBillNumber("3456"));
			AssertNotNull(billsAvailable.FindByBillNumber("99999"));
		}

		protected override FDARelatedBillsCollection GetCollectionToTest()
		{
			return new FDARelatedBillsCollection(FDA);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FDARelatedBill(FDA);
		}

		void AddBill(ZString billNumber)
		{
			var bill = InvoiceLine.Declaration.Bills.AddNew();
			bill.CU_BillNum = billNumber;
		}

		Bill GetBill(ZString billNumber)
		{
			return GetBill(billNumber, ZString.Empty);
		}

		Bill GetBill(ZString billNumber, ZString billType)
		{
			var result = Factory.New<Bill>();
			result.CU_BillNum = billNumber;
			result.CU_BillType = billType;
			return result;
		}

		void RemoveAndDeleteAllTestObjects()
		{
			fda.Delete();
			fda = null;
			invoiceLine.Delete();
			invoiceLine = null;
		}

		FDA FDA
		{
			get
			{
				if (fda == null)
				{
					fda = InvoiceLine.FDAs.AddNew();
				}
				return fda;
			}
		}
		FDA fda;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;
	}
}
