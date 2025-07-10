using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This is a utility class for converting AutoRateInfo to Rate.
	/// </summary>
	public class AutoRateInfoToRateConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		public AutoRateInfoToRateConverter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		/// <summary>
		/// Converts calculation results of autorating (AutoRateInfo) to a collection of Rate objects.
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="inputs"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public IReadOnlyCollection<Rate> Convert(RateQueryRatingAdapter adapter, IEnumerable<AutoRateInfo> inputs, ILogger logger)
		{
			var result = new List<Rate>();

			var rateEntryToRateConverter = new RateEntryToRateConverter(factory);

			foreach (var group in inputs.GroupBy(i => i.Entry))
			{
				var rate = rateEntryToRateConverter.Convert(group.Key, GetRateType(group.Key.ParentRatingHeader), logger, adapter, group.ToArray());
				result.Add(rate);
			}

			return result.ToArray();
		}

		string GetRateType(IRatingHeader ratingHeader)
		{
			if (ratingHeader.IsClientRate())
			{
				return RatingConstants.RatingHeaderTypes.ClientRate;
			}

			if (ratingHeader.IsTariff())
			{
				return RatingConstants.RatingHeaderTypes.Tariff;
			}

			if (ratingHeader.IsQuote())
			{
				return RatingConstants.RatingHeaderTypes.Quote;
			}

			if (ratingHeader.IsCosting())
			{
				return RatingConstants.RatingHeaderTypes.Costing;
			}

			if (ratingHeader.IsIntercompanyTariff())
			{
				return RatingConstants.RatingHeaderTypes.IntercompanyTariff;
			}

			return string.Empty;
		}
	}
}
