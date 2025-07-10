using System.Web.Script.Services;
using System.Web.Services;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Summary description for ZCommonWebService
	/// </summary>
	[WebService(Namespace = "Enterprise.Tracking.Web.WebService")]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class WebServiceShared : WebServiceBase
	{
		#region Constructors

		public WebServiceShared()
				: base()
		{
		}

		#endregion

		#region Overrides

		protected override void RegisterMethods()
		{
			base.RegisterMethods();
			RegisterMethod(new DateFormatterWebServiceMethod());
			RegisterMethod(new VolumeCalculatorWebServiceMethod());
			RegisterMethod(new PortFinderWebServiceMethod());
		}

		#endregion
	}
}
