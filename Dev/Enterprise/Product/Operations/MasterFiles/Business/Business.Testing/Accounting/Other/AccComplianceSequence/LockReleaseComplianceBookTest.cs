using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LockReleaseComplianceBook))]
	class LockReleaseComplianceBookTest : NonPersistentBusinessObjectTestCase
	{
		protected AccComplianceSequence existedSequence;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			existedSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			existedSequence.XD_SequenceClass = "TXI";
			existedSequence.XD_Code = "LM5";
			existedSequence.XD_StartNumber = 200;
			existedSequence.XD_EndNumber = 300;
			existedSequence.XD_MaximumNumberDigits = 6;
			existedSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;

			Factory.Save();
		}

		public void TestSetXD_Calc_PK()
		{
			var book = (LockReleaseComplianceBookForTest)GetNewBusinessObject();
			AssertEquals(book.XD_Calc_PK, ZGuid.Empty);
			AssertEquals(book.ComplianceSequenceBoForTest, null);

			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(book.XD_Calc_PK, existedSequence.PK);
			AssertEquals(book.ComplianceSequenceBoForTest.PK, existedSequence.PK);
		}

		public void TestSetComplianceSequenceBo()
		{
			var book = (LockReleaseComplianceBookForTest)GetNewBusinessObject();
			AssertEquals(book.ComplianceSequenceBoForTest, null);

			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(book.ComplianceSequenceBoForTest, existedSequence);
		}

		public void TestIsExistsCheckLockBy()
		{
			var checkLockByMethodInfo = typeof(AccComplianceSequenceValidation).GetMethod("CheckXD_LockBy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var validateLockByMethodInfo = typeof(AutoAccComplianceSequenceValidation).GetMethod("ValidateXD_LockBy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			AssertNull("Please refactor LockComplianceBook OnFactorySaving, and refactor ReleaseComplianceBook OnFactorySaving.", checkLockByMethodInfo);
			AssertNull("Please refactor LockComplianceBook OnFactorySaving, and refactor ReleaseComplianceBook OnFactorySaving.", validateLockByMethodInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LockReleaseComplianceBookForTest(Factory);
		}

		class LockReleaseComplianceBookForTest : LockReleaseComplianceBook
		{
			public LockReleaseComplianceBookForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public AccComplianceSequence ComplianceSequenceBoForTest
			{
				get
				{
					return ComplianceSequenceBo;
				}
				set
				{
					ComplianceSequenceBo = value;
				}
			}
		}

		#endregion
	}
}
