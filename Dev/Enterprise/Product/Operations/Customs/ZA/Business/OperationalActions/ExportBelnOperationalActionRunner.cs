using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	class ExportBelnOperationalActionRunner : ExportOperationalActionRunner
	{
		public ExportBelnOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory, Func<bool> askForInsufficientStockConfirmationFunc) : base(log, factory)
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
					var shipmentMessageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.BelnShipment);
					var exbondMessageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.BelnExbond);

					if (exbondMessageNumber?.Length == 0)
					{
						LogResultForSingleMessage(shipmentMessageNumber);
					}
					else if (shipmentMessageNumber?.Length == 0)
					{
						log.Notify(OperationalActionLogErrorLevel.Error, "Failed to create the EDI message");
					}
					else
					{
						log.Notify(OperationalActionLogErrorLevel.Warning, $"Created messages {string.Join(", ", shipmentMessageNumber)}, {string.Join(", ", exbondMessageNumber)}");
					}
				}
			}
		}

		protected override string ExportType => WarehouseOperatorTransactionExportTypeList.Codes.BLN;
		protected override bool SortAscending => true;
		protected override bool CheckSufficientQuantitiesForAllocation => true;

		protected override bool AskForConfirmationToContinueWithInsufficientStock() => askForInsufficientStockConfirmationFunc();
		readonly Func<bool> askForInsufficientStockConfirmationFunc;
	}
}
