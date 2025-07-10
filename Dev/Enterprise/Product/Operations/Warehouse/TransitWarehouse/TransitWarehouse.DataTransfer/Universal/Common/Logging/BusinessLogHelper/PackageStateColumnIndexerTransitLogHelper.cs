using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class PackageStateColumnIndexerTransitLogHelper : TransitLogTableHelper<IColumnIndexer, TransitLogColumnIDs.PackageStateIndexerColumn>
	{
		protected readonly UniversalObjectFactory Factory;
		protected IEnumerable<(ZString Type, long Qty)> TypeAndQty { get; set; }
		public PackageStateColumnIndexerTransitLogHelper(UniversalObjectFactory factory)
		{
			Factory = factory;
		}

		public ZString GetTable(ZString tableMessage, IEnumerable<IColumnIndexer> businessObjects, IEnumerable<(ZString Type, long Qty)> typeAndQty, params TransitLogColumnIDs.PackageStateIndexerColumn[] columnIDs)
		{
			TypeAndQty = typeAndQty;
			return base.GetTable(tableMessage, businessObjects, columnIDs);
		}

		protected override void ProcessRows(TransitLogColumn<TransitLogColumnIDs.PackageStateIndexerColumn>[] columns)
		{
			var firstColumn = columns.FirstOrDefault();
			if (firstColumn != null && firstColumn.ColumnID == TransitLogColumnIDs.PackageStateIndexerColumn.UxmlPackage)
			{
				var currentValue = "";
				for (int i = 0; i < firstColumn.Values.Length; i++)
				{
					if (firstColumn.Values[i] == currentValue)
					{
						firstColumn.Values[i] = "";
					}
					else
					{
						currentValue = firstColumn.Values[i];
					}
				}
			}
		}

		protected override ZString GetValue(IColumnIndexer packageState, TransitLogColumnIDs.PackageStateIndexerColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.PackageStateIndexerColumn.Package:
					return PackageStateColumnIndexerHelper.GetPackageReference(Factory, packageState);
				case TransitLogColumnIDs.PackageStateIndexerColumn.RCN:
					var rcn = PackageStateColumnIndexerHelper.GetRCN(Factory, packageState);
					return RCNColumnIndexerHelper.GetFormattedReference(Factory, rcn);
				case TransitLogColumnIDs.PackageStateIndexerColumn.DCN:
					var dcn = PackageStateColumnIndexerHelper.GetDCN(Factory, packageState);
					return DCNColumnIndexerHelper.GetFormattedReference(Factory, dcn);
				case TransitLogColumnIDs.PackageStateIndexerColumn.LoadList:
					var ddl = PackageStateColumnIndexerHelper.GetLoadList(Factory, packageState);
					return LoadListColumnIndexerHelper.GetFormattedReference(Factory, ddl);
				case TransitLogColumnIDs.PackageStateIndexerColumn.Status:
					return PackageStateColumnIndexerHelper.GetStatus(packageState);
				case TransitLogColumnIDs.PackageStateIndexerColumn.UxmlPackage:
					return GetUXMLPackage(packageState);
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.PackageStateIndexerColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.PackageStateIndexerColumn.Package:
					return Res.GetString("a5078599-aa4a-4b32-8cc8-71b7e5f4df39", "Package");
				case TransitLogColumnIDs.PackageStateIndexerColumn.RCN:
					return Res.GetString("749a3dbb-5bb0-4839-9ca9-a28a0417d35b", "RCN");
				case TransitLogColumnIDs.PackageStateIndexerColumn.DCN:
					return Res.GetString("6dba5829-6a5d-4501-b732-1b3a13a2cf34", "DCN");
				case TransitLogColumnIDs.PackageStateIndexerColumn.LoadList:
					return Res.GetString("3ec099a2-30f3-4a30-a84b-9f7bf595ccdb", "Load List");
				case TransitLogColumnIDs.PackageStateIndexerColumn.Status:
					return Res.GetString("893c5823-f024-41cc-a02e-514f5a65ae5d", "Status");
				case TransitLogColumnIDs.PackageStateIndexerColumn.UxmlPackage:
					return Res.GetString("a1bbe865-1a2a-4fbc-97c0-f38235fc0109", "UXML Package");
				default:
					return "";
			}
		}

		ZString GetUXMLPackage(IColumnIndexer packageState)
		{
			if (packageState != null && TypeAndQty != null)
			{
				var matchedTypeAndQty = TypeAndQty.Where(t => t.Type == PackageStateColumnIndexerHelper.GetPackageType(Factory, packageState));
				if (matchedTypeAndQty.Any())
				{
					return $"{matchedTypeAndQty.First().Qty} {matchedTypeAndQty.First().Type}"; // This is a format string only contains symbols
				}
			}
			return "";
		}
	}
}
