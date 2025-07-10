using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsProductBarcodeInfoCollection : DataObjectInfoCollection<WhsProductBarcodeInfo>
	{
		#region Constructors

		public WhsProductBarcodeInfoCollection(OrgSupplierPartBarcodeCollection barcodes)
		{
			AddBarcodes(barcodes);
		}

		public WhsProductBarcodeInfoCollection()
		{
		}

		#endregion

		void AddBarcodes(OrgSupplierPartBarcodeCollection barcodes)
		{
			foreach (OrgSupplierPartBarcode barcode in barcodes)
			{
				WhsProductBarcodeInfo barcodeInfo = new WhsProductBarcodeInfo(barcode);
				this.Add(barcodeInfo);
			}
		}
	}
}
