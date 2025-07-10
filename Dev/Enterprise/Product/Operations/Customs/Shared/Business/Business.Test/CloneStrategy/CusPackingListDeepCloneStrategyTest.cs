using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingListDeepCloneStrategy))]
	sealed class CusPackingListDeepCloneStrategyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCloneInJobDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTesting>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_InvoiceUQ = "PCE";
			var line2 = invoice1.InvoiceLines.AddNew();
			line2.JI_InvoiceQuantity = 2m;
			line2.JI_InvoiceUQ = string.Empty;
			var line3 = invoice2.InvoiceLines.AddNew();
			line3.JI_InvoiceQuantity = 3m;
			line3.JI_InvoiceUQ = "PCE";
			var line4 = invoice2.InvoiceLines.AddNew();
			line4.JI_InvoiceQuantity = 4m;
			line4.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);

			var cusPackage1 = cusPackingList.PackageJob.Packages.AddNew();
			cusPackage1.KP_GoodsDescription = "desc 1";
			var relations1 = cusPackage1.PackableItemRelataions;
			var packableItem1 = cusPackingList.PackableItems.First(x => x.CUI_Sequence == 1);
			packableItem1.CUI_GoodsDescription = "11";
			packableItem1.CUI_NetWeight = 10m;
			packableItem1.CUI_NetWeightUQ = "KG";
			packableItem1.CUI_PackableQty = 12m;
			packableItem1.CUI_PackableUQ = "ANT";
			packableItem1.Grouping = "grouping 1";
			cusPackage1.CustomsPackItem(packableItem1, 3m);

			var cusPackage2 = cusPackingList.PackageJob.Packages.AddNew();
			cusPackage2.KP_GoodsDescription = "desc 2";
			var relations2 = cusPackage2.PackableItemRelataions;
			var packableItem2 = cusPackingList.PackableItems.First(x => x.CUI_Sequence == 2);
			packableItem2.CUI_GoodsDescription = "22";
			packableItem2.CUI_NetWeight = 20m;
			packableItem2.CUI_NetWeightUQ = "G";
			packableItem2.CUI_PackableQty = 22m;
			packableItem2.CUI_PackableUQ = "PLT";
			packableItem2.Grouping = "grouping 2";
			cusPackage2.CustomsPackItem(packableItem2, 4m);

			var cusPackage1PackedItemDivot = cusPackage1.PackedItemDivots.FirstOrDefault();
			cusPackage1PackedItemDivot.PkgNetWeight = 2m;
			cusPackage1PackedItemDivot.PkgNetWeightUQ = "ANT";

			var cusPackage2PackedItemDivot = cusPackage2.PackedItemDivots.FirstOrDefault();
			cusPackage2PackedItemDivot.PkgNetWeight = 6m;
			cusPackage2PackedItemDivot.PkgNetWeightUQ = "BAG";

			Factory.Save();
			AssertEquals("Precondition: Count of Packages", 2, cusPackingList.PackageJob.Packages.Count);
			AssertEquals("Precondition: Count of PackableItems", 4, cusPackingList.PackableItems.Count);
			AssertEquals("Precondition: Count of Package1 PackedItemDivots", 1, cusPackage1.PackedItemDivots.Count);
			AssertEquals("Precondition: Count of Package2 PackedItemDivots", 1, cusPackage2.PackedItemDivots.Count);

			var anotherFactory = new BusinessObjectFactory();
			var clonedDec = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, anotherFactory).Clone();
			var clonedPacklist = clonedDec.LoadCusPackingList(anotherFactory);
			var clonedPackage1 = clonedPacklist.PackageJob.Packages.First(x => x.KP_Sequence == 1);
			var clonedPackage2 = clonedPacklist.PackageJob.Packages.First(x => x.KP_Sequence == 2);
			var clonedPackableItem1 = clonedPacklist.PackableItems.First(x => x.CUI_Sequence == 1);
			var clonedPackableItem2 = clonedPacklist.PackableItems.First(x => x.CUI_Sequence == 2);
			var clonedCusPackage1PackedItemDivot = clonedPackage1.PackedItemDivots.FirstOrDefault();
			var clonedCusPackage2PackedItemDivot = clonedPackage2.PackedItemDivots.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals(clonedDec.PK, clonedPacklist.CUL_JE);
				AssertEquals("Packages should be copied", 2, clonedPacklist.PackageJob.Packages.Count);
				AssertEquals("PackableItems should be copied", 4, clonedPacklist.PackableItems.Count);
				AssertEquals("PackedItemDivots should be copied", 2, clonedPacklist.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Count());

				AssertEquals("clonedPackage1.KP_GoodsDescription", "desc 1", clonedPackage1.KP_GoodsDescription);
				AssertEquals("clonedPackage2.KP_GoodsDescription", "desc 2", clonedPackage2.KP_GoodsDescription);

				AssertEquals("clonedPackage1.CUI_GoodsDescription", "11", clonedPackableItem1.CUI_GoodsDescription);
				AssertEquals("clonedPackage1.CUI_NetWeight", 10m, clonedPackableItem1.CUI_NetWeight);
				AssertEquals("clonedPackage1.CUI_NetWeightUQ", "KG", clonedPackableItem1.CUI_NetWeightUQ);
				AssertEquals("clonedPackage1.CUI_PackableQty", 12m, clonedPackableItem1.CUI_PackableQty);
				AssertEquals("clonedPackage1.CUI_PackableUQ", "ANT", clonedPackableItem1.CUI_PackableUQ);
				AssertEquals("clonedPackage1.Grouping", "grouping 1", clonedPackableItem1.Grouping);

				AssertEquals("clonedPackage2.CUI_GoodsDescription", "22", clonedPackableItem2.CUI_GoodsDescription);
				AssertEquals("clonedPackage2.CUI_NetWeight", 20m, clonedPackableItem2.CUI_NetWeight);
				AssertEquals("clonedPackage2.CUI_NetWeightUQ", "G", clonedPackableItem2.CUI_NetWeightUQ);
				AssertEquals("clonedPackage2.CUI_PackableQty", 22m, clonedPackableItem2.CUI_PackableQty);
				AssertEquals("clonedPackage2.CUI_PackableUQ", "PLT", clonedPackableItem2.CUI_PackableUQ);
				AssertEquals("clonedPackage2.Grouping", "grouping 2", clonedPackableItem2.Grouping);

				AssertEquals("clonedCusPackage1PackedItemDivot.KI_PackedQty", 3m, clonedCusPackage1PackedItemDivot.KI_PackedQty);
				AssertEquals("clonedCusPackage1PackedItemDivot.PkgNetWeight", 2m, clonedCusPackage1PackedItemDivot.PkgNetWeight);
				AssertEquals("clonedCusPackage1PackedItemDivot.PkgNetWeightUQ", "ANT", clonedCusPackage1PackedItemDivot.PkgNetWeightUQ);

				AssertEquals("clonedCusPackage2PackedItemDivot.KI_PackedQty", 4m, clonedCusPackage2PackedItemDivot.KI_PackedQty);
				AssertEquals("clonedCusPackage2PackedItemDivot.PkgNetWeight", 6m, clonedCusPackage2PackedItemDivot.PkgNetWeight);
				AssertEquals("clonedCusPackage2PackedItemDivot.PkgNetWeightUQ", "BAG", clonedCusPackage2PackedItemDivot.PkgNetWeightUQ);

				Assert("Validation should be suspended", !clonedPacklist.HasNotifications());
				Assert("Setting HasChanges should be suspended", !clonedPacklist.HasChanges);
			});
		}

		public void TestCloneInJobComInvoiceHeader()
		{
			var expectedDesc = "Goods Description";
			var invoice = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			invoice.JZ_Description = expectedDesc;
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_InvoiceUQ = "PCE";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_InvoiceQuantity = 2m;
			line2.JI_InvoiceUQ = string.Empty;
			Factory.Save();

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			packingList.CUL_PackingListDate = invoice.JZ_InvoiceDate.Date;
			packingList.CUL_Description = invoice.JZ_Description;

			var cusPackage1 = packingList.PackageJob.Packages.AddNew();
			cusPackage1.KP_GoodsDescription = "desc 1";
			var relations1 = cusPackage1.PackableItemRelataions;
			var packableItem1 = packingList.PackableItems.AddNew();
			packableItem1.CUI_Sequence = 1;
			packableItem1.CUI_JI = line1.PK;
			packableItem1.CUI_GoodsDescription = "11";
			packableItem1.CUI_NetWeight = 10m;
			packableItem1.CUI_NetWeightUQ = "KG";
			packableItem1.CUI_PackableQty = 12m;
			packableItem1.CUI_PackableUQ = "ANT";
			packableItem1.Grouping = "grouping 1";
			cusPackage1.CustomsPackItem(packableItem1, 3m);

			var cusPackage2 = packingList.PackageJob.Packages.AddNew();
			cusPackage2.KP_GoodsDescription = "desc 2";
			var relations2 = cusPackage2.PackableItemRelataions;
			var packableItem2 = packingList.PackableItems.AddNew();
			packableItem2.CUI_Sequence = 2;
			packableItem2.CUI_JI = line2.PK;
			packableItem2.CUI_GoodsDescription = "22";
			packableItem2.CUI_NetWeight = 20m;
			packableItem2.CUI_NetWeightUQ = "G";
			packableItem2.CUI_PackableQty = 22m;
			packableItem2.CUI_PackableUQ = "PLT";
			packableItem2.Grouping = "grouping 2";
			cusPackage2.CustomsPackItem(packableItem2, 4m);
			Factory.Save();

			var cusPackingListCloneStrategy = new CusPackingListDeepCloneStrategy(packingList, CloneType.TemplateCopy, invoice, invoice.Factory);
			var clonedPacklist = (CusPackingList)cusPackingListCloneStrategy.Clone();
			var clonedPackage1 = clonedPacklist.PackageJob.Packages.First(x => x.KP_Sequence == 1);
			var clonedPackage2 = clonedPacklist.PackageJob.Packages.First(x => x.KP_Sequence == 2);
			var clonedPackableItem1 = clonedPacklist.PackableItems.First(x => x.CUI_Sequence == 1);
			var clonedPackableItem2 = clonedPacklist.PackableItems.First(x => x.CUI_Sequence == 2);
			var clonedCusPackage1PackedItemDivot = clonedPackage1.PackedItemDivots.FirstOrDefault();
			var clonedCusPackage2PackedItemDivot = clonedPackage2.PackedItemDivots.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("Copied CusPackingList Description should be the same", expectedDesc, clonedPacklist.CUL_Description);
				AssertEquals("Packages should be copied", 2, clonedPacklist.PackageJob.Packages.Count);
				AssertEquals("PackableItems should be copied", 2, clonedPacklist.PackableItems.Count);
				AssertEquals("PackedItemDivots should be copied", 2, clonedPacklist.PackageJob.Packages.SelectMany(p => p.PackedItemDivots).Count());

				AssertEquals("clonedPackage1.KP_GoodsDescription", "desc 1", clonedPackage1.KP_GoodsDescription);
				AssertEquals("clonedPackage2.KP_GoodsDescription", "desc 2", clonedPackage2.KP_GoodsDescription);

				AssertEquals("clonedPackage1.CUI_GoodsDescription", "11", clonedPackableItem1.CUI_GoodsDescription);
				AssertEquals("clonedPackage1.CUI_NetWeight", 10m, clonedPackableItem1.CUI_NetWeight);
				AssertEquals("clonedPackage1.CUI_NetWeightUQ", "KG", clonedPackableItem1.CUI_NetWeightUQ);
				AssertEquals("clonedPackage1.CUI_PackableQty", 12m, clonedPackableItem1.CUI_PackableQty);
				AssertEquals("clonedPackage1.CUI_PackableUQ", "ANT", clonedPackableItem1.CUI_PackableUQ);
				AssertEquals("clonedPackage1.Grouping", "grouping 1", clonedPackableItem1.Grouping);

				AssertEquals("clonedPackage2.CUI_GoodsDescription", "22", clonedPackableItem2.CUI_GoodsDescription);
				AssertEquals("clonedPackage2.CUI_NetWeight", 20m, clonedPackableItem2.CUI_NetWeight);
				AssertEquals("clonedPackage2.CUI_NetWeightUQ", "G", clonedPackableItem2.CUI_NetWeightUQ);
				AssertEquals("clonedPackage2.CUI_PackableQty", 22m, clonedPackableItem2.CUI_PackableQty);
				AssertEquals("clonedPackage2.CUI_PackableUQ", "PLT", clonedPackableItem2.CUI_PackableUQ);
				AssertEquals("clonedPackage2.Grouping", "grouping 2", clonedPackableItem2.Grouping);

				AssertEquals("clonedCusPackage1PackedItemDivot.KI_PackedQty", 3m, clonedCusPackage1PackedItemDivot.KI_PackedQty);
				AssertEquals("clonedCusPackage1PackedItemDivot.PkgNetWeight", 2.5m, clonedCusPackage1PackedItemDivot.PkgNetWeight);
				AssertEquals("clonedCusPackage1PackedItemDivot.PkgNetWeightUQ", "KG", clonedCusPackage1PackedItemDivot.PkgNetWeightUQ);

				AssertEquals("clonedCusPackage2PackedItemDivot.KI_PackedQty", 4m, clonedCusPackage2PackedItemDivot.KI_PackedQty);
				AssertEquals("clonedCusPackage2PackedItemDivot.PkgNetWeight", 3.636m, clonedCusPackage2PackedItemDivot.PkgNetWeight);
				AssertEquals("clonedCusPackage2PackedItemDivot.PkgNetWeightUQ", "G", clonedCusPackage2PackedItemDivot.PkgNetWeightUQ);

				Assert("Validation should be suspended", !clonedPacklist.HasNotifications());
				Assert("Setting HasChanges should be suspended", !clonedPacklist.HasChanges);
			});
		}

		class JobDeclarationForTesting : BaseJobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override bool SupportsCusPackingListCore => true;
		}
	}
}
