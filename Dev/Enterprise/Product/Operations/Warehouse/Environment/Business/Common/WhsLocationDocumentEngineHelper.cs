using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationDocumentEngineHelper : IWhsLocationDocumentEngineHelper
	{
		public void CheckForWhsLocationRecords(ZStringBuilder builder, ZGuid printQueuePk, BusinessObjectFactory factory)
		{
			var locations = PrintQueueUtils.GetReferencedRecords<WhsLocation>(factory, printQueuePk, AutoWhsLocationView.Schema.TableName, AutoWhsLocationView.Schema.WLV_SQ_DefaultPrintQueue);
			var message = string.Join(System.Environment.NewLine, locations.Select(l => l.WLV_LocationString));

			if (!string.IsNullOrEmpty(message))
			{
				if (!builder.IsEmpty)
				{
					builder.AppendLine("");
				}
				builder.AppendLine(Res.GetString("64AB0ABC-6887-42F1-B03D-8D0B742C03AF", @"This record is in use by one or more record(s) of the module Locations with the following descriptions, and thus cannot be deleted."));
				builder.AppendLine(message);
			}
		}
	}
}
