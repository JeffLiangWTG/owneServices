using System.Web.Configuration;

namespace CargoWise.eHub.Portal.Helpers
{
    public class Setting
	{
		public static bool ShowHidden()
		{
			bool isHidden;
			bool.TryParse(WebConfigurationManager.AppSettings["ShowHidden"], out isHidden);
			return isHidden;
		}
	}
}