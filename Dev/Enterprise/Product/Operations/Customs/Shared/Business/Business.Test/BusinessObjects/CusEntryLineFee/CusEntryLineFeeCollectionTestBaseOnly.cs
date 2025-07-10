using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryLineFeeCollectionTestBaseOnly : TestCaseWithFactory
	{
		public void TestAdditionalFilter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee1 = Factory.New<CusEntryLineFee>();
			fee1.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			fee1.CF_CL = entryLine.PK;
			var fee2 = Factory.New<CusEntryLineFee>();
			fee2.CF_Source = null;
			fee2.CF_CL = entryLine.PK;
			var fee3 = Factory.New<CusEntryLineFee>();
			fee3.CF_CL = entryLine.PK;

			AssertContainsExactElementsInAnyOrder(new CusEntryLineFee[] { fee2, fee3 }, entryLine.Fees);
		}

		public void TestGetAmount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee1 = entryLine.Fees.AddOrUpdate("TS1", 1m);
			fee1.CF_IsLandedCostOnly = true;
			var fee2 = entryLine.Fees.AddOrUpdate("TS2", 11m);
			var fee3 = entryLine.Fees.AddOrUpdate("TS3", 111m);
			fee3.CF_IsLandedCostOnly = true;
			var fee4 = entryLine.Fees.AddOrUpdate("TS4", 1111m);
			var fee4lc = entryLine.Fees.AddOrUpdate("TS4", 11m, true);

			AssertEquals("GetAmount", ZDecimal.Zero, entryLine.Fees.GetAmount("TS1"));
			AssertEquals("GetTotalAmount W/O Landed Cost", ZDecimal.Zero, entryLine.Fees.GetTotalAmount("TS1", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 1m, entryLine.Fees.GetTotalAmount("TS1", includeLandedCostOnly: true));

			AssertEquals("GetAmount", 11m, entryLine.Fees.GetAmount("TS2"));
			AssertEquals("GetTotalAmount W/O Landed Cost", 11m, entryLine.Fees.GetTotalAmount("TS2", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 11m, entryLine.Fees.GetTotalAmount("TS2", includeLandedCostOnly: true));

			AssertEquals("GetAmount", ZDecimal.Zero, entryLine.Fees.GetAmount("TS3"));
			AssertEquals("GetTotalAmount W/O Landed Cost", ZDecimal.Zero, entryLine.Fees.GetTotalAmount("TS3", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 111m, entryLine.Fees.GetTotalAmount("TS3", includeLandedCostOnly: true));

			AssertEquals("GetAmount", 1111m, entryLine.Fees.GetAmount("TS4"));
			AssertEquals("GetTotalAmount W/O Landed Cost", 1111m, entryLine.Fees.GetTotalAmount("TS4", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 1122m, entryLine.Fees.GetTotalAmount("TS4", includeLandedCostOnly: true));
		}

		public void TestGetFeeForLandedCosting()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var fee1 = entryLine.Fees.AddOrUpdate("TS1", 10m);
			fee1.CF_IsLandedCostOnly = true;

			var fee2 = entryLine.Fees.AddOrUpdate("TS3", 20m);
			var fee3 = entryLine.Fees.AddOrUpdate("TS5", 30m);
			var fee4 = entryLine.Fees.AddOrUpdate("TS2", 40m);
			fee4.CF_IsLandedCostOnly = true;
			var fee5 = entryLine.Fees.AddOrUpdate("TS4", 50m);
			fee5.CF_IsLandedCostOnly = true;

			AssertEquals(10m, entryLine.Fees.GetAmountIncludingLCOnly("TS1"));
			AssertEquals(20m, entryLine.Fees.GetAmountIncludingLCOnly("TS3"));
			AssertEquals(30m, entryLine.Fees.GetAmountIncludingLCOnly("TS5"));
			AssertEquals(40m, entryLine.Fees.GetAmountIncludingLCOnly("TS2"));
			AssertEquals(50m, entryLine.Fees.GetAmountIncludingLCOnly("TS4"));
			AssertEquals(ZDecimal.Zero, entryLine.Fees.GetTotalAmount("TS4", includeLandedCostOnly: false));
		}

		public void TestSetAmount()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			entryLine.Fees.GetOrAddFeeByFeeType("AAA").CF_ChargeAmount = 10m;
			entryLine.Fees.GetOrAddFeeByFeeType("BBB").CF_ChargeAmount = 20m;

			entryLine.Fees.SetAmount("AAA", 0m);
			entryLine.Fees.SetAmount("BBB", 30m);
			entryLine.Fees.SetAmount("CCC", 40m);
			entryLine.Fees.SetAmount("DDD", 40m);
			entryLine.Fees.SetAmount("EEE", 50m);

			AssertEquals(0m, entryLine.Fees.GetAmount("AAA"));
			AssertEquals(30m, entryLine.Fees.GetAmount("BBB"));
			AssertEquals(40m, entryLine.Fees.GetAmount("CCC"));

			AssertEquals(false, entryLine.Fees.GetOrAddFeeByFeeType("DDD").CF_IsLandedCostOnly);
			AssertEquals(false, entryLine.Fees.GetOrAddFeeByFeeType("EEE").CF_IsLandedCostOnly);
		}

		public void TestCopyValuesFrom()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLineFee lineFee = entryLine.Fees.AddOrUpdate("AAA", 10m);
			CusEntryLineFee lineFee2 = entryLine.Fees.AddOrUpdate("BBB", 20m);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			CusEntryLineFee lineFee3 = entryLine2.Fees.AddOrUpdate("AAA", 15m);
			CusEntryLineFee lineFee4 = entryLine2.Fees.AddOrUpdate("CCC", 30m);

			entryLine.Fees.CopyChargesValuesFrom(entryLine2.Fees);
			AssertEquals("Charge with AAA", 15m, entryLine.Fees.GetAmount("AAA"));
			AssertEquals("Charge with BBB", 0m, entryLine.Fees.GetAmount("BBB"));
			AssertEquals("Charge with CCC", 30m, entryLine.Fees.GetAmount("CCC"));
		}

		public void TestConstructor()
		{
			AssertNotNull(collection);
		}

		public void TestOverriddenAddNew()
		{
			Assert(typeof(CusEntryLineFee).IsAssignableFrom(collection.AddNew().GetType()));
		}

		public void TestTypedIndexer()
		{
			CusEntryLineFee fee = collection.AddNew();
			AssertEquals(fee, collection[0]);
		}

		public void TestFeeIsCW1()
		{
			CusEntryLineFee fee = collection.AddNew();
			AssertEquals(CusEntryLineFeeSourceCodeList.Codes.CW1, fee.CF_Source);
			Assert(!fee.IsConfirmed);
		}

		public void TestStringTypedIndexer()
		{
			CusEntryLineFee fee = collection.AddNew();
			const string WoodLevy = "WDL";
			fee.CF_ChargeType = WoodLevy;
			AssertEquals(fee, collection.GetOrAddFeeByFeeType(WoodLevy));
		}

		public void TestStringTypedIndexerAutomaticallyGeneratesFee()
		{
			AssertEquals("Precondition : Collection.Count", 0, collection.Count);
			const string WoodLevy = "WDL";
			CusEntryLineFee fee = collection.GetOrAddFeeByFeeType(WoodLevy);
			AssertEquals("Collection.Count after accessing string indexer", 1, collection.Count);
			AssertEquals(fee, collection[0]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(Factory.New<CusEntryLine>(), Factory);
		}
		CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> collection;
	}
}
