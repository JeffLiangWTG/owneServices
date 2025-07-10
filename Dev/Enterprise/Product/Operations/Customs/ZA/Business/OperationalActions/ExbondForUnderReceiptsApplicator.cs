using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business
{
	public class ExbondForUnderReceiptsApplicator : BaseExportApplicator
	{
		public ExbondForUnderReceiptsApplicator(BusinessObjectFactory factory) : base("Generate Ex-bond for Under Receipts", factory, "%")
		{
			RequiredTransactionType = WarehouseOperatorTransactionTypeList.Codes.ADJ;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new ExbondForUnderReceiptsOperationalActionRunner(log, Factory);
			Lock();
			if (HasLock)
			{
				runner.Run(Records.Where(record => record.Select));
				Unlock();
			}
		}
	}
}
