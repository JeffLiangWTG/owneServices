using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LockComplianceBook))]
	class LockComplianceBookTest : NonPersistentBusinessObjectTestCase
	{
		protected BusinessObjectFactory TestFactory;
		protected AccComplianceSequence existedSequence;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			existedSequence = TestFactory.NewWithValidTestData<AccComplianceSequence>();
			existedSequence.XD_SequenceClass = "TXI";
			existedSequence.XD_Code = "LM5";
			existedSequence.XD_StartNumber = 200;
			existedSequence.XD_EndNumber = 300;
			existedSequence.XD_MaximumNumberDigits = 6;
			existedSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			existedSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			TestFactory.Save();
		}

		public void TestIsLock()
		{
			var book = (LockComplianceBook)GetNewBusinessObject();
			AssertEquals(true, book.IsLock);
		}

		public void TestLockEvent()
		{
			var book = (LockComplianceBookForTest)GetNewBusinessObject();
			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(false, book.ComplianceSequenceBoForTest.Logs.HasLogWith(x => x.SL_Reference == "Compliance book locked" && x.SL_SE_NKEvent == Events.EditedARecord.Code));

			Factory.Save();
			AssertEquals(true, book.ComplianceSequenceBoForTest.Logs.HasLogWith(x => x.SL_Reference == "Compliance book locked" && x.SL_SE_NKEvent == Events.EditedARecord.Code));
		}

		public void TestAfterSaveIsLocked()
		{
			var book = (LockComplianceBookForTest)GetNewBusinessObject();
			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(ZGuid.Empty, book.ComplianceSequenceBoForTest.XD_LockBy);

			Factory.Save();
			AssertEquals(GlbStaff.CurrentUser.PK, book.ComplianceSequenceBoForTest.XD_LockBy);
		}

		public void TestSequenceQuery()
		{
			var book = (LockComplianceBookForTest)GetNewBusinessObject();

			var expected = @"XD_GC_Company = '" + GlbCompany.CurrentCompany.PK.ToString() + @"'
";

			AssertEquals(expected, book.ComplianceSequenceQueryForTest.LiteralTextSqlFormatted);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LockComplianceBookForTest(Factory);
		}

		class LockComplianceBookForTest : LockComplianceBook
		{
			public LockComplianceBookForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public AccComplianceSequence ComplianceSequenceBoForTest
			{
				get
				{
					return ComplianceSequenceBo;
				}
			}

			public ZQuery ComplianceSequenceQueryForTest
			{
				get
				{
					return GetComplianceSequenceQuery();
				}
			}
		}

		#endregion
	}
}
