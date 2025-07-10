using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class CENEventProcessor
	{
		public CENEventProcessor(BusinessObjectFactory factory, IXmlImportLogger logger, UniversalEvent xmlEvent)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
			this.xmlEvent = Argument.NotNull(xmlEvent, "UniversalEvent xmlEvent");
		}

		public void Execute()
		{
			SetUpEntryKeyOfMatchingOrderLines();
		}

		protected void SetUpEntryKeyOfMatchingOrderLines()
		{
			if (CachedOrderLines.Count > 0)
			{
				var entryNumber = EventProcessorHelper.GetEventEntryNumber(xmlEvent);
				var updatedOrderLines = 0;

				foreach (var orderLine in CachedOrderLines)
				{
					if (orderLine != null)
					{
						if (orderLine.CustomsData.WB_EntryKey.IsEmpty)
						{
							orderLine.CustomsData.WB_EntryKey = entryNumber;
						}

						updatedOrderLines += 1;
					}
				}

				if (updatedOrderLines == 1)
				{
					logger.Log(LogType.Information, Res.GetString("9018A05B - F946 - 47BA - 8A6A - 04C1A6645B36", "1 order line Entry Key was updated with Entry Number.", updatedOrderLines.ToString()));
				}
				else if (updatedOrderLines > 1)
				{
					logger.Log(LogType.Information, Res.GetString("1E149C0F-8EDD-45CE-B4BC-AE192820AE7A", "{0} order lines Entry Key was updated with Entry Number.", updatedOrderLines.ToString()));
				}
			}
			else
			{
				logger.Log(LogType.Information, Res.GetString("A83EBF89-A3DF-44D1-B670-977A9A6D74E8", "No order line matching the event was found."));
			}
		}

		List<WhsOrderLine> CachedOrderLines => cachedOrderLines ?? (cachedOrderLines = EventProcessorHelper.GetEventRelatedOrderLines(AutoEvents.CustomsNumberEnteredCode, xmlEvent, cachedOrderLines, logger, factory));
		List<WhsOrderLine> cachedOrderLines;

		readonly IXmlImportLogger logger;
		readonly UniversalEvent xmlEvent;
		readonly BusinessObjectFactory factory;
	}
}
