using System.Web.Script.Services;
using System.Web.Services;
using Enterprise.Tracking.Web.WebService;

namespace Enterprise.Tracking.Web.ServerServices
{
	/// <summary>
	/// Summary description for ZCommonWebService
	/// </summary>
	[WebService(Namespace = "Enterprise.Tracking.Web.ServerServices")]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class TrackingWebService : WebServiceShared
	{
		#region Constructors

		public TrackingWebService()
			: base()
		{
		}

		#endregion

		#region Overrides

		protected override void RegisterMethods()
		{
			base.RegisterMethods();
			RegisterMethod(new AWBHandlingCodeLookupWSMethod());
			RegisterMethod(new AWBOtherChargeCodeLookupWSMethod());
			RegisterMethod(new WarehouseOrderLineUpdateWSMethod());
			RegisterMethod(new WarehouseReceiveLineUpdateWSMethod());
			RegisterMethod(new PackLineUpdateWSMethod());
		}

		#endregion
	}
}
