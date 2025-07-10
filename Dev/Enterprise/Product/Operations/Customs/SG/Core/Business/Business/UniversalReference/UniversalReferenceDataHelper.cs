using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static bool IsDutiableType(this TariffView tariffView)
		{
			var value = tariffView?.GetAttribute(SGConstants.Attributes.Names.CommodityType)?.ZZ3_Value ?? string.Empty;
			return value != string.Empty && duitableTypes.Contains(value);
		}

		static readonly ImmutableList<string> duitableTypes = ImmutableList.Create(CommodityTypeList.Codes.Alcohol, CommodityTypeList.Codes.Tobacco, CommodityTypeList.Codes.Vehicle, CommodityTypeList.Codes.Petroleum);

		public static TariffView[] GetTariffCommodities(this TariffView tariff, ZDateTime effectiveDate)
		{
			var result = new TariffView.Loader(tariff.Factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, tariff.ZZ1_TariffCode, effectiveDate, Constants.TariffTypes.Commodity);
			if (result.Length > 1)
			{
				result = result.OrderBy(x => x.ZZ1_TariffCode).ToArray();
			}
			return result;
		}

		static bool IsCurrent(this TariffView tariffCommodity)
		{
			return (tariffCommodity.ZZ1_StartDate.IsEmpty || tariffCommodity.ZZ1_StartDate <= ZDateTime.Now)
				&& (tariffCommodity.ZZ1_EndDate.IsEmpty || tariffCommodity.ZZ1_EndDate >= ZDateTime.Now);
		}

		public static TariffView GetTariffCommodity(this TariffView tariff, CusLineTariffDetail cusLine)
		{
			var tariffCommodities = tariff.GetTariffCommodities(cusLine.EffectiveAssessmentDate);
			return tariffCommodities.FirstOrDefault(x => x.ZZ1_TariffCode == cusLine.BZ_Tariff && IsCurrent(x));
		}

		public static bool IsUnderExportControl(this TariffView tariff, ZDateTime effectiveDate)
		{
			var tariffCommodities = tariff.GetTariffCommodities(effectiveDate);
			return tariffCommodities.Any(c => c.HasAttribute(SGConstants.Attributes.Names.ISEXPORTCONTROL, SGConstants.Attributes.Values.Yes));
		}

		public static bool IsUnderImportControl(this TariffView tariff, ZDateTime effectiveDate)
		{
			var tariffCommodities = tariff.GetTariffCommodities(effectiveDate);
			return tariffCommodities.Any(c => c.HasAttribute(SGConstants.Attributes.Names.ISIMPORTCONTROL, SGConstants.Attributes.Values.Yes));
		}

		public static bool IsUnderTranshipmentControl(this TariffView tariff, ZDateTime effectiveDate)
		{
			var tariffCommodities = tariff.GetTariffCommodities(effectiveDate);
			return tariffCommodities.Any(c => c.HasAttribute(SGConstants.Attributes.Names.ISTRANSHIPMENTCONTROL, SGConstants.Attributes.Values.Yes));
		}

		public static TariffView LoadLatestTariff(BusinessObjectFactory factory, ZString code)
		{
			TariffView result = null;
			code = new TariffFormatter().Format(code);
			if (!code.IsEmpty)
			{
				result = new TariffView.Loader(factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, code);
			}
			return result;
		}

		public static TariffView LoadBestMatch(BusinessObjectFactory factory, ZString code, ZDateTime assessmentDate)
		{
			TariffView result = null;
			code = new TariffFormatter().Format(code);
			if (!code.IsEmpty)
			{
				result = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem, code, assessmentDate);
			}
			return result;
		}

		public static SGTariffRate GetExciseRate(this TariffView tariff, ZDateTime effectiveDate)
		{
			var cusRate = RateView.Loader.LoadMostRecentCachedRate(tariff, Constants.RateTypes.Excise, effectiveDate);
			return GetRate(tariff, cusRate);
		}

		public static SGTariffRate GetDutyRate(this TariffView tariff, JobComInvoiceLine invoiceLine)
		{
			var cusRate = tariff.GetApplicableRate(invoiceLine.DutyRateSelectionCriteria);
			return GetRate(tariff, cusRate);
		}

		public static SGTariffRate GetRate(this TariffView tariff, RateView rate)
		{
			var result = new SGTariffRate();
			var rateFormula = rate?.ZZ2_RateFormula ?? ZString.Empty;
			if (rateFormula != ZString.Empty)
			{
				result = SGRateFormulaExtractionVisitor.ExtractSGRate(rateFormula, tariff.UnitsOfMeasure.Select(uom => uom.ZZ8_UOM).ToHashSet());
			}
			return result;
		}
	}
}
