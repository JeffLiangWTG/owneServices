using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignClicksByUrl
	{
		#region Properties

		public ZString LinkURL
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
