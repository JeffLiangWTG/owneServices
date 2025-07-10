using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class OrgSupplierPartDataLoadTest : DataLoadTestCase<OrgSupplierPartDataLoad>
	{
		#region TestLoadUnitConversions

		public void TestLoadUnitConversions()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org";
			Factory.Save();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package");
					sw.WriteLine($"1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,UNT,BAG,20,BAG,PLT,,,,,,,,,,GEN,,,,,,,,,,,,");
					sw.WriteLine($"2,P2,BBG,{org.OH_Code},,1100,,LB,,,,,,,671,,,,,,,1,UNT,BBG,40,BBG,PLT,,,,,,,,,,GEN,,,,,,,,,,,,");
				}
				GetNewDataLoader().ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(string.Join("\r\n", product.PartUnits.Cast<OrgPartUnit>().Select(p => $"{p.OF_QuantityInParent} x {p.OF_PackType} in {p.OF_ParentPackType}")), 3, product.PartUnits.Count);
			AssertEquals("BAG", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_ParentPackType);
			AssertEquals("PLT", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 20m).OF_ParentPackType);
			AssertEquals("LB", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 55m).OF_PackType);

			product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "2")).Single();
			AssertEquals(3, product.PartUnits.Count);
			AssertEquals("BBG", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1m).OF_ParentPackType);
			AssertEquals("PLT", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 40m).OF_ParentPackType);
			AssertEquals("LB", product.PartUnits.Cast<OrgPartUnit>().Single(p => p.OF_QuantityInParent == 1100m).OF_PackType);
		}

		#endregion

		#region TestLoadBarCode

		public void TestLoadBarCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org";
			Factory.Save();
			using (var csvFile = TempFile.New())
			{
				using (var sw = new StreamWriter(csvFile.Filename))
				{
					sw.WriteLine("Code,Description,UQ,Owner,Supplier,Unit_Weight,Unit_NetWeight,Weight_Unit,Unit_Volume,Volume_Unit,Department,Division,QtyInStock,Origin,Last_Cost,UNDG_Code,LocalPartNumber,LocalPartDescription,Use_Attribute1,Use_Attribute2,Use_Attribute3,UC1_QtyParent,UC1_Package,UC1_ParentPackage,UC2_QtyParent,UC2_Package,UC2_ParentPackage,UC3_QtyParent,UC3_Package,UC3_ParentPackage,UC4_QtyParent,UC4_Package,UC4_ParentPackage,UC5_QtyParent,UC5_Package,UC5_ParentPackage,Commodity,BrandName,Model,Barcode1,Barcode1_Package,Barcode2,Barcode2_Package,Barcode3,Barcode3_Package,Barcode4,Barcode4_Package,Barcode5,Barcode5_Package");
					sw.WriteLine($"1,P1,BAG,{org.OH_Code},,55  ,,LB,,,,,,,40 ,,,,,,,1,DRM,BAG,2,CTN,BAG,3,BOX,BAG,4,PLT,BAG,5,UNT,BAG,GEN,,,Barcode11,UNT,Barcode21,DRM,Barcode31,CTN,Barcode41,BOX,Barcode51,PLT");
					sw.WriteLine($"2,P2,BBG,{org.OH_Code},,1100,,LB,,,,,,,671,,,,,,,1,UNT,BBG,40,BBG,PLT,,,,,,,,,,GEN,,,,,,,,,,,,");
				}
				GetNewDataLoader().ImportProductData(csvFile.Filename, true, false);
				Factory.Save();
			}

			var product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "1")).Single();
			AssertEquals(5, product.PartBarcodes.Count);
			AssertEquals("Barcode11", product.PartBarcodes.FindPartBarcode("UNT", "Barcode11").PH_Barcode);
			AssertEquals("Barcode21", product.PartBarcodes.FindPartBarcode("DRM", "Barcode21").PH_Barcode);
			AssertEquals("Barcode31", product.PartBarcodes.FindPartBarcode("CTN", "Barcode31").PH_Barcode);
			AssertEquals("Barcode41", product.PartBarcodes.FindPartBarcode("BOX", "Barcode41").PH_Barcode);
			AssertEquals("Barcode51", product.PartBarcodes.FindPartBarcode("PLT", "Barcode51").PH_Barcode);

			product = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "2")).Single();
			AssertEquals("Should not mix product 1 barcode with product 2", 0, product.PartBarcodes.Count);
		}

		#endregion
	}
}
