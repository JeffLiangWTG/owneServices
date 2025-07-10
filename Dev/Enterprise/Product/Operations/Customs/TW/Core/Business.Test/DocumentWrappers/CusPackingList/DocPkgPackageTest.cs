using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocPkgPackage))]
	sealed class DocPkgPackageTest : DocBasePkgPackageTestClass<CusPackage, DocPkgPackage>
	{
		public void TestPackNoInfo()
		{
			PkgPackageInternal.KP_MarksAndNumbers = "MARK1";
			AssertEquals("PackDate", "MARK1", PkgPackageWrapperInternal.PackNoInfo);
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
			PkgPackageInternal.KP_PackageQty = packageQty;
			PkgPackageInternal.KP_Weight = weight;
			PkgPackageInternal.KP_WeightUQ = weightUQ;
			AssertEquals("WeightInfo", expectedGrossWeightInfo, PkgPackageWrapperInternal.GrossWeightInfo);
		}

		public void TestVolumeUQAndVolumeInfo()
		{
			PkgPackageInternal.KP_PackageQty = 1;
			PkgPackageInternal.KP_F3_NKPackType = "BAG";
			PkgPackageInternal.KP_MarksAndNumbers = "MARK1";
			PkgPackageInternal.KP_Weight = 10.001;
			PkgPackageInternal.KP_WeightUQ = "KG";
			PkgPackageInternal.KP_Length = 1.10M;
			PkgPackageInternal.KP_Width = 1.2M;
			PkgPackageInternal.KP_Height = 1.0M;
			PkgPackageInternal.KP_DimensionUQ = "CM";
			PkgPackageInternal.KP_Volume = 10.001;
			PkgPackageInternal.KP_VolumeUQ = "L";

			AssertEquals(@"10.001 L
1.1*1.2*1 CM³", PkgPackageWrapperInternal.VolumeInfo);
			AssertEquals(@"L", PkgPackageWrapperInternal.VolumeUQInfo);

			PkgPackageInternal.KP_VolumeUQ = "M3";
			AssertEquals(@"CBM", PkgPackageWrapperInternal.VolumeUQInfo);
			AssertEquals(@"10.001 CBM
1.1*1.2*1 CM³", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_VolumeUQ = "CF";
			AssertEquals(@"Cuft", PkgPackageWrapperInternal.VolumeUQInfo);
			AssertEquals(@"10.001 Cuft
1.1*1.2*1 CM³", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Length = 0.10M;
			PkgPackageInternal.KP_Width = 1.5M;
			PkgPackageInternal.KP_Height = 2.0M;
			PkgPackageInternal.KP_DimensionUQ = "PM";
			PkgPackageInternal.KP_Volume = 10.001;
			AssertEquals(@"10.001 Cuft
0.1*1.5*2 PM³", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Volume = 0M;
			AssertEquals(@"0.1*1.5*2 PM³", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Volume = 12M;
			PkgPackageInternal.KP_Length = 0M;

			AssertEquals(@"12 Cuft", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Length = 1.5M;
			PkgPackageInternal.KP_Width = 0M;
			AssertEquals(@"12 Cuft", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Width = 1.5M;
			PkgPackageInternal.KP_Height = 0M;
			AssertEquals(@"12 Cuft", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Height = 1.5M;
			PkgPackageInternal.KP_DimensionUQ = "";
			AssertEquals(@"12 Cuft", PkgPackageWrapperInternal.VolumeInfo);

			PkgPackageInternal.KP_Volume = 0M;
			AssertEquals(@"", PkgPackageWrapperInternal.VolumeInfo);
		}

		public void TestNetWeightInfo()
		{
			AssertNetWeightInfo(0, "", 0, "");
			AssertNetWeightInfo(1m, "PK", 0, "1 PK");
			AssertNetWeightInfo(1m, "PK", 1, "1 PK");
			AssertNetWeightInfo(2m, "PK", 2, @"@1 PK
2 PK");
			AssertNetWeightInfo(1.899m, "PK", 2, @"@0.95 PK
1.899 PK");
			AssertNetWeightInfo(1m, "PK", 2, @"@0.5 PK
1 PK");
		}

		void AssertNetWeightInfo(ZDecimal weight, ZString weightUQ, ZInt packageQty, ZString expectedNetWeightInfo)
		{
			PkgPackageInternal.KP_PackageQty = packageQty;
			PkgPackageInternal.NetWeight = weight;
			PkgPackageInternal.KP_WeightUQ = weightUQ;
			AssertEquals("NetWeightInfo", expectedNetWeightInfo, PkgPackageWrapperInternal.NetWeightInfo);
		}

		public void TestPackedItemRelation()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader1 = dec.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceHeader2 = dec.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			var invoieceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
			invoieceLine1_1.JI_Description = "line4";
			invoieceLine1_1.JI_InvoiceQuantity = 1m;
			invoieceLine1_1.JI_InvoiceUQ = "BAG";
			var invoieceLine1_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoieceLine1_2.JI_Description = "line3";
			invoieceLine1_2.JI_InvoiceQuantity = 1m;
			invoieceLine1_2.JI_InvoiceUQ = "BAG";
			var invoieceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
			invoieceLine2_1.JI_Description = "line2";
			invoieceLine2_1.JI_InvoiceQuantity = 1m;
			invoieceLine2_1.JI_InvoiceUQ = "BAG";
			var invoieceLine2_2 = invoiceHeader2.InvoiceLines.AddNew();
			invoieceLine2_2.JI_Description = "line1";
			invoieceLine2_2.JI_InvoiceQuantity = 1m;
			invoieceLine2_2.JI_InvoiceUQ = "BAG";

			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = (CusPackage)packageJob.Packages.AddNew();

			var docCusPackage = CreatePkgPackageWrapper(package);
			AssertEquals(0, docCusPackage.PackedItemRelations.Count());

			cusPackingList.PackableItems.ForEach(x => package.CustomsPackItem(x, x.CUI_PackableQty));
			package.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.GoodsDescription, ListSortDirection.Ascending);
			var relations = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			AssertEquals(relations.First().GoodsDescription, "line1");
			AssertEquals(relations.ElementAt(1).GoodsDescription, "line2");
			AssertEquals(relations.ElementAt(2).GoodsDescription, "line3");
			AssertEquals(relations.ElementAt(3).GoodsDescription, "line4");

			docCusPackage = CreatePkgPackageWrapper(package);
			var packedItemRelations = docCusPackage.PackedItemRelations;
			AssertEquals(4, packedItemRelations.Count());
			AssertEquals("line4", packedItemRelations.First().GoodsDescription);
			AssertEquals("line3", packedItemRelations.ElementAt(1).GoodsDescription);
			AssertEquals("line2", packedItemRelations.ElementAt(2).GoodsDescription);
			AssertEquals("line1", packedItemRelations.ElementAt(3).GoodsDescription);
		}

		public void TestPackedItemRelationsRelation()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader1 = dec.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "1";
			var invoiceHeader2 = dec.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "2";
			var invoieceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
			invoieceLine1_1.JI_Description = "line4";
			invoieceLine1_1.JI_InvoiceQuantity = 1m;
			invoieceLine1_1.JI_InvoiceUQ = "BAG";
			var invoieceLine1_2 = invoiceHeader1.InvoiceLines.AddNew();
			invoieceLine1_2.JI_Description = "line3";
			invoieceLine1_2.JI_InvoiceQuantity = 1m;
			invoieceLine1_2.JI_InvoiceUQ = "BAG";
			var invoieceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
			invoieceLine2_1.JI_Description = "line2";
			invoieceLine2_1.JI_InvoiceQuantity = 1m;
			invoieceLine2_1.JI_InvoiceUQ = "BAG";
			var invoieceLine2_2 = invoiceHeader2.InvoiceLines.AddNew();
			invoieceLine2_2.JI_Description = "line1";
			invoieceLine2_2.JI_InvoiceQuantity = 1m;
			invoieceLine2_2.JI_InvoiceUQ = "BAG";

			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = (CusPackage)packageJob.Packages.AddNew();

			var docCusPackage = CreatePkgPackageWrapper(package);
			AssertEquals(0, docCusPackage.PackedItemRelationsRelation.Count);

			var firstItem = cusPackingList.PackableItems.First();
			package.CustomsPackItem(firstItem, firstItem.CUI_PackableQty);

			docCusPackage = CreatePkgPackageWrapper(package);
			var packedItemRelationsRelation = docCusPackage.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>();
			AssertEquals(1, packedItemRelationsRelation.Count());
			AssertEquals("line4", packedItemRelationsRelation.ElementAt(0).GoodsDescription);

			cusPackingList.PackableItems.Where(x => !package.IsPacked(x)).ForEach(x => package.CustomsPackItem(x, x.CUI_PackableQty));
			package.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.GoodsDescription, ListSortDirection.Ascending);
			var relations = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			AssertEquals(relations.First().GoodsDescription, "line1");
			AssertEquals(relations.ElementAt(1).GoodsDescription, "line2");
			AssertEquals(relations.ElementAt(2).GoodsDescription, "line3");
			AssertEquals(relations.ElementAt(3).GoodsDescription, "line4");

			docCusPackage = CreatePkgPackageWrapper(package);
			packedItemRelationsRelation = docCusPackage.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>();
			AssertEquals(4, packedItemRelationsRelation.Count());
			AssertEquals("line4", packedItemRelationsRelation.ElementAt(0).GoodsDescription);
			AssertEquals("line3", packedItemRelationsRelation.ElementAt(1).GoodsDescription);
			AssertEquals("line2", packedItemRelationsRelation.ElementAt(2).GoodsDescription);
			AssertEquals("line1", packedItemRelationsRelation.ElementAt(3).GoodsDescription);
		}
		#region Implementation

		protected override string TestingCountry => Core.Constants.CountryCodes.Taiwan;

		protected override CusPackage GetNewPkgPackage()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine.JI_Description = "line4";
			invoieceLine.JI_InvoiceQuantity = 1m;
			invoieceLine.JI_InvoiceUQ = "BAG";
			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = CusPackageJob.LoadOrCreatePackageJob(cusPackingList) as CusPackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_Sequence = 1;
			package.KP_Weight = 1m;
			package.KP_Length = 2m;
			package.KP_Width = 3m;
			package.KP_Height = 4m;
			package.KP_Volume = 5m;
			package.NetWeight = 0.5m;
			package.KP_PackageQty = 6;
			package.KP_WeightUQ = "KG";
			package.KP_DimensionUQ = "CM";
			package.KP_VolumeUQ = "M3";
			package.KP_F3_NKPackType = "BAG";
			package.KP_MarksAndNumbers = "marks";
			package.KP_GoodsDescription = "Summary";
			return package;
		}

		protected override DocPkgPackage CreatePkgPackageWrapper(CusPackage pkgPackage)
		{
			return DocPkgPackage.New(pkgPackage, Factory);
		}

		#endregion
	}
}
