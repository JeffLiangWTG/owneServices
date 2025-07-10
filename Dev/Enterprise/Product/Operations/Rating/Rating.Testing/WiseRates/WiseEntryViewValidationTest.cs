using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;

namespace Enterprise.RatingTests.WiseRates
{
	sealed class WiseEntryViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidate()
		{
			var entry = new WiseEntry(new Rate(), Factory);
			entry.Errors[RateEntrySchema.TI_OriginLRC] = "Invalid origin";
			entry.Errors[RateEntrySchema.TI_DestinationLRC] = "Invalid destination";
			entry.Errors[RateEntrySchema.TI_RC] = "Invalid container";
			entry.Errors[RateEntrySchema.TI_RH_NKCommodityCode] = "Invalid commodity";
			entry.Errors[RateEntrySchema.TI_OH_ControllingCustomer] = "Invalid controlling customer";
			entry.Errors[RateEntrySchema.TI_PL_NKCarrierServiceLevel] = "Invalid service level";
			entry.Errors[RateEntrySchema.TI_Mode] = "Invalid mode";
			entry.Errors[RateEntrySchema.TI_RateCategory] = "Invalid category";

			var header = new WiseHeader(Factory);
			header.Errors[RatingHeaderSchema.TH_OH] = "Invalid carrier";
			header.ChildRateEntries = new[] { entry };

			var entryView = new WiseEntryView(entry, new RatesSearchResponse());
			entryView.Validation.ValidateAll();

			AssertHasError(entryView.TI_OriginLRCInfo, "Invalid origin");
			AssertHasError(entryView.TI_DestinationLRCInfo, "Invalid destination");
			AssertHasError(entryView.TI_RCInfo, "Invalid container");
			AssertHasError(entryView.TI_RH_NKCommodityCodeInfo, "Invalid commodity");
			AssertHasError(entryView.TI_OH_ControllingCustomerInfo, "Invalid controlling customer");
			AssertHasError(entryView.TI_PL_NKCarrierServiceLevelInfo, "Invalid service level");
			AssertHasError(entryView.TI_ModeInfo, "Invalid mode");
			AssertHasError(entryView.TI_RateCategoryInfo, "Invalid category");
			AssertHasError(entryView.TI_OH_TransportProviderInfo, "Invalid carrier");
		}
	}
}
