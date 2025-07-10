using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business.Testing;

static class RefCusTaxOrFeeHelper
{
	public static void CreateRefCusTaxOrFeeList(BusinessObjectFactory factory)
	{
		var norway = Core.Constants.CountryCodes.Norway;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateTaxOrFee(RefCusTaxOrFee.MV1, 0.25m, norway, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MVA 25%").ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(RefCusTaxOrFee.MV2, 0.15m, norway, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MVA 15%").ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(RefCusTaxOrFee.MVF, 0m, norway, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MVA 0%").ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(RefCusTaxOrFee.MVK, 0.05m, norway, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MVA 5%").ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		factory.Save();
	}

	const string TaxOrFeeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
}
