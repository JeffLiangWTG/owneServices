using System;
using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PackageInfo : DataObjectInfo
	{
		public PackageInfo()
		{
			PK = Guid.Empty;
			PackageID = "";
			PackType = "";
		}

		public PackageInfo(PkgPackage package)
		{
			PK = package.PK.ToGuid();
			PackageID = package.KP_PackageID;
			PackType = package.KP_F3_NKPackType;
			QtyPacked = package.PackedItemDivots.Sum(p => p.KI_PackedQty);
		}

		public Guid PK
		{
			get;
			set;
		}

		public string PackageID
		{
			get;
			set;
		}

		public string PackType
		{
			get;
			set;
		}

		public decimal QtyPacked
		{
			get;
			set;
		}
	}
}
