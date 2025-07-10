using System.Web;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public interface IWebTrackerPreloadService
	{
		void PreloadModule(HttpContext context, string modulePath);
		void PreloadWebResources(params ZWebResource[] webResources);
	}
}
