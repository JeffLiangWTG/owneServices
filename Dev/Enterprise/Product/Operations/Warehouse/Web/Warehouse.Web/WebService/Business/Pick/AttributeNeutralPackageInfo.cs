using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class AttributeNeutralPackageInfo : DataObjectInfo
	{
		public AttributeNeutralPackageInfo()
		{
			PackType = string.Empty;
			ExpectedQuantityInPackage = 0;
			PalletID = string.Empty;
			ProductPK = Guid.Empty;
			PackagePK = Guid.Empty;
			SerialNumbers = Array.Empty<string>();
		}

		public AttributeNeutralPackageInfo(string packType, int expectedQuantity, string palletID, Guid productPK, string[] serials, bool isCompletePalletPicked = false)
		{
			PackagePK = Guid.Empty;
			PackType = packType;
			ExpectedQuantityInPackage = expectedQuantity;
			PalletID = palletID;
			ProductPK = productPK;
			SerialNumbers = serials;
		}

		public Guid PackagePK { get; set; }
		public string PackType { get; set; }
		public int ExpectedQuantityInPackage { get; set; }
		public string PalletID { get; set; }
		public Guid ProductPK { get; set; }
		public string[] SerialNumbers { get; set; }
	}
}
