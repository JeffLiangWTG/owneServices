using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsProductBarcodeInfo : DataObjectInfo
	{
		#region Constructors

		public WhsProductBarcodeInfo(OrgSupplierPartBarcode partBarcode)
			: this()
		{
			this.barcode = partBarcode.PH_Barcode;
			this.packType = partBarcode.PH_F3_NKPackType;
		}

		public WhsProductBarcodeInfo()
		{
			this.barcode = "";
			this.packType = "";
		}

		#endregion

		#region Properties

		public string Barcode
		{
			get { return this.barcode; }
			set { this.barcode = value; }
		}

		public string PackType
		{
			get { return this.packType; }
			set { this.packType = value; }
		}

		#endregion

		#region Implementation

		string barcode;
		string packType;

		#endregion
	}
}
