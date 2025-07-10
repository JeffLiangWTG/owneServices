using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AllocateNumber))]
	public abstract class AllocateNumberTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2020, 06, 22)]
		public void TestData()
		{
			var allocateNumber = CreateAllocateNumber();
			var entryNumberGenerator = allocateNumber.EntryNumberGenerator;

			CombineAssertions(() =>
			{
				AssertEquals("Part1Number", entryNumberGenerator.Part1, allocateNumber.Part1Number);
				AssertEquals("Part3Number", entryNumberGenerator.Part3, allocateNumber.Part3Number);
				AssertEquals("Part5Number", ExpectPart5Number, allocateNumber.Part5Number);
				AssertEquals("Part1NumberInfo.ReadOnly", PartNumberReadOnlyArray[0], allocateNumber.Part1NumberInfo.ReadOnly);
				AssertEquals("Part2NumberInfo.ReadOnly", PartNumberReadOnlyArray[1], allocateNumber.Part2NumberInfo.ReadOnly);
				AssertEquals("Part3NumberInfo.ReadOnly", PartNumberReadOnlyArray[2], allocateNumber.Part3NumberInfo.ReadOnly);
				AssertEquals("Part4NumberInfo.ReadOnly", PartNumberReadOnlyArray[3], allocateNumber.Part4NumberInfo.ReadOnly);
				AssertEquals("Part5NumberInfo.ReadOnly", PartNumberReadOnlyArray[4], allocateNumber.Part5NumberInfo.ReadOnly);
			});
		}

		protected virtual ZString ExpectPart5Number => ZString.Empty;

		public abstract void TestNumber();

		protected virtual ImmutableArray<bool> PartNumberReadOnlyArray => ImmutableArray.Create(true, false, true, false, false);

		public void TestAllowedGeneratorDescriptionWhenAutoGenerate()
		{
			InitRangNumber();
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("All sequential numbers for the current entry number range have been used up. Please visit Maintain > User Admin > Companies > EDI - Eagle Datamation International > Number Ranges to maintain the number ranges.", allocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate);
			CustomsNumberViewStmNumsWrapper.CurrentValue = "E0001";
			AssertNullOrEmpty("Should be empty", allocateNumber.AllowedGeneratorDescriptionWhenAutoGenerate);
		}

		public virtual void TestCheckBeforeAutoRegenerateEntryNumber()
		{
			InitRangNumber();
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("All sequential numbers for the current entry number range have been used up. Please visit Maintain > User Admin > Companies > EDI - Eagle Datamation International > Number Ranges to maintain the number ranges.", allocateNumber.CheckBeforeAutoRegenerateEntryNumber());
			CustomsNumberViewStmNumsWrapper.CurrentValue = "E0001";
			AssertNullOrEmpty("Should be empty", allocateNumber.CheckBeforeAutoRegenerateEntryNumber());
		}

		public void TestIsPart5NumberOutrangeWhenAllowOutrange()
		{
			InitRangNumber();
			var allocateNumber = CreateAllocateNumber();
			AssertEquals(ZBool.False, allocateNumber.IsPart5NumberOutrangeWhenAllowOutrange);
			allocateNumber.Part5Number = "E0006";
			AssertEquals(ZBool.False, allocateNumber.IsPart5NumberOutrangeWhenAllowOutrange);

			allocateNumber = CreateAllocateNumber(true);
			allocateNumber.Part5Number = "E0006";
			AssertEquals(ZBool.True, allocateNumber.IsPart5NumberOutrangeWhenAllowOutrange);
			allocateNumber.Part5Number = "E0005";
			AssertEquals(ZBool.False, allocateNumber.IsPart5NumberOutrangeWhenAllowOutrange);
			allocateNumber.Part5Number = ZString.Empty;
			AssertEquals(ZBool.False, allocateNumber.IsPart5NumberOutrangeWhenAllowOutrange);
		}

		public void TestPart4Caption()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Part4Caption", "Entry number cannot be generated because 'Box Number' is missing.", allocateNumber.Part4Caption);
		}

		public void TestIsPart4NumberEmpty()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals(allocateNumber.Part4Number.IsEmpty, allocateNumber.IsPart4NumberEmpty);
		}

		void InitRangNumber()
		{
			GetTWCustomsNumberViewStmNumsWrapperCore();
		}

		TWCustomsNumberViewStmNumsWrapper CustomsNumberViewStmNumsWrapper => GetTWCustomsNumberViewStmNumsWrapperCore();

		protected abstract TWCustomsNumberViewStmNumsWrapper GetTWCustomsNumberViewStmNumsWrapperCore();

		protected abstract AllocateNumber CreateAllocateNumber(bool allowPart5NumberOutrangeWhenEntered = false);

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateAllocateNumber();
		}
	}
}
