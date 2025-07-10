using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEEntryStmNumsSettingTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert an Empty or Invalid ZGuid to a Guid!", () => new ACEEntryStmNumsSetting(ZGuid.Invalid));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot convert an Empty or Invalid ZGuid to a Guid!", () => new ACEEntryStmNumsSetting(ZGuid.Empty));
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			AssertEquals(GlbCompany.CurrentCompany.PK, setting.Company.PK);
			AssertEquals("XJ5", setting.EntryFilerCode);
			var provider = setting.Provider;
			AssertEquals(0, provider.CustomsNumbers.Count);
			var wrapper = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 2000, 3000);
			var stmNum = wrapper.StmNums;
			AssertEquals(GlbBranch.CurrentBranch.PK, stmNum.SN_Owner);
			AssertEquals(2000L, stmNum.SN_MinimumValue);
			AssertEquals(3000L, stmNum.SN_MaximumValue);
			setting = new ACEEntryStmNumsSetting(GlbCompany.CurrentCompany.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, setting.Company.PK);
			AssertEquals(ZString.Empty, setting.EntryFilerCode);
			AssertNull(setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch));
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			setting = new ACEEntryStmNumsSetting(GlbCompany.CurrentCompany.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, setting.Company.PK);
			AssertEquals("XJ5", setting.EntryFilerCode);
			var stmNum2 = setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch);
			AssertEquals(stmNum.PK, stmNum2.PK);
			AssertEquals(GlbBranch.CurrentBranch.PK, stmNum2.SN_Owner);
			stmNum.Delete();
			stmNum.Factory.Save();
			setting = new ACEEntryStmNumsSetting(GlbCompany.CurrentCompany.PK);
			AssertEquals(GlbCompany.CurrentCompany.PK, setting.Company.PK);
			AssertEquals("XJ5", setting.EntryFilerCode);
			AssertNull(setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch));
			setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5", true);
			AssertEquals(GlbCompany.CurrentCompany.PK, setting.Company.PK);
			AssertEquals("XJ5", setting.EntryFilerCode);
			stmNum = setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch);
			AssertEquals(GlbCompany.CurrentCompany.PK, stmNum.SN_Owner);
			AssertEquals(1L, stmNum.SN_MinimumValue);
			AssertEquals(USCustomsNumberViewStmNumsSetting.USMaximumFormalEntryNumber, (long)stmNum.SN_MaximumValue);
		}

		public void TestGetAnyReasonForNotAbleToAllocateNumber()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum.SetNextNumber(20000);
			stmNum.GenerateNextCustomsNumber(Factory);
			AssertEquals(ACEEntryStmNumsSetting.EntryFilerCodeIsRequiredForEntryNumberAllocation, ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(otherBranch, ZString.Empty));
			AssertEquals(string.Format(ACEEntryStmNumsSetting.EntryNumberRangeNotSetup, "$#@", "XJX", GlbCompany.CurrentCompany.GC_Code), ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(otherBranch, "XJX"));
			stmNum = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 30000, 40000).StmNums;
			stmNum.SetNextNumber(40000);
			AssertEquals("", ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(otherBranch, "XJX"));
			stmNum.GenerateNextCustomsNumber(Factory);
			AssertEquals(string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForCompany, GlbCompany.CurrentCompany.GC_Code, "XJX"), ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(otherBranch, "XJX"));
			AssertEquals(string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForBranch, GlbBranch.CurrentBranch.GB_Code, "XJX"), ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(GlbBranch.CurrentBranch, "XJX"));
		}

		public void TestGetFirstAvailableOrLastSequenceStmNums()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum1.SetNextNumber(20000);
			stmNum1.GenerateNextCustomsNumber(Factory);
			var stmNum2 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 30000).StmNums;
			var stmNum3 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 40000).StmNums;
			AssertEquals(stmNum2.PK, ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(GlbBranch.CurrentBranch, "XJX").PK);
			AssertNull(ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(otherBranch, "XJX"));
			AssertNotNull(ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(otherBranch, "XJZ"));
			stmNum2.SetNextNumber(stmNum2.SN_MaximumValue);
			stmNum2.GenerateNextCustomsNumber(Factory);
			setting.Company.Factory.InvalidateCachedProperties();
			AssertEquals(stmNum3.PK, ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(GlbBranch.CurrentBranch, "XJX").PK);
			var stmNum4 = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 40001, 50000).StmNums;
			AssertEquals(stmNum4.PK, ACEEntryStmNumsSetting.GetFirstAvailableOrLastSequenceStmNums(otherBranch, "XJX").PK);
		}

		public void TestTryGetNextCustomsNumber()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum1.SetNextNumber(20000);
			ZString entryNumber;
			AssertEquals(true, setting.TryGetNextCustomsNumber(Factory, GlbBranch.CurrentBranch, out entryNumber));
			AssertEquals(stmNum1.GenerateCustomsNumber(20000), entryNumber);
			AssertEquals(false, setting.TryGetNextCustomsNumber(Factory, GlbBranch.CurrentBranch, out entryNumber));
			AssertEquals(ZString.Empty, entryNumber);
			AssertEquals(false, setting.TryGetNextCustomsNumber(Factory, otherBranch, out entryNumber));
			AssertEquals(ZString.Empty, entryNumber);
			var stmNum2 = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 30000, 40000).StmNums;
			stmNum2.SetNextNumber(35000);
			AssertEquals(true, setting.TryGetNextCustomsNumber(Factory, otherBranch, out entryNumber));
			AssertEquals(stmNum2.GenerateCustomsNumber(35000), entryNumber);
		}

		public void TestGetFirstAvailableOrLastSequence()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum1.SetNextNumber(20000);
			stmNum1.GenerateNextCustomsNumber(Factory);
			var stmNum2 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 30000).StmNums;
			var stmNum3 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 40000).StmNums;
			AssertEquals(stmNum2.PK, setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch).PK);
			AssertNull(setting.GetFirstAvailableOrLastSequence(otherBranch));
			stmNum2.SetNextNumber(stmNum2.SN_MaximumValue);
			stmNum2.GenerateNextCustomsNumber(Factory);
			setting.Company.Factory.InvalidateCachedProperties();
			AssertEquals(stmNum3.PK, setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch).PK);
			var stmNum4 = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 40001, 50000).StmNums;
			AssertEquals(stmNum4.PK, setting.GetFirstAvailableOrLastSequence(otherBranch).PK);
		}

		public void TestGetFirstAvailableOrLastSequenceWrapper()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum1.SetNextNumber(20000);
			stmNum1.GenerateNextCustomsNumber(Factory);
			var stmNum2 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 30000).StmNums;
			var stmNum3 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 40000).StmNums;
			AssertEquals(stmNum2.Wrapper, setting.GetFirstAvailableOrLastSequenceWrapper(GlbBranch.CurrentBranch));
			AssertNull(setting.GetFirstAvailableOrLastSequenceWrapper(otherBranch));
			stmNum2.SetNextNumber(stmNum2.SN_MaximumValue);
			stmNum2.GenerateNextCustomsNumber(Factory);
			setting.Company.Factory.InvalidateCachedProperties();
			AssertEquals(stmNum3.Wrapper, setting.GetFirstAvailableOrLastSequenceWrapper(GlbBranch.CurrentBranch));
			var stmNum4 = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 40001, 50000).StmNums;
			AssertEquals(stmNum4.Wrapper, setting.GetFirstAvailableOrLastSequenceWrapper(otherBranch));
		}

		public void TestGetMatchingSequenceWrapper()
		{
			var otherBranch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			otherBranch.GB_Code = "$#@";
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJX");
			var stmNum1 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000).StmNums;
			stmNum1.SetNextNumber(20000);
			stmNum1.GenerateNextCustomsNumber(Factory);
			var stmNum2 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 30000).StmNums;
			var stmNum3 = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 40000).StmNums;
			AssertEquals(stmNum2.Wrapper, setting.GetMatchingSequenceWrapper(GlbBranch.CurrentBranch, 20001));
			AssertNull(setting.GetMatchingSequenceWrapper(otherBranch, 20001));
			AssertEquals(stmNum3.Wrapper, setting.GetMatchingSequenceWrapper(GlbBranch.CurrentBranch, 30001));
			AssertNull(setting.GetMatchingSequenceWrapper(otherBranch, 30001));
			stmNum2.SetNextNumber(stmNum2.SN_MaximumValue);
			stmNum2.GenerateNextCustomsNumber(Factory);
			setting.Company.Factory.InvalidateCachedProperties();
			AssertEquals(stmNum3.Wrapper, setting.GetMatchingSequenceWrapper(GlbBranch.CurrentBranch, 30001));
			var stmNum4 = setting.AddForTesting(GlbCompany.CurrentCompany.PK, 40001, 50000).StmNums;
			AssertEquals(stmNum4.Wrapper, setting.GetMatchingSequenceWrapper(otherBranch, 40001));
			AssertEquals(stmNum4.Wrapper, setting.GetMatchingSequenceWrapper(otherBranch, 90000));
		}
	}
}
