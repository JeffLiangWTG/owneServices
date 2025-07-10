using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WhsRowDataData),
	Enterprise.Core.Constants.DocManagerCodes.WarehouseRow)]

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRowDataData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsRow); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("7ec129e2-6a92-49b0-9881-841e94d4ca81", "Warehouse Row"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}