using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsProductBarcodeInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsProductBarcodeInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgSupplierPart part = factory.New<OrgSupplierPart>();
			WhsProductBarcodeInfoCollection collection = new WhsProductBarcodeInfoCollection(new OrgSupplierPartBarcodeCollection(part, factory));
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			OrgSupplierPartBarcodeCollection partBarcodes = new OrgSupplierPartBarcodeCollection(part, factory);
			OrgSupplierPartBarcode partBarcode1 = partBarcodes.AddNew();
			partBarcode1.PH_Barcode = "12345";
			partBarcode1.PH_F3_NKPackType = "CTN";

			AssertEquals(1, partBarcodes.Count);
			WhsProductBarcodeInfoCollection collection1 = new WhsProductBarcodeInfoCollection(partBarcodes);
			AssertNotNull(collection1);
			AssertEquals(1, collection1.Count);
			AssertEquals("12345", collection1[0].Barcode);
			AssertEquals("CTN", collection1[0].PackType);

			OrgSupplierPartBarcode partBarcode2 = partBarcodes.AddNew();
			partBarcode2.PH_Barcode = "67890";
			partBarcode2.PH_F3_NKPackType = "PLT";

			AssertEquals(2, partBarcodes.Count);
			WhsProductBarcodeInfoCollection collection2 = new WhsProductBarcodeInfoCollection(partBarcodes);
			AssertNotNull(collection2);
			AssertEquals(2, collection2.Count);

			WhsProductBarcodeInfo barcodeInfo1 = GetProductBarcodeInfoByBarcode("12345", collection2);
			AssertEquals("12345", barcodeInfo1.Barcode);
			AssertEquals("CTN", barcodeInfo1.PackType);

			WhsProductBarcodeInfo barcodeInfo2 = GetProductBarcodeInfoByBarcode("67890", collection2);
			AssertEquals("67890", barcodeInfo2.Barcode);
			AssertEquals("PLT", barcodeInfo2.PackType);
		}

		WhsProductBarcodeInfo GetProductBarcodeInfoByBarcode(string barcode, WhsProductBarcodeInfoCollection collection)
		{
			foreach (WhsProductBarcodeInfo barcodeInfo in collection)
			{
				if (barcodeInfo.Barcode == barcode)
				{
					return barcodeInfo;
				}
			}
			return null;
		}

		#endregion

		#region Implementation

		protected new WhsProductBarcodeInfoCollection Parent
		{
			get
			{
				return (WhsProductBarcodeInfoCollection)base.Parent;
			}
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsProductBarcodeInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsProductBarcodeInfoCollection);
		}

		protected override WhsProductBarcodeInfo GetNewObjectInfo()
		{
			return new WhsProductBarcodeInfo();
		}

		protected override DataObjectInfoCollection<WhsProductBarcodeInfo> GetNewObjectInfoCollection()
		{
			return new WhsProductBarcodeInfoCollection();
		}

		#endregion
	}
}
