using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class COEEventProcessor
	{
		public COEEventProcessor(BusinessObjectFactory factory, IXmlImportLogger logger, UniversalEvent xmlEvent)
		{
			this.factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
			this.xmlEvent = Argument.NotNull(xmlEvent, "UniversalEvent xmlEvent");
		}

		public void Execute()
		{
			AddExitDateToMatchingOrderLines();
		}

		protected void AddExitDateToMatchingOrderLines()
		{
			var eventTime = xmlEvent.EventTime.GetValueOrDefault();
			if (eventTime.IsZDateTimeOffset)
			{
				var exitDateTime = eventTime.ToZDateTimeOffset();
				if (CachedOrderLines.Count > 0)
				{
					var updatedOrderLines = 0;
					foreach (var orderLine in CachedOrderLines)
					{
						if (orderLine != null)
						{
							var addInfo = orderLine.CustomsData.BondedAdditionalInfo;
							if (!addInfo.Cast<WhsBWAAddInfo>().Any(x => x.KeyString == "ExitDate"))
							{
								addInfo.Add(new WhsBWAAddInfo("ExitDate", exitDateTime.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture)));
							}
							orderLine.CustomsData.WB_AddInfo = addInfo.Serialize();
							updatedOrderLines += 1;
						}
					}

					if (updatedOrderLines == 1)
					{
						logger.Log(LogType.Information, Res.GetString("DD18B972-9191-4619-B7D9-32698DC213A9", "1 order line was updated with exit date.", updatedOrderLines.ToString()));
					}
					else if (updatedOrderLines > 1)
					{
						logger.Log(LogType.Information, Res.GetString("1F9D2207-CD01-4AA1-9F3C-8B770A461526", "{0} order lines were updated with exit date.", updatedOrderLines.ToString()));
					}
				}
				else
				{
					logger.Log(LogType.Information, Res.GetString("53241E99-75C1-45B4-9AD5-754A5B2E313B", "No order line matching the event was found."));
				}
			}
			else
			{
				logger.Log(LogType.Information, Res.GetString("3220AE53-C35D-4FE2-ABE3-4E4D5169C27B", "The COE event misses Event Time information."));
			}
		}

		List<WhsOrderLine> CachedOrderLines => cachedOrderLines ?? (cachedOrderLines = EventProcessorHelper.GetEventRelatedOrderLines(AutoEvents.ConfirmationOfExitCode, xmlEvent, cachedOrderLines, logger, factory));
		List<WhsOrderLine> cachedOrderLines;

		readonly IXmlImportLogger logger;
		readonly UniversalEvent xmlEvent;
		readonly BusinessObjectFactory factory;
	}
}
