using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class PeriodValidationProviderTest : TestCaseWithFactory
	{
		public void TestDateFallsWithinValidPeriod()
		{
			Header.AH_PostDate = PeriodHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(-30);
			Assert("Should be error on post date", Header.AH_PostDateInfo.HasErrors());
			AssertEquals("Error Expected", ValidationProvider.InvalidDateError, Header.AH_PostDateInfo.GetErrors().GetFirstMessage());
		}

		public void TestDateFallsWithinValidRange()
		{
			Header.AH_PostDate = ZDateTime.MaxSmallDateTime.AddDays(1);
			Assert("Should be error on post date", Header.AH_PostDateInfo.HasErrors());
			Assert(Header.AH_PostDateInfo.HasError(ValidationProvider.OutOfDateRangeError));

			Header.AH_PostDate = PeriodHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			Assert("Should be no error on Post Date", !Header.AH_PostDateInfo.HasErrors());

			Header.AH_PostDate = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
			Assert("Should be error on post date", Header.AH_PostDateInfo.HasErrors());
			Assert(Header.AH_PostDateInfo.HasError(ValidationProvider.OutOfDateRangeError));
		}

		public virtual void TestLedgerPeriodValidation()
		{
			Header.AH_PostDate = PeriodHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			Assert("Should be no error on Post Date", !Header.AH_PostDateInfo.HasErrors());

			Header.AH_PostDate = PeriodHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
			Assert("Should be error on Post Date", Header.AH_PostDateInfo.HasErrors());
			AssertEquals("Sub ledger period closed error", ValidationProvider.SubLedgerPeriodClosedError, Header.AH_PostDateInfo.GetErrors().GetFirstMessage());
		}

		#region Implementation

		public class DummyHeader : AccTransactionHeader
		{
			public DummyHeader(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override AccTransactionHeaderValidation GetNewValidation()
			{
				return new DummyHeaderValidation(this);
			}
		}

		public class DummyHeaderValidation : AccTransactionHeaderValidation
		{
			public DummyHeaderValidation(AccTransactionHeader parent)
				: base(parent)
			{
			}

			protected override void CheckAH_PostDate()
			{
				base.CheckAH_PostDate();
				new PeriodValidationProvider(Parent.Factory).CheckDateFallsIntoValidPeriod(Parent.AH_PostDateInfo);
			}
		}

		protected PeriodValidationProvider ValidationProvider;
		protected DummyHeader Header;
		protected AccountingPeriodTestHelper PeriodHelper;

		protected ZString GeneralLedgerPeriodClosedError
		{
			get
			{
				return ValidationProvider.GeneralLedgerPeriodClosedError;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			int yearToSetup = ZDateTime.Now.Month > 6 ? ZDateTime.Now.Year + 1 : ZDateTime.Now.Year;
			PeriodHelper = new AccountingPeriodTestHelper();
			PeriodHelper.SetupPeriods();
			ValidationProvider = GetValidationProvider();
			Header = Factory.New<DummyHeader>();
		}

		protected virtual PeriodValidationProvider GetValidationProvider()
		{
			return new PeriodValidationProvider(Factory);
		}

		#endregion
	}
}
