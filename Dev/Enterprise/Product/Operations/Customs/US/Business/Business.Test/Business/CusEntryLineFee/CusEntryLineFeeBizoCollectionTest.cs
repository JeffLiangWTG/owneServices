using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryLineFeeCollection))]
	sealed class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		public void TestTotalAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 100m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 200m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 200m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 300m);
			AssertEquals("Total all fees", 900m, entryLine.Fees.TotalAmount);
		}

		public void TestUpdateOrAddCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 0m);
			AssertEquals(0m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
			AssertNull(entryLine.Fees.GetElementWithThisCode(Core.Constants.USCustoms.FeeCodes.Avocado));
			entryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			AssertEquals(100m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
			entryLine.Fees.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 0m);
			AssertEquals(0m, entryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
		}

		public void TestIFees()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate("AAA", 10m);
			entryLine.Fees.AddOrUpdate("BBB", 20m);
			IFees feeAndCharges = entryLine.Fees;
			AssertEquals(20m, feeAndCharges.GetFeeOrChargeAmount("BBB"));
			AssertEquals(0m, feeAndCharges.GetFeeOrChargeAmount("CCC"));
			AssertNotNull(feeAndCharges.AddNew());
			AssertEquals("Three elements in collection now", 3, entryLine.Fees.Count);
		}

		public void TestGetExciseTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 200m);
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.OtherExcise, 400m);
			AssertEquals("Excise tax", 600m, entryLine.Fees.GetExciseTax());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			return new CusEntryLineFeeCollection(entryLine, Factory);
		}
	}
}
