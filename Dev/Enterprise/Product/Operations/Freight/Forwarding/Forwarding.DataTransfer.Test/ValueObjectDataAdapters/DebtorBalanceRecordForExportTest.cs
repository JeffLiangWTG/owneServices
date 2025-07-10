using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(DebtorBalanceRecordForExport))]
	class DebtorBalanceRecordForExportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCalculateDebtorBalance()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(200904, ZDateTime.Now.AddDays(-15), ZDateTime.Now.AddDays(15));

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_OutstandingAmount = 20;
			header.AH_InvoiceAmount = 25;
			header.AH_GSTAmount = 5;
			header.AH_PostToGL = "Y";
			header.AH_InvoiceDate = Env.Time.CurrentLocalDate;
			header.AH_PostDate = Env.Time.CurrentLocalDate;
			header.AH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			header.AH_TransactionNum = "11";
			header.AH_TransactionType = "INV";
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_OH = debtor.PK;

			MatchLinkCollectionForTest collection = new MatchLinkCollectionForTest(Factory);
			AccTransactionMatchLink matchLink1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			collection.Add(matchLink1);
			matchLink1.AP_AH = header.PK;
			matchLink1.AP_MatchDate = Env.Time.CurrentLocalDate;
			matchLink1.AP_Amount = 10;
			matchLink1.AP_MatchGroupNum = "GROUP1";

			AccTransactionLines line1 = Factory.NewWithValidTestData<AccTransactionLines>();

			line1.AL_LineType = TransactionLineTypes.WIP;
			line1.AL_OH = debtor.PK;
			line1.AL_LineAmount = -222.22;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			DebtorBalanceRecordForExport debtorBalance = new DebtorBalanceRecordForExport(debtor, Factory);
			AssertEquals((ZDecimal)20, debtorBalance.OutstandingBalanceAmount);
			AssertEquals((ZDecimal)222.22, debtorBalance.WIPsAmount);
		}

		public void TestProperties()
		{
			AssertNotNull("Record 's Debtor", DebtorBalance.Debtor);
			AssertEquals("Debtor's PK", Debtor.PK, DebtorBalance.Debtor.PK);
			AssertEquals("Outstanding Balance Amount", 100m, DebtorBalance.OutstandingBalanceAmount);
			AssertEquals("WIPAmount", 99.85m, DebtorBalance.WIPsAmount);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestThrowExceptionForNullDebtor()
		{
			DebtorBalanceRecordForExport record = new DebtorBalanceRecordForExport(null, Factory);
		}

		DebtorBalanceRecordForExport DebtorBalance
		{
			get
			{
				if (debtorBalance == null)
				{
					debtorBalance = new DebtorBalanceRecordForExport(Debtor, Factory);
					debtorBalance.OutstandingBalanceAmount = 100m;
					debtorBalance.WIPsAmount = 99.85m;
				}

				return debtorBalance;
			}
		}
		DebtorBalanceRecordForExport debtorBalance;

		OrgHeader Debtor;

		protected override void SetUp()
		{
			base.SetUp();
			Debtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DebtorBalanceRecordForExport(Debtor, Factory);
		}

		class MatchLinkCollectionForTest : BusinessObjectCollection<AccTransactionMatchLink>, ISupportCriticalValidation, IFactoryProvider
		{
			public MatchLinkCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			ICriticalValidation ISupportCriticalValidation.CriticalValidation
			{
				get { return new CriticalValidationForTest(this); }
			}

			void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		class CriticalValidationForTest : CriticalValidation<MatchLinkCollectionForTest>
		{
			public CriticalValidationForTest(MatchLinkCollectionForTest parent)
				: base(parent)
			{
			}
		}
	}
}
