using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackableItemListSection))]
	sealed class PackableItemListSectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGrouping()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.Grouping);

			var cusPackageCusPackableItemRelation = CreatCusPackageCusPackableItemRelation();
			cusPackageCusPackableItemRelation.Grouping = "testGrouping";
			var docCusPackageCusPackableItemRelation = DocCusPackageCusPackableItemRelation.New(cusPackageCusPackableItemRelation, Factory);
			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, false);
			AssertEquals("testGrouping", section.Grouping);

			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, true);
			AssertEquals("testGrouping", section.Grouping);
		}

		public void TestSummary()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.Summary);

			var cusPackage = Factory.NewWithValidTestData<CusPackage>();
			cusPackage.KP_GoodsDescription = "testSummary";
			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals("testSummary", section.Summary);

			section.SetDocPkgPackage(docPkgPackage, true);
			AssertEquals("testSummary", section.Summary);
		}

		public void TestPackNoInfo()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.PackNo);

			var cusPackage = Factory.NewWithValidTestData<CusPackage>();
			cusPackage.KP_MarksAndNumbers = "testPackNoInfo";
			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals("testPackNoInfo", section.PackNo);

			section.SetDocPkgPackage(docPkgPackage, true);
			AssertNullOrEmpty(section.PackNo);
		}

		public void TestNetWeightInfo()
		{
			AssertNetWeightInfo(0, "", 0, "", "");
			AssertNetWeightInfo(1m, "PK", 0, "1 PK", "1 DT");
			AssertNetWeightInfo(1m, "PK", 1, "1 PK", "1 DT");
			AssertNetWeightInfo(2m, "PK", 2, @"@1 PK
2 PK", @"@1 DT
2 DT");
			AssertNetWeightInfo(1.899m, "PK", 2, @"@0.95 PK
1.899 PK", @"@0.95 DT
1.899 DT");
			AssertNetWeightInfo(1m, "PK", 2, @"@0.5 PK
1 PK", @"@0.5 DT
1 DT");
		}

		void AssertNetWeightInfo(ZDecimal weight, ZString weightUQ, ZInt packageQty, ZString expectedNetWeightInfo, ZString expectedNetWeightInfoForAnyItemNeight)
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.NetWeightInfo);

			var cusPackageCusPackableItemRelation = CreatCusPackageCusPackableItemRelation();
			var cusPackage = (CusPackage)cusPackageCusPackableItemRelation.Package;
			cusPackage.KP_PackageQty = packageQty;
			cusPackage.NetWeight = weight;
			cusPackage.KP_WeightUQ = weightUQ;

			cusPackageCusPackableItemRelation.NetWeight = weight;
			cusPackageCusPackableItemRelation.NetWeightUQ = "DT";
			var docCusPackageCusPackableItemRelation = DocCusPackageCusPackableItemRelation.New(cusPackageCusPackableItemRelation, Factory);

			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, false);
			AssertEquals("NetWeightInfo", expectedNetWeightInfo, section.NetWeightInfo);

			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, true, false);
			AssertEquals("NetWeightInfo", expectedNetWeightInfoForAnyItemNeight, section.NetWeightInfo);

			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, true, true);
			AssertNullOrEmpty("NetWeightInfo", section.NetWeightInfo);
		}

		public void TestGrossWeightInfo()
		{
			AssertGrossWeightInfo(0, "", 0, "");
			AssertGrossWeightInfo(1m, "PK", 0, "1 PK");
			AssertGrossWeightInfo(1m, "PK", 1, "1 PK");
			AssertGrossWeightInfo(2m, "PK", 2, @"@1 PK
2 PK");
			AssertGrossWeightInfo(1.899m, "PK", 2, @"@0.95 PK
1.899 PK");
			AssertGrossWeightInfo(1m, "PK", 2, @"@0.5 PK
1 PK");
		}

		void AssertGrossWeightInfo(ZDecimal weight, ZString weightUQ, ZInt packageQty, ZString expectedGrossWeightInfo)
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.GrossWeightInfo);

			var cusPackage = Factory.NewWithValidTestData<CusPackage>();
			cusPackage.KP_PackageQty = packageQty;
			cusPackage.KP_Weight = weight;
			cusPackage.KP_WeightUQ = weightUQ;
			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals("WeightInfo", expectedGrossWeightInfo, section.GrossWeightInfo);

			section.SetDocPkgPackage(docPkgPackage, true);
			AssertEquals("WeightInfo", expectedGrossWeightInfo, section.GrossWeightInfo);
		}

		public void TestVolumeInfo()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.VolumeInfo);

			var cusPackage = Factory.NewWithValidTestData<CusPackage>();
			cusPackage.KP_PackageQty = 1;
			cusPackage.KP_F3_NKPackType = "BAG";
			cusPackage.KP_MarksAndNumbers = "MARK1";
			cusPackage.KP_Weight = 10.001;
			cusPackage.KP_WeightUQ = "KG";
			cusPackage.KP_Length = 1.10M;
			cusPackage.KP_Width = 1.2M;
			cusPackage.KP_Height = 1.0M;
			cusPackage.KP_DimensionUQ = "CM";
			cusPackage.KP_Volume = 10.001;
			cusPackage.KP_VolumeUQ = "L";
			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"10.001 L
1.1*1.2*1 CM³", section.VolumeInfo);

			cusPackage.KP_VolumeUQ = "M3";
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"10.001 CBM
1.1*1.2*1 CM³", section.VolumeInfo);

			cusPackage.KP_VolumeUQ = "CF";
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"10.001 Cuft
1.1*1.2*1 CM³", section.VolumeInfo);

			cusPackage.KP_Length = 0.10M;
			cusPackage.KP_Width = 1.5M;
			cusPackage.KP_Height = 2.0M;
			cusPackage.KP_DimensionUQ = "PM";
			cusPackage.KP_Volume = 10.001;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"10.001 Cuft
0.1*1.5*2 PM³", section.VolumeInfo);

			section.SetDocPkgPackage(docPkgPackage, true);
			AssertNullOrEmpty(section.VolumeInfo);

			cusPackage.KP_Volume = 0M;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"0.1*1.5*2 PM³", section.VolumeInfo);

			cusPackage.KP_Volume = 12M;
			cusPackage.KP_Length = 0M;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"12 Cuft", section.VolumeInfo);

			cusPackage.KP_Length = 1.5M;
			cusPackage.KP_Width = 0M;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"12 Cuft", section.VolumeInfo);

			cusPackage.KP_Width = 1.5M;
			cusPackage.KP_Height = 0M;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"12 Cuft", section.VolumeInfo);

			cusPackage.KP_Height = 1.5M;
			cusPackage.KP_DimensionUQ = "";
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"12 Cuft", section.VolumeInfo);

			cusPackage.KP_Volume = 0M;
			docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(@"", section.VolumeInfo);
		}

		public void TestPackageSequence()
		{
			var section = new PackableItemListSection();
			AssertEquals(ZInt.Zero, section.PackageSequence);

			var cusPackage = Factory.NewWithValidTestData<CusPackage>();
			cusPackage.KP_Sequence = 1;
			var docPkgPackage = DocPkgPackage.New(cusPackage, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(1, section.PackageSequence);

			var cusPackage2 = Factory.NewWithValidTestData<CusPackage>();
			cusPackage2.KP_Sequence = 2;
			docPkgPackage = DocPkgPackage.New(cusPackage2, Factory);
			section.SetDocPkgPackage(docPkgPackage, false);
			AssertEquals(2, section.PackageSequence);

			section.SetDocPkgPackage(docPkgPackage, true);
			AssertEquals(2, section.PackageSequence);
		}

		public void TestGoodsDescription()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.GoodsDescription);

			var cusPackageCusPackableItemRelation = CreatCusPackageCusPackableItemRelation();
			cusPackageCusPackableItemRelation.GoodsDescription = "testGoodsDescription";
			var docCusPackageCusPackableItemRelation = DocCusPackageCusPackableItemRelation.New(cusPackageCusPackableItemRelation, Factory);
			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, false);
			AssertEquals("testGoodsDescription", section.GoodsDescription);

			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, true);
			AssertEquals("testGoodsDescription", section.GoodsDescription);
		}

		public void TestPackedQtyInfo()
		{
			var section = new PackableItemListSection();
			AssertNullOrEmpty(section.PackedQtyInfo);

			var cusPackageCusPackableItemRelation = CreatCusPackageCusPackableItemRelation();
			var docCusPackageCusPackableItemRelation = DocCusPackageCusPackableItemRelation.New(cusPackageCusPackableItemRelation, Factory);
			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, false);
			AssertEquals("1 BAG", section.PackedQtyInfo);

			docCusPackageCusPackableItemRelation.PackedQtyDecimalPlace = 1;
			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, false);
			AssertEquals("1.0 BAG", section.PackedQtyInfo);

			section.SetDocCusPackageCusPackableItemRelation(docCusPackageCusPackableItemRelation, false, true);
			AssertNullOrEmpty(section.PackedQtyInfo);
		}

		Customs.Business.CusPackageCusPackableItemRelation CreatCusPackageCusPackableItemRelation()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";

			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.PackableItemRelataions.RebuildElements();
			var cusPackableItem = cusPackingList.PackableItems.First();
			package.CustomsPackItem(cusPackableItem, cusPackableItem.CUI_PackableQty);
			return package.PackableItemRelataions.Cast<Customs.Business.CusPackageCusPackableItemRelation>().First();
		}
	}
}
