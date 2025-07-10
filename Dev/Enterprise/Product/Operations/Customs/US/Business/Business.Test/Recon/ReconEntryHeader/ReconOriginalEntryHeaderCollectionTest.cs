using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconOriginalEntryHeaderCollection))]
	sealed class ReconOriginalEntryHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReconOriginalEntryHeaderCollection>
	{
		public void TestFindEntryBy()
		{
			ReconOriginalEntryHeader entry = ReconDeclaration.OriginalEntries.AddNew();
			entry.CH_OrigEntryReference = "ABC897342"; //this import entry belongs to a different broker;
			ReconOriginalEntryHeader entry2 = ReconDeclaration.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "XJ5978478";
			AssertEquals(entry, ReconDeclaration.OriginalEntries.FindEntryBy("ABC897342"));
			AssertEquals(entry2, ReconDeclaration.OriginalEntries.FindEntryBy("XJ5978478"));
		}

		public void TestHasImportableEntriesWithoutAnyLinesAttached()
		{
			ReconDeclaration.US_EntryFilerCode = "XJ5";
			AssertEquals(false, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
			ReconOriginalEntryHeader entry = ReconDeclaration.OriginalEntries.AddNew();
			entry.CH_OrigEntryReference = "ABC897342"; //this import entry belongs to a different broker;
			AssertEquals(false, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
			ReconOriginalEntryHeader entry2 = ReconDeclaration.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "XJ5978478";
			AssertEquals(true, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
			entry2.US_R_NoLineDetails = true;
			AssertEquals(false, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
			entry2.US_R_NoLineDetails = false;
			AssertEquals(true, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
			entry2.Invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, ReconDeclaration.OriginalEntries.HasImportableEntriesWithoutAnyLinesAttached);
		}

		public void TestHasBeenShortPaid()
		{
			ReconOriginalEntryHeader reconEntry = ReconDeclaration.OriginalEntries.AddNew();
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1m);
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 2m);
			reconEntry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 3m);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 0m);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Avocado, 2m);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 5m);
			AssertEquals("HasBeenShortPaid", true, reconEntry.HasBeenShortPaid);
			reconEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Tobacco, 0m);
			AssertEquals("HasBeenShortPaid", false, reconEntry.HasBeenShortPaid);
		}

		public void TestGetOriginalChargeAmount()
		{
			ReconOriginalEntryHeaderCollection coll = new ReconOriginalEntryHeaderCollection(Declaration, ReconDeclaration);
			ReconOriginalEntryHeader reconEntry = coll.AddNew();
			reconEntry.ReconCharges.AddNew("AAA", 10m);
			reconEntry.ReconCharges.AddNew("BBB", 11m);
			reconEntry.OriginalCharges.AddNew("AAA", 12m);
			ReconOriginalEntryHeader reconEntry2 = coll.AddNew();
			reconEntry2.ReconCharges.AddNew("AAA", 13m);
			reconEntry2.ReconCharges.AddNew("BBB", 14m);
			reconEntry2.OriginalCharges.AddNew("AAA", 15m);
			AssertEquals("OriginalCharge amount for AAA", 27m, coll.GetOriginalChargeAmount("AAA"));
			AssertEquals("OriginalCharge amount for BBB", 0m, coll.GetOriginalChargeAmount("BBB"));
			AssertEquals("ReconCharge amount for AAA", 23m, coll.GetReconChargeAmount("AAA"));
			AssertEquals("ReconCharge amount for BBB", 25m, coll.GetReconChargeAmount("BBB"));
		}

		public void TestResetReconChargesIfNecessary()
		{
			var coll = new ReconOriginalEntryHeaderCollection(Declaration, ReconDeclaration);
			var reconEntry = coll.AddNew();
			reconEntry.Invoice.InvoiceLines.AddNew();
			reconEntry.ReconCharges.AddNew("AAA", 10m);
			reconEntry.ReconCharges.AddNew("BBB", 10m);
			var reconEntry2 = coll.AddNew();
			reconEntry2.US_R_NoLineDetails = true;
			reconEntry2.Invoice.InvoiceLines.AddNew();
			reconEntry2.ReconCharges.AddNew("AAA", 20m);
			reconEntry2.ReconCharges.AddNew("BBB", 20m);
			coll.ResetReconCharges();
			AssertEquals("Two Recon Charges should be cleared, because no Original Charges", 0, reconEntry.ReconCharges.Count);
			AssertEquals("two recon charges remain as there are no invoices attached", 20m, reconEntry2.ReconCharges[0].C1_ChargeAmount);
			AssertEquals("two recon charges remain as there are no invoices attached", 20m, reconEntry2.ReconCharges[1].C1_ChargeAmount);
		}

		public void TestCancelNewDeletesBusinessObject()
		{
			ReconOriginalEntryHeaderCollection coll = new ReconOriginalEntryHeaderCollection(Declaration, ReconDeclaration);
			ReconOriginalEntryHeader element = (ReconOriginalEntryHeader)((IBindingList)coll).AddNew(); //to add an uncommitted row
			CusEntryHeader reconImportEntry = element.GetWrappedEntry();
			((ICancelAddNew)coll).CancelNew(0);
			AssertEquals("new entry is deleted", true, reconImportEntry.IsDeleted);
		}

		public void TestPopulateCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var entry3 = Declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			var coll = new ReconOriginalEntryHeaderCollection(Declaration, ReconDeclaration);
			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(entry));
			AssertEquals(false, coll[0].US_R_NoLineDetails);
			AssertEquals(true, coll.Contains(entry2));
			AssertEquals(false, coll.Contains(entry3));
			var declaration2 = Factory.New<JobDeclaration>();
			var entry_declaration2 = declaration2.CustomsEntryHeaders.AddNew();
			entry_declaration2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var reconDeclaration2 = new ReconDeclaration(declaration2);
			var coll2 = reconDeclaration2.OriginalEntries;
			AssertEquals(1, coll2.Count);
			AssertEquals(true, coll2.Contains(entry_declaration2));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = Declaration.PK;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			return new ReconOriginalEntryHeader(entry, ReconDeclaration);
		}

		protected override ReconOriginalEntryHeaderCollection GetCollectionToTest() => new ReconOriginalEntryHeaderCollection(Declaration, ReconDeclaration);

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					reconDeclaration = new ReconDeclaration(Declaration);
				}

				return reconDeclaration;
			}
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}

				return declaration;
			}
		}
	}
}
