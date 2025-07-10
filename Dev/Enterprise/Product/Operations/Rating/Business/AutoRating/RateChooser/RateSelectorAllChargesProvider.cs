#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using WiseRates.Constants;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.RateSelector;

/// <summary>
/// Given an <see cref="AutoRateInfoCollection"/> of carrier charges and a <see cref="RatingCriteria"/>,
/// provides a collection of all charges including carrier charges, subject to fallback charges, and non-carrier charges.
/// </summary>
public class RateSelectorAllChargesProvider(AutoRateInfoCollection carrierCharges, RatingCriteria criteria, ILogger logger, bool applyZeroCharges = true, OrgHeader? carrierOrg = null)
{
	readonly AutoRateInfoCollection carrierCharges = carrierCharges;
	readonly RatingCriteria criteria = criteria;
	readonly ILogger logger = logger;
	readonly BusinessObjectFactory factory = new();
	readonly bool applyZeroCharges = applyZeroCharges;
	readonly OrgHeader? carrierOrg = carrierOrg ??= carrierCharges
		.Select(charge => charge.Entry?.TransportProvider ?? charge.Entry?.ParentRatingHeader.Header)
		.WhereNotNull()
		.FirstOrDefault();

	public AutoRateInfoCollection GetAllCharges()
	{
		var copyCriteria = criteria.CreateCopy(useOriginalProxy: true);
		copyCriteria.IsManualCostSelectMode = true;

		var relatedChargesProvider = new RateChooserRelatedChargesProvider(factory, logger, copyCriteria, new CW1RatesProvider(factory, logger), carrierOrg);
		var allCW1Charges = relatedChargesProvider.GetAllCW1Charges();

		var updatedCarrierCharges = carrierCharges.Select(c => c.HasExplicitZeroAmount ? ApplySubjectToFallback(c, allCW1Charges) : c);
		var nonCarrierCharges = relatedChargesProvider
			.GetNonCarrierCharges(carrierCharges)
			// We exclude carrier on the consol, if any, since it is going to be overridden
			// by the selected carrier
			.Where(autoRateInfo => criteria.Carrier == null || autoRateInfo.ProviderPK != criteria.Carrier.PK);

		var combinedCharges = updatedCarrierCharges
			.Concat(nonCarrierCharges)
			.Where(autoRateInfo => applyZeroCharges || autoRateInfo.Amount > 0);

		var charges = new AutoRateInfoCollection(factory);
		charges.AddRange(combinedCharges);

		NotApplicableRateLineRemover.RemoveItemsOverriddenBySpotCosts(
			criteria,
			charges.Select(c => new Tuple<AccChargeCode, AutoRateInfo>(c.ChargeCode, c)).ToList(),
			(autoRateInfo, log) =>
			{
				charges.Remove(autoRateInfo);
				logger.Information(Res.GetString("44CF6D3C-05D3-4570-8F1D-ED3840454B94", "Charge '{0}' from selected rate, {1}", autoRateInfo.ChargeCode.AC_Code, log));
			}
		);

		return charges;
	}

	/// <summary>
	/// Given rateService charge with SubjectTo type, return CW1 charge with same charge code
	/// </summary>
	AutoRateInfo ApplySubjectToFallback(AutoRateInfo possibleSubjectToCharge, IEnumerable<AutoRateInfo> cw1Charges)
	{
		if (!possibleSubjectToCharge.IsFromRatesService || !possibleSubjectToCharge.IsSubjectTo)
		{
			return possibleSubjectToCharge;
		}

		if (!DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.GetIsFallbackAllowed(WRConstants.RateProviders.CargoGuide, TransportModes.Air, criteria.ContainerMode.ToString()))
		{
			return possibleSubjectToCharge;
		}

		var bestMatchingCW1Charge = cw1Charges.FirstOrDefault(cw1Charge => cw1Charge.ChargeCode.PK == possibleSubjectToCharge.ChargeCode.PK);
		if (bestMatchingCW1Charge is null)
		{
			logger.Information($"{LogEventTypes.RateLineFound} {possibleSubjectToCharge.Line?.DisplayInfo() ?? string.Empty} with Subject To charge could not fallback to Costing as no match was found.");
			return possibleSubjectToCharge;
		}

		logger.Information($"{LogEventTypes.RateLineFound} {possibleSubjectToCharge.Line?.DisplayInfo() ?? string.Empty} with Subject To charge fallback to RateLine {bestMatchingCW1Charge.Line.DisplayInfo()}.");
		return bestMatchingCW1Charge;
	}
}
