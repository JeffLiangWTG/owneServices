using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DocCusPackageCusPackableItemRelation))]
	sealed class DocCusPackageCusPackableItemRelationTest : DocBaseWrapperTest
	{
		public void TestGrouping()
		{
			packableItemRelationInternal.Grouping = "test grouping";
			AssertEquals("test grouping", PackableItemRelationWrapperInternal.Grouping);
		}

		public void TestPackedQty()
		{
			packableItemRelationInternal.PackedQty = 1;
			AssertEquals(1m, PackableItemRelationWrapperInternal.PackedQty);

			packableItemRelationInternal.PackedQty = 2;
			AssertEquals(2m, PackableItemRelationWrapperInternal.PackedQty);
		}

		public void TestGoodsDescription()
		{
			packableItemRelationInternal.GoodsDescription = "GoodsDescription1";
			AssertEquals("GoodsDescription1", PackableItemRelationWrapperInternal.GoodsDescription);

			packableItemRelationInternal.GoodsDescription = "GoodsDescription2";
			AssertEquals("GoodsDescription2", PackableItemRelationWrapperInternal.GoodsDescription);
		}

		public void TestPackedUQ()
		{
			packableItemRelationInternal.PackableUQ = "BAG";
			AssertEquals("BAG", PackableItemRelationWrapperInternal.PackedUQ);

			packableItemRelationInternal.PackableUQ = "CAS";
			AssertEquals("CAS", PackableItemRelationWrapperInternal.PackedUQ);
		}

		public void TestPackedQtyInfo()
		{
			packableItemRelationInternal.Package.KP_PackageQty = 0;
			AssertEquals("1 BAG", PackableItemRelationWrapperInternal.PackedQtyInfo);

			packableItemRelationInternal.Package.KP_PackageQty = 1;
			AssertEquals("1 BAG", PackableItemRelationWrapperInternal.PackedQtyInfo);

			packableItemRelationInternal.Package.KP_PackageQty = 2;
			AssertEquals("@0.5 BAG\r\n1 BAG", PackableItemRelationWrapperInternal.PackedQtyInfo);
		}

		public void TestPackedQtyInfoDecimal()
		{
			var packableItemRelationWrapperInternal = PackableItemRelationWrapperInternal;
			packableItemRelationInternal.Package.KP_PackageQty = 0;
			AssertEquals("1 BAG", packableItemRelationWrapperInternal.PackedQtyInfo);

			packableItemRelationWrapperInternal.PackedQtyDecimalPlace = 1;
			AssertEquals("1.0 BAG", packableItemRelationWrapperInternal.PackedQtyInfo);

			packableItemRelationWrapperInternal.PackedQtyDecimalPlace = 2;
			AssertEquals("1.00 BAG", packableItemRelationWrapperInternal.PackedQtyInfo);

			packableItemRelationWrapperInternal.PackedQtyDecimalPlace = 3;
			AssertEquals("1.000 BAG", packableItemRelationWrapperInternal.PackedQtyInfo);
		}

		public void TestPackedQtyDetailInfo()
		{
			AssertPackedQtyDetailInfo(0m, ZString.Empty, 0, ZString.Empty, 0);
			AssertPackedQtyDetailInfo(1m, "PK", 0, "1 PK", 0);
			AssertPackedQtyDetailInfo(1m, "PK", 1, "1 PK", 0);
			AssertPackedQtyDetailInfo(2m, "PK", 2, "@1 PK\r\n2 PK", 0);
			AssertPackedQtyDetailInfo(1.899m, "PK", 2, "@0.95 PK\r\n1.899 PK", 3);
			AssertPackedQtyDetailInfo(1m, "PK", 3, "@0.333 PK\r\n1 PK", 3);
			AssertPackedQtyDetailInfo(1m, "PK", 2, "@0.5 PK\r\n1 PK", 1);
		}

		void AssertPackedQtyDetailInfo(ZDecimal packedQty, ZString packedUQ, ZInt packageQty, ZString expectedDetailInfo, int expectedMaxDecimalPlace)
		{
			packableItemRelationInternal.PackedQty = packedQty;
			packableItemRelationInternal.PackableUQ = packedUQ;
			packableItemRelationInternal.Package.KP_PackageQty = packageQty;

			var packedQtyDetailInfo = PackableItemRelationWrapperInternal.PackedQtyDetailInfo;
			AssertEquals(expectedDetailInfo, packedQtyDetailInfo.GetDetail(0));
			AssertEquals(expectedMaxDecimalPlace, packedQtyDetailInfo.MaxDecimalPlace);
		}

		public void TestNetWeightInfoDecimal()
		{
			var packableItemRelationWrapperInternal = PackableItemRelationWrapperInternal;
			packableItemRelationInternal.NetWeight = 1m;
			packableItemRelationInternal.NetWeightUQ = "KG";
			packableItemRelationInternal.Package.KP_PackageQty = 0;
			AssertEquals("1 KG", packableItemRelationWrapperInternal.NetWeightInfo);

			packableItemRelationWrapperInternal.NetWeightDecimalPlace = 1;
			AssertEquals("1.0 KG", packableItemRelationWrapperInternal.NetWeightInfo);

			packableItemRelationWrapperInternal.NetWeightDecimalPlace = 2;
			AssertEquals("1.00 KG", packableItemRelationWrapperInternal.NetWeightInfo);

			packableItemRelationWrapperInternal.NetWeightDecimalPlace = 3;
			AssertEquals("1.000 KG", packableItemRelationWrapperInternal.NetWeightInfo);
		}

		public void TestNetWeightDetailInfo()
		{
			AssertNetWeightDetailInfo(0m, ZString.Empty, 0, ZString.Empty, 0);
			AssertNetWeightDetailInfo(1m, "PK", 0, "1 PK", 0);
			AssertNetWeightDetailInfo(1m, "PK", 1, "1 PK", 0);
			AssertNetWeightDetailInfo(2m, "PK", 2, "@1 PK\r\n2 PK", 0);
			AssertNetWeightDetailInfo(1.899m, "PK", 2, "@0.95 PK\r\n1.899 PK", 3);
			AssertNetWeightDetailInfo(1m, "PK", 3, "@0.333 PK\r\n1 PK", 3);
			AssertNetWeightDetailInfo(1m, "PK", 2, "@0.5 PK\r\n1 PK", 1);
		}

		void AssertNetWeightDetailInfo(ZDecimal netWeight, ZString netWeightUQ, ZInt packageQty, ZString expectedDetailInfo, int expectedMaxDecimalPlace)
		{
			packableItemRelationInternal.NetWeight = netWeight;
			packableItemRelationInternal.NetWeightUQ = netWeightUQ;
			packableItemRelationInternal.Package.KP_PackageQty = packageQty;

			var netWeightDetailInfo = PackableItemRelationWrapperInternal.NetWeightQtyDetailInfo;
			AssertEquals(expectedDetailInfo, netWeightDetailInfo.GetDetail(0));
			AssertEquals(expectedMaxDecimalPlace, netWeightDetailInfo.MaxDecimalPlace);
		}

		#region Implementation

		CusPackageCusPackableItemRelation GetNewCusPackableItemRelation()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			var invoieceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoieceLine1.JI_Description = "line1";
			invoieceLine1.JI_InvoiceQuantity = 1m;
			invoieceLine1.JI_InvoiceUQ = "BAG";

			cusPackingList = dec.LoadOrCreateCusPackingList(Factory) as CusPackingList;
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			package.PackableItemRelataions.RebuildElements();
			var cusPackableItem = cusPackingList.PackableItems.First();
			package.CustomsPackItem(cusPackableItem, cusPackableItem.CUI_PackableQty);

			return package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
		}

		CusPackingList cusPackingList;

		DocCusPackageCusPackableItemRelation CreatePackableItemRelationWrapper(CusPackageCusPackableItemRelation packableItemRelation)
		{
			return DocCusPackageCusPackableItemRelation.New(packableItemRelation, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocCusPackageCusPackableItemRelation.New(packableItemRelationInternal, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
			packableItemRelationInternal = GetNewCusPackableItemRelation();
			base.SetUp();
		}

		DocCusPackageCusPackableItemRelation PackableItemRelationWrapperInternal => CreatePackableItemRelationWrapper(packableItemRelationInternal);
		CusPackageCusPackableItemRelation packableItemRelationInternal;

		#endregion
	}
}
