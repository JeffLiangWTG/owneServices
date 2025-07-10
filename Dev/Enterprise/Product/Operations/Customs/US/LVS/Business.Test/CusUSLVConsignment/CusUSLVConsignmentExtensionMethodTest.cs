using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVConsignmentExtensionMethodTest : TestCaseWithFactory
	{
		public void TestAllocateEntryNumbersForConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignmentOne = clearance.CusUSLVConsignments.AddNew();
			var consignmentTwo = clearance.CusUSLVConsignments.AddNew();
			var consignmentsList = new List<CusUSLVConsignmentForMessaging>();
			var reasonForUnableToAllocate = consignmentsList.AllocateEntryNumbersForConsignments(clearance);
			AssertEquals(ZString.Empty, reasonForUnableToAllocate);
			AssertEquals(ZString.Empty, consignmentOne.CE_EntryNum);
			AssertEquals(ZString.Empty, consignmentTwo.CE_EntryNum);

			consignmentsList.Add(new CusUSLVConsignmentForMessaging(consignmentOne));
			consignmentsList.Add(new CusUSLVConsignmentForMessaging(consignmentTwo));
			reasonForUnableToAllocate = consignmentsList.AllocateEntryNumbersForConsignments(clearance);
			AssertEquals(ACEEntryStmNumsSetting.EntryFilerCodeIsRequiredForEntryNumberAllocation, reasonForUnableToAllocate);
			AssertEquals(ZString.Empty, consignmentOne.CE_EntryNum);
			AssertEquals(ZString.Empty, consignmentTwo.CE_EntryNum);

			clearance.ULH_EntryFilerCode = "XJ5";
			reasonForUnableToAllocate = consignmentsList.AllocateEntryNumbersForConsignments(clearance);
			AssertEquals(string.Format(ACEEntryStmNumsSetting.EntryNumberRangeNotSetup, GlbBranch.CurrentBranch.GB_Code, "XJ5", GlbCompany.CurrentCompany.GC_Code), reasonForUnableToAllocate);
			AssertEquals(ZString.Empty, consignmentOne.CE_EntryNum);
			AssertEquals(ZString.Empty, consignmentTwo.CE_EntryNum);

			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10000);
			reasonForUnableToAllocate = consignmentsList.AllocateEntryNumbersForConsignments(clearance);
			AssertEquals(string.Format(CusUSLVConsignmentExtensionMethod.NotEnoughAvailableEntryNumbersForBranch, GlbBranch.CurrentBranch.GB_Code, "XJ5"), reasonForUnableToAllocate);
			AssertEquals(ZString.Empty, consignmentOne.CE_EntryNum);
			AssertEquals(ZString.Empty, consignmentTwo.CE_EntryNum);

			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 20000, 20001);
			reasonForUnableToAllocate = consignmentsList.AllocateEntryNumbersForConsignments(clearance);
			AssertEquals(ZString.Empty, reasonForUnableToAllocate);
			AssertEquals("00100004", consignmentOne.CE_EntryNum);
			AssertEquals("00200002", consignmentTwo.CE_EntryNum);
		}
	}
}
