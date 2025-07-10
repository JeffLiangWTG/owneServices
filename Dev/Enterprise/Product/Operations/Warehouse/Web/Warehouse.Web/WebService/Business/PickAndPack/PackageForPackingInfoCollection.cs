using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class PackageForPackingInfoCollection : DataObjectInfoCollection<PackageForPackingInfo>
	{
		public PackageForPackingInfoCollection()
		{
		}

		public PackageForPackingInfoCollection(PackageForPackingInfo[] packageForPackingInfos)
		{
			foreach (var packageForPacking in packageForPackingInfos)
			{
				Add(packageForPacking);
			}
		}
	}
}
