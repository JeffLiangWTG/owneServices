using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(PackableItemsSplitter))]
	sealed class PackableItemsSplitterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddSplitPartsOrderBySequence()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Line 1";
			invoiceLine1.JI_CustomsQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line 2";
			invoiceLine2.JI_CustomsQuantity = 200;
			invoiceLine2.JI_InvoiceUQ = "CTN";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Line 3";
			invoiceLine3.JI_CustomsQuantity = 300;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packingList.CUL_Remarks = "Remarks 1";
			packingList.CUL_PackingListNumber = "PACK#1";
			packingList.CUL_PackingListDate = ZDate.Today;
			var packages1 = packingList.PackageJob.Packages.AddNew();
			packingList.PackageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10;
			packagesR1.PackableUQ = "PCE";
			packagesR1.NetWeight = 100;
			packagesR1.NetWeightUQ = "LB";
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10;
			packagesR2.PackableUQ = "PCE";
			packagesR2.NetWeight = 200;
			packagesR2.NetWeightUQ = "LB";
			var packagesR3 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(2);
			packagesR3.PackableQuantity = 10;
			packagesR3.PackableUQ = "PCE";
			packagesR3.NetWeight = 300;
			packagesR3.NetWeightUQ = "LB";
			Factory.Save();

			var packableItemsSplitter = new PackableItemsSplitter(packages1, packagesR2);
			var part1 = packableItemsSplitter.PackableItemParts.AddNew();
			part1.GoodsDescription = "Part 1";
			part1.PackableQuantity = 101;
			part1.PackableUQ = "CTN";
			var part2 = packableItemsSplitter.PackableItemParts.AddNew();
			part2.GoodsDescription = "Part 2";
			part2.PackableQuantity = 143;
			part2.PackableUQ = "CTN";
			var part3 = packableItemsSplitter.PackableItemParts.AddNew();
			part3.GoodsDescription = "Part 3";
			part3.PackableQuantity = 98;
			part3.PackableUQ = "PCE";
			part3.Sequence = 2;
			AssertEquals(true, packableItemsSplitter.DoSplit());
			AssertEquals(5, packages1.PackableItemRelataions.Count);
			packages1.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending);
			var packageCusPackableItemRelations = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			packagesR1 = packageCusPackableItemRelations.First();
			packagesR2 = packageCusPackableItemRelations.ElementAt(1);
			packagesR3 = packageCusPackableItemRelations.ElementAt(2);
			var packagesR4 = packageCusPackableItemRelations.ElementAt(3);
			var packagesR5 = packageCusPackableItemRelations.ElementAt(4);
			CombineAssertions(() =>
			{
				AssertEquals((ZShort)1, packagesR1.Sequence);
				AssertEquals("Line 1", packagesR1.GoodsDescription);
				AssertEquals((ZShort)2, packagesR2.Sequence);
				AssertEquals("Part 1", packagesR2.GoodsDescription);
				AssertEquals(101m, packagesR2.PackableQuantity);
				AssertEquals("CTN", packagesR2.PackableUQ);
				AssertEquals(0m, packagesR2.NetWeight);
				AssertEquals(ZString.Empty, packagesR2.NetWeightUQ);
				AssertEquals((ZShort)3, packagesR3.Sequence);
				AssertEquals("Part 3", packagesR3.GoodsDescription);
				AssertEquals(98m, packagesR3.PackableQuantity);
				AssertEquals("PCE", packagesR3.PackableUQ);
				AssertEquals(0m, packagesR3.NetWeight);
				AssertEquals(ZString.Empty, packagesR3.NetWeightUQ);
				AssertEquals((ZShort)4, packagesR4.Sequence);
				AssertEquals("Part 2", packagesR4.GoodsDescription);
				AssertEquals(143m, packagesR4.PackableQuantity);
				AssertEquals("CTN", packagesR4.PackableUQ);
				AssertEquals(0m, packagesR4.NetWeight);
				AssertEquals(ZString.Empty, packagesR4.NetWeightUQ);
				AssertEquals((ZShort)5, packagesR5.Sequence);
				AssertEquals("Line 3", packagesR5.GoodsDescription);
			});
		}

		public void TestDoSplit()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Line 1";
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_NetWeight = 10m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line 2";
			invoiceLine2.JI_CustomsQuantity = 200m;
			invoiceLine2.JI_InvoiceUQ = "CTN";
			invoiceLine2.JI_NetWeight = 20m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Line 3";
			invoiceLine3.JI_CustomsQuantity = 300m;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_NetWeight = 30m;
			invoiceLine3.JI_NetWeightUQ = "KG";
			Factory.Save();

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packingList.CUL_Remarks = "Remarks 1";
			packingList.CUL_PackingListNumber = "PACK#1";
			packingList.CUL_PackingListDate = ZDate.Today;
			var packages1 = packingList.PackageJob.Packages.AddNew();
			var packages2 = packingList.PackageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10m;
			packagesR1.PackableUQ = "PCE";
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10m;
			packagesR2.PackableUQ = "PCE";
			var packagesR3 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(2);
			packagesR3.PackableQuantity = 10m;
			packagesR3.PackableUQ = "PCE";
			Factory.Save();
			AssertEquals(3, packages1.PackableItemRelataions.Count);
			AssertEquals((ZShort)1, packagesR1.Sequence);
			AssertEquals((ZShort)2, packagesR2.Sequence);
			AssertEquals((ZShort)3, packagesR3.Sequence);

			var packableItemsSplitter = new PackableItemsSplitter(packages1, packagesR2);
			var part1 = packableItemsSplitter.PackableItemParts.AddNew();
			part1.GoodsDescription = "Part 1";
			part1.PackableQuantity = 101m;
			part1.PackableUQ = "CTN";
			part1.NetWeight = 10m;
			part1.NetWeightUQ = "KG";
			var part2 = packableItemsSplitter.PackableItemParts.AddNew();
			part2.GoodsDescription = "Part 2";
			part2.PackableQuantity = 143m;
			part2.PackableUQ = "CTN";
			part2.NetWeight = 20m;
			part2.NetWeightUQ = "KG";
			var part3 = packableItemsSplitter.PackableItemParts.AddNew();
			part3.GoodsDescription = "Part 3";
			part3.PackableQuantity = 98m;
			part3.PackableUQ = "PCE";
			part3.NetWeight = 30m;
			part3.NetWeightUQ = "KG";
			AssertEquals(true, packableItemsSplitter.DoSplit());
			AssertEquals(5, packages1.PackableItemRelataions.Count);
			packages1.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending);
			var packageCusPackableItemRelations = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			packagesR1 = packageCusPackableItemRelations.First();
			packagesR2 = packageCusPackableItemRelations.ElementAt(1);
			packagesR3 = packageCusPackableItemRelations.ElementAt(2);
			var packagesR4 = packageCusPackableItemRelations.ElementAt(3);
			var packagesR5 = packageCusPackableItemRelations.ElementAt(4);
			CombineAssertions(() =>
			{
				AssertEquals((ZShort)1, packagesR1.Sequence);
				AssertEquals("Line 1", packagesR1.GoodsDescription);
				AssertEquals((ZShort)2, packagesR2.Sequence);
				AssertEquals("Part 1", packagesR2.GoodsDescription);
				AssertEquals(101m, packagesR2.PackableQuantity);
				AssertEquals("CTN", packagesR2.PackableUQ);
				AssertEquals(10m, packagesR2.InvoiceLineNetWeight);
				AssertEquals("KG", packagesR2.InvoiceLineNetWeightUQ);
				AssertEquals((ZShort)3, packagesR3.Sequence);
				AssertEquals("Part 2", packagesR3.GoodsDescription);
				AssertEquals(143m, packagesR3.PackableQuantity);
				AssertEquals("CTN", packagesR3.PackableUQ);
				AssertEquals(20m, packagesR3.InvoiceLineNetWeight);
				AssertEquals("KG", packagesR3.InvoiceLineNetWeightUQ);
				AssertEquals((ZShort)4, packagesR4.Sequence);
				AssertEquals("Part 3", packagesR4.GoodsDescription);
				AssertEquals(98m, packagesR4.PackableQuantity);
				AssertEquals("PCE", packagesR4.PackableUQ);
				AssertEquals(30m, packagesR4.InvoiceLineNetWeight);
				AssertEquals("KG", packagesR4.InvoiceLineNetWeightUQ);
				AssertEquals((ZShort)5, packagesR5.Sequence);
				AssertEquals("Line 3", packagesR5.GoodsDescription);
			});

			packages1.CustomsPackItem(packagesR3.PackableItem, 10m);
			packableItemsSplitter = new PackableItemsSplitter(packages1, packagesR3);
			part1 = packableItemsSplitter.PackableItemParts.AddNew();
			part1.GoodsDescription = "Part 1";
			part1.PackableQuantity = 101m;
			part1.PackableUQ = "CTN";
			part1.NetWeight = 40m;
			part1.NetWeightUQ = "KG";
			part2 = packableItemsSplitter.PackableItemParts.AddNew();
			part2.GoodsDescription = "Part 2";
			part2.PackableQuantity = 143m;
			part2.PackableUQ = "CTN";
			part2.NetWeight = 50m;
			part2.NetWeightUQ = "KG";
			AssertEquals("Cannot be split because the item has been packed.", false, packableItemsSplitter.DoSplit());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packages = packingList.PackageJob.Packages.AddNew();
			var packagesRelation = packages.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			return new PackableItemsSplitter(packages, packagesRelation);
		}
	}
}
