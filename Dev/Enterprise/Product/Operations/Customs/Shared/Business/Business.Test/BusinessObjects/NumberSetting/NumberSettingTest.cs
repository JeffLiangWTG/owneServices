using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class NumberSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateNextNumber()
		{
			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryType = GetNumberType();
			cusEntryNum.CE_EntryNum = GenerateNumberWithCheckDigit(123);
			cusEntryNum.CE_RN_NKCountryCode = GetNumberCountryCode();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			cusEntryNum.CE_ParentID = declaration.PK;
			cusEntryNum.CE_ParentTable = JobDeclarationSchema.Constants.TableName;

			Factory.Save();

			var numberSetting = GetNumberSetting();
			numberSetting.NextNumber = 123;
			AssertHasWarningContaining(numberSetting.NextNumberInfo, "This Next Number has already been used");
		}

		[UseSnapshotProtection]
		public void TestNextNumberDefaultToMinNumber()
		{
			var setting = GetNumberSetting();
			AssertEquals("pre-condition", false, setting.IsInitialized);
			AssertEquals("pre-condition", 0m, setting.MinNumber);
			AssertEquals("pre-condition", 0m, setting.NextNumber);

			setting.MinNumber = 1000000m;
			AssertEquals(1000000m, setting.MinNumber);
			AssertEquals(1000000m, setting.NextNumber);

			Factory.Save();

			AssertEquals("pre-condition", 0m, setting.MinNumber);
			AssertEquals("pre-condition", 0m, setting.NextNumber);

			setting.MinNumber = 2000000m;
			AssertEquals(2000000m, setting.MinNumber);
			AssertEquals(0m, setting.NextNumber);
		}

		[UseSnapshotProtection]
		public void TestValueResetAfterSave()
		{
			var setting = GetNumberSetting();

			setting.MinNumber = 1000000m;
			setting.NextNumber = 2000000m;
			setting.MaxNumber = 3000000m;

			Factory.Save();

			AssertEquals(1000000m, setting.CurrentMinNumber);
			AssertEquals(2000000m, setting.CurrentNextNumber);
			AssertEquals(3000000m, setting.CurrentMaxNumber);

			AssertEquals(0m, setting.MinNumber);
			AssertEquals(0m, setting.NextNumber);
			AssertEquals(0m, setting.MaxNumber);
		}

		[UseSnapshotProtection]
		public void TestMoveRangeToNonAdjacentRange()
		{
			// This is a test for special case that can require two-phase saving.
			// In this example we cannot change the next value to 3500000m before changing min and max values, because it does not fit to the current range (1000000m..1999999m).
			// And we cannot directly change range to (3000000m..3999999m) without changing next value, because the current next value 1500000m does not fit into it.

			var setting = GetNumberSetting();

			setting.MinNumber = 1000000m;
			setting.NextNumber = 1500000m;
			setting.MaxNumber = 1999999m;

			Factory.Save();

			AssertEquals(1000000m, setting.CurrentMinNumber);
			AssertEquals(1500000m, setting.CurrentNextNumber);
			AssertEquals(1999999m, setting.CurrentMaxNumber);

			setting.MinNumber = 3000000m;
			setting.NextNumber = 3500000m;
			setting.MaxNumber = 3999999m;

			Factory.Save();

			AssertEquals(3000000m, setting.CurrentMinNumber);
			AssertEquals(3500000m, setting.CurrentNextNumber);
			AssertEquals(3999999m, setting.CurrentMaxNumber);
		}

		protected abstract ZString GetNumberCountryCode();
		protected abstract ZString GetNumberType();
		protected abstract ZString GenerateNumberWithCheckDigit(ZInt number);
		protected abstract NumberSetting GetNumberSetting();
	}
}
