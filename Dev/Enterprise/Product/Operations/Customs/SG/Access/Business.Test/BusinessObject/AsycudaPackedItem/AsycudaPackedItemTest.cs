using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultGSTPaid()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.GSTPaid = YesNoList.Codes.Yes;
			packedItem.API_Tariff = "25161100";
			AssertEquals("Y", packedItem.GSTPaid);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			packedItem.API_Tariff = "01029010";
			AssertEquals(ZString.Empty, packedItem.GSTPaid);
		}

		public void TestDefaultImportGoodsType()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			var bill = packedItem.Pack.Bill;
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			bill.SG_PartyStatus = YesNoList.Codes.Yes;
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.MajorExporter, packedItem.GoodsType);
			bill.SG_PartyStatus = YesNoList.Codes.No;
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			bill.DutyAmount = ZDecimal.Zero;
			packedItem.API_Tariff = "25161100";
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.ControlledGoods, packedItem.GoodsType);
			packedItem.API_Tariff = "01029010";
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.MajorExporter, packedItem.GoodsType);
			bill.DutyAmount = 1m;
			packedItem.API_Tariff = "25161100";
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.DutiableGoods, packedItem.GoodsType);
			packedItem.API_Tariff = "01029010";
			packedItem.DefaultImportGoodsType();
			AssertEquals(Constants.GoodsType.DutiableGoods, packedItem.GoodsType);
		}

		public void TestCustomsValueRoundingTo1Cent()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Eritrea);
			var now = ZDateTime.Today;
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = now.AddDays(-2);
			rate.RE_ExpiryDate = now.AddDays(2);
			rate.RE_SellRate = 9.8468621;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			pack.LinePrice = 1;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Eritrea;
			AssertEquals(0.10m, packedItem.API_CustomsValue);
			pack.LinePrice = 0m;
			AssertEquals(0m, packedItem.API_CustomsValue);
			pack.LinePrice = 0.01m;
			AssertEquals(0.01m, packedItem.API_CustomsValue);
			Factory.Save();
			AssertEquals(0.01m, packedItem.API_CustomsValue);
		}

		public void TestIAsycudaPackedItem()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPackedItem>(bizObj.PK).GetType());
		}

		public void TestSetDefaultsFromPack()
		{
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			AssertEquals("Goods Type", Constants.GoodsType.NormalGoods, packedItem.GoodsType);
			AssertEquals("Goods Origin", Core.Constants.CountryCodes.Australia, packedItem.API_RN_NKGoodsOrigin);
			AssertEquals("Goods Description", ZString.Empty, packedItem.API_GoodsDescription);
			AssertEquals("Customs Qty", ZDecimal.Zero, packedItem.API_CustomsQty);
			AssertEquals("(Customs) UQ", UnitOfQuantityCodeList.Codes.NMB, packedItem.API_CustomsUQ);
			pack.APA_GoodsDescription = "HELLO WORLD";
			AssertEquals("Goods Description", "HELLO WORLD", packedItem.API_GoodsDescription);
			pack.APA_PackUQ = "KG";
			pack.APA_PackQty = 10;
			AssertEquals("Customs Qty", 10m, packedItem.API_CustomsQty);
			AssertEquals("(Customs) UQ", UnitOfQuantityCodeList.Codes.KGM, packedItem.API_CustomsUQ);
			pack.APA_PackUQ = "T";
			AssertEquals("Customs Qty", 10m, packedItem.API_CustomsQty);
			AssertEquals("(Customs) UQ", UnitOfQuantityCodeList.Codes.TNE, packedItem.API_CustomsUQ);
		}

		public void TestSetTariffOnNewPackInGrid()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();

			var packsForBinding = bill.Packs as IBindingList;
			var pack = packsForBinding.AddNew() as AsycudaPack;
			pack.APA_PackQty = 2;
			pack.APA_PackUQ = UnitOfQuantityCodeList.Codes.KGM;

			var packedItem = pack.PackedItem;
			AssertNotNull("Pack should be accessible through Pivot", packedItem.Pack);
			AssertNoExceptionThrown("Set API_Tariff", () => packedItem.API_Tariff = "01029010");
			AssertEquals("API_CustomsQty", 2.0m, packedItem.API_CustomsQty);
			AssertEquals("API_CustomsUQ", UnitOfQuantityCodeList.Codes.KGM, packedItem.API_CustomsUQ);
			AssertEquals("GoodsType", Constants.GoodsType.MajorExporter, packedItem.GoodsType);
			AssertEquals("GSTPaid", ZString.Empty, packedItem.GSTPaid);
		}

		public void TestDecimalPlaces()
		{
			var packedItem = (AsycudaPackedItem)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("API_CustomsValueDecimalPlaces", 2, packedItem.API_CustomsValueDecimalPlaces);
			AssertEquals("API_DutyAmountDecimalPlaces", 2, packedItem.API_DutyAmountDecimalPlaces);
			AssertEquals("API_TaxAmountDecimalPlaces", 2, packedItem.API_TaxAmountDecimalPlaces);
		}

		public void TestCalculateGST()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.08m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.09m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-15), ZDateTime.Today.AddDays(-10), "Goods and Services Tax");
			Factory.Save();

			var packedItem = (AsycudaPackedItem)GetNewBusinessObject();
			packedItem.Pack.Bill.CycleDate = ZDateTime.Today.AddDays(-12);
			packedItem.Pack.Bill.Header.AMA_ManifestType = SGManifestTypes.Codes.MGI;

			packedItem.API_CustomsValue = 1.5m;
			packedItem.API_DutyAmount = 3.5m;
			AssertEquals("GST should be calculated based on the value in RefCusTaxOrFee", 0.45m, packedItem.API_TaxAmount);

			packedItem.Pack.Bill.Header.AMA_ManifestType = SGManifestTypes.Codes.MGE;
			packedItem.API_CustomsValue = 1.0m;
			packedItem.API_DutyAmount = 3.0m;
			AssertEquals("GST should be calculated based on the value in RefCusTaxOrFee", 0.32m, packedItem.API_TaxAmount);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			return packedItem;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "25161100");
			var com1 = helper.CreateCommodity(tariff1, "com1");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "Y", com1);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "01029010");
			helper.CreateTariffUOM(tariff2, UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.KGM);
			var com2 = helper.CreateCommodity(tariff2, "com2");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, "N", com2);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, "N", com2);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.ISTRANSHIPMENTCONTROL, "N", com2);
			Factory.Save();
		}
	}
}
