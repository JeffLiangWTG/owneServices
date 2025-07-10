using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business
{
	public static class UniversalReferenceConstants
	{
		public static class PaymentMethods
		{
			public const string CASH = "CAS";
			public const string DEFERRED = "DEF";
			public const string OLDCASH = "CASH";
			public const string OLDDEFERRED = "DEFERRED";
		}

		public static class RefCusProcedureAttributes
		{
			public const string DTYPaymentMethod = "DTYPaymentMethod";
			public const string TATPaymentMethod = "TATPaymentMethod";
			public const string COMPaymentMethod = "COMPaymentMethod";
			public const string SSGPaymentMethod = "SSGPaymentMethod";
			public const string TPFPaymentMethod = "TPFPaymentMethod";
			public const string VATPaymentMethod = "VATPaymentMethod";
			public const string IsReExportation = "IsReExportation";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute Name in ZZ DB")]
		public static class RefCusCodeListAttributes
		{
			public const string ControllingAgency = "ControllingAgency";
			public const string Remarks = "Remarks";
			public const string Source = "Source";
			public const string Country = "Country";
			public const string Tariff = "Tariff";
		}

		public static class RefCusTaxOrFeeCodes
		{
			public const string TPF = "TPF";
			public const string VAT = "VAT";
			public const string DDF = "DDF";
		}

		public static class RefCusRateCodes
		{
			public const string CTA = "CTA";
			public const string HWS = "HWS";
			public const string TAT = "TAT";
			public const string CTS = "CTS";
			public const string DTA = "DTA";
			public const string DTS = "DTS";
			public const string ADD = "ADD";
			public const string CVD = "CVD";
			public const string RTD = "RTD";
			public const string ADT = "ADT";
			public const string SSG = "SSG";
		}

		public static class RefCusRateTypes
		{
			public const string Duty = "DTY";
			public const string CommodityTaxes = "COM";
			public const string SpecialServiceAndGoods = "SSG";
		}

		public static class PackageTypes
		{
			public const string Package = "PKG";
		}

		public static class MethodOfCalculation
		{
			public const string Percentage = "%";
		}
	}

	public static class ChargeTypeHelper
	{
		public static CodeDescriptionPairList GetEntryHeaderChargeTypes(BusinessObjectFactory factory, CusEntryHeader header)
		{
			var chargeTypePairList = new CodeDescriptionPairList();
			GetChargeTypes(factory, header?.DateForDutyRate ?? ZDate.Today).ToList().ForEach(charge => chargeTypePairList.AddPair(charge.RateCode, charge.Description));
			return chargeTypePairList;
		}

		public static IEnumerable<ChargeType> GetChargeTypes(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			var cacheKey = string.Format(System.Globalization.CultureInfo.InvariantCulture, "TW_{0}_CusEntryHeaderCharges_ChargeTypeList", dateOfValuation);
			return factory.GetCachedValue<IEnumerable<ChargeType>>(cacheKey, () =>
			{
				var chargeTypePaitList = new SortedList<ZString, ChargeType>();
				foreach (ICodeDescription specialDuty in factory.GetCachedValue<SpecialDutyRateCodeList>())
				{
					var code = specialDuty.Code;
					chargeTypePaitList.Add(code, new ChargeType() { RateCode = code, RateType = RefCusRateTypes.Duty, Description = specialDuty.Description });
				}

				var refCusRateCodeList = CusRefRateCodeView.Loader.Load(factory, Core.Constants.CountryCodes.Taiwan);
				foreach (var refCusRateCode in refCusRateCodeList)
				{
					AddChargeTypeIfNotExist(chargeTypePaitList, refCusRateCode.ZY1_RateCode, refCusRateCode.ZY1_Description, refCusRateCode.RateType.ZZR_RateType);
				}

				var refCusTaxOrFeeList = RefCusTaxOrFee.Loader.GetList(factory, Core.Constants.CountryCodes.Taiwan, dateOfValuation);
				foreach (ICodeDescription refCusTaxOrFee in refCusTaxOrFeeList)
				{
					AddChargeTypeIfNotExist(chargeTypePaitList, refCusTaxOrFee.Code, refCusTaxOrFee.Description, refCusTaxOrFee.Code);
				}
				return chargeTypePaitList.Values;
			});
		}

		static void AddChargeTypeIfNotExist(SortedList<ZString, ChargeType> chargeTypePaitList, string code, string description, string rateType)
		{
			if (!chargeTypePaitList.ContainsKey(code))
			{
				chargeTypePaitList.Add(code, new ChargeType() { RateCode = code, RateType = rateType, Description = description });
			}
		}
	}

	public class ChargeType
	{
		public ZString RateCode { get; set; }
		public ZString RateType { get; set; }
		public ZString Description { get; set; }
	}
}
