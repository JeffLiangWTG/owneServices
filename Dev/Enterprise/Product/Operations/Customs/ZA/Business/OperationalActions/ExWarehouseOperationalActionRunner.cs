using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	class ExWarehouseOperationalActionRunner : ExportOperationalActionRunner
	{
		public ExWarehouseOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory) : base(log, factory)
		{
		}

		public override void Run(IEnumerable<OperatorTransactionSelection> selections)
		{
			bool bondRecordsAffected = false;
			var entryLines = ProcessAndGetEntryLines(selections);
			if (entryLines != null)
			{
				foreach (var lines in entryLines)
				{
					if (lines.Value.Any(x => x.IsCustomsControlled))
					{
						var messageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.ExWarehouse);
						LogResultForSingleMessage(messageNumber);
						bondRecordsAffected = true;
					}
				}
			}

			if (!bondRecordsAffected)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, "No Bond store records affected");
			}
		}

		protected override string ExportType => string.Empty;
		protected override bool SortAscending => true;
	}
}
