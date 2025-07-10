using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRFRegistryDocumentEngineHelper : IWhsRFRegistryDocumentEngineHelper
	{
		public void CheckForWhsRFRegistryRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory)
		{
			var rfRegistryEntries = PrintQueueUtils.GetReferencedRecords<WhsRFRegistry>(factory, printQueuePk, AutoWhsRFRegistry.Schema.TableName, AutoWhsRFRegistry.Schema.WRR_SQ_Printer);
			var users = string.Join(System.Environment.NewLine, rfRegistryEntries.Select(l => l.WRR_GS_NKAssignedTo));

			if (!string.IsNullOrEmpty(users))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("c8246035-2d9f-480b-be11-033e044a9305", @"This record is in use by one or more record(s) of the RF Register Settings for the following users, and thus cannot be deleted."));
				builder.AppendLine(users);
			}
		}
	}
}
