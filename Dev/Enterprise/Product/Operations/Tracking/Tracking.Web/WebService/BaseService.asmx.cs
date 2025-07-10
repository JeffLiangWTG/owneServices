using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Summary description for HelperService
	/// </summary>
	[WebService(Namespace = "Enterprise.Tracking.Web.WebService")]
	[ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class BaseService : System.Web.Services.WebService
	{
	}
}
