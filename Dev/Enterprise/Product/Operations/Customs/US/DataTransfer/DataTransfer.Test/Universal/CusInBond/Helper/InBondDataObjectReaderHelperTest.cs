using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public abstract class InBondDataObjectReaderHelperTest<T> : InBondHelperTest where T : CusInBondHeader
	{
		public void TestHandlingOfUnprocessedBills()
		{
			var header1 = Factory.New<T>();
			var bill1 = (CusInBondBill)header1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "HB1";
			var bill2 = (CusInBondBill)header1.Bills.AddNew();
			bill2.B0_MasterBillNumber = "HB2";
			var bill3 = (CusInBondBill)header1.Bills.AddNew();
			bill3.B0_MasterBillNumber = "HB3";
			var bill4 = (CusInBondBill)header1.Bills.AddNew();
			bill4.B0_MasterBillNumber = "HB4";
			var header2 = Factory.New<T>();
			var bill5 = (CusInBondBill)header2.Bills.AddNew();
			bill5.B0_MasterBillNumber = "HB5";
			var bill6 = (CusInBondBill)header2.Bills.AddNew();
			bill6.B0_MasterBillNumber = "HB6";
			var header3 = Factory.New<T>();
			var bill7 = (CusInBondBill)header3.Bills.AddNew();
			bill7.B0_MasterBillNumber = "HB7";
			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.MarkUnprocessedExistingBillsFor(header1);
			helper.MarkUnprocessedExistingBillsFor(header2);
			helper.MarkProcessed(bill2);
			helper.MarkProcessed(bill3);
			helper.MarkProcessed(bill6);
			helper.MarkProcessed(bill7);
			foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}

			var logger = new TestErrorLogger();
			helper.DeleteUnprocessedBillsFor(header3, logger);
			foreach (var bill in new[] { bill1, bill2, bill3, bill4, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}

			AssertEquals("logger.Logs", "", logger.Logs);
			helper.DeleteUnprocessedBillsFor(header1, logger);
			AssertEquals("bill1.IsDeleted", true, bill1.IsDeleted);
			AssertEquals("bill4.IsDeleted", true, bill4.IsDeleted);
			foreach (var bill in new[] { bill2, bill3, bill5, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}

			AssertMultilineASCIIEquals("logger.Logs", string.Format(@"Information - Deleted {0}HB1 from UniversalShipment.
Information - Deleted {1}HB4 from UniversalShipment.", bill1.HumanReadableName, bill4.HumanReadableName), logger.Logs);
			logger.ClearLogs();
			helper.DeleteUnprocessedBillsFor(header2, logger);
			AssertEquals("bill5.IsDeleted", true, bill5.IsDeleted);
			foreach (var bill in new[] { bill2, bill3, bill6, bill7 })
			{
				AssertEquals("IsDeleted", false, bill.IsDeleted);
			}

			AssertEquals("logger.Logs", string.Format("Information - Deleted {0}HB5 from UniversalShipment.", bill5.HumanReadableName), logger.Logs);
		}

		CusInBondHeader inBondHeader;
		protected CusInBondHeader InBondHeader => inBondHeader ?? (inBondHeader = Factory.New<T>());

		InBondDataObjectReaderHelper helper;
		protected InBondDataObjectReaderHelper Helper => helper ?? (helper = CreateHelper(Factory));

		protected abstract InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory);
	}
}
