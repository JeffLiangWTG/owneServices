using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class UniversalReferenceTestDataHelper : Universal.Testing.UniversalReferenceTestDataHelper
	{
		public UniversalReferenceTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TariffView LoadOrCreateNewTariff(RefCusTariffType tariffType, string tariffCode)
		{
			return LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public RefCusTariffType LoadOrCreateTariffType(string country, string tariffType)
		{
			var query = new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, tariffType);
			query.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, country);
			return factory.LoadTop1<RefCusTariffType>(query)
				?? CreateTariffType(country, tariffType);
		}

		public RateView CreateDutyRate(TariffView tariff, CusRefPreferenceView preference, decimal unitRate, string unitQty)
		{
			var tradeGroup = LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Singapore, "All Countries");
			var rateCode = CreateRateCode(Constants.RateTypes.Duty);
			var dutyRate = CreateRate(tariff, rateCode.PK, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate, string.Format(CultureInfo.InvariantCulture, "[{0}] * {1}", unitQty, unitRate), preference?.PK, dataGrouping: Core.Constants.CountryCodes.Singapore);
			if (unitQty != ZString.Empty)
			{
				LoadOrCreateCreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, unitQty);
			}
			CreateCusApplicability(dutyRate, tradeGroup, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate);
			return dutyRate;
		}
		public RateView CreateDutyRate(TariffView tariff, CusRefPreferenceView preference, decimal percentageRate)
		{
			var tradeGroup = LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Singapore, "All Countries");
			var rateCode = CreateRateCode(Constants.RateTypes.Duty);
			var dutyRate = CreateRate(tariff, rateCode.PK, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate, string.Format(CultureInfo.InvariantCulture, "VFD * {0}", percentageRate / 100), preference?.PK, dataGrouping: Core.Constants.CountryCodes.Singapore);
			CreateCusApplicability(dutyRate, tradeGroup, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate);
			return dutyRate;
		}

		public RateView CreateTariffExciseRate(TariffView tariff, ZDecimal percentageRate)
		{
			var rateCode = CreateRateCode(Constants.RateTypes.Excise);
			return CreateRate(tariff, rateCode.PK, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate, string.Format(CultureInfo.InvariantCulture, "VFD * {0}", percentageRate / 100));
		}

		public RateView CreateTariffExciseRate(TariffView tariff, ZDecimal unitRate, ZString unitQty)
		{
			var rateCode = CreateRateCode(Constants.RateTypes.Excise);
			if (unitQty != ZString.Empty)
			{
				LoadOrCreateCreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, unitQty);
			}
			return CreateRate(tariff, rateCode.PK, tariff.ZZ1_StartDate, tariff.ZZ1_EndDate, string.Format(CultureInfo.InvariantCulture, "[{0}] * {1}", unitQty, unitRate));
		}

		public CusRefRateCodeView CreateRateCode(string rateType)
		{
			var type = CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Singapore, rateType);
			var rateCode = LoadOrCreateNewCusRateCode(factory, rateType, type.PK);
			factory.Save();
			return rateCode;
		}

		#region Commodity

		public TariffView CreateCommodity(TariffView tariff, ZString commodityCode, ZDateTime startDate = new ZDateTime(), ZDateTime endDate = new ZDateTime())
		{
			var commodityType = CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.Commodity);
			var hsnType = CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			factory.Save();
			startDate = !startDate.IsValid ? tariff.ZZ1_StartDate : startDate;
			endDate = !endDate.IsValid ? tariff.ZZ1_EndDate : endDate;
			var commodity = LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, commodityType.PK, commodityCode, startDate, endDate);
			CreateTariffRelationship(commodity.PK, hsnType.PK, tariff.ZZ1_TariffCode);
			return commodity;
		}

		#endregion

		public RefCusTariffUOM LoadOrCreateCreateTariffUOM(TariffView tariff, ZString uomType, ZString uomValue)
		{
			var query = new ZQuery(RefCusTariffUOMSchema.ZZ8_ZZ1_Tariff, tariff.PK);
			query.AddToFilter(RefCusTariffUOMSchema.ZZ8_Type, uomType);
			query.AddToFilter(RefCusTariffUOMSchema.ZZ8_UOM, uomValue);
			var result = factory.LoadTop1<RefCusTariffUOM>(query);

			if (result == null)
			{
				result = factory.New<RefCusTariffUOM>();
				if (tariff != null)
				{
					result.ZZ8_ZZ1_Tariff = tariff.PK;
					result.ZZ8_Type = uomType;
					result.ZZ8_UOM = uomValue;
					result.ZZ8_ZZZ_NKDataGrouping = "";
				}
			}
			return result;
		}
	}
}
