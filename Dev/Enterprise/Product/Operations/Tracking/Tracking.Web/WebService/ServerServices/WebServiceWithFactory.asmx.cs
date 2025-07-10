using System;
using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Summary description for ZBaseWebService
	/// </summary>
	[WebService(Namespace = "Enterprise.Tracking.Web.WebService")]
	[ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public abstract class WebServiceWithFactory : System.Web.Services.WebService
	{
#region Properties

#region Factory

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(Db.Connection)); }
		}

		BusinessObjectFactory factory;

#endregion

#endregion

#region Implementation

		protected T ExecuteAndDispose<T>(Func<T> method)
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					return method();
				}
				finally
				{
					factory = null;
					Db.DisposeThreadConnection();
#if DEBUG
					ExecuteAndDisposeHasBeenCalled = true;
#endif
				}
			}
		}

#if DEBUG
		public bool ExecuteAndDisposeHasBeenCalled;
#endif

#endregion
	}
}
