namespace Enterprise.Packing.Business
{
	public class PkgPackageItemDivotsWrapperAndBarcode
	{
		public PkgPackageItemDivotsWrapperAndBarcode(PkgPackageItemDivotsWrapper packedItem, BarcodeMatch barcode)
		{
			PackedItem = packedItem;
			Barcode = barcode;
		}

		public PkgPackageItemDivotsWrapper PackedItem { get; }
		public BarcodeMatch Barcode { get; }
	}
}
