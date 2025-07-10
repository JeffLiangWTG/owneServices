using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostAutoRatingStrategy : IAutoRatingStrategy
	{
		public LandedCostAutoRatingStrategy(LandedCostHeader header, ILandedCostHeader host, RatingAdaptersProvider parentProvider = null)
		{
			Argument.NotNull(header, nameof(header));
			Argument.NotNull(host, nameof(host));

			landedCostHeader = header;
			HostBusinessEntity = host as IBusiness;
			ParentProvider = parentProvider;

			if (HostBusinessEntity == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The argument doesn't implent {0}", nameof(IBusiness)), nameof(host));
			}
		}

		public IBusiness HostBusinessEntity { get; private set; }

		public Job Job { get; private set; }

		public bool ShouldAddAutoRates => true;

		public RatingAdaptersProvider ParentProvider { get; }

		public bool Supports(CostSell costOrSell)
		{
			return true;
		}

		public AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			var createdCosts = new List<ChargeWrapper>();

			foreach (var rateInfo in autoRatingResults.Where(r => r.Amount > 0))
			{
				var chargeCodePK = rateInfo.ChargeCode == null ? ZGuid.Empty : rateInfo.ChargeCode.PK;

				var cost = landedCostHeader.CostInputs.AddNew();
				cost.LI_AC_ChargeCode = chargeCodePK;

				cost.LI_ChargeDescription = rateInfo.SingleLineDescription.Left(LandCostInput.Schema.LI_ChargeDescriptionMaxLength);

				cost.LI_CostAmount = rateInfo.Amount;
				cost.LI_RX_NKCostCurrency = rateInfo.Currency;
				cost.LI_IsUserEntered = false;

				cost.DefaultDistributeLevel(HostBusinessEntity as ILandedCostDistributeTo);

				createdCosts.Add(new ChargeWrapper(cost, rateInfo));
			}

			return new AutoRatesAdditionResult(createdCosts, new List<ChargeWrapper>(), 0, autoRatingResults, costOrSell);
		}

		readonly LandedCostHeader landedCostHeader;
	}
}
