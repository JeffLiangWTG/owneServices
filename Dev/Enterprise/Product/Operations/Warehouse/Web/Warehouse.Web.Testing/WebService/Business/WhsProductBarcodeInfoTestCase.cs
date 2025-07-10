using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsProductBarcodeInfo))]
	public class WhsProductBarcodeInfoTestCase : DataObjectInfoTestCase<WhsProductBarcodeInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgSupplierPartBarcode barcode = factory.New<OrgSupplierPartBarcode>();
			barcode.PH_OP = factory.New<OrgSupplierPart>().PK;
			AssertNotNull(barcode.SupplierPart);
			barcode.SupplierPart.OP_PartNum = "PART NO";
			barcode.PH_Barcode = "BARCODE";
			barcode.PH_F3_NKPackType = "KG";

			WhsProductBarcodeInfo barcodeInfo = new WhsProductBarcodeInfo(barcode);
			AssertEquals("BARCODE", barcodeInfo.Barcode);
			AssertEquals("KG", barcodeInfo.PackType);
		}

		public void TestBarcode()
		{
			AssertEquals("", Parent.Barcode);

			Parent.Barcode = "1234";
			AssertEquals("1234", Parent.Barcode);

			Parent.Barcode = "4321";
			AssertEquals("4321", Parent.Barcode);
		}

		public void TestPackType()
		{
			AssertEquals("", Parent.PackType);

			Parent.PackType = "1234";
			AssertEquals("1234", Parent.PackType);

			Parent.PackType = "4321";
			AssertEquals("4321", Parent.PackType);
		}

		#endregion

		#region Implementation

		protected new WhsProductBarcodeInfo Parent
		{
			get
			{
				return (WhsProductBarcodeInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsProductBarcodeInfo();
		}

		#endregion
	}
}
