//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRatingDateConfigLookups
//
//    This class should be used for overriding collections in AutoRatingDateConfigLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDateConfigLookups : AutoRatingDateConfigLookups
	{
		public RatingDateConfigLookups(AutoRatingDateConfig parent) : base(parent)
		{
		}

		public RatingDateConfigLookups(RatingDateConfig parent) : base(parent)
		{
		}

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (jobTypeList == null)
				{
					jobTypeList = new CodeDescriptionPairList();
					jobTypeList.InsertInSortOrder(JobInvoicingConsumerTypes.ForwardingConsol);
					jobTypeList.InsertInSortOrder(JobInvoicingConsumerTypes.GatewayConsol);
				}
				return jobTypeList;
			}
		}
		CodeDescriptionPairList jobTypeList;

		public CodeDescriptionPairList DirectionList => AutoRateDateLookupsHelper.DirectionList;

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair("SEA", ResString.GetMultilingualString("27C63D10-90D6-4CCC-B908-E74992C31E39", "Sea Freight"));
					transportModeList.AddPair("AIR", ResString.GetMultilingualString("034b66bd-0286-4255-b2ab-feb4cd027db8", "Air Freight"));
				}
				return transportModeList;
			}
		}
		CodeDescriptionPairList transportModeList;

		public CodeDescriptionPairList RateTypeList
		{
			get
			{
				if (rateTypeList == null)
				{
					var rateTypeList = new CodeDescriptionPairList();
					rateTypeList.AddPair(JobRateTypes.Codes.Cost, JobRateTypes.Descriptions.Cost);
					return rateTypeList;
				}
				return rateTypeList;
			}
		}
		readonly CodeDescriptionPairList rateTypeList;

		public CodeDescriptionPairList ContainerModeList => AutoRateDateLookupsHelper.ContainerModeList;

		public CodeDescriptionPairList DateTypeList => AutoRateDateLookupsHelper.DateTypeList;

		public LocationCollection AutoRatingLocationCollection => AutoRateDateLookupsHelper.AutoRatingLocationCollection;

		AutoRateDateLookupsHelper AutoRateDateLookupsHelper => autoRateDateLookupsHelper ??= new AutoRateDateLookupsHelper((IAutoRateDate)Parent);
		AutoRateDateLookupsHelper autoRateDateLookupsHelper;
	}
}
