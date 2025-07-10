using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business
{
	public class ExportBelnApplicator : BaseExportApplicator
	{
		public ExportBelnApplicator(BusinessObjectFactory factory, Func<bool> askForInsufficientStockConfirmationFunc) : base("Export BELN", factory, WarehouseOperatorTransactionExportTypeList.Codes.BLN)
		{
			this.askForInsufficientStockConfirmationFunc = askForInsufficientStockConfirmationFunc;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new ExportBelnOperationalActionRunner(log, Factory, askForInsufficientStockConfirmationFunc);
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
