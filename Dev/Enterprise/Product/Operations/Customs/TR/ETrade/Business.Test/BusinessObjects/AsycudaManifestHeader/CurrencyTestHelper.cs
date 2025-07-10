using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class CurrencyTestHelper
	{
		public CurrencyTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public RefCurrency USDCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates); }
		}

		public RefCurrency EURCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.EuropeanUnion); }
		}

		public RefCurrency TRYCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Turkey); }
		}

		protected readonly BusinessObjectFactory Factory;

		public void SetExchangeRate(RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate)
		{
			TR.Business.Testing.CurrencyTestHelper.SetExchangeRate(currency, rate, effectiveDate);
		}

		public void SetManifestValues(AsycudaManifestHeader header, GlbCompany company, GlbBranch branch, OrgHeader organization, OrgAddress shipper, OrgAddress carrier)
		{
			SetValues(header, company, branch, organization, shipper, carrier);
			Factory.Save();
		}

		public void SetPackValues(AsycudaManifestHeader header, GlbCompany company, GlbBranch branch, OrgHeader organization, OrgAddress shipper, OrgAddress carrier)
		{
			SetValues(header, company, branch, organization, shipper, carrier);
			Factory.Save();
		}
		public void SetValues(AsycudaManifestHeader header, GlbCompany company, GlbBranch branch, OrgHeader organization, OrgAddress shipper, OrgAddress carrier)
		{
			SetRefValues();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.AMA_JobReference = "Test-Ref";
			header.Branch.Company.GC_IsReciprocal = true;
			header.RegistrationNumber = "XXX1234";
			header.RegistrationDate = ZDateTime.Today;
			header.AMA_CustomsOffice = "0044";
			branch = Factory.NewWithValidTestData<GlbBranch>();
			organization = Factory.NewWithValidTestData<OrgHeader>();
			company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			company.CompanyName = "TestName";
			company.GC_OH_OrgProxy = organization.PK;
			company.Branches.Add(branch);
			var bill = header.Bills.AddNew();
			bill.ExemptionCode1 = "HK18";
			bill.ExemptionCode2 = "DOC";
			bill.ExportCountry = Core.Constants.CountryCodes.Germany;
			bill.ABL_CustomsValue = 620;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			bill.TradeCountry = Core.Constants.CountryCodes.Germany;
			bill.DepartureCountry = Core.Constants.CountryCodes.Germany;
			bill.ArrivalCountry = Core.Constants.CountryCodes.Germany;
			bill.ExportCountry = Core.Constants.CountryCodes.Germany;
			bill.ABL_ManifestQty = 1;
			bill.ABL_ManifestUQ = "BI";
			bill.ABL_GrossWeight = (ZDecimal)1500;
			bill.ABL_GrossWeightUQ = "G";
			bill.ABL_Procedure = "9041";
			bill.ABL_BillNumber = "Bill-No-123";
			bill.ABL_ShipperName = "Shipper name";
			bill.ABL_ConsigneeName = "Consignee name";
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_GoodsValue = (ZDecimal)500;
			pack.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.France;
			pack.PackedItem.API_Tariff = "1000";
			pack.PackedItem.API_CustomsQty2 = (ZDecimal)1.5;
			pack.PackedItem.API_CustomsUQ2 = "KGM";
			pack.PackedItem.API_GoodsDescription = "Description";
			var pack2 = bill.Packs.AddNew();
			pack2.PackedItem.API_GoodsValue = (ZDecimal)120;
			pack2.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.France;
			pack2.PackedItem.API_Tariff = "1000";
			pack2.PackedItem.API_NetWeight = (ZDecimal)1490;
			pack2.PackedItem.API_NetWeightUQ = "G";
			pack2.PackedItem.API_CustomsQty2 = (ZDecimal)1.5;
			pack2.PackedItem.API_CustomsUQ2 = "KGM";
			pack2.PackedItem.API_GoodsDescription = "Description";
			header.CalculateDuties();
			Factory.Save();
		}

		public void SetRefValues()
		{
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TRBWH", "TRBWH");
			helper.CreateNewOrGetExistingCusCodeType("TRCWH", "TRCWH");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "TRBWH", "A0001", "Test line 1", new ZDateTime(2019, 10, 31), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "TRCWH", "AAAAAAAAAA", "For String Max Length Test", new ZDateTime(2019, 10, 31), new ZDateTime(2079, 6, 6));
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", true);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0044", "İstanbul Gümrük Dairesi", startDate, endDate);
			helper.CreateCusMap("CNTRY", Core.Constants.CountryCodes.Germany, "004", startDate, endDate, Core.Constants.CountryCodes.Turkey);

			var tariffTypeetr = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "HSN");
			var tariffTypehsn = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETR");

			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "SCD");
			var rateCode10 = helper.CreateCusRateCode(Factory, "10", rateType.PK);
			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "ETR");
			var rateCode = helper.CreateCusRateCode(Factory, "10", rateType2.PK);

			helper.CreateTaxOrFee("89", 119, Core.Constants.CountryCodes.Turkey, 0, 0, "OTH", startDate, endDate, "E-Trade Stamp Tax");

			var tradeGroupCountry2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", startDate, endDate);
			helper.AddCountry(tradeGroupCountry2, Core.Constants.CountryCodes.Germany);

			var tradeGroupCountry3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "All Countries", startDate, endDate);
			helper.AddCountry(tradeGroupCountry3, Core.Constants.CountryCodes.Germany);

			Factory.Save();

			SetExchangeRate(EURCurrency, 8.5m, ZDateTime.Today);
			SetExchangeRate(USDCurrency, 7m, ZDateTime.Today);
			SetExchangeRate(TRYCurrency, 1m, ZDateTime.Today);

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeetr.PK, "1000", startDate, endDate);
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeetr.PK, "2000", startDate, endDate);

			var rateTariff1 = helper.CreateRefCusRate(tariff1.PK, rateCode10.PK, startDate, endDate, "VFD * 0.20", null, "20%", Core.Constants.CountryCodes.Turkey);
			helper.CreateRefCusRate(tariff4.PK, rateCode10.PK, startDate, endDate, "VFD * 0", null, "0%", Core.Constants.CountryCodes.Turkey);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypehsn.PK, "HK18", startDate, endDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypehsn.PK, "DOC", startDate, endDate);

			var rate2 = helper.CreateRefCusRate(tariff2.PK, rateCode.PK, startDate, endDate, "VFD * 0.18", null, "18");
			rate2.ZZ2_RateFormulaDerivedFrom = "18";

			var rate3 = helper.CreateRefCusRate(tariff3.PK, rateCode.PK, startDate, endDate);
			rate3.ZZ2_RateFormulaDerivedFrom = "0";

			helper.CreateCusApplicability(rate2.PK, tradeGroupCountry2, startDate, endDate);
			helper.CreateCusApplicability(rate3.PK, tradeGroupCountry3, startDate, endDate);
			helper.CreateCusApplicability(rateTariff1.PK, tradeGroupCountry3, startDate, endDate);
		}
	}
}
