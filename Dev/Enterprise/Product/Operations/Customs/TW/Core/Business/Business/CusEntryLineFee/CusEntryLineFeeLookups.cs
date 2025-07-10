using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee => Parent;

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		public override CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusEntryLineFeeLookups.MethodOfPaymentList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(TaxFeePaymentMethodList.Codes.CAS, TaxFeePaymentMethodList.Descriptions.CAS);
					result.AddPair(TaxFeePaymentMethodList.Codes.DEF, TaxFeePaymentMethodList.Descriptions.DEF);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList MethodOfCalculationList => TWRefCusCodeListTypes.GetMethodOfCalculationList(Factory);

		public override CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<TWRateOverrideReasonList>();

		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var isExport = Parent.EntryLine?.Declaration?.IsExport ?? ZBool.False;
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_ChargeTypeList", isExport ? Common.Shared.SharedJobMessageTypeList.Codes.Export : Common.Shared.SharedJobMessageTypeList.Codes.Import);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var taiwanCountryCode = Core.Constants.CountryCodes.Taiwan;
					var result = new CodeDescriptionPairList();
					if (isExport)
					{
						result.AddRange(new RefCusTaxOrFee.Loader(Factory).LoadTaxOrFeeFromCodeDate(taiwanCountryCode, UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF, ZDateTime.Today));
					}
					else
					{
						result.AddRange(CusRefRateCodeView.Loader.Load(Factory, taiwanCountryCode));
						result.AddRange(RefCusTaxOrFee.Loader.GetList(Factory, taiwanCountryCode, ZDateTime.Now).Cast<RefCusTaxOrFee>().Where(c => c.ZZF_Code == UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT || c.ZZF_Code == UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF).ToList());
					}
					return result;
				});
			}
		}
	}
}
