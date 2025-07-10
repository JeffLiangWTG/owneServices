using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business
{
	public class ExportNonBelnApplicator : BaseExportApplicator
	{
		public ExportNonBelnApplicator(BusinessObjectFactory factory, Func<bool> askForInsufficientStockConfirmationFunc) : base("Export Non-BELN", factory, WarehouseOperatorTransactionExportTypeList.Codes.EXP)
		{
			this.askForInsufficientStockConfirmationFunc = askForInsufficientStockConfirmationFunc;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new ExportNonBelnOperationalActionRunner(log, Factory, askForInsufficientStockConfirmationFunc);
			Lock();
			if (HasLock)
			{
				runner.Run(Records.Where(record => record.Select));
				Unlock();
			}
		}

		readonly Func<bool> askForInsufficientStockConfirmationFunc;
	}
}
