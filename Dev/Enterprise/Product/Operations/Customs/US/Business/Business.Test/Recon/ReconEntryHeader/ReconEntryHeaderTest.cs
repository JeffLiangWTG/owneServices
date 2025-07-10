using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconEntryHeader))]
	sealed class ReconEntryHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			ReconEntryHeader reconEntry = reconDec.ReconEntry;
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("GetEntry", entry, reconEntry.GetEntry());
		}

		public void TestReconEntryHeaderHasSamePKAsCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconEntry = new ReconEntryHeader(entry);
			AssertEquals("ReconEntryHeader should have the same PK as the wrapped CusEntryHeader as required by ModuleResultsPKCollection", entry.PK, reconEntry.PK);
		}

		public void TestAmounts()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			ReconEntryHeader reconEntry = reconDec.ReconEntry;
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("PreCondition:ReconEntry", CusEntryHeaderMessageTypeList.Codes.ReconEntry, entry.CH_MessageType);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 1m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 2m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 4m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Avocado, 8m);
			AssertEquals("DutyAmount", 1m, reconEntry.DutyAmount);
			AssertEquals("TaxAmount", 2m, reconEntry.TaxPaymentAmount);
			AssertEquals("Interest", 4m, reconEntry.InterestPaymentAmount);
			AssertEquals("Fee", 8m, reconEntry.FeePaymentAmount);
		}

		public void TestCH_TotalPaid()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			ReconEntryHeader reconEntry = reconDec.ReconEntry;
			AssertEquals("CH_TotalPaid", 0m, reconEntry.CH_TotalPaid);
			reconEntry.CH_TotalPaid = 100m;
			AssertEquals("CH_TotalPaid", 100m, reconEntry.CH_TotalPaid);
		}

		public void TestBusinessObjectOverrides()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDec = new ReconDeclaration(declaration);
			AssertNotNull(reconDec.ReconEntry);
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Logs", entry.Logs, reconDec.ReconEntry.GetLogs());
			AssertEquals("Notes", entry.Notes, reconDec.ReconEntry.GetNotes());
			AssertEquals("Messages", entry.Messages, reconDec.ReconEntry.Messages);
			AssertEquals("IsInDatabase", entry.IsInDatabase, reconDec.ReconEntry.IsInDatabase);
			AssertEquals("isDelted", entry.IsDeleted, reconDec.ReconEntry.IsDeleted);
			AssertEquals("IsSavedByFactory", true, reconDec.ReconEntry.IsSavedByFactory);
			Factory.Save();
			AssertEquals("IsInDatabase", entry.IsInDatabase, reconDec.ReconEntry.IsInDatabase);
			AssertEquals("isDelted", entry.IsDeleted, reconDec.ReconEntry.IsDeleted);
			reconDec.ReconEntry.Delete();
			AssertEquals("isDelted", entry.IsDeleted, reconDec.ReconEntry.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new ReconEntryHeader(entry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
