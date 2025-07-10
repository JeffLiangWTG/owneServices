using Enterprise.Licensing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for TrackingGlobalConfig.
	/// </summary>
	public class TrackingGlobalConfig : ZGlobalConfig
	{
		public TrackingGlobalConfig() : base()
		{
			WebSiteUrlKey = "WebTrackerUrl";
		}

		protected override LicenceCheckpoint GetLicenceCheckPoint(Licences licences)
		{
			return licences.WebTracker;
		}
	}
}
