#nullable enable
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect
{
	/// <summary>
	/// A wrapper for job rating adapters to allow a user to override fields on the fly within C3.
	/// </summary>
	internal sealed class JobRatingAdapter(IAutoRating autoRating, RateQueryBusinessObject rateQuery, BusinessObjectFactory factory)
		: AutoRatingProxy(autoRating)
	{
		ILocation? origin;
		public override ILocation Origin
		{
			get
			{
				if (rateQuery.Origin != base.Origin.Code)
				{
					if (origin == null || origin.Code != rateQuery.Origin)
					{
						origin = LocationHelper.GetLocationFromString(rateQuery.Origin, factory);
					}

					return origin;
				}

				return base.Origin;
			}
		}

		ILocation? destination;
		public override ILocation Destination
		{
			get
			{
				if (rateQuery.Destination != base.Destination.Code)
				{
					if (destination == null || destination.Code != rateQuery.Destination)
					{
						destination = LocationHelper.GetLocationFromString(rateQuery.Destination, factory);
					}

					return destination;
				}

				return base.Destination;
			}
		}

		IJobDatesProvider? jobDatesProviderOverride;
		public override IJobDatesProvider JobDatesProvider
		{
			get
			{
				if (!rateQuery.EffectiveDate.HasValue)
				{
					return base.JobDatesProvider;
				}

				jobDatesProviderOverride ??= new RateSelectorDatesProvider(rateQuery);
				return jobDatesProviderOverride;
			}
			set
			{
				base.JobDatesProvider = value;
			}
		}

		public override Creditors Creditors { get; set; } = [];

		public override DebtorOrgCollection DebtorOrgs { get; set; } = [];

		public override IEnumerable<ZString> CarrierContractNumbers { get; set; } = [];
	}
}
