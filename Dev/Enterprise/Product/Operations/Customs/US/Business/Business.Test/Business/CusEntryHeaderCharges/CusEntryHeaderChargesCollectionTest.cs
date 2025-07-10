using System.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderChargesCollection))]
	sealed class CusEntryHeaderChargesCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestHasDuplicate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew("AAA", 10m);
			AssertEquals(false, entry.Charges.HasDuplicate("AAA"));
			entry.Charges.AddNew("BBB", 11m);
			AssertEquals(false, entry.Charges.HasDuplicate("AAA"));
			AssertEquals(false, entry.Charges.HasDuplicate("BBB"));
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeType = "AAA";
			AssertEquals(true, entry.Charges.HasDuplicate("AAA"));
			AssertEquals(false, entry.Charges.HasDuplicate("BBB"));
		}

		public void TestGetTotalCustomsFeeAmount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("GetTotalCustomsFeeAmount", 0m, entry.Charges.GetTotalCustomsFeeAmount());
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 10m);
			AssertEquals("GetTotalCustomsFeeAmount", 10m, entry.Charges.GetTotalCustomsFeeAmount());
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 10m);
			AssertEquals("GetTotalCustomsFeeAmount should not include tax amount", 10m, entry.Charges.GetTotalCustomsFeeAmount());
		}

		public void TestIFees()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew("AAA", 10m);
			entry.Charges.AddNew("BBB", 20m);
			IFees feeAndCharges = entry.Charges;
			AssertEquals(20m, feeAndCharges.GetFeeOrChargeAmount("BBB"));
			AssertEquals(0m, feeAndCharges.GetFeeOrChargeAmount("CCC"));
			AssertNotNull(feeAndCharges.AddNew());
			AssertEquals("Three elements in collection now", 3, entry.Charges.Count);
		}

		public void TestAddingReconFeeWillAddOriginalFee()
		{
			var reconInnerDec = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(reconInnerDec);
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			var reconCharges = reconOriginalEntry.ReconCharges;
			var originalCharges = reconOriginalEntry.OriginalCharges;
			ICancelAddNew collection = reconCharges;
			var charge1 = reconCharges.AddNew();
			AssertEquals(1, reconOriginalEntry.ReconCharges.Count);
			AssertEquals(0, reconOriginalEntry.OriginalCharges.Count);
			charge1.C1_ChargeType = "AAA";
			collection.EndNew(0);
			AssertEquals(0, reconOriginalEntry.OriginalCharges.Count);
			var charge2 = (CusEntryHeaderCharges)((IBindingList)reconCharges).AddNew();
			AssertEquals(2, reconOriginalEntry.ReconCharges.Count);
			AssertEquals(0, reconOriginalEntry.OriginalCharges.Count);
			charge2.C1_ChargeType = "BBB";
			collection.EndNew(1);
			AssertEquals(1, reconOriginalEntry.OriginalCharges.Count);
			AssertEquals("BBB", reconOriginalEntry.OriginalCharges[0].CY_Code);
		}

		public void TestAllowNewAndRemove()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDeclaration.OriginalEntries.AddNew();
			reconEntry.Invoice.InvoiceLines.AddNew();
			reconEntry.US_R_NoLineDetails = true;
			Assert(reconEntry.ReconCharges.AllowNew);
			Assert(reconEntry.ReconCharges.AllowRemove);
			reconEntry.US_R_NoLineDetails = false;
			Assert(!reconEntry.ReconCharges.AllowNew);
			Assert(!reconEntry.ReconCharges.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			return new CusEntryHeaderChargesCollection(entryHeader);
		}
	}
}
