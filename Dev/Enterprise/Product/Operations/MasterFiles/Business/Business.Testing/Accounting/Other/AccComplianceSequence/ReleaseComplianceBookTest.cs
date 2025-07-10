using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReleaseComplianceBook))]
	class ReleaseComplianceBookTest : NonPersistentBusinessObjectTestCase
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
			existedSequence.XD_LockBy = GlbStaff.CurrentUser.PK;
			TestFactory.Save();
		}
		public void TestIsLock()
		{
			var book = (ReleaseComplianceBook)GetNewBusinessObject();
			AssertEquals(false, book.IsLock);
		}

		public void TestReleaseEvent()
		{
			var book = (ReleaseComplianceBookForTest)GetNewBusinessObject();
			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(false, book.ComplianceSequenceBoForTest.Logs.HasLogWith(x => x.SL_Reference == "Compliance book released" && x.SL_SE_NKEvent == Events.EditedARecord.Code));

			Factory.Save();
			AssertEquals(true, book.ComplianceSequenceBoForTest.Logs.HasLogWith(x => x.SL_Reference == "Compliance book released" && x.SL_SE_NKEvent == Events.EditedARecord.Code));
		}

		public void TestAfterSaveIsNotLocked()
		{
			var book = (ReleaseComplianceBookForTest)GetNewBusinessObject();
			book.XD_Calc_PK = existedSequence.PK;
			AssertEquals(GlbStaff.CurrentUser.PK, book.ComplianceSequenceBoForTest.XD_LockBy);

			Factory.Save();
			AssertEquals(ZGuid.Empty, book.ComplianceSequenceBoForTest.XD_LockBy);
		}

		public void TestSequenceQuery()
		{
			var book = (ReleaseComplianceBookForTest)GetNewBusinessObject();
			var expected = @"XD_GC_Company = '" + GlbCompany.CurrentCompany.PK + @"'
";
			AssertEquals(expected, book.ComplianceSequenceQueryForTest.LiteralTextSqlFormatted);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReleaseComplianceBookForTest(Factory);
		}

		class ReleaseComplianceBookForTest : ReleaseComplianceBook
		{
			public ReleaseComplianceBookForTest(BusinessObjectFactory factory) : base(factory)
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
