using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class WiseEntryViewValidation : WiseRatesViewsValidation<WiseEntryView>
	{
		public WiseEntryViewValidation(WiseEntryView parent) : base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			ValidateCalculatedProperty(parent.TI_RCInfo);
			ValidateCalculatedProperty(parent.TI_RH_NKCommodityCodeInfo);
			ValidateCalculatedProperty(parent.TI_OH_ControllingCustomerInfo);
			ValidateCalculatedProperty(parent.TI_PL_NKCarrierServiceLevelInfo);
			ValidateCalculatedProperty(parent.TI_ModeInfo);
			ValidateCalculatedProperty(parent.TI_RateCategoryInfo);
			ValidateCalculatedProperty(parent.TI_OH_TransportProviderInfo);
			ValidateCalculatedProperty(parent.TI_OriginLRCInfo);
			ValidateCalculatedProperty(parent.TI_DestinationLRCInfo);
			parent.ChildWiseRateLineViews.RunPreSaveValidation();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_RC()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_RC, out var error))
			{
				parent.TI_RCInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.Container?.Code, Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_RH_NKCommodityCode()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_RH_NKCommodityCode, out var error))
			{
				parent.TI_RH_NKCommodityCodeInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.Commodity, Constants.OrgPatternMatchOverrideRelationships.Commodities);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_OH_ControllingCustomer()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_OH_ControllingCustomer, out var error))
			{
				parent.TI_OH_ControllingCustomerInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.ControllingCustomer, Constants.OrgPatternMatchOverrideRelationships.Organisation);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_PL_NKCarrierServiceLevel()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_PL_NKCarrierServiceLevel, out var error))
			{
				parent.TI_PL_NKCarrierServiceLevelInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.ServiceLevel, Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_Mode()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_Mode, out var error))
			{
				parent.TI_ModeInfo.AddError(error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_RateCategory()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_RateCategory, out var error))
			{
				parent.TI_RateCategoryInfo.AddError(error);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_OH_TransportProvider()
		{
			var entryErrors = parent.UnderlyingWiseEntry.Errors;
			var headerErrors = (parent.UnderlyingWiseEntry.ParentRatingHeader as WiseHeader)?.Errors;

			if (entryErrors.TryGetValue(RateEntrySchema.TI_OH_TransportProvider, out var error)
			|| (headerErrors != null && headerErrors.TryGetValue(RatingHeaderSchema.TH_OH, out error)))
			{
				parent.TI_OH_TransportProviderInfo.AddError(error);

				var wiseRate = parent.UnderlyingWiseEntry?.WiseRate;
				var carrier = wiseRate?.Carrier;
				AddUnmappedForeignCode(carrier, Constants.OrgPatternMatchOverrideRelationships.Organisation);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_OriginLRC()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_OriginLRC, out var error))
			{
				parent.TI_OriginLRCInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.Origin, Constants.OrgPatternMatchOverrideRelationships.Port);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for UI Components")]
		void CheckTI_DestinationLRC()
		{
			if (parent.UnderlyingWiseEntry.Errors.TryGetValue(RateEntrySchema.TI_DestinationLRC, out var error))
			{
				parent.TI_DestinationLRCInfo.AddError(error);
				AddUnmappedForeignCode(parent.UnderlyingWiseEntry?.WiseRate?.Destination, Constants.OrgPatternMatchOverrideRelationships.Port);
			}
		}
	}
}
