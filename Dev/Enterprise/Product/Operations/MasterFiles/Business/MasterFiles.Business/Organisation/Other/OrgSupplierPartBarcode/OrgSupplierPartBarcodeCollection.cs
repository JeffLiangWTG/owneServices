using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartBarcodeCollection : DependentBusinessObjectCollection<OrgSupplierPartBarcode, OrgSupplierPart>
	{
		#region Constructors

		public OrgSupplierPartBarcodeCollection(OrgSupplierPart part, BusinessObjectFactory factory)
			: base(part, factory)
		{
		}

		#endregion

		#region Properties

		public OrgSupplierPart SupplierPart
		{
			get { return Master; }
		}

		#endregion

		#region Methods

		public OrgSupplierPartBarcode FindUseForDocumentsPartBarcodeByPackage(ZString packType)
		{
			foreach (OrgSupplierPartBarcode partBarcode in this)
			{
				if (partBarcode.PH_F3_NKPackType.EqualsIgnoringCase(packType) && partBarcode.PH_UseForDocuments)
				{
					return partBarcode;
				}
			}
			return null;
		}

		public OrgSupplierPartBarcode FindPartBarcode(ZString packType, ZString barcode)
		{
			foreach (OrgSupplierPartBarcode partBarcode in this)
			{
				if (partBarcode.PH_F3_NKPackType.EqualsIgnoringCase(packType) && partBarcode.PH_Barcode.EqualsIgnoringCase(barcode))
				{
					return partBarcode;
				}
			}
			return null;
		}

		#endregion
	}
}
