using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsDefaultPrinterHelper : IWhsDefaultPrinterHelper
	{
		public void CheckForStmDefaultPrinterRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory)
		{
			var messages = new Dictionary<string, List<string>>();
			var defaultPrinter = PrintQueueUtils.GetReferencedRecords<StmDefaultPrinter>(factory, printQueuePk, AutoStmDefaultPrinter.Schema.TableName, AutoStmDefaultPrinter.Schema.SDP_SQ_Printer);
			foreach (var printer in defaultPrinter)
			{
				var prefix = printer.SDP_SubjectTableCode;
				var pk = printer.SDP_SubjectID;

				if (prefix == WhsWarehouseSchema.Constants.Prefix ||
					prefix == WhsAreaSchema.Constants.Prefix)
				{
					var bo = factory.Load(prefix, pk);

					switch (bo)
					{
						case WhsWarehouse warehouse:
							AddMessage(Res.GetString("121fdc28-8d8f-44af-9b74-dedea4e9df7d", "Warehouse"), warehouse.WW_WarehouseNameMultilingual);
							break;
						case WhsArea area:
							AddMessage(Res.GetString("73528d4a-3508-4290-9948-7e6bd8d5255b", "Areas"), area.WA_NameMultilingual);
							break;
						default:
							break;
					}
				}
			}

			void AddMessage(string moduleName, string whsBoName)
			{
				if (!messages.TryGetValue(moduleName, out var messagesForThisModule))
				{
					messages[moduleName] = messagesForThisModule = new List<string>();
				}

				messagesForThisModule.Add(whsBoName);
			}

			foreach (var msg in messages)
			{
				var names = msg.Value.Where(s => !string.IsNullOrEmpty(s)).ToArray();
				if (names.Length > 0)
				{
					if (!builder.IsEmpty)
					{
						builder.AppendLine("");
					}
					builder.AppendLine(Res.GetString("1E684D7D-2666-4663-8D2A-09ACB70CE4E0", @"This record is in use by one or more record(s) of the module {0} with the following names, and thus cannot be deleted.", msg.Key));
					foreach (var name in names)
					{
						builder.AppendLine(name);
					}
				}
			}
		}
	}
}
