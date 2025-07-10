using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackage))]
	sealed class CusPackageTest : PkgPackageTest
	{
		public void TestSupportsClone()
		{
			var package = packageJob.Packages.AddNew();
			Assert("SupportsClone should be true", package.SupportsClone());
		}

		public void TestNetWeight()
		{
			var package = packageJob.Packages.AddNew();
			package.NetWeight = 100m;
			AssertEquals(100m, package.KP_Weight);
			AssertEquals(0m, package.KP_TareWeight);

			package.NetWeight = 90;
			AssertEquals(100m, package.KP_Weight);
			AssertEquals(10m, package.KP_TareWeight);

			package = packageJob.Packages.AddNew();
			package.NetWeight = -2m;
			AssertEquals(0m, package.NetWeight);
		}

		public void TestKP_Weight()
		{
			var package = packageJob.Packages.AddNew();
			package.KP_Weight = 10.5m;
			AssertEquals(10.5m, package.NetWeight);
			AssertEquals(0m, package.KP_TareWeight);

			package.KP_Weight = 25m;
			AssertEquals(10.5m, package.NetWeight);
			AssertEquals(14.5m, package.KP_TareWeight);

			package = packageJob.Packages.AddNew();
			package.KP_Weight = -2m;
			AssertEquals(0m, package.KP_Weight);
		}

		public void TestKP_TareWeight()
		{
			var package = packageJob.Packages.AddNew();
			package.KP_TareWeight = 10.5m;
			AssertEquals(0m, package.NetWeight);
			AssertEquals(0m, package.KP_Weight);

			package.NetWeight = 100m;
			AssertEquals(110.5m, package.KP_Weight);

			package.KP_TareWeight = 50m;
			AssertEquals(150m, package.KP_Weight);

			package.NetWeight = 0m;
			AssertEquals(150m, package.KP_Weight);
			AssertEquals(150m, package.KP_TareWeight);

			package.KP_TareWeight = 60m;
			AssertEquals(150m, package.KP_Weight);
			AssertEquals(90m, package.NetWeight);

			package = packageJob.Packages.AddNew();
			package.KP_TareWeight = -2m;
			AssertEquals(0m, package.KP_TareWeight);
		}

		protected override void ResetPackageIfNeeded(PkgPackage package)
		{
			base.ResetPackageIfNeeded(package);
			((CusPackage)package).NetWeight = 0m;
			package.KP_Weight = 0m;
			package.KP_TareWeight = 0m;
		}

		protected override bool IsContainerTypeValid => false;

		protected override bool IsTareWeightDefaulted => false;

		public void TestKP_DunnageWeight()
		{
			var package = packageJob.Packages.AddNew();
			package.KP_DunnageWeight = 30m;
			package.KP_Weight = 100m;
			package.KP_TareWeight = 10.5m;
			AssertEquals(70m, package.NetWeight);
			AssertEquals(110.5m, package.KP_Weight);

			package.KP_DunnageWeight = 40m;
			AssertEquals(70m, package.NetWeight);
			AssertEquals(10.5m, package.KP_TareWeight);
			AssertEquals(110.5m, package.KP_Weight);

			package = packageJob.Packages.AddNew();
			package.KP_DunnageWeight = -2m;
			AssertEquals(0m, package.KP_DunnageWeight);
		}

		public override void TestGetNewValidation()
		{
			var package = packageJob.Packages.AddNew();
			AssertType(typeof(CusPackageValidationForUnfinalisedPackageJob), package.Validation);

			packageJob.KJ_IsFinalized = true;
			AssertType(typeof(PkgPackageValidation), package.Validation);
		}

		public void TestCustomsPackItem()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 10;
			cusPackage.CustomsPackItem(cusPackableItem, 5);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			var divot = cusPackage.PackedItemDivots.First();
			AssertEquals(5m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsPackItem(cusPackableItem, 3);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			divot = cusPackage.PackedItemDivots.First();
			AssertEquals(8m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsUnpackItem(cusPackableItem, 10);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			divot = cusPackage.PackedItemDivots.First();
			AssertEquals(-2m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsPackItem(cusPackableItem, 2);

			AssertEquals(0, cusPackage.PackedItemDivots.Count);
		}

		public void TestCalPkgNetWeight()
		{
			var cusPackage = packingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_NetWeight = 10m;
			cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 30m;
			CombineAssertions(() =>
			{
				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				var divot = cusPackage.PackedItemDivots.FirstOrDefault();
				AssertEquals(3.333m, divot.PkgNetWeight);

				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				AssertEquals(6.667m, divot.PkgNetWeight);

				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				AssertEquals(10m, divot.PkgNetWeight);
			});
		}

		public void TestGetCustomsPackedNetWeight()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 30m;
			cusPackableItem.CUI_NetWeight = 10m;
			cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			CombineAssertions(() =>
			{
				AssertEquals(0m, cusPackage.GetCustomsPackedNetWeight(cusPackableItem));

				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				AssertEquals(3.333m, cusPackage.GetCustomsPackedNetWeight(cusPackableItem));

				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				AssertEquals(6.667m, cusPackage.GetCustomsPackedNetWeight(cusPackableItem));

				cusPackage.CustomsPackItem(cusPackableItem, 10m);
				AssertEquals(10m, cusPackage.GetCustomsPackedNetWeight(cusPackableItem));
			});
		}

		public void TestGetCustomsPackedNetWeightUQ()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 10m;
			cusPackableItem.CUI_NetWeight = 30m;
			cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Grams;
			cusPackage.CustomsPackItem(cusPackableItem, 10m);
			AssertEquals("G", cusPackage.GetCustomsPackedNetWeightUQ(cusPackableItem));

			cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			cusPackage.CustomsPackItem(cusPackableItem, 10m);
			AssertEquals("KG", cusPackage.GetCustomsPackedNetWeightUQ(cusPackableItem));
		}

		public void TestCalculateNetWeight()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var divots = cusPackage.PackedItemDivots;
			var divot1 = divots.AddNew();
			divot1.PkgNetWeight = 10m;
			var divot2 = divots.AddNew();
			divot2.PkgNetWeight = 20m;
			var divot3 = divots.AddNew();
			divot3.PkgNetWeight = 30m;
			AssertEquals(3, cusPackage.PackedItemDivots.Count);
			AssertEquals(60m, cusPackage.CalculateNetWeight());

			divot1.PkgNetWeight = 0m;
			divot2.PkgNetWeight = 0m;
			divot3.PkgNetWeight = 0m;
			cusPackage.KP_TareWeight = 2m;
			cusPackage.KP_Weight = 11m;
			cusPackage.KP_DunnageWeight = 1m;
			AssertEquals(3, cusPackage.PackedItemDivots.Count);
			AssertEquals(8m, cusPackage.CalculateNetWeight());
		}

		public void TestUpdateNetWeightIfItemsChanged()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var divots = cusPackage.PackedItemDivots;
			var divot1 = divots.AddNew();
			divot1.PkgNetWeight = 10m;
			var divot2 = divots.AddNew();
			divot2.PkgNetWeight = 20m;
			var divot3 = divots.AddNew();
			divot3.PkgNetWeight = 30m;

			AssertEquals(3, cusPackage.PackedItemDivots.Count);
			cusPackage.UpdateNetWeightIfItemsChanged();
			AssertEquals(60m, cusPackage.NetWeight);
		}

		public void TestCalculateAllPackedItemDivotsNetWeight()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var divots = cusPackage.PackedItemDivots;
			var divot1 = divots.AddNew();
			divot1.PkgNetWeight = 10m;
			var divot2 = divots.AddNew();
			divot2.PkgNetWeight = 20m;
			var divot3 = divots.AddNew();
			divot3.PkgNetWeight = 30m;

			AssertEquals(3, cusPackage.PackedItemDivots.Count);
			AssertEquals(60m, cusPackage.CalculateAllPackedItemDivotsNetWeight());
		}

		public void TestCustomsUnpackItem()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 10;
			cusPackage.CustomsPackItem(cusPackableItem, 10);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			var divot = cusPackage.PackedItemDivots.First();
			AssertEquals(10m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsUnpackItem(cusPackableItem, 3);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			divot = cusPackage.PackedItemDivots.First();
			AssertEquals(7m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsUnpackItem(cusPackableItem, 8);

			AssertEquals(1, cusPackage.PackedItemDivots.Count);
			divot = cusPackage.PackedItemDivots.First();
			AssertEquals(-1m, divot.KI_PackedQty);
			AssertEquals(cusPackableItem.PK, divot.KI_ParentID);
			AssertEquals(cusPackableItem.TablePrefix, divot.KI_ParentTableCode);
			AssertEquals(cusPackage.PK, divot.KI_KP_Package);

			cusPackage.CustomsPackItem(cusPackableItem, 8);
			cusPackage.CustomsUnpackItem(cusPackableItem, 7);
			AssertEquals(0, cusPackage.PackedItemDivots.Count);
		}

		public void TestGetCustomsPackedQty()
		{
			var cusPackage = packageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = Factory.New<CusPackableItem>();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_PackableQty = 10;

			AssertEquals(0m, cusPackage.GetCustomsPackedQty(cusPackableItem));

			cusPackage.CustomsPackItem(cusPackableItem, 10);
			AssertEquals(10m, cusPackage.GetCustomsPackedQty(cusPackableItem));

			cusPackage.CustomsPackItem(cusPackableItem, 10);
			AssertEquals(20m, cusPackage.GetCustomsPackedQty(cusPackableItem));

			cusPackage.CustomsUnpackItem(cusPackableItem, 15);
			AssertEquals(5m, cusPackage.GetCustomsPackedQty(cusPackableItem));

			cusPackage.CustomsUnpackItem(cusPackableItem, 15);
			AssertEquals(-10m, cusPackage.GetCustomsPackedQty(cusPackableItem));

			cusPackage.CustomsPackItem(cusPackableItem, 10);
			AssertEquals(0m, cusPackage.GetCustomsPackedQty(cusPackableItem));
		}

		public void TestICanDeleteMembers()
		{
			var packages = packageJob.Packages;
			var cusPackage1 = packages.AddNew();
			AssertEquals(1, packages.Count);
			AssertEquals(false, ((ICanDelete)cusPackage1).CanDelete);

			var cusPackage2 = packages.AddNew();
			AssertEquals(2, packages.Count);
			AssertEquals(true, ((ICanDelete)cusPackage1).CanDelete);
			AssertEquals(true, ((ICanDelete)cusPackage2).CanDelete);
		}

		public void TestHasPackItemNewWeight()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine.JI_Description = "line4";
			invoieceLine.JI_InvoiceQuantity = 1m;
			invoieceLine.JI_InvoiceUQ = "BAG";
			var package = packageJob.Packages.AddNew();

			package.PackableItemRelataions.RebuildElements();
			var packableItem = packingList.PackableItems.Cast<CusPackableItem>().FirstOrDefault();
			package.CustomsPackItem(packableItem, 1m);
			var relation = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault();

			Assert(!package.NetWeightSpecifiedOnPackItems);

			relation.NetWeight = 1m;
			Assert(package.NetWeightSpecifiedOnPackItems);
		}

		public void TestDoNotResetKP_WeightWhenPackTypeChanged()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine.JI_Description = "line4";
			invoieceLine.JI_InvoiceQuantity = 1m;
			invoieceLine.JI_InvoiceUQ = "BAG";
			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = "PLT";
			package.KP_Weight = 20m;

			package.KP_F3_NKPackType = "BAG";
			AssertEquals(20m, package.KP_Weight);
		}

		public void TestIsPackableItemRelataionsLoaded()
		{
			var package = packageJob.Packages.AddNew();
			Assert("Should be false", !package.IsPackableItemRelataionsLoaded);

			package.PackableItemRelataions.RebuildElements();
			Assert("Should be true", package.IsPackableItemRelataionsLoaded);
		}

		public void TestUnitGrossWeight()
		{
			var package = packageJob.Packages.AddNew();
			package.KP_Weight = 100m;
			AssertEquals(100m, package.UnitGrossWeight);
			package.KP_PackageQty = 3;
			AssertEquals(33.333m, package.UnitGrossWeight);

			package.KP_Weight = 0;
			package.KP_PackageQty = 5;
			package.UnitGrossWeight = 33.333;
			AssertEquals(166.665m, package.KP_Weight);

			package.UnitGrossWeight = 20.003;
			AssertEquals(100.015m, package.KP_Weight);

			package.KP_PackageQty = 6;
			AssertEquals(100.015m, package.KP_Weight);
			AssertEquals(16.669m, package.UnitGrossWeight);

			package.KP_Weight = 220;
			AssertEquals(36.667m, package.UnitGrossWeight);
		}

		public void TestUnitNetWeight()
		{
			var package = packageJob.Packages.AddNew();
			package.NetWeight = 100m;
			AssertEquals(100m, package.UnitNetWeight);
			package.KP_PackageQty = 3;
			AssertEquals(33.333m, package.UnitNetWeight);

			package.KP_PackageQty = 5;
			package.UnitNetWeight = 33.333;
			AssertEquals(166.665m, package.NetWeight);

			package.UnitNetWeight = 20.003;
			AssertEquals(100.015m, package.NetWeight);

			package.KP_PackageQty = 6;
			AssertEquals(100.015m, package.NetWeight);
			AssertEquals(16.669m, package.UnitNetWeight);

			package.NetWeight = 220;
			AssertEquals(36.667m, package.UnitNetWeight);
		}

		public void TestShouldSetUnitNetWeightWhenGrossWeightChanged()
		{
			var package = packageJob.Packages.AddNew();
			package.NetWeight = 0m;
			package.KP_Weight = 100;
			var unitNetWeightFieldInfo = typeof(CusPackage).GetField("unitNetWeight", BindingFlags.NonPublic | BindingFlags.Instance);
			object unitNetWeight = unitNetWeightFieldInfo.GetValue(package);
			AssertEquals(100m, unitNetWeight);
		}

		public void TestKP_Sequence()
		{
			var package1 = packageJob.Packages.AddNew();
			var package2 = packageJob.Packages.AddNew();
			var package3 = packageJob.Packages.AddNew();
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);

			package1.KP_Sequence = 4;
			AssertEquals((ZShort)3, package1.KP_Sequence);
			AssertEquals((ZShort)1, package2.KP_Sequence);
			AssertEquals((ZShort)2, package3.KP_Sequence);

			package3.KP_Sequence = 0;
			AssertEquals((ZShort)3, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);

			var package4 = packageJob.Packages.AddNew();
			AssertEquals((ZShort)4, package4.KP_Sequence);

			package4.KP_Sequence = 6;
			AssertEquals((ZShort)3, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);
			AssertEquals((ZShort)4, package4.KP_Sequence);
		}

		public void TestCustomAttribute1()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomAttribute1 = "A1";
			AssertEquals("A1", package.CustomAttribute1);
		}

		public void TestCustomAttribute2()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomAttribute2 = "A2";
			AssertEquals("A2", package.CustomAttribute2);
		}

		public void TestCustomFlag1()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomFlag1 = true;
			AssertEquals(true, package.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomFlag2 = true;
			AssertEquals(true, package.CustomFlag2);
		}

		public void TestCustomDate1()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomDate1 = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, package.CustomDate1);
		}

		public void TestCustomDate2()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomDate2 = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, package.CustomDate2);
		}

		public void TestCustomDecimal1()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomDecimal1 = 1m;
			AssertEquals(1m, package.CustomDecimal1);
		}

		public void TestCustomDecimal1_DecimalPlaces()
		{
			var package = packageJob.Packages.AddNew();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(package.GetType(), "CustomDecimal1", true, attrib => attrib.DecimalPlaces == 6);
		}

		public void TestCustomDecimal2()
		{
			var package = packageJob.Packages.AddNew();
			package.CustomDecimal2 = 2m;
			AssertEquals(2m, package.CustomDecimal2);
		}

		public void TestCustomDecimal2_DecimalPlaces()
		{
			var package = packageJob.Packages.AddNew();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(package.GetType(), "CustomDecimal2", true, attrib => attrib.DecimalPlaces == 6);
		}

		protected override BusinessObject GetNewBusinessObject() => GeNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GeNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GeNewBusinessObject(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packageJob = packingList.PackageJob;
		}
		BaseJobDeclaration declaration;
		CusPackageJob packageJob;
		CusPackingList packingList;

		BusinessObject GeNewBusinessObject(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<BaseJobDeclaration>();
			factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(factory);
			factory.Save();
			var package = cusPackingList.PackageJob.Packages.AddNew();
			return package;
		}
	}
}
