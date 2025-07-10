using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BorderLineReleaseAllocator))]
	sealed class BorderLineReleaseAllocatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllocate()
		{
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 20001, 20002);
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10001, 10003);
			setting.AddForTesting(GlbBranch.CurrentBranch.PK, 30001, 30010);
			var factory = new BusinessObjectFactory();
			var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var stmNums = factory.Load<CustomsNumberViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, setting.GetFirstAvailableOrLastSequence(GlbBranch.CurrentBranch).SN_NameWithoutSequence)).OrderBy(x => x.SN_Name).ToArray();
			AssertEquals(3, stmNums.Length);
			var stmNum1 = stmNums[0];
			stmNum1.Provider = company.CustomsNumberProvider;
			var stmNum2 = stmNums[1];
			stmNum2.Provider = company.CustomsNumberProvider;
			var stmNum3 = stmNums[2];
			stmNum3.Provider = company.CustomsNumberProvider;
			var wrapper = (USCustomsNumberViewStmNumsWrapper)stmNum2.Wrapper;
			var allocator = new BorderLineReleaseAllocator(wrapper);
			AssertEquals("AppliesTo", "XJ5", allocator.AppliesTo);
			AssertEquals("TotalAvailableNumbers", 15L, allocator.TotalAvailableNumbers);
			AssertEquals("NumberToAllocate", ZLong.Zero, allocator.NumberToAllocate);
			allocator.RunPreSaveValidation();
			AssertHasError(allocator.NumberToAllocateInfo, "Number to allocate must be greater than 0 and less than or equal to 15.");
			AssertEquals(BorderLineReleaseAllocator.NumberToAllocateCannotBeGreaterThanAvailableNumbers(15), allocator.GetReasonForNotAbleToAllocate());
			AssertEquals(0, allocator.AllocateNumbers().Length);
			allocator.NumberToAllocate = 16;
			AssertHasError(allocator.NumberToAllocateInfo, "Number to allocate must be greater than 0 and less than or equal to 15.");
			AssertEquals(BorderLineReleaseAllocator.NumberToAllocateCannotBeGreaterThanAvailableNumbers(15), allocator.GetReasonForNotAbleToAllocate());
			AssertEquals(0, allocator.AllocateNumbers().Length);
			allocator.NumberToAllocate = 10;
			AssertNoErrors(allocator.NumberToAllocateInfo);
			AssertEquals(ZString.Empty, allocator.GetReasonForNotAbleToAllocate());
			AssertArrayEqualsByElements(new string[] { "00200010", "00200028", "00100012", "00100020", "00100038", "00300018", "00300026", "00300034", "00300042", "00300059" }, allocator.AllocateNumbers());
			AssertEquals("stmNum1.Wrapper.SN_ValueForDisplay", -1L, stmNum1.Wrapper.SN_ValueForDisplay);
			AssertEquals("stmNum2.Wrapper.SN_ValueForDisplay", -1L, stmNum2.Wrapper.SN_ValueForDisplay);
			AssertEquals("stmNum3.Wrapper.SN_ValueForDisplay", 30006L, stmNum3.Wrapper.SN_ValueForDisplay);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var setting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			var wrapper = setting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 20000);
			return new BorderLineReleaseAllocator(wrapper);
		}
	}
}
