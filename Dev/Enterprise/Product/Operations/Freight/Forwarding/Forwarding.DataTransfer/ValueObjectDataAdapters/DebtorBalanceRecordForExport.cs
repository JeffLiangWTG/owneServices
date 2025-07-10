using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class DebtorBalanceRecordForExport : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DebtorBalanceRecordForExport(OrgHeader debtor, BusinessObjectFactory factory)
			: base(factory)
		{
			Debtor = Argument.NotNull(debtor, "Debtor");
			CalculateDebtorBalance();
		}

		void CalculateDebtorBalance()
		{
			GetOutstandingBalancesRecords();
			GetWIPRecords();
		}

		void GetOutstandingBalancesRecords()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(Factory);

			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
			parameters.Add("@Ledger", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@Period", calculator.GetPeriodFromDate(ZDateTime.Now, GlbCompany.CurrentCompany.PK), AccPeriodManagementSchema.AM_Period);
			parameters.Add("@Debtor", Debtor.PK, AccTransactionHeaderSchema.AH_OH);
			collection.Load(BalanceSQLScript, parameters);

			if (collection.Count > 0)
			{
				OutstandingBalanceAmount = Utilities.Round((ZDecimal)collection[0]["BalanceInLocal"], GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
		}

		void GetWIPRecords()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();

			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
			parameters.Add("@LineType", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@Debtor", Debtor.PK, AccTransactionLinesSchema.AL_OH);

			collection.Load(WIPSSQLScript, parameters);

			if (collection.Count > 0)
			{
				WIPsAmount = Utilities.Round((ZDecimal)collection[0]["WIPAmount"], GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
		}

		public void ResetBalance()
		{
			OutstandingBalanceAmount = ZDecimal.Zero;
			WIPsAmount = ZDecimal.Zero;
		}

		const string BalanceSQLScript = @"SELECT
										SUM(AH_BalanceInLocal) AS BalanceInLocal
										FROM
										csfn_TransactionsBalances(@Period, @Company, @Ledger)
										Where AH_OH = @Debtor
										Group by AH_OH
										Having SUM(AH_BalanceInLocal) != 0";

		const string WIPSSQLScript = @"SELECT 
										SUM(CASE WHEN AL_LineType = @LineType THEN -AL_LineAmount ELSE 0 END) as WIPAmount
										FROM dbo.AccTransactionLines   
										INNER JOIN dbo.GlbBranch  ON AL_GB = GB_PK
										WHERE AL_ReverseDate IS NULL
										AND AL_OH = @Debtor
										AND GB_GC = @Company
										Group by AL_OH 
										HAVING SUM(CASE WHEN AL_LineType = @LineType THEN -AL_LineAmount ELSE 0 END) != 0";

		public readonly OrgHeader Debtor;

		public ZDecimal OutstandingBalanceAmount { get; set; }
		public ZDecimal WIPsAmount { get; set; }
	}
}
