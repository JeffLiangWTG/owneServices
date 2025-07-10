using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignClicksPerInterval
	{
		#region Properties

		public ZInt IntervalIndex
		{
			get;
			set;
		}

		public ZDateTime StartDateInclusive
		{
			get;
			set;
		}

		public ZDateTime EndDateExclusive
		{
			get;
			set;
		}

		public ZGuid LinkPk
		{
			get;
			set;
		}

		public ZInt ClickCount
		{
			get;
			set;
		}

		public ZInt UniqueClickCount
		{
			get;
			set;
		}

		#endregion
	}
}
