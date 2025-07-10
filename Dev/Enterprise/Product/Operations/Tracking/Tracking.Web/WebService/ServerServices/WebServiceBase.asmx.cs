using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Summary description for ZBaseWebService
	/// </summary>
	[WebService(Namespace = "Enterprise.Tracking.Web.WebService")]
	[ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class WebServiceBase : WebServiceWithFactory
	{
		#region Constructors

		public WebServiceBase()
			: base()
		{
			RegisteredMethods = new Dictionary<string, IWebServiceMethod>();
			RegisterMethods();
		}

		#endregion
		#region Methods

		[WebMethod(EnableSession = true)]
		[ScriptMethod]
		public string Execute(string methodName, string methodParameters)
		{
			return ExecuteAndDispose(() =>
			{
				IWebServiceMethod serviceMethod = FindMethod(methodName);
				if (serviceMethod != null)
				{
					return serviceMethod.Execute(methodParameters);
				}
				return new WebServiceResponse().ToString();
			});
		}

		#endregion

		#region Implementation

		protected virtual void RegisterMethods()
		{
		}

		protected void RegisterMethod(IWebServiceMethod webMethod)
		{
			if (RegisteredMethods.Keys.Contains(webMethod.MethodName))
			{
				throw new Exception("Method: " + webMethod.MethodName + " already registered. Duplicated method name detected ");
			}
			RegisteredMethods.Add(webMethod.MethodName, webMethod);
		}

		protected virtual IWebServiceMethod FindMethod(string methodName)
		{
			if (RegisteredMethods.Keys.Contains(methodName))
			{
				return RegisteredMethods[methodName];
			}
			return null;
		}

		readonly Dictionary<string, IWebServiceMethod> RegisteredMethods;

		#endregion
	}
}
