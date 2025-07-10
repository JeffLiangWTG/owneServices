using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public static class RatingHeaderExtensions
	{
		public static bool IsClientRate(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.ClientRate;
		}

		public static bool IsGlobalClientRate(this IRatingHeader ratingHeader)
		{
			return ratingHeader.IsClientRate() && ratingHeader.IsGlobal();
		}

		/// <summary>
		/// Check if OrgHeader on client rate has any subsidiaries having party type ManagingGroup
		/// </summary>
		public static bool IsClientRateHavingSubsidiaryRelations(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && IsClientRate(ratingHeader) && ratingHeader.Header != null && ratingHeader.Header.RelatedManagementSubsidiaryRelations.Count > 0;
		}

		public static bool IsTariff(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.Tariff;
		}

		public static bool IsGlobalTariff(this IRatingHeader ratingHeader)
		{
			return ratingHeader.IsTariff() && ratingHeader.IsGlobal();
		}

		public static bool IsQuote(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.Quote;
		}

		public static bool IsOneOffQuote(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.TH_OneTimeQuote;
		}

		public static bool IsCosting(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.Costing;
		}

		public static bool IsGlobalCostRate(this IRatingHeader ratingHeader)
		{
			return ratingHeader.IsCosting() && ratingHeader.IsGlobal();
		}

		public static bool IsIntercompanyTariff(this IRatingHeader ratingHeader) =>
			ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.IntercompanyTariff;

		public static bool IsWiseCostRate(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.RateTypeSafe() == RatingConstants.RatingHeaderTypes.WiseCost;
		}

		public static bool IsStandardCostRate(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.IsCosting() && ratingHeader.TH_OH.IsEmpty;
		}

		public static bool IsGlobal(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null && ratingHeader.Company == null;
		}

		public static bool SupportsRateEntryPublish(this IRatingHeader ratingHeader)
		{
			if (ratingHeader.IsGlobal() || ratingHeader.IsQuote())
			{
				return false;
			}

			var canCreateGlobalRate = (ratingHeader.IsCosting() && Env.Security.GlobalCostingRates.IsAllowed)
				|| (ratingHeader.IsClientRate() && Env.Security.GlobalClientRates.IsAllowed)
				|| (ratingHeader.IsLevelOneTariff() && Env.Security.GlobalTariffRates.IsAllowed);

			return canCreateGlobalRate;
		}

		public static bool SupportsPublishedFilter(this IRatingHeader ratingHeader)
		{
			if (ratingHeader.IsGlobal() || ratingHeader.IsQuote())
			{
				return false;
			}

			var canHavePublishedFilter = (ratingHeader.IsCosting() && Env.Security.GlobalCostingRates.IsAllowed)
				|| (ratingHeader.IsClientRate() && Env.Security.GlobalClientRates.IsAllowed)
				|| (ratingHeader.IsTariff() && Env.Security.GlobalTariffRates.IsAllowed);

			return canHavePublishedFilter;
		}

		public static string RateTypeSafe(this IRatingHeader ratingHeader)
		{
			var type = ratingHeader.DefaultRateType();

			return type != ZString.Empty ? type : (string)ratingHeader.TH_RateType;
		}

		public static string DefaultRateType(this IRatingHeader ratingHeader)
		{
			return ratingHeader != null ? (ZString)RatingHeader.TypesAndCodes.GetCode(ratingHeader.GetType()) : ZString.Empty;
		}

		public static ZQuery GetRelationshipFilter(this IRatingHeader ratingHeader, bool includeGlobal = true)
		{
			var filter = new ZQuery();

			if (ratingHeader.IsAdditionalTariff() && ratingHeader is CompanyTariff tariff)
			{
				var levelOneTariff = tariff.LevelOneTariff;
				if (levelOneTariff != null)
				{
					filter.AddToFilter(RateEntrySchema.TI_TH, levelOneTariff.PK);
				}
			}
			else if (includeGlobal && ratingHeader is RatingHeader header && header.GlobalRatingHeader != null)
			{
				filter.AddToFilter(RateEntrySchema.TI_GC_Publisher, Env.CurrentCompanyPK);
				filter.AddToFilter(RateEntrySchema.TI_TH, new[] { ratingHeader.PK, header.GlobalRatingHeader.PK });
			}
			else
			{
				filter.AddToFilter(RateEntrySchema.TI_TH, ratingHeader.PK);
			}

			return filter;
		}

		public static ZBool IsLevelOneTariff(this IRatingHeader ratingHeader)
		{
			return ratingHeader.IsTariff() && ratingHeader.TH_GlobalRateLevel == 1;
		}

		public static ZBool IsAdditionalTariff(this IRatingHeader ratingHeader)
		{
			return ratingHeader.IsTariff() && !ratingHeader.IsLevelOneTariff();
		}

		public static bool HasLocalClient(this IRatingHeader ratingHeader) =>
			ratingHeader != null
			&&
			(
				ratingHeader.IsTariff()
				|| ratingHeader.Header != null && ratingHeader.Header.IsLocalCountry
			);
	}
}
