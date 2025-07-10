using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackingList))]
	sealed class CusPackingListTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsClone()
		{
			var packingList = base.GetBusinessObjectForFetchForLoad() as CusPackingList;
			AssertEquals(true, packingList.SupportsClone());
		}

		public void TestICustomLabelsConfigOrgProviderConfigOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decl.JE_OH_Supplier = org.PK;
			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			AssertEquals(org, ((ICustomLabelsConfigOrgProvider)packingList).ConfigOrg);
		}

		public void TestTestICustomLabelsConfigOrgProviderConfigOrgChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			bool configOrgChangedCalled = false;
			((ICustomLabelsConfigOrgProvider)packingList).ConfigOrgChanged += delegate
			{ configOrgChangedCalled = true; };
			decl.JE_OH_Supplier = org.PK;

			CombineAssertions(() =>
			{
				AssertEquals("ConfigOrgChanged called", true, configOrgChangedCalled);
				AssertEquals(org, ((ICustomLabelsConfigOrgProvider)packingList).ConfigOrg);
			});
		}

		public void TestPackableItems_SequenceNumberGenerator()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = dec.LoadOrCreateCusPackingList(Factory);
			var packableItem1 = packingList.PackableItems.AddNew();
			var packableItem2 = packingList.PackableItems.AddNew();
			var packableItem3 = packingList.PackableItems.AddNew();
			AssertEquals((ZShort)1, packableItem1.CUI_Sequence);
			AssertEquals((ZShort)2, packableItem2.CUI_Sequence);
			AssertEquals((ZShort)3, packableItem3.CUI_Sequence);
		}

		public void TestDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList = Factory.New<CusPackingList>();
			AssertNull(packingList.Declaration);

			packingList.CUL_JE = declaration.PK;
			AssertSame(declaration, packingList.Declaration);

			declaration.Delete();
			Assert(packingList.IsDeleted);
		}

		public void TestInvoice()
		{
			var invoice = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			var packingList = Factory.New<CusPackingList>();
			AssertNull(packingList.Invoice);

			packingList.CUL_JZ = invoice.PK;
			AssertSame(invoice, packingList.Invoice);

			invoice.Delete();
			Assert(packingList.IsDeleted);
		}

		public void TestCountyCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DJC";
			company.GC_RN_NKCountryCode = "LV";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "LVRIX";
			branch.GB_Code = "DJC";
			Factory.Save();

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.EU.IJobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_GB = branch.PK;

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JE = declaration.PK;
			AssertEquals("LV", packingList.CountryCode);

			packingList = Factory.New<CusPackingList>();
			packingList.CUL_JE = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, packingList.CountryCode);

			var invoice = declaration.Invoices.AddNew();
			packingList = Factory.New<CusPackingList>();
			packingList.CUL_JZ = invoice.PK;
			AssertEquals("LV", packingList.CountryCode);
		}

		public void TestPackageJob()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = packingList.PackageJob;

			AssertNotNull("PackageJob should be created", packageJob);
			Assert("PackageJob.HasChanges should not be set", !packageJob.HasChanges);
			AssertEquals("KJ_ParentID", packingList.PK, packageJob.KJ_ParentID);
			AssertEquals("KJ_ParentTableCode", packingList.TablePrefix, packageJob.KJ_ParentTableCode);
			AssertEquals("KJ_ReleasedTimeUtc", false, packageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("KJ_IsFinalized", false, packageJob.KJ_IsFinalized);

			packageJob.Packages.AddNew();
			Factory.Save();
			Assert(!packageJob.KJ_JobID.IsEmpty);
		}

		public void TestIPackingParent_IsPackingJobReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			AssertEquals(false, ((IPackingParent)packingList).IsPackingJobReadOnly);
		}

		public void TestClusterKey()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();

			AssertEquals(declaration.JE_ClusterKey, packingList.CUL_ClusterKey);
		}

		[TestDate(2020, 10, 11)]
		public void TestLoadFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			var loadedPackingList = declaration.LoadCusPackingList(Factory);
			AssertNull(loadedPackingList);

			var otherFactory = new BusinessObjectFactory();
			var createdPackingList = otherFactory.New<CusPackingList>();
			createdPackingList.CUL_JE = declaration.PK;
			otherFactory.Save();

			loadedPackingList = declaration.LoadCusPackingList(Factory);
			AssertNotNull(loadedPackingList);
			AssertEquals(createdPackingList.PK, loadedPackingList.PK);
		}

		[TestDate(2020, 10, 11)]
		public void TestCreateFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			var createdPackingList = declaration.CreateCusPackingList(Factory);
			AssertNotNull(createdPackingList);
		}

		public void TestLoadOrCreateFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var createdPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Assert(!createdPackingList.PK.IsEmpty);
			AssertEquals(ZString.Empty, createdPackingList.CUL_PackingListNumber);
			AssertEquals(ZString.Empty, createdPackingList.CUL_Remarks);
			AssertEquals(declaration.PK, createdPackingList.CUL_JE);

			createdPackingList.CUL_PackingListNumber = "111";
			createdPackingList.CUL_PackingListDate = new ZDate(2020, 10, 12);
			createdPackingList.CUL_Remarks = "2222";

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedPackingList = declaration.LoadOrCreateCusPackingList(otherFactory);
			AssertEquals("111", loadedPackingList.CUL_PackingListNumber);
			AssertEquals(new ZDate(2020, 10, 12), loadedPackingList.CUL_PackingListDate);
			AssertEquals("2222", loadedPackingList.CUL_Remarks);
			AssertEquals(declaration.PK, loadedPackingList.CUL_JE);
			AssertEquals(loadedPackingList.PK, createdPackingList.PK);
		}

		public void TestJobNumber()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();

			AssertEquals(packingList.JobNumber, packingList.PackageJob.KJ_JobID);
		}

		public void TestDocumentSupporter()
		{
			var cusPackingList = Factory.New<CusPackingList>();
			AssertType<CusPackingListDocumentSupporter>(cusPackingList.DocumentSupporter);
		}

		public void TestDocManagerInfo()
		{
			var cusPackingList = Factory.New<CusPackingList>();
			AssertEquals(Enterprise.Core.Constants.DocManagerCodes.CustomsPackingList, cusPackingList.DocManagerInfo.DocManagerCode);
		}

		public void TestPackableItemsIsDeletedWhenDelete()
		{
			var cusPackingList = Factory.New<CusPackingList>();
			var packableItem1 = cusPackingList.PackableItems.AddNew();
			var packableItem2 = cusPackingList.PackableItems.AddNew();
			var packableItem3 = cusPackingList.PackableItems.AddNew();

			packableItem3.Delete();
			cusPackingList.Delete();
			Assert(cusPackingList.IsDeleted);
			Assert(packableItem1.IsDeleted);
			Assert(packableItem2.IsDeleted);
			Assert(packableItem3.IsDeleted);
		}

		public void TestTotalPackedQty()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BBG";
			var invoieceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine2.JI_Description = "line2";
			invoieceLine2.JI_InvoiceQuantity = 2m;
			invoieceLine2.JI_InvoiceUQ = "BAG";
			var invoieceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine3.JI_Description = "line3";
			invoieceLine3.JI_InvoiceQuantity = 3m;
			invoieceLine3.JI_InvoiceUQ = "PKG";

			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageQty = 1;
			package1.PackableItemRelataions.RebuildElements();

			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageQty = 5;
			package2.PackableItemRelataions.RebuildElements();

			var item1 = cusPackingList.PackableItems[0];
			var item2 = cusPackingList.PackableItems[1];
			var item3 = cusPackingList.PackableItems[2];

			package1.CustomsPackItem(item1, item1.CUI_PackableQty);
			AssertEquals("1.00 BBG", cusPackingList.TotalPackedQty);

			package2.CustomsPackItem(item2, item2.CUI_PackableQty);
			AssertEquals("2.00 BAG, 1.00 BBG", cusPackingList.TotalPackedQty);

			package2.CustomsPackItem(item3, item3.CUI_PackableQty);
			AssertEquals("2.00 BAG, 1.00 BBG, 3.00 PKG", cusPackingList.TotalPackedQty);
		}

		public void TestTotalNetWeight()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_Sequence = 1;
			package1.NetWeight = 1000m;
			package1.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(1000m, cusPackingList.TotalNetWeight);

			var package2 = packageJob.Packages.AddNew();
			package2.KP_Sequence = 2;
			package2.NetWeight = 2000m;
			package2.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(3000m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.MetricCarat;
			AssertEquals(1000.4m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Milligrams;
			AssertEquals(1000.002m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals(1056.699046m, cusPackingList.TotalNetWeight);

			package2.NetWeight = 1m;
			package2.KP_WeightUQ = Core.Constants.Weight.Decitons;
			AssertEquals(1100m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.LongTons;
			AssertEquals(2016.04691m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.ShortTons;
			AssertEquals(1907.184996m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.OuncesTroy;
			AssertEquals(1000.031103m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.PoundsTroy;
			AssertEquals(1000.373242m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(2000m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(1000.453592m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Hectograms;
			AssertEquals(1000.1m, cusPackingList.TotalNetWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1000.001m, cusPackingList.TotalNetWeight);

			var item1 = cusPackingList.PackableItems.AddNew();
			item1.CUI_PackableQty = 10m;
			item1.CUI_NetWeight = 20m;
			item1.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			var item2 = cusPackingList.PackableItems.AddNew();
			item2.CUI_PackableQty = 20m;
			item2.CUI_NetWeight = 10m;
			item2.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			package1.CustomsPackItem(item1, 2m);
			package2.CustomsPackItem(item1, 8m);

			AssertEquals(20m, cusPackingList.TotalNetWeight);

			package1.CustomsPackItem(item2, 4m);
			package2.CustomsPackItem(item2, 16m);

			AssertEquals(30m, cusPackingList.TotalNetWeight);
		}

		public void TestTotalGrossWeight()
		{
			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = dec.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_Sequence = 1;
			package1.KP_Weight = 1000m;
			package1.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(1000m, cusPackingList.TotalGrossWeight);

			var package2 = packageJob.Packages.AddNew();
			package2.KP_Sequence = 2;
			package2.KP_Weight = 2000m;
			package2.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(3000m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.MetricCarat;
			AssertEquals(1000.4m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Milligrams;
			AssertEquals(1000.002m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals(1056.699046m, cusPackingList.TotalGrossWeight);

			package2.KP_Weight = 1;
			package2.KP_WeightUQ = Core.Constants.Weight.Decitons;
			AssertEquals(1100m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.LongTons;
			AssertEquals(2016.04691m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.ShortTons;
			AssertEquals(1907.184996m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.OuncesTroy;
			AssertEquals(1000.031103m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.PoundsTroy;
			AssertEquals(1000.373242m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(2000m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(1000.453592m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Hectograms;
			AssertEquals(1000.1m, cusPackingList.TotalGrossWeight);

			package2.KP_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1000.001m, cusPackingList.TotalGrossWeight);
		}

		public void TestCUL_Description_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			var resourceStrings = DataBoundResourceStrings.GetDataForProperty(packingList.CUL_DescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Goods Description", resourceStrings.Caption);
				AssertEquals("Goods Desc.", resourceStrings.ShortCaption);
			});
		}

		public void TestCUL_CustomAttribute1_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			var resourceStrings = DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomAttribute1Info);
			CombineAssertions(() =>
			{
				AssertEquals("Custom Attribute 1", resourceStrings.Caption);
				AssertEquals("Custom Attr. 1", resourceStrings.ShortCaption);
			});
		}

		public void TestCUL_CustomAttribute2_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			var resourceStrings = DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomAttribute2Info);
			CombineAssertions(() =>
			{
				AssertEquals("Custom Attribute 2", resourceStrings.Caption);
				AssertEquals("Custom Attr. 2", resourceStrings.ShortCaption);
			});
		}

		public void TestCUL_CustomFlag1_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Flag 1", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomFlag1Info).Caption);
		}

		public void TestCUL_CustomFlag2_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Flag 2", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomFlag2Info).Caption);
		}

		public void TestCUL_CustomDate1_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Date 1", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomDate1Info).Caption);
		}

		public void TestCUL_CustomDate2_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Date 2", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomDate2Info).Caption);
		}

		public void TestCUL_CustomDecimal1_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Decimal 1", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomDecimal1Info).Caption);
		}

		public void TestCUL_CustomDecimal2_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Custom Decimal 2", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_CustomDecimal2Info).Caption);
		}

		public void TestCUL_PackageDescription_Caption()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertEquals("Package Description", DataBoundResourceStrings.GetDataForProperty(packingList.CUL_PackageDescriptionInfo).Caption);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.NewWithValidTestData<BaseJobDeclaration>();
			return dec.LoadOrCreateCusPackingList(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var bo = base.GetBusinessObjectForFetchForLoad() as CusPackingList;
			var dec = bo.Factory.NewWithValidTestData<BaseJobDeclaration>();
			bo.CUL_JE = dec.PK;
			return bo;
		}
	}
}
