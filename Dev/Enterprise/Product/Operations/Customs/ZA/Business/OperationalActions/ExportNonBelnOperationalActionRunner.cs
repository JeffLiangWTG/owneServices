using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	class ExportNonBelnOperationalActionRunner : ExportOperationalActionRunner
	{
		public ExportNonBelnOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory, Func<bool> askForInsufficientStockConfirmationFunc) : base(log, factory)
		{
			this.askForInsufficientStockConfirmationFunc = askForInsufficientStockConfirmationFunc;
		}

		public override void Run(IEnumerable<OperatorTransactionSelection> selections)
		{
			var entryLines = ProcessAndGetEntryLines(selections);
			if (entryLines != null)
			{
				foreach (var lines in entryLines)
				{
					var messageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.NonBeln);
					LogResultForSingleMessage(messageNumber);
				}
			}
		}

		protected override string ExportType => WarehouseOperatorTransactionExportTypeList.Codes.EXP;
		protected override bool SortAscending => false;
		protected override bool CheckSufficientQuantitiesForAllocation => true;

		protected override bool AskForConfirmationToContinueWithInsufficientStock() => askForInsufficientStockConfirmationFunc();
		readonly Func<bool> askForInsufficientStockConfirmationFunc;
	}
}
