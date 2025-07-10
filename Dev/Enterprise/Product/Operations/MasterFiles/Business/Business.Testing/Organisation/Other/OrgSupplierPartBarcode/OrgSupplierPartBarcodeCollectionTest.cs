using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartBarcodeCollection))]
	public class OrgSupplierPartBarcodeCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Test Cases

		public void TestConstructors()
		{
			AssertEquals(Part, PartBarcodeCollection.Master);

			var part2 = Factory.New<OrgSupplierPart>();
			PartBarcodeCollection = new OrgSupplierPartBarcodeCollection(part2, Factory);
			AssertEquals(part2, PartBarcodeCollection.Master);
			AssertNotEquals(Part, PartBarcodeCollection.Master);
		}

		public void TestSupplierPart()
		{
			AssertEquals(Part, PartBarcodeCollection.SupplierPart);

			var part2 = Factory.New<OrgSupplierPart>();
			PartBarcodeCollection = new OrgSupplierPartBarcodeCollection(part2, Factory);
			AssertEquals(part2, PartBarcodeCollection.SupplierPart);
			AssertNotEquals(Part, PartBarcodeCollection.SupplierPart);
		}

		public void TestFindUseForDocumentsPartBarcodeByPackage()
		{
			var barcode = PartBarcodeCollection.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_UseForDocuments = false;

			AssertNull(PartBarcodeCollection.FindUseForDocumentsPartBarcodeByPackage("UNT"));
			AssertNull(PartBarcodeCollection.FindUseForDocumentsPartBarcodeByPackage("BND"));

			barcode.PH_UseForDocuments = true;

			AssertEquals(barcode, PartBarcodeCollection.FindUseForDocumentsPartBarcodeByPackage("UNT"));
			AssertNull(PartBarcodeCollection.FindUseForDocumentsPartBarcodeByPackage("BND"));
		}

		public void TestFindPartBarcode()
		{
			var barcode = PartBarcodeCollection.AddNew();
			barcode.PH_F3_NKPackType = "UNT";

			AssertNull(PartBarcodeCollection.FindPartBarcode("UNT", "Test"));
			AssertNull(PartBarcodeCollection.FindPartBarcode("BND", "Test"));

			barcode.PH_Barcode = "Test";

			AssertEquals(barcode, PartBarcodeCollection.FindPartBarcode("UNT", "Test"));
			AssertNull(PartBarcodeCollection.FindPartBarcode("BND", "Test"));
		}

		#region TestChangingListCountTriggersOP_IsBarcodedValidation

		public void TestChangingListCountTriggersOP_IsBarcodedValidation()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_IsBarcoded = false;

			var barcodes = new OrgSupplierPartBarcodeCollection(product, Factory);
			AssertEquals("Precondition - collection is empty", 0, barcodes.Count);

			var barcode = barcodes.AddNew();
			barcode.PH_F3_NKPackType = "UNT";
			barcode.PH_Barcode = "123";
			product.PartBarcodes.Add(barcode);
			AssertHasError(barcode.PH_BarcodeInfo, "Non barcoded products should not have barcodes defined.");
		}

		#endregion

		#endregion

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new OrgSupplierPartBarcodeCollection(part, Factory);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Part = Factory.New<OrgSupplierPart>();
			PartBarcodeCollection = new OrgSupplierPartBarcodeCollection(Part, Factory);
		}

		OrgSupplierPart Part;
		OrgSupplierPartBarcodeCollection PartBarcodeCollection;

		#endregion
	}
}
