using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignItemClicks
	{
		#region Properties

		public ZDateTime FirstClickDateTime
		{
			get;
			set;
		}

		public ZDateTime LastClickDateTime
		{
			get;
			set;
		}

		public ZInt ClickCount
		{
			get;
			set;
		}

		public ZString LinkContext
		{
			get;
			set;
		}

		public ZString LinkUrl
		{
			get;
			set;
		}

		public ZBool IsImage
		{
			get;
			set;
		}

		#endregion
	}
}
